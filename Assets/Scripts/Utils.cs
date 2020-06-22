using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Calculates an angle in degrees between 2 vectors.
    /// </summary>
    /// <param name="a">Vector3 a.</param>
    /// <param name="b">Vector3 b.</param>
    /// <returns>Signed angle between 2 vectors.</returns>
    public static float GetSignedAngle(Vector3 a, Vector3 b){
        var angle = Vector3.Angle(a, b); 
        return angle * Mathf.Sign(Vector3.Cross(a, b).y);
    }
    
    /// <summary>
    /// Calculates the center of polygon by it's points.
    /// </summary>
    /// <param name="points">Polygon points list.</param>
    /// <typeparam name="T">Should be MonoBehaviour.</typeparam>
    /// <returns>Polygon centroid.</returns>
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
