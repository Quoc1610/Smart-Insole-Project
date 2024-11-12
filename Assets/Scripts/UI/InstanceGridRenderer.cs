using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceGridRenderer : MonoBehaviour
{
    public GameObject goCubeTile;
    public Material instanceMaterial;

    void Update()
    {
        // Define the number of instances in the grid
        int gridWidth = 2;
        int gridHeight = 2;

        Matrix4x4[] matrices = new Matrix4x4[gridWidth * gridHeight];
        Vector4[] colors = new Vector4[gridWidth * gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int index = x * gridHeight + y;

                // Calculate position for each instance in the grid
                Vector3 localPosition = new Vector3(x*2, 0, y*2);
                Vector3 position = gameObject.transform.position + gameObject.transform.TransformVector(localPosition);
                Debug.Log(position);

                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;

                // Populate the transformation matrix and color for each instance
                matrices[index] = Matrix4x4.TRS(position, rotation, scale);
                colors[index] = new Vector4(Random.value, Random.value, Random.value, 1f); // Random color

            }
        }

        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        properties.SetVectorArray("_Color", colors);

        RenderParams rp = new RenderParams(instanceMaterial);

        // Render instances using GPU instancing
        Graphics.RenderMeshInstanced(rp, goCubeTile.GetComponent<MeshFilter>().sharedMesh, 0, matrices, gridWidth * gridHeight);
    }
}