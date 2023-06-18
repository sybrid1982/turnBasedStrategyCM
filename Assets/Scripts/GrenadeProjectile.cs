using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrenadeExploded;

    [SerializeField] private Transform grenadeExplodeVfxPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;

    private Vector3 targetPosition;
    private Action onGrenadeBehaviorComplete;
    private float totalDistance;
    private Vector3 positionXZ;

    // Update is called once per frame
    void Update()
    {
        MoveGrenade();

        float closeEnoughDistance = 0.2f;
        if (GrenadeNearTargetPosition(closeEnoughDistance))
        {
            BlowUpGrenade();
        }
    }

    private void BlowUpGrenade()
    {
        float damageRadius = 4f;
        Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

        foreach (Collider collider in colliderArray)
        {
            TryDamageUnit(collider);
            TryDamageCrate(collider);
        }

        OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

        trailRenderer.transform.parent = null;

        Instantiate(grenadeExplodeVfxPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        Destroy(gameObject);

        onGrenadeBehaviorComplete();
    }

    private void TryDamageCrate(Collider collider)
    {
        if (collider.TryGetComponent<DestructibleCrate>(out DestructibleCrate crate))
        {
            crate.Damage();
        }
    }

    private void TryDamageUnit(Collider collider)
    {
        if (collider.TryGetComponent<Unit>(out Unit targetUnit))
        {
            targetUnit.Damage(30, transform.position, 2000f);
        }
    }

    private bool GrenadeNearTargetPosition(float closeEnoughDistance)
    {
        return Vector3.Distance(positionXZ, targetPosition) < closeEnoughDistance;
    }

    private void MoveGrenade()
    {
        Vector3 moveDirection = (targetPosition - positionXZ).normalized;

        float moveSpeed = 15f;
        positionXZ += moveDirection * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);
    }

    public void Setup(GridPosition targetGridPosition, Action onGrenadeBehaviorComplete)
    {
        this.onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        
        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(transform.position, targetPosition);
    }
}
