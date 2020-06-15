using System.Linq;
using Boomlagoon.JSON;
using UnityEngine;

namespace Services
{
    public class RoomSyncService : IRoomSyncService
    {
        private GameObject _targetRoom;
        
        private int c0Index;
        private int c1Index; 
        
        public void GenerateTestRoom()
        {
            var room = new GameObject("TargetRoom");
            room.AddComponent<RoomData>().SetRoomDataFromJson("test.json");
            _targetRoom = room.transform.parent.gameObject;
            _targetRoom.SetActive(false);
        }

        private Vector3 GetDockingPoint(RoomData arRoom)
        {
            var targetRoomData = _targetRoom.GetComponentInChildren<RoomData>();
            var targetSyncVector = targetRoomData.RoomCentroid - targetRoomData.corners[c0Index].Position;
            var targetWallVector = targetRoomData.corners[c1Index].Position - targetRoomData.corners[c0Index].Position;
            var targetSyncAngle = Utils.GetSignedAngle(targetWallVector, targetSyncVector);

            Debug.Log(arRoom.corners.Count);
            var arWallVector = arRoom.corners[1].Position - arRoom.corners[0].Position;
            var arSyncVector = Quaternion.Euler(0, targetSyncAngle, 0) * arWallVector;
            arSyncVector = arSyncVector.normalized * targetSyncVector.magnitude;
        
            return arRoom.corners[0].Position + arSyncVector;
        }

        public void SynchronizeRooms(RoomData arRoom)
        {
            _targetRoom.SetActive(true);
            c0Index = RoomPlanVisualizer.Instance.cornerIndexes[0];
            c1Index = RoomPlanVisualizer.Instance.cornerIndexes[1];
            
            var arRoomDockingPoint = GetDockingPoint(arRoom);
            _targetRoom.transform.position = arRoomDockingPoint;
            var targetRoomData = _targetRoom.GetComponentInChildren<RoomData>();
        
            var targetRoomDirection = (targetRoomData.corners[c0Index].Position - targetRoomData.RoomCentroid).normalized;
            var arRoomDirection = (arRoom.corners[0].Position - arRoomDockingPoint).normalized;
        
            float deltaAngle = Utils.GetSignedAngle(targetRoomDirection, arRoomDirection);
        
            _targetRoom.transform.rotation = Quaternion.Euler(0, _targetRoom.transform.eulerAngles.y + deltaAngle, 0);
        }
    }
}
