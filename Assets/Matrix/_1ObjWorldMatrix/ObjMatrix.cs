using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjMatrix : MonoBehaviour
{

    public GameObject cubeWhite;

    public GameObject cubeRed;
    
    // Start is called before the first frame update
    void Start()
    {

        Vector3 u = cubeWhite.transform.right;
        Vector3 v = cubeWhite.transform.up;
        Vector3 n = cubeWhite.transform.forward;

        Matrix4x4 Mr = new Matrix4x4(
            new Vector4(u.x, u.y, u.z, 0),
            new Vector4(v.x, v.y, v.z, 0),
            new Vector4(n.x, n.y, n.z, 0),
            new Vector4(0  , 0,   0,   1)
            );

        cubeRed.transform.rotation = Mr.rotation;

    }
}
