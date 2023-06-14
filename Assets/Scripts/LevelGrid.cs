using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    public const float FLOOR_HEIGHT = 3f;

    public event EventHandler OnAnyUnitMovedGridPosition;

    [SerializeField] private Transform debugGridObjectPrefab;

    private List<GridSystem<GridObject>> gridSystemList;

    [SerializeField] private int gridWidth = 15;
    [SerializeField] private int gridHeight = 15;
    [SerializeField] private float gridScale = 2f;
    [SerializeField] private int floorCount = 2;



    void Awake()
    {
        if (Instance != null) {
            Debug.LogError("There's more than one UnitAction System! " + transform + "-" + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystemList = new List<GridSystem<GridObject>>();
        for (int floor = 0; floor < floorCount; floor++)
        {
            GridSystem<GridObject> gridSystem = new GridSystem<GridObject>(gridWidth, gridHeight, gridScale, floor, FLOOR_HEIGHT,
                (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
            // gridSystem.CreateDebugObjects(debugGridObjectPrefab);
            gridSystemList.Add(gridSystem);
        }
    }

    private void Start()
    {
        Pathfinding.Instance.Setup(gridWidth, gridHeight, gridScale, floorCount);
    }

    private GridSystem<GridObject> GetGridSystem(int floor) {
        return gridSystemList[floor];
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    public int GetFloor(Vector3 worldPosition)
    {
        return Mathf.RoundToInt(worldPosition.y / FLOOR_HEIGHT);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
        int floor = GetFloor(worldPosition);
        return GetGridSystem(floor).GetGridPosition(worldPosition);
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystem(gridPosition.floor).GetWorldPosition(gridPosition);

    public bool IsValidGridPosition(GridPosition gridPosition) {
        if (gridPosition.floor < 0 || gridPosition.floor >= floorCount)
        {
            return false;
        }
        return GetGridSystem(gridPosition.floor).IsValidGridPosition(gridPosition);
    }

    public int GetWidth() => GetGridSystem(0).GetWidth();
    public int GetHeight() => GetGridSystem(0).GetHeight();
    public int GetFloorCount() => floorCount;

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition) 
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable) 
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    public List<GridPosition> GetGridPositionsInRangeOfPosition(GridPosition gridPosition, int range, bool squareShape = false) => GetGridSystem(gridPosition.floor).GetGridPositionsInRangeOfPosition(gridPosition, range, squareShape);
}
