using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothesSimulate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DrawMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DrawMesh()
    {
        GameObject meshfilterGo = new GameObject("Mesh");
        MeshFilter meshFilter = meshfilterGo.AddComponent<MeshFilter>();
        meshfilterGo.AddComponent<MeshRenderer>();
        
        List<Vector3> vector3s = new List<Vector3>();

        Mesh mesh = new Mesh();

        vector3s.Add(new Vector3(0, 0, 0));
        vector3s.Add(new Vector3(1, 0, 0));
        vector3s.Add(new Vector3(0, 1, 0));
        vector3s.Add(new Vector3(1, 1, 0));

        mesh.vertices = vector3s.ToArray();
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };

        meshFilter.mesh = mesh;

    }
}
