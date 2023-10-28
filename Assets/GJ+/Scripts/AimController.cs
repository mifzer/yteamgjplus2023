using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using GJPlus2023;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
public class AimController : MonoBehaviour
{

    [FoldoutGroup("Aim Controller")][SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [FoldoutGroup("Aim Controller")][SerializeField] private _InputController inputController;
    [FoldoutGroup("Aim Controller")][SerializeField] private _PlayerController playerController;
    [FoldoutGroup("Aim Controller")][SerializeField] private MultiAimConstraint multiAimConstraint;
    [FoldoutGroup("Aim Controller")][SerializeField] private RigBuilder rig;
    [FoldoutGroup("Aim Controller")][SerializeField] private float _NormalSensitivity, _AimSensitiviy;
    [FoldoutGroup("Aim Controller")] public static bool IsAim;
    [FoldoutGroup("Aim Controller")][SerializeField] private GameObject objCrossHair;
    [FoldoutGroup("Aim Controller")][SerializeField] public GameObject objAim;
    [FoldoutGroup("Aim Controller")][SerializeField] private LayerMask aimColliderLayerMask;
    void Update()
    {
        if (inputController.aim)
        {
            IsAim = true;
            aimVirtualCamera.enabled = true;
            playerController.ChangeSensitivity(_AimSensitiviy);
            playerController.ChangeRotateOnMove(false);
        }
        else
        {
            IsAim = false;
            aimVirtualCamera.enabled = false;
            playerController.ChangeSensitivity(_AimSensitiviy);
            playerController.ChangeRotateOnMove(true);
        }
        objCrossHair.SetActive(IsAim);
        objAim.SetActive(IsAim);
        if (!IsAim) return;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            objAim.transform.position = raycastHit.point;
    }

}
