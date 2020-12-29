using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World2ViewSpace : MonoBehaviour
{
    public GameObject camera;

    public GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 eyePos = camera.transform.position;

        // 平移矩阵
        Matrix4x4 tM = new Matrix4x4(
            new Vector4(1, 0, 0, -1 * eyePos.x), 
            new Vector4(0, 1, 0, -1 * eyePos.y), 
            new Vector4(0, 0, 1, -1 * eyePos.z), 
            new Vector4(0, 0, 0, 1)
            );

        // 摄像机的xyz轴
        Vector3 n = camera.transform.forward * -1;
        Vector3 u = Vector3.Cross(Vector3.up, n) * -1;
        Vector3 v = Vector3.Cross(n, u) * -1;

        // 旋转矩阵
        Matrix4x4 rM = new Matrix4x4(
            new Vector4(u.x, u.y, u.z, 0),
            new Vector4(v.x, v.y, v.z, 0),
            new Vector4(n.x, n.y, n.z, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 M = rM.transpose * tM.transpose;

        // 变换后的P的坐标
        Vector3 viewPos = M.MultiplyPoint(cube.transform.position);

        Debug.Log(viewPos);
    }
}
