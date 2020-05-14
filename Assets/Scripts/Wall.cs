using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Vector3 A;
    public Vector3 B;

    public float lenght;

    public Wall SetWall(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;

        lenght = Vector3.Distance(A, B);
        
        GenerateWallMesh();

        return this;
    }

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

        vertices[0] = A;
        vertices[1] = A + Vector3.up * 2.5f;
        vertices[2] = B;
        vertices[3] = B + Vector3.up * 2.5f;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;

        gameObject.AddComponent<MeshCollider>();
    }
}
