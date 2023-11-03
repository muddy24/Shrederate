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
        while (i * batchSize + j < positions.Count)
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
                else
                    break;
                j++;
            }
            positionBatches.Add(thisBatch);
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!renderMeshes)
        {
            return;
        }
        /*
        int i = 0;
        int j = 0;

        //loop through all positions
        while (i * batchSize + j < positions.Count)
        {
            j = 0;
            Matrix4x4[] instData = new Matrix4x4[batchSize];

            //send position and rotation data to a batch
            while (j < batchSize)
            {
                if (i * batchSize + j < positions.Count - 1)
                {
                    Matrix4x4 m = Matrix4x4.identity;
                    m.SetTRS(positions[i * batchSize + j], Quaternion.Euler(eulerAngles), Vector3.one);
                    instData[j] = m;
                }
                else
                    break;

                j++;
            }

            //render current batch
            Graphics.DrawMeshInstanced(mesh, 0, material, instData);//rp, mesh, 0, instData, -1, 0);
            i++;
        }*/
        foreach(Matrix4x4[] batch in positionBatches)
        {
            Graphics.DrawMeshInstanced(mesh, 0, material, batch);
        }

    }
}