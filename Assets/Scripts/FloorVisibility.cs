using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorVisibility : MonoBehaviour
{
    [SerializeField] private bool dynamicFloorPosition = false;
    [SerializeField] private List<Renderer> ignoreRenderer;

    private Renderer[] rendererArray;
    private int floor;


    private void Awake() {
        rendererArray = GetComponentsInChildren<Renderer>(true);
    }

    private void Start() {
        floor = LevelGrid.Instance.GetFloor(transform.position);

        if (floor == 0 && !dynamicFloorPosition)
        {
            Destroy(this);
        }

        CameraController.Instance.OnCameraZoomChange += CameraController_OnCameraZoomChange;
    }

    private void Show()
    {
        foreach (Renderer renderer in rendererArray)
        {
            if (ignoreRenderer.Contains(renderer))
            {
                continue;
            }
            renderer.enabled = true;
        }
    }

    private void Hide()
    {
        foreach (Renderer renderer in rendererArray)
        {
            if (ignoreRenderer.Contains(renderer))
            {
                continue;
            }

            renderer.enabled = false;
        }
    }

    private void CameraController_OnCameraZoomChange(object sender, EventArgs e)
    {
        if (dynamicFloorPosition)
        {
            floor = LevelGrid.Instance.GetFloor(transform.position);
        }

        float cameraHeight = CameraController.Instance.GetCameraHeight();

        float floorHeightOffset = 4f;
        bool showObject = cameraHeight > LevelGrid.FLOOR_HEIGHT * floor + floorHeightOffset;

        if (showObject || floor == 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
}
