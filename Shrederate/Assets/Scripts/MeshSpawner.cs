using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSpawner : MonoBehaviour
{
    List<Vector3> positions;
    public Material material;
    Vector3 eulerAngles;
    public int batchSize = 1000;
    public Mesh mesh;
    private List<Matrix4x4[]> positionBatches;

    bool renderMeshes = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    //Sets positions and default rotation of models to be rendered, turns on rendering
    public void SetPositions(List<Vector3> posList, Vector3 modelRotation)
    {
        positions=posList;
        eulerAngles = modelRotation;
        renderMeshes = true;

        positionBatches = new List<Matrix4x4[]>();

        int i = 0;
        int j = 0;
        Matrix4x4[] thisBatch;
        //loop through all positions
        while (i * batchSize < positions.Count)
        {
            j = 0;
            thisBatch = new Matrix4x4[batchSize];
            //send position and rotation data to a batch
            while (j < batchSize)
            {
                if (i * batchSize + j < positions.Count - 1)
                {
                    Matrix4x4 m = Matrix4x4.identity;
                    m.SetTRS(positions[i * batchSize + j], Quaternion.Euler(eulerAngles), Vector3.one);
                    thisBatch[j] = m;
                }

                j++;
            }
            positionBatches.Add(thisBatch);
            i++;
        }
        Debug.Log(positionBatches.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (!renderMeshes)
        {
            return;
        }

        int renderCount = 0;
        foreach(Matrix4x4[] batch in positionBatches)
        {
            foreach(Matrix4x4 m in batch)
            {
                if(m != null)
                    renderCount++;
            }
            Graphics.DrawMeshInstanced(mesh, 0, material, batch);
        }
    }
}