using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [SerializeField] private Corner _cornerPref;
    [SerializeField] private Wall _wallPref;
    
    public List<Wall> walls;
    public List<Corner> corners;

    public Vector3 roomCentroid;
    
    // Start is called before the first frame update
    void Awake()
    {
        corners = new List<Corner>();
        walls = new List<Wall>();
    }

    public void SetCorner(Vector3 pos)
    {
        var cornerPoint = Instantiate(_cornerPref, transform);
        cornerPoint.transform.position = pos;
        corners.Add(cornerPoint);
    }

    public void SetPerimeter()
    {
        roomCentroid = Utils.GetCentroid(corners);
        
        for (int i = 0; i < corners.Count; i++)
        {
            var previousCornerPos = corners[i > 0 ? i - 1 : corners.Count - 1].transform.position;
            var currentCornerPos = corners[i].transform.position;
            var nextCornerPos = corners[i < corners.Count - 1 ? i + 1 : 0].transform.position;

            var corner = corners[i];
            corner.angle = Utils.GetSignedAngle(previousCornerPos - currentCornerPos, nextCornerPos - currentCornerPos);
            
            var wall = Instantiate(_wallPref, transform).SetWall(currentCornerPos, nextCornerPos);

            walls.Add(wall);
        }
    }
}
