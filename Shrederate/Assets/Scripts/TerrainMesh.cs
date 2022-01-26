using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMesh : MonoBehaviour
{
    GameObject player;

    //terrain shape controls
    public int mapWidth = 100;
    public float vectorSpacing = 1f;
    public float noiseScale = 100;
    public float noiseAmplitude = 100;
    public int octaves = 3;
    public float persistence = .5f;
    public float lacunarity = 2;
    public float ridgeSmoothing = 0.5f;

    float noiseSeed = 0;

    //could use these for a preview, but really they're obsoleted since chunks were implimented
    Mesh mesh;
    MeshCollider col;

    //stores high res mesh of entire map
    //Split into chunks for display and optimization
    Vector3[] vertices;
    int[] triangles;

    //chunk stuff
    public int numChunks = 2; //number PER SIDE. MUST BE a divisor of mapWidth
    GameObject[,] chunks;
    public GameObject chunkPrefab;

    public Vector3 GetVert(int i)
    {
        return vertices[i];
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        col = gameObject.GetComponent<MeshCollider>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        chunks = new GameObject[numChunks, numChunks];

        CreateShape();
        GenerateTerrain();
        CreateChunks();
        //UpdateMesh();

        col.sharedMesh = mesh;

       
    }

    void Update()
    {

        //Just set Chunk LODs based on player position
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
        int chunkSize = (int)(mapWidth * vectorSpacing) / numChunks;
        int newLOD;

        for(int x = 0; x < numChunks; x++)
        {
            for(int y = 0; y < numChunks; y++)
            {
                MeshRenderer chunkMR = chunks[x, y].GetComponentInChildren<MeshRenderer>();
                Vector2 chunkPos = new Vector2(chunkMR.bounds.center.x, chunkMR.bounds.center.z);

                float dist = Vector2.Distance(chunkPos, playerPos);

                if (dist < chunkSize * 1)
                    newLOD = 1;
                else if (dist < chunkSize * 2)
                    newLOD = 2;
                else if (dist < chunkSize * 3)
                    newLOD = 3;
                else if (dist < chunkSize * 4)
                    newLOD = 4;
                else
                    newLOD = 5;

                if(chunks[x,y].GetComponent<TerrainChunk>().LOD != newLOD)
                    setChunkMesh(x,y,newLOD);
            }
        }
    }

    //create chunks from vertices array
    public void CreateChunks()
    {
        for (int y = 0; y < numChunks; y++)
        {
            for(int x = 0; x < numChunks; x++)
            {
                setChunkMesh(x, y, 5);
            }
        }
    }

    //sets the chunk mesh for chunk [x.y] with given level of detail
    //NOTE: chunkSize must be divisible by spacing, and chunkSize can't be larger than 255
    //with chunkSize of 240, you can use LOD 1-5
    public void setChunkMesh(int x, int y, int lod)
    {

        int spacing = (int)Mathf.Pow(2, (lod - 1));
        int chunkSize = mapWidth / numChunks; //number of EDGES on a chunk side
        int indexToAdd;

        List<Vector3> chunkVerts = new List<Vector3>();

        //loop through points in each chunk
        for (int chunkY = 0; chunkY <= chunkSize; chunkY += spacing)
        {
            for (int chunkX = 0; chunkX <= chunkSize; chunkX += spacing)
            {
                indexToAdd = (y * (numChunks * (int)Mathf.Pow(chunkSize, 2) + chunkSize)) + (chunkY * (mapWidth + 1)) + (x * (chunkSize)) + chunkX;
                chunkVerts.Add(vertices[indexToAdd]);
            }
        }

        //clear old chunk
        if (chunks[x, y] != null)
            Destroy(chunks[x, y]);

        //add new chunk to chunks matrix
        chunks[x, y] = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        chunks[x, y].GetComponent<TerrainChunk>().CreateChunk(chunkVerts.ToArray(), lod);
        chunks[x, y].transform.parent = gameObject.transform;
    }

    //uses Noise to generate heights for each vertex. This is the master matrix with highest LOD
    //just the matrix of points, does not send data to a mesh
    public void GenerateTerrain()
    {
        float[,] heights = Noise.GenerateNoise(mapWidth+1, noiseScale, new Vector2(Random.Range(0,1000),Random.Range(0,1000)), octaves, persistence, lacunarity, ridgeSmoothing);
        
        int vertexIndex = 0;

        for(int y = 0; y <= mapWidth; y++)
        {
            for(int x = 0; x <= mapWidth; x++)
            {
                vertexIndex = (y * (mapWidth+1)) + x;
                MoveVert(vertexIndex, new Vector3(vertices[vertexIndex].x, heights[x, y] * noiseAmplitude, vertices[vertexIndex].z));
            }
        }

        
    }

    //fills vertices[] with position data to create a flat plane
    //only used for preview mesh. must be 255 or less mesh size
    public void CreateShape()
    {
        vertices = new Vector3[(mapWidth + 1) * (mapWidth + 1)];

        for (int i = 0, z = 0; z <= mapWidth; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                vertices[i] = new Vector3(x * vectorSpacing, 0, z * vectorSpacing);
                i++;
            }
        }

        triangles = new int[mapWidth * mapWidth * 6];

        UpdateTris();

        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    //fills tris[] with appropriate vertex indices
    public void UpdateTris()
    {
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < mapWidth; z++)
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
