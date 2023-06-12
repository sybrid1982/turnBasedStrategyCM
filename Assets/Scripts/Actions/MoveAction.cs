using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    [SerializeField] private int maxMoveDistance = 4;

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    private List<Vector3> targetPositions;
    private int currentPositionIndex;

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;
        MoveUnitToPosition(targetPositions[currentPositionIndex]);
    }

    private void MoveUnitToPosition(Vector3 targetPosition) {
        float stoppingDistance = 0.1f;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        // rotate towards goal
        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        // move
        if(Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        } 
        else 
        {
            currentPositionIndex++;

            if (currentPositionIndex >= targetPositions.Count)
            {
                transform.position = targetPosition;
                OnStopMoving?.Invoke(this, EventArgs.Empty);

                ActionComplete();
            }
        }

    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
        List<GridPosition> gridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
        targetPositions = new List<Vector3>();
        currentPositionIndex = 0;

        foreach (GridPosition pathGridPosition in gridPositionList)
        {
            targetPositions.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }
    
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (unitGridPosition == testGridPosition)
                {
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                {
                    continue;
                }

                int pathfindingDistanceModifer = 10;
                int pathLength = Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition);
                if (pathLength > maxMoveDistance * pathfindingDistanceModifer)
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetValue = 10;
        int actionValue = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition) * targetValue;
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = actionValue
        };
    }

}
