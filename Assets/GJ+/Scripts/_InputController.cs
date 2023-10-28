using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace GJPlus2023
{
    public class _InputController : MonoBehaviour
    {
        [FoldoutGroup("Input Controller")] public Vector2 move, look;
        [FoldoutGroup("Input Controller")] public bool jump, sprint, atk, aim, mgc, spc, basic, fire, thunder, water;
        [FoldoutGroup("Input Setting")] public bool analogMovement;
        [FoldoutGroup("Cursor Cursor")] public bool cursorInputForLook = true;
#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
        public void OnAim(InputValue value)
        {
            AimInput(value.isPressed);
        }
        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }
        public void OnMagic(InputValue value)
        {
            MagicInput(value.isPressed);
        }
        public void OnSpecial(InputValue value)
        {
            SpecialInput(value.isPressed);
        }
        public void OnBasic(InputValue value)
        {
            BasicInput(value.isPressed);
        }
        public void OnFire(InputValue value)
        {
            FireInput(value.isPressed);
        }
        public void OnThunder(InputValue value)
        {
            ThunderInput(value.isPressed);
        }
        public void onWater(InputValue value)
        {
            WaterInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void AimInput(bool newAimState)
        {
            aim = newAimState;
        }

        public void AttackInput(bool newAttackState)
        {
            atk = newAttackState;
        }

        public void MagicInput(bool newMagicState)
        {
            mgc = newMagicState;
        }

        public void SpecialInput(bool newSpecialState)
        {
            spc = newSpecialState;
        }

        public void BasicInput(bool newBasicState)
        {
            basic = newBasicState;
        }

        public void FireInput(bool newFireState)
        {
            fire = newFireState;
        }

        public void ThunderInput(bool newThunderState)
        {
            thunder = newThunderState;
        }

        public void WaterInput(bool newWaterState)
        {
            water = newWaterState;
        }

        public void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}