using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomData roomData = (RoomData) target;

        if (GUILayout.Button("Configure room data"))
        {
            roomData.corners.AddRange(roomData.GetComponentsInChildren<Corner>());
            roomData.SetPerimeter();
            roomData.PlaceRoomToCentroid();
        }
    }
}
