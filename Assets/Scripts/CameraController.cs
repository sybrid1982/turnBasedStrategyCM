using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private const float MIN_FOLLOW_Y_OFFSET = 2f;
    private const float MAX_FOLLOW_Y_OFFSET = 15f;

    public static CameraController Instance { get; private set; }

    public event EventHandler OnCameraZoomChange;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;

    void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("multiple instances of CameraController");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }

    private void HandleCameraZoom()
    {
        float previousTargetZoom = targetFollowOffset.y;

        if (InputManager.Instance.GetCameraZoomAmount() != 0) {
            float zoomAmount = cinemachineTransposer.m_FollowOffset.y + InputManager.Instance.GetCameraZoomAmount();
            SetTargetZoomAmount(zoomAmount);
        }

        float zoomSpeed = 12f;
        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
    }

    private void HandleCameraRotation()
    {
        float rotateAmount = InputManager.Instance.GetCameraRotateAmount();

        Vector3 rotationVector = new Vector3(0, rotateAmount, 0);

        float rotationSpeed = 100f;
        transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;
    }

    private void HandleCameraMovement()
    {
        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();

        float moveSpeed = 10f;

        Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    public float GetCameraHeight()
    {
        return targetFollowOffset.y;
    }

    public void SetTargetZoomAmount(float zoomHeight)
    {
        targetFollowOffset.y = zoomHeight;
        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);
        OnCameraZoomChange?.Invoke(this, EventArgs.Empty);
    }
}
