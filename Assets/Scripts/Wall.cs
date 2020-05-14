using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GameObject A;
    public GameObject B;

    public float lenght;

    public void SetWall(GameObject a, GameObject b)
    {
        A = a;
        B = b;

        lenght = Vector3.Distance(A.transform.position, B.transform.position);
    }
}
