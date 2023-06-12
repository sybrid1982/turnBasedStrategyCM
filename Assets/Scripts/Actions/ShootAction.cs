using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    private enum State
    {
        Aiming,
        Shooting,
        Cooloff
    }

    private float stateTimer;

    private State state;

    private float totalSpin;
    private Unit targetUnit;
    private bool canShootBullet;

    public static event EventHandler<OnShootEventArgs> OnAnyShoot;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    [SerializeField] private int maxShootingRange = 7;
    [SerializeField] private LayerMask obstacleLayerMask;

    void Update()
    {
        if (!isActive)
        {
            return;
        }
        switch(state)
        {
            case State.Aiming:
                TurnTowardsTargetUnit();
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                }
                break;
            case State.Cooloff:
                break;
        }
        stateTimer -= Time.deltaTime;
        if(stateTimer <= 0)
        {
            NextState();
        }
    }

    private void NextState()
    {
        switch(state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }


    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        List<GridPosition> gridPositionsInRange = LevelGrid.Instance.GetGridPositionsInRangeOfPosition(unitGridPosition, maxShootingRange);

        foreach (GridPosition gridPosition in gridPositionsInRange)
        {
            if(IsTestGridPositionValidTargetFromUnitGridPosition(unitGridPosition, gridPosition))
            {
                validGridPositionList.Add(gridPosition);
            }
        }

        return validGridPositionList;
    }

    private bool IsTestGridPositionValidTargetFromUnitGridPosition(GridPosition unitGridPosition, GridPosition testGridPosition)
    {
        if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
        {
            return false;
        }

        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
        if (targetUnit.IsEnemy() == unit.IsEnemy())
        {
            return false;
        }
        
        Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
        Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;

        float unitShoulderHeight = 1.7f;
        if (Physics.Raycast(
            unitWorldPosition + Vector3.up * unitShoulderHeight,
            shootDir,
            Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
            obstacleLayerMask))
        {
            return false;
        }
        return true;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canShootBullet = true;

        ActionStart(onActionComplete);
    }

    private void Shoot()
    {
        OnAnyShoot?.Invoke(this, new OnShootEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit
        });

        OnShoot?.Invoke(this, new OnShootEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit
        });

        targetUnit.Damage(40);
        canShootBullet = false;
    }

    private void TurnTowardsTargetUnit()
    {
        Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetRange()
    {
        return maxShootingRange;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if(targetUnit.GetHealthNormalized() == 0f) {
            // do not bother shooting dead units
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = 0
            };
        }
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f)
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
}
