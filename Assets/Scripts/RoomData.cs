using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [SerializeField] private Corner _cornerPref;
    [SerializeField] private Wall _wallPref;
    
    public List<Wall> walls = new List<Wall>();
    public List<Corner> corners = new List<Corner>();
    public List<GameObject> props = new List<GameObject>();

    public Vector3 RoomCentroid => Utils.GetCentroid(corners);

    public void SetRoomDataFromJson(string fileName)
    {
        var jsonObject = JsonLoader.LoadJsonFromFile(fileName);

        _cornerPref = new GameObject("corner").AddComponent<Corner>();
        _wallPref = Resources.Load<Wall>("Wall");
        
        if (jsonObject.ContainsKey("items"))
        {
            foreach (var item in jsonObject["items"].Array)
            {
                var posObj = item.Obj.GetObject("position");
                var position = JsonLoader.GetVector3(posObj);
                if (item.Obj["type"].Str == "corner")
                {
                    SetCorner(position);
                }

                if (item.Obj["type"].Str == "prop")
                {
                    var propPrefab = Resources.Load<GameObject>($"props/{item.Obj.GetString("name")}");
                    var prop = Instantiate(propPrefab, transform, true);
                    prop.transform.localPosition = position;

                    var rotationObj = item.Obj.GetObject("rotation");
                    prop.transform.localRotation = Quaternion.Euler(JsonLoader.GetVector3(rotationObj));
                    
                    props.Add(prop);
                }
            }
        }
        PlaceRoomToCentroid();
        RoomPlanVisualizer.Instance.Build2DPlan(this);
    }

    public void SetCorner(Vector3 pos)
    {
        var cornerPoint = Instantiate(_cornerPref, transform);
        cornerPoint.transform.position = pos;
        corners.Add(cornerPoint);
    }

    public void SetPerimeter()
    {
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

    public void PlaceRoomToCentroid()
    {
        var rootObject = new GameObject($"{gameObject.name}Root");
        rootObject.transform.position = RoomCentroid;
        transform.parent = rootObject.transform;
    }
}
