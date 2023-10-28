using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using Unity.Mathematics;
namespace GJPlus2023
{
    [RequireComponent(typeof(CharacterController))]
    public class _PlayerController : MonoBehaviour
    {
        #region Player Controller
        [FoldoutGroup("Player Controller")][SerializeField] private Animator _animator;
        [FoldoutGroup("Player Controller")][SerializeField] private CharacterController _controller;
        [FoldoutGroup("Player Controller")][SerializeField] private _InputController _input;
        [FoldoutGroup("Player Controller")][SerializeField] private GameObject _mainCamera;
        [FoldoutGroup("Player Controller")] private const float _threshold = 0.01f;
        [FoldoutGroup("Player Controller")] private bool _hasAnimator;
        public void LockCursor(bool value) => _input.SetCursorState(value);
        #endregion

        #region Movement

        [Tooltip("Move speed of the character in m/s")]
        [FoldoutGroup("Movement Parameters")] public float MoveSpeed = 2.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [FoldoutGroup("Movement Parameters")] public float SprintSpeed = 5.335f;
        [Tooltip("How fast the character turns to face movement direction")]
        [FoldoutGroup("Movement Parameters")][Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
        [Tooltip("Acceleration and deceleration")]
        [FoldoutGroup("Movement Parameters")] public float SpeedChangeRate = 10.0f;
        [FoldoutGroup("Movement Parameters")] private float _speed, _animationBlend, _targetRotation = 0.0f, _rotationVelocity, _verticalVelocity, _terminalVelocity = 53.0f;
        [FoldoutGroup("Movement Parameter")] private bool _RotateOnMove;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                if (_RotateOnMove)
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }
        public void ChangeRotateOnMove(bool value) => _RotateOnMove = value;
        #endregion

        #region Jump

        [Tooltip("The height the player can jump")]
        [FoldoutGroup("Jump Parameters")] public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [FoldoutGroup("Jump Parameters")] public float Gravity = -15.0f;
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [FoldoutGroup("Jump Parameters")] public float JumpTimeout = 0.50f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [FoldoutGroup("Jump Parameters")] public float FallTimeout = 0.15f;
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [FoldoutGroup("Jump Parameters")] public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        [FoldoutGroup("Jump Parameters")] public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [FoldoutGroup("Jump Parameters")] public float GroundedRadius = 0.28f;
        [Tooltip("What layers the character uses as ground")]
        [FoldoutGroup("Jump Parameters")] public LayerMask GroundLayers;
        [FoldoutGroup("Jump Parameters")] private float _jumpTimeoutDelta, _fallTimeoutDelta;

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        #endregion

        #region Look Around

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [FoldoutGroup("Cinema Machine Parameters")] public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        [FoldoutGroup("Cinema Machine Parameters")] public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        [FoldoutGroup("Cinema Machine Parameters")] public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        [FoldoutGroup("Cinema Machine Parameters")] public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        [FoldoutGroup("Cinema Machine Parameters")] public bool LockCameraPosition = false;
        [FoldoutGroup("Cinema Machine Parameters")] private float _cinemachineTargetYaw, _cinemachineTargetPitch;
        [FoldoutGroup("Cinema Machine Parameters")] private float _LookSensitivity;
        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * _LookSensitivity;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * _LookSensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public void ChangeSensitivity(float value) => _LookSensitivity = value;

        #endregion

        #region Attack

        [FoldoutGroup("Attack Parameter")] public float attackDamage = 5, attackTimeOut = 0.5f;
        [FoldoutGroup("Attack Parameter")] private float _atkTimeoutDelta;
        [FoldoutGroup("Attack Parameter")] private bool _holdingGun;
        [FoldoutGroup("Attack Parameter")] public ElementStatus statusElement = ElementStatus.Basic;
        [FoldoutGroup("Attack Parameter/Bullet")] public float bulletSpeed;
        [FoldoutGroup("Attack Parameter/Bullet")][SerializeField] private Transform transSpawnAttack, transTargetAim;
        [FoldoutGroup("Attack Parameter/Bullet")][SerializeField] private GameObject prefabBullet;
        [FoldoutGroup("Attack Parameter/Bullet")][SerializeField] private Transform transObjectPulling;
        [FoldoutGroup("Attack Parameter/Bullet")][SerializeField] ObjectPullingBullet oPBullet;

        void Attack()
        {
            if (AimController.IsAim && _input.atk && _atkTimeoutDelta <= 0.0f)
            {
                _input.atk = false;
                GameObject bullet;
                if (oPBullet.listBullet.Count > 0)
                {
                    oPBullet.listBullet[0].transform.parent = null;
                    bullet = oPBullet.listBullet[0];
                    oPBullet.listBullet.Remove(bullet);
                }
                else
                {
                    bullet = Instantiate(prefabBullet);
                    bullet.GetComponent<BulletController>().SetUp(transObjectPulling);
                }
                Vector3 aimDir = (transTargetAim.position - transSpawnAttack.position).normalized;
                bullet.transform.position = transSpawnAttack.position;
                bullet.transform.rotation = quaternion.LookRotation(aimDir, Vector3.up);
                bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;
                _atkTimeoutDelta = attackTimeOut;
            }
            if (_atkTimeoutDelta >= 0)
            {
                _atkTimeoutDelta -= Time.deltaTime;
            }
        }
        #endregion

        #region Animation Id

        [FoldoutGroup("Animation Id")] private int _animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed;

        #endregion

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            AssignAnimationIDs();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _hasAnimator = TryGetComponent(out _animator);
            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        private void Update()
        {
            if (_GameManager.StatusGame != _GameManager.GameStatus.Play) return;
            JumpAndGravity();
            GroundedCheck();
            Move();
            Attack();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        #region Audio Manager
        [FoldoutGroup("Audio Manager")][SerializeField] private AudioSource audioSourcePlayer;
        [FoldoutGroup("Audio Manager")][SerializeField] private AudioClip _AudioClipFootStep, _AudioLanding;
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                audioSourcePlayer.PlayOneShot(_AudioClipFootStep);
        }
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                audioSourcePlayer.PlayOneShot(_AudioLanding);
        }
        #endregion
    }
}