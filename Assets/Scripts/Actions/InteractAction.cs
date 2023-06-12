using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : BaseAction
{
    private int maxInteractDistance = 1;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }

    public override string GetActionName()
    {
        return "Interact";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> possibleGridPositions = LevelGrid.Instance.GetGridPositionsInRangeOfPosition(unit.GetGridPosition(), maxInteractDistance, true);
        List<GridPosition> validGridPositions = new List<GridPosition>();
        foreach (GridPosition testGridPosition in possibleGridPositions)
        {
            IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(testGridPosition);
            if (interactable == null)
            {
                continue;
            }

            validGridPositions.Add(testGridPosition);
        }

        return validGridPositions;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        IInteractable interactable = LevelGrid.Instance.GetInteractableAtGridPosition(gridPosition);

        interactable.Interact(OnInteractComplete);

        ActionStart(onActionComplete);
    }

    private void OnInteractComplete()
    {
        ActionComplete();
    }
}
