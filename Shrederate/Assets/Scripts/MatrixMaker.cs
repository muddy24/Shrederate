using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixMaker : MonoBehaviour
{
    public Vector3 Position, Rotation, Scale;

    // Start is called before the first frame update
    void Start()
    {
        Matrix4x4 Matrix = Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), Scale);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
