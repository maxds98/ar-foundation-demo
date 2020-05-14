﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float GetSignedAngle(Vector3 a, Vector3 b){
        var angle = Vector3.Angle(a, b); 
        return angle * Mathf.Sign(Vector3.Cross(a, b).y);
    }
    
    public static Vector3 GetCentroid<T>(List<T> points) where T : MonoBehaviour
    {
        Vector3 pos = Vector3.zero;
        foreach(T p in points)
        {
            pos += p.transform.position;
        }
        return pos / points.Count;
    }
}
