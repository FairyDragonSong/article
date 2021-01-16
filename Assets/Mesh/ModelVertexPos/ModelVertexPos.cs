using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModelVertexPos : MonoBehaviour
{
    private List<Vector3> posVector3s = new List<Vector3>();
    private List<Vector3> worldposVector3s = new List<Vector3>();

    public GameObject oriPos;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter[] meshFilter = this.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter filter in meshFilter)
        {

            foreach (var vertex in filter.mesh.vertices)
            {
                Vector3 worldVector3 = filter.transform.TransformPoint(vertex);

                worldVector3 = GetMatrix4X4(filter.transform, Matrix4x4.identity, worldVector3);

                // Vector3 localParVector3;
                // if (filter.transform.parent)
                // {
                //     localParVector3 = filter.transform.parent.InverseTransformPoint(worldVector3);
                // }
                // else
                // {
                //     localParVector3 = worldVector3;
                // }
                //
                // worldposVector3s.Add(localParVector3);
                // Vector3 localVector3 = matrix4.MultiplyPoint(worldVector3);

                // if (filter.transform.parent)
                // {
                //     posVector3s.Add(filter.transform.parent.InverseTransformPoint(localVector3));
                // }
                // else
                // {
                //     posVector3s.Add(localVector3);
                // }
                // localVector3 = oriPos.transform.TransformPoint(localVector3);
                posVector3s.Add(worldVector3);

            }
        }

    }

    public Vector3 GetMatrix4X4(Transform t, Matrix4x4 lastMatrix4X4, Vector3 point)
    {
        Vector3 rightVector3;
        Vector3 upVector3;
        Vector3 forwardVector3;
        
        if (t.parent == null)
        {
            rightVector3 = t.right;
            upVector3 = t.up;
            forwardVector3 = t.forward;
        }
        else
        {
            rightVector3 = t.parent.InverseTransformVector(t.right);
            upVector3 = t.parent.InverseTransformVector(t.up);
            forwardVector3 = t.parent.InverseTransformVector(t.forward);
        }
        Matrix4x4 rotaMatrix4X4 = new Matrix4x4();
        rotaMatrix4X4.SetRow(0, new Vector4(rightVector3.x, rightVector3.y, rightVector3.z, 0));
        rotaMatrix4X4.SetRow(1, new Vector4(upVector3.x, upVector3.y, upVector3.z, 0));
        rotaMatrix4X4.SetRow(2, new Vector4(forwardVector3.x, forwardVector3.y, forwardVector3.z, 0));
        rotaMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        
        Matrix4x4 PosMatrix4X4 = new Matrix4x4();
        PosMatrix4X4.SetRow(0, new Vector4(1, 0, 0, -t.localPosition.x));
        PosMatrix4X4.SetRow(1, new Vector4(0, 1, 0, -t.localPosition.y));
        PosMatrix4X4.SetRow(2, new Vector4(0, 0, 1, -t.localPosition.z));
        PosMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        //
        // Matrix4x4 scaleMatrix4X4 = new Matrix4x4();
        // scaleMatrix4X4.SetRow(0, new Vector4(1f / t.localScale.x, 0, 0, 0));
        // scaleMatrix4X4.SetRow(1, new Vector4(0, 1f / t.localScale.y, 0, 0));
        // scaleMatrix4X4.SetRow(2, new Vector4(0, 0, 1f / t.localScale.z, 0));
        // scaleMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        //
    
        // Matrix4x4 matrix4 = scaleMatrix4X4 * rotaMatrix4X4 * PosMatrix4X4;
        // Matrix4x4 matrix4 = rotaMatrix4X4 * scaleMatrix4X4 * PosMatrix4X4;
        Matrix4x4 matrix4 = rotaMatrix4X4 * PosMatrix4X4;

        Debug.Log(matrix4);

        matrix4 = matrix4 * lastMatrix4X4;


        if (t.parent == null)
        {
            
            return point;
        }
        else
        {
            point = matrix4.MultiplyPoint(t.parent.InverseTransformPoint(point));
            point = GetMatrix4X4(t.parent, matrix4, t.parent.TransformPoint(point));
        }
        return point;
    }


    private void OnDrawGizmos()
    {
        if (posVector3s.Count < 1)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < posVector3s.Count; i++)
        {
            Vector3 worldPos = posVector3s[i];
            Gizmos.DrawSphere(worldPos, 0.03f);
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < worldposVector3s.Count; i++)
        {
            Vector3 worldPos = worldposVector3s[i];
            Gizmos.DrawSphere(worldPos, 0.03f);
        }
    }

    // Update is called once per frame
}
