using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingLinkMonobehavior : MonoBehaviour
{
    public Vector3 worldPositionA;
    public Vector3 worldPositionB;

    public PathfindingLink GetPathfindingLink()
    {
        return new PathfindingLink {
            gridPositionA = LevelGrid.Instance.GetGridPosition(worldPositionA),
            gridPositionB = LevelGrid.Instance.GetGridPosition(worldPositionB)
        };
    }
}
