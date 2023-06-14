using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    [Serializable]
    public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material material;
    }

    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        RedSoft,
        Yellow
    }
    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;
    
    private GridSystemVisualSingle[,,] gridSystemVisualSingleArray;

    private void Awake() 
    {
        if (Instance != null) {
            Debug.LogError("There's more than one GridSystemVisual System! " + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        gridSystemVisualSingleArray = new GridSystemVisualSingle[
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight(),
            LevelGrid.Instance.GetFloorCount()
        ];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorCount(); floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor); 
                    GridSystemVisualSingle visualSingle = Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity).GetComponent<GridSystemVisualSingle>();
                    gridSystemVisualSingleArray[x, z, floor] = visualSingle;
                }
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChange += UnitActionSystem_OnSelectedActionChange;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;
        // LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMoved;

        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;

        UpdateGridVisual();
    }

    public void HideAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorCount(); floor++)
                {
                    gridSystemVisualSingleArray[x, z, floor].Hide();
                }
            }
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z, 0);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > range)
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }
        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    public void ShowGridPositionList(List<GridPosition> gridPositions, GridVisualType gridVisualType)
    {
        foreach(GridPosition gridPosition in gridPositions)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z, gridPosition.floor].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private void UpdateGridVisual()
    {
        HideAllGridPositions();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        GridVisualType gridVisualType = GridVisualType.White;
        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case GrenadeAction grenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetRange(), GridVisualType.RedSoft);
                break;
            case SwordAction swordAction:
                gridVisualType = GridVisualType.Red;
                ShowGridPositionList(LevelGrid.Instance.GetGridPositionsInRangeOfPosition(selectedUnit.GetGridPosition(), swordAction.GetRange(), true), GridVisualType.RedSoft);
                break;
            case InteractAction interactAction:
                gridVisualType = GridVisualType.Blue;
                break;
        }

        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }
    
    private void UnitActionSystem_OnSelectedActionChange(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void LevelGrid_OnAnyUnitMoved(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void UnitActionSystem_OnBusyChanged(object sender, bool busy)
    {
        UpdateGridVisual();
    }

    private void Unit_OnAnyUnitDied(object sender, EventArgs e) {
        UpdateGridVisual();
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }

        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType: " + gridVisualType);
        return null;
    }
}
