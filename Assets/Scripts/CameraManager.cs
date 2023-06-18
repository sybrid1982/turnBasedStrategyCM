using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCameraGameObject;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private Transform cameraControllerTransform;

    private CinemachineVirtualCamera actionCameraVirtualCamera;

    private void Start() {
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
        UnitActionSystem.Instance.OnSelectedUnitChange += UnitActionSystem_OnSelectedUnitChange;

        actionCameraVirtualCamera = actionCameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }
    private void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
    }

    private void HideActionCamera()
    {
        actionCameraGameObject.SetActive(false);
    }

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                PositionCameraForShootAction(shootAction);

                ShowActionCamera();
                break;
            case MoveAction moveAction:
                PositionCameraForMoveAction(moveAction);

                ShowActionCamera();
                break;
        }
    }

    private void PositionCameraForShootAction(ShootAction shootAction)
    {

        actionCameraVirtualCamera.LookAt = null;
        actionCameraVirtualCamera.Follow = null;

        Unit shooterUnit = shootAction.GetUnit();
        Unit targetUnit = shootAction.GetTargetUnit();
        Vector3 cameraCharacterHeight = Vector3.up * 1.7f;
        Vector3 shootDir = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

        float shoulderOffsetAmount = 0.5f;
        Vector3 shoulderOffset = Quaternion.Euler(0, 90, 0) * shootDir * shoulderOffsetAmount;

        Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameraCharacterHeight + shoulderOffset + shootDir * -1;
        actionCameraGameObject.transform.position = actionCameraPosition;
        actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);
    }

    private void PositionCameraForMoveAction(MoveAction moveAction)
    {
        Unit movingUnit = moveAction.GetUnit();
        Vector3 cameraMoveHeight = Vector3.up * 15f;
        actionCameraGameObject.transform.position = movingUnit.GetWorldPosition() + cameraMoveHeight;

        actionCameraVirtualCamera.LookAt = movingUnit.transform;
        actionCameraVirtualCamera.Follow = movingUnit.transform;
    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                HideActionCamera();
                break;
        }
        switch (sender)
        {
            case MoveAction moveAction:
                HideActionCamera();
                break;
        }
    }

    private void UnitActionSystem_OnSelectedUnitChange(object sender, Unit unit) 
    {
        Vector3 cameraTargetFocusPosition = UnitActionSystem.Instance.GetSelectedUnit().GetWorldPosition();
        cameraTargetFocusPosition.y = 0;
        cameraControllerTransform.position = cameraTargetFocusPosition;
        float zoomOffset = 5.0f;
        float zoomHeight = unit.GetWorldPosition().y + zoomOffset;
        CameraController cameraController = cameraControllerTransform.GetComponent<CameraController>();
        cameraController.SetTargetZoomAmount(zoomHeight);
    }
}
