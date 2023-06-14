using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    private static MouseWorld instance;

    [SerializeField] private LayerMask mousePlaneLayerMask;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, mousePlaneLayerMask)) {
            transform.position = raycastHit.point;
        }
    }

    public static Vector3 GetPosition() {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, instance.mousePlaneLayerMask);
        return raycastHit.point;
    }

    public static Vector3 GetPositionOnlyHitVisible()
    {
        RaycastHit[] raycastHits = GetAllRaycastHitsOnMousePlane();

        System.Array.Sort(raycastHits, (RaycastHit raycastHitA, RaycastHit raycastHitB) =>
        {
            return Mathf.RoundToInt(raycastHitA.distance - raycastHitB.distance);
        });

        foreach (RaycastHit raycastHit in raycastHits)
        {
            if (raycastHit.transform.TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (renderer.enabled)
                {
                    return raycastHit.point;
                }
            }
        }
        return Vector3.zero;
    }

    private static RaycastHit[] GetAllRaycastHitsOnMousePlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, float.MaxValue, instance.mousePlaneLayerMask);
        return raycastHits;
    }
}
