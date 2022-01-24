using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMesh : MonoBehaviour
{
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float vectorSpacing = 1f;
    public float noiseScale = 100;
    public float noiseAmplitude = 100;
    public int octaves = 3;
    public float persistence = .5f;
    public float lacunarity = 2;

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public Vector3 GetVert(int i)
    {
        return vertices[i];
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        GenerateTerrain();
        UpdateMesh();
    }

    //uses Noise to generate heights for each vertex
    public void GenerateTerrain()
    {
        float[,] heights = Noise.GenerateNoise(mapWidth+1, mapHeight+1, noiseScale, Random.Range(0,1000), octaves, persistence, lacunarity);
        
        int vertexIndex = 0;

        for(int y = 0; y <= mapHeight; y++)
        {
            for(int x = 0; x <= mapWidth; x++)
            {
                vertexIndex = (y * (mapWidth+1)) + x;
                MoveVert(vertexIndex, new Vector3(vertices[vertexIndex].x, heights[x, y] * noiseAmplitude, vertices[vertexIndex].z));
            }
        }
    }

    //fills vertices[] with position data to create a flat plane
    public void CreateShape()
    {
        vertices = new Vector3[(mapWidth + 1) * (mapHeight + 1)];

        for (int i = 0, z = 0; z <= mapHeight; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                vertices[i] = new Vector3(x * vectorSpacing, 0, z * vectorSpacing);
                i++;
            }
        }

        triangles = new int[mapWidth * mapHeight * 6];

        UpdateTris();

        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    //fills tris[] with appropriate vertex indices
    public void UpdateTris()
    {
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                triangles[0 + tris] = vert + 0;
                triangles[1 + tris] = vert + mapWidth + 1;
                triangles[2 + tris] = vert + 1;
                triangles[3 + tris] = vert + 1;
                triangles[4 + tris] = vert + mapWidth + 1;
                triangles[5 + tris] = vert + mapWidth + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    //return neighboring vertices. checks to make sure they are valid
    //these may not be valid indices ie: may be off the map or on other side of map
    public List<int> getAdjacentIndices(int i)
    {
        List<int> ret = new List<int>();

        ret.Add(i + 1);
        ret.Add(i - 1);
        ret.Add(i - mapWidth - 1);
        ret.Add(i - mapWidth);
        ret.Add(i - mapWidth + 1);
        ret.Add(i + mapWidth - 1);
        ret.Add(i + mapWidth);
        ret.Add(i + mapWidth + 1);

        return ret;
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
