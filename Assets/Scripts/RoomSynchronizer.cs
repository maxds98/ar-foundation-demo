using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomSynchronizer : MonoBehaviour
{
    public static RoomSynchronizer Instance;
    
    public GameObject targetRoom;

    private void Awake()
    {
        Instance = this;
    }

    public void SynchronizeRooms(RoomData arRoom)
    {
        targetRoom.SetActive(true);
        targetRoom.transform.position = arRoom.RoomCentroid;
        var targetRoomData = targetRoom.GetComponentInChildren<RoomData>();
        
        var targetRoomDirection = (targetRoomData.corners.First().Position - targetRoomData.RoomCentroid).normalized;
        var arRoomDirection = (arRoom.corners.First().Position - arRoom.RoomCentroid).normalized;
        
        float deltaAngle = Utils.GetSignedAngle(targetRoomDirection, arRoomDirection);
        
        targetRoom.transform.rotation = Quaternion.Euler(0, targetRoom.transform.eulerAngles.y + deltaAngle, 0);
    }
}
