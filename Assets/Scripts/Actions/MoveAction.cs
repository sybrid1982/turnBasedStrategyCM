using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    [SerializeField] private int maxMoveDistance = 4;

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<OnChangeFloorsStartedEventArgs> OnChangedFloorsStarted;
    public class OnChangeFloorsStartedEventArgs : EventArgs
    {
        public GridPosition unitGridPosition;
        public GridPosition targetGridPosition;
    }

    private List<Vector3> targetPositions;
    private int currentPositionIndex;
    private bool isChangingFloors = false;
    private float differentFloorsTeleportTimer;
    private float differentFloorsTeleportTimerMax = 0.5f;

    // Update is called once per frame
    void Update()
    {
        if (!isActive) 
        {
            return;
        }

        Vector3 targetPosition = targetPositions[currentPositionIndex];
        if (isChangingFloors)
        {
            Vector3 moveDirection = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z).normalized;
            RotateTowardsPosition(moveDirection);
            MoveUnitToPositionOnDifferentFloor(targetPosition);
        }
        else
        {
            MoveUnitToPosition(targetPosition);
        }

        if (HasUnitReachedCurrentTargetPosition(targetPosition))
        {
            HandleUnitReachedTargetPosition(targetPosition);
        }
    }

    private void MoveUnitToPosition(Vector3 targetPosition)
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        RotateTowardsPosition(moveDirection);

        float moveSpeed = 4f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void RotateTowardsPosition(Vector3 moveDirection)
    {
        if(moveDirection.y > 0)
        {
            Debug.Log("moveDirection in RotateTowardsPosition has non-zero y, unit could act weird");
        }
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
    }

    private void HandleUnitReachedTargetPosition(Vector3 targetPosition)
    {
        currentPositionIndex++;

        if (ReachedEnd())
        {
            transform.position = targetPosition;
            OnStopMoving?.Invoke(this, EventArgs.Empty);

            ActionComplete();
        }
        else if (NextMoveIsJumpOrFall())
        {
            isChangingFloors = true;
            differentFloorsTeleportTimer = differentFloorsTeleportTimerMax;

            Vector3 nextTargetPosition = targetPositions[currentPositionIndex];

            GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

            OnChangedFloorsStarted?.Invoke(this, new OnChangeFloorsStartedEventArgs {
                unitGridPosition = unitGridPosition,
                targetGridPosition = LevelGrid.Instance.GetGridPosition(nextTargetPosition)
            });
        }
    }
    

    private void MoveUnitToPositionOnDifferentFloor(Vector3 targetPosition)
    {
        differentFloorsTeleportTimer -= Time.deltaTime;
        if (differentFloorsTeleportTimer <= 0f)
        {
            isChangingFloors = false;
            transform.position = targetPosition;
        }
    }

    private bool ReachedEnd()
    {
        return currentPositionIndex >= targetPositions.Count;
    }

    private bool HasUnitReachedCurrentTargetPosition(Vector3 targetPosition)
    {
        float stoppingDistance = 0.1f;
        return Vector3.Distance(transform.position, targetPosition) <= stoppingDistance;
    }

    private bool NextMoveIsJumpOrFall()
    {
        Vector3 targetPosition = targetPositions[currentPositionIndex];
        GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition);
        GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        bool nextMoveIsJumpOrFall = targetGridPosition.floor != unitGridPosition.floor;

        return targetGridPosition.floor != unitGridPosition.floor;
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
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
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
