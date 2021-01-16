using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModelVertexPos1 : MonoBehaviour
{
    private List<Vector3> posVector3s = new List<Vector3>();
    private List<Vector3> worldposVector3s = new List<Vector3>();

    public GameObject oriPos;

    // Start is called before the first frame update
    void Start()
    {
        
        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();

        foreach (var vertex in meshFilter.mesh.vertices)
        {
            Vector3 worldVector3 = GetMatrix4X4(meshFilter.transform, vertex);
            posVector3s.Add(worldVector3); 
            
        }
    }

    public Vector3 GetMatrix4X4(Transform t, Vector3 localVector3)
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
        // Matrix4x4 rotaMatrix4X4 = new Matrix4x4();
        // rotaMatrix4X4.SetRow(0, new Vector4(rightVector3.x, upVector3.x, forwardVector3.x, 0));
        // rotaMatrix4X4.SetRow(1, new Vector4(rightVector3.y, upVector3.y, forwardVector3.y, 0));
        // rotaMatrix4X4.SetRow(2, new Vector4(rightVector3.z, upVector3.z, forwardVector3.z, 0));
        // rotaMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        
        Matrix4x4 PosMatrix4X4 = new Matrix4x4();
        PosMatrix4X4.SetRow(0, new Vector4(1, 0, 0, t.localPosition.x));
        PosMatrix4X4.SetRow(1, new Vector4(0, 1, 0, t.localPosition.y));
        PosMatrix4X4.SetRow(2, new Vector4(0, 0, 1, t.localPosition.z));
        PosMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        //
        Matrix4x4 scaleMatrix4X4 = new Matrix4x4();
        scaleMatrix4X4.SetRow(0, new Vector4(t.localScale.x, 0, 0, 0));
        scaleMatrix4X4.SetRow(1, new Vector4(0, t.localScale.y, 0, 0));
        scaleMatrix4X4.SetRow(2, new Vector4(0, 0, t.localScale.z, 0));
        scaleMatrix4X4.SetRow(3, new Vector4(0, 0, 0, 1));
        //
    
        // Matrix4x4 matrix4 = scaleMatrix4X4 * rotaMatrix4X4 * PosMatrix4X4;
        Matrix4x4 matrix4 = scaleMatrix4X4 * PosMatrix4X4;

        Debug.Log(matrix4);

        localVector3 = matrix4.MultiplyPoint(localVector3);


        if (t.childCount == 0)
        {
            return localVector3;
        }
        else
        {
            localVector3 = GetMatrix4X4(t.GetChild(0), localVector3);
        }
        return localVector3;
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
