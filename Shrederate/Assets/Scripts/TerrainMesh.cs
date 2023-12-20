using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class TerrainMesh : MonoBehaviour
{
    GameObject player;
    public GameObject dirtSemiSphere;

    //terrain shape controls
    public int mapWidth = 100;
    public float vectorSpacing = 1f;
    public float noiseScale = 100;
    public float noiseAmplitude = 100;
    public int octaves = 3;
    public float persistence = .5f;
    public float lacunarity = 2;
    public float ridgeSmoothing = 0.5f;
    public AnimationCurve edgeSmoothCurve;
    public float reshapeEdgeRadiusPercent = 1f; //radius of map edge as a percent of original mesh size
    public float bowlHeightPercent = 0.5f;
    public Vector2 noiseSeed = new Vector2();
    public AnimationCurve slopeEdgeSmoothing;
    public AnimationCurve slopeStartSmoothing;

    public Vector3 peak = Vector3.zero;
    public Vector3 terrainMin = new Vector3(0, 99999, 0);
    

    //float noiseSeed = 0;

    //could use these for a preview, but really they're obsoleted since chunks were implimented
    Mesh mesh;
    MeshCollider col;

    //stores high res mesh of entire map
    //Split into chunks for display and optimization
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    public Gradient gradient;

    //chunk stuff
    public int numChunks = 2; //number PER SIDE. MUST BE a divisor of mapWidth
    GameObject[,] chunks;
    bool[,] isChunkEmpty;
    public GameObject chunkPrefab;

    public Vector3 GetVert(int i)
    {
        return vertices[i];
    }

    void Start()
    {
        //put the dirt ball in its place
        float halfMapSize = mapWidth * vectorSpacing / 2;
        dirtSemiSphere.transform.position = new Vector3(halfMapSize, 0, halfMapSize);
        dirtSemiSphere.transform.localScale = new Vector3(1,1,bowlHeightPercent) * halfMapSize * reshapeEdgeRadiusPercent;

        player = GameObject.FindWithTag("Player");

        col = gameObject.GetComponent<MeshCollider>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        chunks = new GameObject[numChunks, numChunks];
        isChunkEmpty = new bool[numChunks, numChunks];
        for(int x = 0; x < numChunks; x++)
        {
            for(int y = 0; y < numChunks; y++)
            {
                isChunkEmpty[x, y] = false;
            }
        }

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
                if (!isChunkEmpty[x, y]) //do nothing for empty chunks
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
                        newLOD = 3;
                    else
                        newLOD = 3;

                    if (chunks[x, y].GetComponent<TerrainChunk>().LOD != newLOD)
                        setChunkMesh(x, y, newLOD);
                }
            }
        }

        //put player back on top of mountain
        if(player.transform.position.y < -0.25f * mapWidth * vectorSpacing)
        {
            SpawnPlayerAtPeak();
        }
    }

    public void SpawnPlayerAtPeak()
    {
        /*float halfMapSize = mapWidth * vectorSpacing / 2;
        Rigidbody playerRB = player.GetComponent<PlayerMovement>().rb;
        playerRB.transform.position = new Vector3(halfMapSize, noiseAmplitude * 1f, halfMapSize);
        playerRB.velocity = Vector3.zero;*/

        Rigidbody playerRB = player.GetComponent<PlayerMovement>().rb;
        playerRB.transform.position = peak + Vector3.up * 10;
        playerRB.velocity = Vector3.zero;
        GameEvents.current.LapStart();
    }

    //takes a list of all the slopes and levels the green and blue ones
    public void LevelSlopes(List<GameObject> slopes)
    {
        SplineSample sample = new SplineSample();
        List<float> slopeHeights;
        List<float> slopeWeights;

        //get list of all indices of points that are on a slope
        List<int> allSlopePointIndices = new List<int>();
        foreach(GameObject s in slopes)
        {
            foreach(int index in s.GetComponent<Slope>().terrainPointIndices)
            {
                if(!allSlopePointIndices.Contains(index))
                    allSlopePointIndices.Add(index);
            }
        }

        //set height for each slope point
        foreach(int index in allSlopePointIndices)
        {
            slopeHeights = new List<float>();
            slopeWeights = new List<float>();

            foreach (GameObject s in slopes)
            {
                if (s.GetComponent<Slope>().terrainPointIndices.Contains(index))
                {
                    s.GetComponent<Slope>().spline.Project(vertices[index], ref sample);
                    float slopeWidthPercent = Vector3.Distance(vertices[index], sample.position) / 50;

                    if (slopeWidthPercent < 1)
                    {
                        //at start and end of run, smooth linearly
                        float endSmoothing = 1f;
                        if (sample.percent == 1.0 || sample.percent == 0.0)
                            slopeHeights.Add(Mathf.Lerp(sample.position.y, vertices[index].y, slopeWidthPercent));// endSmoothing = slopeWidthPercent;// 1-(slopeWidthPercent)*Mathf.Cos(Vector3.Angle(sample.forward * -1, vertices[i]-sample.position) * 3.14f/180);
                        else
                            slopeHeights.Add(Mathf.Lerp(vertices[index].y, sample.position.y, slopeEdgeSmoothing.Evaluate(slopeWidthPercent)));// * endSmoothing));
                        slopeWeights.Add((50 - Vector3.Distance(vertices[index], sample.position)) / 50);
                    }
                }
            }

            //compile heights from all affecting slopes
            if(slopeWeights.Count > 0)
            {
                float average = 0f;
                float totalWeight = 0f;
                for (int j = 0; j < slopeWeights.Count; j++)
                    totalWeight += slopeWeights[j];
                for (int j = 0; j < slopeHeights.Count; j++)
                {
                    average += slopeHeights[j] * (slopeWeights[j] / totalWeight);
                }                
                vertices[index] = new Vector3(vertices[index].x, average, vertices[index].z);
            }
        }
        UpdateMesh();
        CreateChunks();

    }

    //create chunks from vertices array
    public void CreateChunks()
    {
        for (int y = 0; y < numChunks; y++)
        {
            for(int x = 0; x < numChunks; x++)
            {
                setChunkMesh(x, y, 1);
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
        List<Color> chunkColors = new List<Color>();

        //loop through points in each chunk
        for (int chunkY = 0; chunkY <= chunkSize; chunkY += spacing)
        {
            for (int chunkX = 0; chunkX <= chunkSize; chunkX += spacing)
            {
                indexToAdd = (y * (numChunks * (int)Mathf.Pow(chunkSize, 2) + chunkSize)) + (chunkY * (mapWidth + 1)) + (x * (chunkSize)) + chunkX;
                chunkVerts.Add(vertices[indexToAdd]);
                //chunkColors.Add(colors[indexToAdd]);
            }
        }

        //check if mesh is just all points combined under mountain
        bool vertsAllSame = true;
        foreach (Vector3 vert in chunkVerts)
        {
            if (vert != chunkVerts[0])
            {
                vertsAllSame = false;
                break;
            }
        }
        isChunkEmpty[x, y] = vertsAllSame;
        if (vertsAllSame)
            return;

        //clear old chunk
        List<Vector3> treePosTemp = new List<Vector3>();
        if(chunks[x, y] != null)
            treePosTemp = chunks[x, y].GetComponent<TerrainChunk>().treePositions;
        Destroy(chunks[x, y]);


        //add new chunk to chunks matrix
        chunks[x, y] = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        chunks[x, y].GetComponent<TerrainChunk>().CreateChunk(chunkVerts.ToArray(), lod, new Vector2(x * chunkSize, y * chunkSize) * vectorSpacing, new Vector2((x + 1) * chunkSize, ((y + 1) * chunkSize)) * vectorSpacing);//, chunkColors.ToArray());
        chunks[x, y].transform.parent = gameObject.transform;
        chunks[x, y].GetComponent<TerrainChunk>().treePositions = treePosTemp;
    }

    //uses Noise to generate heights for each vertex. This is the master matrix with highest LOD
    //just the matrix of points, does not send data to a mesh
    public void GenerateTerrain()
    {
        noiseSeed = new Vector2(Random.Range(0, 1000), Random.Range(0, 1000));
        float[,] heights = Noise.GenerateNoise(mapWidth+1, noiseScale, noiseSeed, octaves, persistence, lacunarity, ridgeSmoothing, edgeSmoothCurve);
        int vertexIndex = 0;

        for(int y = 0; y <= mapWidth; y++)
        {
            for(int x = 0; x <= mapWidth; x++)
            {
                vertexIndex = (y * (mapWidth+1)) + x;
                MoveVert(vertexIndex, new Vector3(vertices[vertexIndex].x, heights[x, y] * noiseAmplitude, vertices[vertexIndex].z));

                //keep track of highest/lowest point
                if(heights[x,y]*noiseAmplitude > peak.y)
                {
                    peak = vertices[vertexIndex];
                }
                if(heights[x,y]*noiseAmplitude < terrainMin.y)
                {
                    terrainMin = vertices[vertexIndex];
                }
            }
        }

        /*colors = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= mapWidth; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                //float height = vertices[i].y;
                colors[i] = gradient.Evaluate(EstimateNormal(i).y);
                i++;
            }
        }*/

        ShapeBottomBowl();
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

    //moves points lower than reshapeEdgeStartHeight towards 
    void ShapeBottomBowl()
    {
        
        int vertexIndex = 0;
        Vector3 thisVertex;
        float halfMapSize = mapWidth * vectorSpacing / 2;

        Vector2 center = new Vector2(halfMapSize, halfMapSize);
        Vector3 centerPoint = new Vector3(halfMapSize, -halfMapSize/5, halfMapSize);

        for (int y = 0; y <= mapWidth; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                vertexIndex = (y * (mapWidth + 1)) + x;
                thisVertex = vertices[vertexIndex];

                if (Vector2.Distance(center, new Vector2(thisVertex.x,thisVertex.z)) > reshapeEdgeRadiusPercent * halfMapSize)
                {
                    vertices[vertexIndex] = Vector3.MoveTowards(thisVertex, centerPoint, Vector3.Distance(thisVertex, centerPoint) * .8f);

                    //this method makes the bowl out of the mesh itself but is very buggy
                    /*float chordMultiplier;
                    if (Mathf.Abs(thisVertex.x - halfMapSize) > Mathf.Abs(thisVertex.z - halfMapSize))
                    {
                        chordMultiplier = Mathf.Abs(halfMapSize / (halfMapSize - thisVertex.x));
                    }
                    else
                        chordMultiplier = Mathf.Abs(halfMapSize / (halfMapSize - thisVertex.z));

                    //distance from center point to edge of square through thisVertex
                    float chordLength = DistanceToCenterAxis(thisVertex) * chordMultiplier;

                    float outerChordPercentage = (DistanceToCenterAxis(thisVertex) - halfMapSize) / (chordLength - halfMapSize);
                    float reshapeEdgeRadius = reshapeEdgeRadiusPercent * halfMapSize;
                    float distFromCenter = (1 - outerChordPercentage) * reshapeEdgeRadius;
                    float newHeight = bowlHeightPercent * Mathf.Pow(Mathf.Pow(reshapeEdgeRadius, 2) - Mathf.Pow(distFromCenter, 2), .5f);
                    
                    Vector3 newPos = Vector3.MoveTowards(new Vector3(halfMapSize, 0, halfMapSize), thisVertex, distFromCenter);
                    newPos = new Vector3(newPos.x, newPos.y - newHeight, newPos.z);

                    vertices[vertexIndex] = newPos;*/

                }
            }
        }
    }

    public float DistanceToCenterAxis(Vector3 point)
    {
        float centerDist = mapWidth * vectorSpacing / 2;
        return Vector2.Distance(new Vector2(point.x, point.z), new Vector2(centerDist, centerDist));
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

    //Estimates normal based on adjacent points
    //Not working correctly
    public Vector3 EstimateNormal(int i)
    {
        Vector3 normal = Vector3.zero;
        foreach (int index in getAdjacentIndices(i))
        {
            if(index < vertices.Length && index >= 0)
                normal += vertices[index]-vertices[i];
        }
        return normal.normalized;
    }

    //Move vertex to newPos
    void MoveVert(int i, Vector3 newPos)
    {
        vertices[i] = newPos;
    }

    public float GetHeightAtXY(Vector2 pos)
    {
        int terrainSideLength = (int)Mathf.Pow(vertices.Length, 0.5f);
        return vertices[(int)(pos.x + pos.y * terrainSideLength)].y;
    }

    //Update triangle and vertex data in mesh
    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    public GameObject[,] GetChunks()
    {
        return chunks;
    }
}
