using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathfindingLinkMonobehavior))]
public class PathfindingLinkMonobehaviorEditor : Editor
{
    private void OnSceneGUI() 
    {
        PathfindingLinkMonobehavior pathfindingLinkMonobehavior = (PathfindingLinkMonobehavior)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newLinkPositionA = Handles.PositionHandle(pathfindingLinkMonobehavior.worldPositionA, Quaternion.identity);
        Vector3 newLinkPositionB = Handles.PositionHandle(pathfindingLinkMonobehavior.worldPositionB, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pathfindingLinkMonobehavior, "Change Link Position");
            pathfindingLinkMonobehavior.worldPositionA = newLinkPositionA;
            pathfindingLinkMonobehavior.worldPositionB = newLinkPositionB;
        }
    }
}
