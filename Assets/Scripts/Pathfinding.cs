using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    [SerializeField] private Transform debugGridObjectPrefab;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private LayerMask mousePlaneLayerMask;
    [SerializeField] private Transform pathfindingLinkContainer;


    private int width;
    private int height;
    private float cellSize;
    private int floorAmount;

    private List<GridSystem<PathNode>> gridSystemList;
    private List<PathfindingLink> pathfindingLinkList;

    public static Pathfinding Instance { get; private set; }

    private void Awake() {
        if (Instance != null)
        {
            Debug.LogError("There is more than one Pathfiniding object! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize, int floorAmount)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floorAmount = floorAmount;

        gridSystemList = new List<GridSystem<PathNode>>();

        for (int floor = 0; floor < floorAmount; floor++)
        {
            GridSystem<PathNode> gridSystem = new GridSystem<PathNode>(width, height, cellSize, floor, LevelGrid.FLOOR_HEIGHT,
                (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

            gridSystemList.Add(gridSystem);

            // gridSystem.CreateDebugObjects(debugGridObjectPrefab);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastoffsetDistance = 1f;
                    if(Physics.Raycast(
                        worldPosition + Vector3.down * raycastoffsetDistance,
                        Vector3.up,
                        raycastoffsetDistance * 2,
                        obstacleLayerMask))
                    {
                        GetNode(x, z, floor).SetIsWalkable(false);
                    }

                    if(floor > 0)
                    {
                        if(!Physics.Raycast(
                            worldPosition + Vector3.up * raycastoffsetDistance,
                            Vector3.down,
                            raycastoffsetDistance * 2,
                            mousePlaneLayerMask))
                        {
                            GetNode(x, z, floor).SetIsWalkable(false);
                        }
                    }
                }
            }

        }

        pathfindingLinkList = new List<PathfindingLink>();
        foreach (Transform pathfindlingLinkTransform in pathfindingLinkContainer)
        {
            if (pathfindlingLinkTransform.TryGetComponent(out PathfindingLinkMonobehavior pathfindingLinkMonobehavior))
            {
                pathfindingLinkList.Add(pathfindingLinkMonobehavior.GetPathfindingLink());
            }
        }
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        PathNode startNode = GetGridSystemForFloor(startGridPosition.floor).GetGridObject(startGridPosition);
        PathNode endNode = GetGridSystemForFloor(endGridPosition.floor).GetGridObject(endGridPosition);

        openList.Add(startNode);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int floor = 0; floor < floorAmount; floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    PathNode pathNode = GetGridSystemForFloor(floor).GetGridObject(gridPosition);

                    pathNode.SetGCost(int.MaxValue);
                    pathNode.SetHCost(0);
                    pathNode.CalculateFCost();
                    pathNode.ResetCameFromPathNode();
                }
            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            if (currentNode == endNode)
            {
                // reached final node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighborList(currentNode))
            {
                if (closedList.Contains(neighborNode))
                {
                    continue;
                }

                if (!neighborNode.IsWalkable())
                {
                    closedList.Add(neighborNode);
                    continue;
                }

                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());
                if (tentativeGCost < neighborNode.GetGCost())
                {
                    neighborNode.CameFromPathNode(currentNode);
                    neighborNode.SetGCost(tentativeGCost);
                    neighborNode.SetHCost(CalculateDistance(neighborNode.GetGridPosition(), endGridPosition));
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        pathLength = 0;
        return null;
    }

    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<GridPosition> gridPositions = new List<GridPosition>();
        gridPositions.Add(endNode.GetGridPosition());
        PathNode currentNode = endNode;
        while (currentNode.GetCameFromPathNode() != null)
        {
            gridPositions.Add(currentNode.GetCameFromPathNode().GetGridPosition());
            currentNode = currentNode.GetCameFromPathNode();
        }

        gridPositions.Reverse();

        return gridPositions;
    }

    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCost = pathNodeList[0];

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].GetFCost() < lowestFCost.GetFCost())
            {
                lowestFCost = pathNodeList[i];
            }
        }
        return lowestFCost;
    }

    private GridSystem<PathNode> GetGridSystemForFloor(int floor)
    {
        if (floor >= floorAmount) {
            Debug.LogError("Requested floor " + floor + " out of bounds for floor count of " + floorAmount);
        }
        return gridSystemList[floor];
    }

    private PathNode GetNode(GridPosition gridPosition)
    {
        return GetGridSystemForFloor(gridPosition.floor).GetGridObject(gridPosition);
    }

    private PathNode GetNode(int x, int z, int floor)
    {
        return GetNode(new GridPosition(x, z, floor));
    }

    private List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x > 0)
        {
            // Left
            neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0, gridPosition.floor));
            // Left Down
            if (gridPosition.z > 0)
            {
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor));
            }
            // Left Up
            if (gridPosition.z + 1 < height)
            {
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor));
            }
        }
        if (gridPosition.x + 1 < width)
        {
            // Right
            neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0, gridPosition.floor));
            // Right Down
            if (gridPosition.z > 0)
            {
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));
            }
            // Right Up
            if (gridPosition.z + 1 < height)
            {
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor));
            }
        }
        // Up, Down
        if (gridPosition.z + 1 < height)
        {
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1, gridPosition.floor));
        }
        if (gridPosition.z > 0)
        {
            neighborList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1, gridPosition.floor));
        }

        // Link Neighbors
        List<GridPosition> pathfindingLinkGridPositionList = GetPathfindingLinkConnectedGridPositionList(gridPosition);

        foreach (GridPosition pathfindingLinkGridPosition in pathfindingLinkGridPositionList)
        {
            neighborList.Add(GetNode(pathfindingLinkGridPosition));
        }

        return neighborList;
    }

    private List<GridPosition> GetPathfindingLinkConnectedGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        foreach (PathfindingLink pathfindingLink in pathfindingLinkList)
        {
            if (pathfindingLink.gridPositionA == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionB);
            }

            if (pathfindingLink.gridPositionB == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionA);
            }

        }

        return gridPositionList;
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return GetGridSystemForFloor(gridPosition.floor).GetGridObject(gridPosition).IsWalkable();
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
    {
        GetGridSystemForFloor(gridPosition.floor).GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition stopGridPosition)
    {
        return FindPath(startGridPosition, stopGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition stopGridPosition) {
        FindPath(startGridPosition, stopGridPosition, out int pathLength);
        return pathLength;
    }
}
