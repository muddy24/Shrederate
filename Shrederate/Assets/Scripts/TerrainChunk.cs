using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    Mesh mesh;
    MeshCollider col;

    //just here so the terrainMesh can easily check
    public int LOD = 1;
    //the number of EDGES on one side of the mesh
    int chunkWidth;

    Vector3[] vertices;
    int[] triangles;

    //fills vertices[] with position data to create a flat plane
    public void CreateChunk(Vector3[] verts, int lod)
    {
        LOD = lod;
        vertices = verts;

        col = gameObject.GetComponent<MeshCollider>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //calculate triangle length based on verts length
        chunkWidth = (int)Mathf.Pow(verts.Length,0.5f) - 1;
        triangles = new int[(int)Mathf.Pow(chunkWidth, 2) * 6]; 

        UpdateTris();

        UpdateMesh();

        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    //fills tris[] with appropriate vertex indices
    public void UpdateTris()
    {
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < chunkWidth; z++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                triangles[0 + tris] = vert + 0;
                triangles[1 + tris] = vert + chunkWidth + 1;
                triangles[2 + tris] = vert + 1;
                triangles[3 + tris] = vert + 1;
                triangles[4 + tris] = vert + chunkWidth + 1;
                triangles[5 + tris] = vert + chunkWidth + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    //Move vertex to newPos
    void MoveVert(int i, Vector3 newPos)
    {
        vertices[i] = newPos;
    }

    //Update triangle and vertex data in mesh
    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

}
