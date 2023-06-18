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
        if (gameObject.TryGetComponent<DestructibleCrate>(out DestructibleCrate destructibleCrate))
        {
            DestructibleCrate.OnAnyDestroyed += DestructibleCrate_OnAnyDestroyed;
        }
        if (gameObject.TryGetComponent<Unit>(out Unit unit))
        {
            Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        }
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

    private void DestructibleCrate_OnAnyDestroyed(object sender, EventArgs e)
    {
        DestructibleCrate destructibleCrate = sender as DestructibleCrate;

        if (destructibleCrate.TryGetComponent<FloorVisibility>(out FloorVisibility floorVis))
        {
            if (floorVis == this)
            {
                CameraController.Instance.OnCameraZoomChange -= CameraController_OnCameraZoomChange;
                DestructibleCrate.OnAnyDestroyed -= DestructibleCrate_OnAnyDestroyed;
            }
        }
    }

    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        if (unit.TryGetComponent<FloorVisibility>(out FloorVisibility floorVis))
        {
            if (floorVis == this)
            {
                CameraController.Instance.OnCameraZoomChange -= CameraController_OnCameraZoomChange;
                Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
            }
        }
    }
}
