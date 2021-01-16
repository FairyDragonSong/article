using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _3View2ClipSpace : MonoBehaviour
{
    public Camera camera;

    public GameObject go;

    public GameObject projectMeshGo;

    private MeshRenderer projectMeshRenderer;

    private MeshFilter projectMeshFilter;

    private List<Vector3> posVector3s = new List<Vector3>();

    private float fieldOfView;

    private float aspect;

    private float far;

    private float near;

    private Matrix4x4 projectionMatrix4X4;

    Vector3 lastVector3 = new Vector3(2000, 2000, 2000);

    // Start is called before the first frame update
    void Start()
    {
        projectMeshRenderer = projectMeshGo.GetComponent<MeshRenderer>();
        projectMeshFilter = projectMeshGo.GetComponent<MeshFilter>();
        projectMeshRenderer.material.color = new Color(1, 1, 1, 0);

        projectMeshFilter.mesh = null;

        fieldOfView = camera.fieldOfView;
        aspect = camera.aspect;
        far = camera.farClipPlane;
        near = camera.nearClipPlane;

        projectionMatrix4X4 = new Matrix4x4();
        projectionMatrix4X4.SetRow(0, new Vector4(1 / (Mathf.Tan(fieldOfView * Mathf.PI / (2 * 180)) * aspect), 0, 0, 0));
        projectionMatrix4X4.SetRow(1, new Vector4(0, 1 / (Mathf.Tan(fieldOfView * Mathf.PI / (2 * 180))), 0, 0));
        projectionMatrix4X4.SetRow(2, new Vector4(0, 0, far / (near - far), near * far / (near - far)));
        projectionMatrix4X4.SetRow(3, new Vector4(0, 0, -1, 0));

        go.SetActive(false);
    }

    void Update()
    {
        MeshFilter goMeshFilter = go.GetComponent<MeshFilter>();

        Vector3[] verticesVector3 = goMeshFilter.mesh.vertices;
        posVector3s.Clear();
        for (int i = 0; i < verticesVector3.Length; i++)
        {
            Vector3 worldPos = go.transform.TransformPoint(verticesVector3[i]);
            Vector3 cameraPos = TransformPosInCameraSpace(worldPos);
            Vector4 HomogeneousPos = new Vector4(cameraPos.x, cameraPos.y, cameraPos.z, 1);

            Matrix4x4 m = new Matrix4x4();
            m.SetRow(0, new Vector4(1, 0, 0, HomogeneousPos.x));
            m.SetRow(1, new Vector4(0, 1, 0, HomogeneousPos.y));
            m.SetRow(2, new Vector4(0, 0, 1, HomogeneousPos.z));
            m.SetRow(3, new Vector4(0, 0, -1, HomogeneousPos.w));

            Matrix4x4 homogeneousProMatrix4X4 = projectionMatrix4X4 * m;

            Vector4 HomogeneousProVector4 = new Vector4(homogeneousProMatrix4X4.m03, homogeneousProMatrix4X4.m13, homogeneousProMatrix4X4.m23, homogeneousProMatrix4X4.m33);
            Vector3 proVector3 = new Vector3(HomogeneousProVector4.x, HomogeneousProVector4.y, HomogeneousProVector4.z) / HomogeneousProVector4.w;
            posVector3s.Add(proVector3);
        }

        if (projectMeshFilter.mesh != null)
        {
            projectMeshFilter.mesh.vertices = posVector3s.ToArray();
            projectMeshFilter.mesh.triangles = goMeshFilter.mesh.triangles;
        }
        else
        {
            Mesh mesh = new Mesh();
            mesh.vertices = posVector3s.ToArray();
            mesh.triangles = goMeshFilter.mesh.triangles;

            projectMeshFilter.mesh = mesh;
        }

    }

    Vector3 TransformPosInCameraSpace(Vector3 worldPos)
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

        Vector3 viewPos = M.MultiplyPoint(worldPos);

        return viewPos;
    }
}
