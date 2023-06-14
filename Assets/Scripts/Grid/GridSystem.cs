using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private int floor;
    private float floorHeight;
    private TGridObject[,] gridObjectArray;


    public GridSystem(int width, int height, float cellSize, int floor, float floorHeight, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floor = floor;
        this.floorHeight = floorHeight;

        gridObjectArray = new TGridObject[width, height];

        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                GridPosition gridPosition = new GridPosition(x, z, floor);
                gridObjectArray[x, z] = createGridObject(this, gridPosition);
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z) * cellSize +
        new Vector3(0, gridPosition.floor, 0) * floorHeight;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize),
            floor
        );
    }

    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                GridPosition gridPosition = new GridPosition(x, z, floor);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return  gridPosition.x >= 0 && 
                gridPosition.z >= 0 && 
                gridPosition.x < width && 
                gridPosition.z < height &&
                gridPosition.floor == floor;
    }

    public List<GridPosition> GetGridPositionsInRangeOfPosition(GridPosition gridPosition, int range, bool squareShape)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int floor = -range; floor <= range; floor++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
                    GridPosition testGridPosition = gridPosition + offsetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        continue;
                    }
                    if (!squareShape) {
                        int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                        if (testDistance > range)
                        {
                            continue;
                        }
                    }
                    validGridPositionList.Add(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }
}
