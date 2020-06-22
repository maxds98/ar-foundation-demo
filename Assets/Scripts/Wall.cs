using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Vector3 a;
    public Vector3 b;

    public float length;

    /// <summary>
    /// Generates the wall from 2 points and calculating it length.
    /// </summary>
    /// <param name="aPoint">The wall start point.</param>
    /// <param name="bPoint">The wall end point</param>
    /// <returns></returns>
    public Wall SetWall(Vector3 aPoint, Vector3 bPoint)
    {
        this.a = aPoint;
        this.b = bPoint;

        length = Vector3.Distance(this.a, this.b);
        
        GenerateWallMesh();

        return this;
    }

    /// <summary>
    /// Generates double sided wall mesh.
    /// </summary>
    private void GenerateWallMesh()
    {
        var wallMeshFilter = GetComponent<MeshFilter>();

        var triangles = new [] 
        {
            0, 3, 1,
            3, 0, 2,
            1, 3, 0,
            2, 0, 3
        };

        var mesh = wallMeshFilter.mesh;
        var normals = mesh.normals;

        mesh = new Mesh();
        wallMeshFilter.mesh = mesh;

        var vertices = new Vector3[4];

        vertices[0] = a;
        vertices[1] = a + Vector3.up * 2.5f;
        vertices[2] = b;
        vertices[3] = b + Vector3.up * 2.5f;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;

        gameObject.AddComponent<MeshCollider>();
    }
}
