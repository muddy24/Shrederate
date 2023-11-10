using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour
{
    TerrainMesh terrain;
    float mapSize;

    public GameObject greenMarker;
    public GameObject blueMarker;
    public GameObject blackMarker;
    public GameObject redMarker;
    public GameObject cannon;
    public GameObject camTarget;
    public GameObject camPosition;
    public List<GameObject> minima = new List<GameObject>();

    //slop stuff
    List<List<Vector3>> slopePoints = new List<List<Vector3>>();
    List<string> slopeGrades = new List<string>();
    Vector2 greenSlopeRange = new Vector2(.9f, 1.0f);
    Vector2 blueSlopeRange = new Vector2(.7f, .9f);
    Vector2 blackSlopeRange = new Vector2(0, .7f);

    //tree stuff
    public GameObject tree;
    public int treeSpacing = 100;
    GameObject[] trees;
    public int maxActiveTrees = 200;
    List<Vector3> treePositions;
    public int treeDrawDistance = 200;
    public int treeLODDistance = 50;
    public float treeAltitudeModifier = .5f;
    public float treeGradientModifier = .9f;
    MeshSpawner treeSpawner;
    public float treeNoiseScale;//should be around 10
    public float treeNoiseCuttoff;//should be between 0.0 and 1.0

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        terrain = gameObject.GetComponent<TerrainMesh>();

        mapSize = terrain.mapWidth * terrain.vectorSpacing;

        
        //create tree objects
        trees = new GameObject[maxActiveTrees];
        for(int i = 0; i < maxActiveTrees; i++)
        {
            trees[i] = Instantiate(tree, Vector3.zero, Quaternion.identity);
        }
        
        treePositions = new List<Vector3>();

        /*Vector2 peakPoint = new Vector2(terrain.peak.x, terrain.peak.z);
        CreateSlope(GetPointAtPosition(peakPoint + new Vector2(50, 0)));
        CreateSlope(GetPointAtPosition(peakPoint + new Vector2(-50, 0)));
        CreateSlope(GetPointAtPosition(peakPoint + new Vector2(0, 50)));
        CreateSlope(GetPointAtPosition(peakPoint + new Vector2(0, -50)));
        */

        //generate tree locations
        float samplePoint = Random.Range(0, 10000);
        
        for (int x = 0; x < mapSize; x += treeSpacing)
        {
            for(int z = 0; z < mapSize; z += treeSpacing)
            {
                if (Mathf.PerlinNoise(samplePoint + (x / mapSize) * treeNoiseScale, (z / mapSize) * treeNoiseScale) < Random.Range(0f,treeNoiseCuttoff))
                {
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(x,9999,z), Vector3.down, out hit, Mathf.Infinity);

                    if (hit.collider != null)
                    {
                        treePositions.Add(hit.point);

                        //assign tree position to chunk
                        foreach(GameObject chunk in terrain.GetChunks())
                        {
                            /*Bounds b = chunk.GetComponent<Renderer>().bounds;
                            if(b.min.x < hit.point.x && b.max.x > hit.point.x && b.min.z < hit.point.z && b.max.z > hit.point.z)
                            {
                                chunk.GetComponent<TerrainChunk>().treePositions.Add(hit.point);
                                break;
                            }*/
                            if (chunk.GetComponent<TerrainChunk>().chunkBounds.Contains(hit.point))
                            {
                                chunk.GetComponent<TerrainChunk>().treePositions.Add(hit.point);
                                break;
                            }
                        }
                    }
                }
            }
        }

        //GPU instancing for distant trees
        treeSpawner = transform.GetComponent<MeshSpawner>();
        treeSpawner.SetPositions(treePositions, new Vector3(-90,0,0));

        //find local maxima and minima
        int initialSearchSize = 124;
        Vector2 searchStartPoint = new Vector2(300, 300);
        //int refinements = 3;
        
        foreach(Vector2 v in findLocalMinima(searchStartPoint, initialSearchSize, (int)(terrain.mapWidth*terrain.vectorSpacing/searchStartPoint.x)))
        {
            minima.Add(Instantiate(cannon, new Vector3(v.x * terrain.vectorSpacing, terrain.GetHeightAtXY(v), v.y * terrain.vectorSpacing), Quaternion.identity));
        }

        camTarget.transform.position = new Vector3(.5f, 0.0f, .5f) * terrain.mapWidth * terrain.vectorSpacing;
    }
    
    public List<Vector3> CreateSlope(Vector3 startPos)
    {
        List<Vector3> ret = new List<Vector3>();
        ret.Add(startPos);        

        Vector3 currentPos = startPos;
        Vector3 nextPos;
        Vector3 normal = new Vector3();
        Vector2 moveDirection;
        float stepSize = 50f;

        string thisSlopeGrade = GetGradeAtPosition(new Vector2(startPos.x,startPos.z));

        while(GetGradeAtPosition(new Vector2(currentPos.x, currentPos.z)) == thisSlopeGrade && ret.Count < 20)
        {            
            normal = GetNormalAtPosition(currentPos);
            moveDirection = new Vector2(normal.x, normal.z).normalized * stepSize;
            nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);

            //if slope aint right, sweep left and right to look for a spot that's good
            float rotateSteps = 0;
            while(GetGradeAtPosition(new Vector2(nextPos.x, nextPos.z)) != thisSlopeGrade && rotateSteps < 8)
            {
                rotateSteps++;
                moveDirection = rotateV2(moveDirection, 4 * rotateSteps * (-1 * rotateSteps%2));
                //moveDirection = new Vector2(normal.x * stepSize, normal.z * stepSize);
                nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);
            }
            //ret.Add(GetPointAtPosition(new Vector2(currentPos.x + normal.x * 50, currentPos.z + normal.z * 50)));
            ret.Add(nextPos);
            currentPos = ret[ret.Count - 1];
        }

        slopeGrades.Add(thisSlopeGrade);

        GameObject g = new GameObject();
        switch (thisSlopeGrade)
        {
            case "green":
                g = greenMarker;
                break;
            case "blue":
                g = blueMarker;
                break;
            default:
                g = blackMarker;
                break;
        }
        foreach (Vector3 point in ret)
        {
            Instantiate(g, point, Quaternion.identity);
        }

        return ret;
    }

    //rotates vector2 by delta degrees
    public static Vector2 rotateV2(Vector2 v, float delta)
    {
        float deltaRad = delta * 3.14f / 180;
        return new Vector2(
            v.x * Mathf.Cos(deltaRad) - v.y * Mathf.Sin(deltaRad),
            v.x * Mathf.Sin(deltaRad) + v.y * Mathf.Cos(deltaRad)
        );
    }

    //returns result of raycast down at point x,9999,y
    private Vector3 GetPointAtPosition(Vector2 pos)
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(pos.x, 9999, pos.y), Vector3.down, out hit, Mathf.Infinity);

        if (hit.collider != null)
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    // green, blue, or black
    private string GetGradeAtPosition(Vector2 pos)
    {
        if (GetNormalAtPosition(new Vector3(pos.x, 0, pos.y)).y >= greenSlopeRange.x)
            return "green";
        if (GetNormalAtPosition(new Vector3(pos.x, 0, pos.y)).y >= blueSlopeRange.x)
            return "blue";
        else
            return "black";
    }

    //returns normal of raycast down at x,9999,z
    private Vector3 GetNormalAtPosition(Vector3 pos)
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(pos.x, 9999, pos.z), Vector3.down, out hit, Mathf.Infinity);

        if(hit.collider != null)
        {
            return hit.normal;
        }
        return Vector3.zero;
    }

    private List<Vector2> findLocalMinima(Vector2 startPoint, int stepSize, int gridSize)
    {
        List<Vector2> ret = new List<Vector2>();

        //get points to check
        float[,] positions = new float[gridSize, gridSize];
        for(int i = 0;i < gridSize; i++)
        {
            for(int j = 0;j < gridSize; j++)
            {
                positions[i,j] = terrain.GetHeightAtXY(new Vector2(startPoint.x+(i*stepSize), startPoint.y+(j*stepSize)));
            }
        }

        //check for local minima
        for(int i = 1; i <gridSize-1; i++)
        {
            for(int j=1;j < gridSize-1; j++)
            {
                if (positions[i, j] < positions[i - 1, j] && positions[i, j] < positions[i + 1, j] && positions[i, j] < positions[i, j - 1] && positions[i, j] < positions[i, j + 1])
                    ret.Add(startPoint + new Vector2(i * stepSize, j * stepSize));
                //shows non-mins
                //else
                //    Instantiate(redMarker, new Vector3((startPoint.x + i * stepSize)*terrain.vectorSpacing, positions[i, j], (startPoint.y + j * stepSize)* terrain.vectorSpacing), Quaternion.identity);
            }
        }

        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        //tree management//
        //despawn distant trees
        foreach(GameObject tree in trees)
        {
            if(Vector3.Distance(player.transform.position, tree.transform.position) > treeDrawDistance)
            {
                tree.GetComponent<ObjectScript>().ReturnToObjectPool();
            }
        }
        //spawn close trees
        //only checks close chunk positions
        foreach(GameObject chunk in terrain.GetChunks())
        {
            if(chunk.GetComponent<TerrainChunk>().LOD <= 3)
            {
                foreach (Vector3 pos in chunk.GetComponent<TerrainChunk>().treePositions)
                {
                    if (Vector3.Distance(player.transform.position, pos) < treeDrawDistance)
                    {
                        //check if there's already a tree there
                        bool alreadySpawned = false;

                        foreach (GameObject tree in trees)
                        {
                            if (tree.transform.position == pos)
                                alreadySpawned = true;
                        }
                        if (!alreadySpawned)
                            GetFreeTree().GetComponent<ObjectScript>().SpawnAt(pos);
                    }
                }
            }
        }

        //set tree LODs
        foreach (GameObject o in trees)
        {
            if(Vector3.Distance(o.transform.position, player.transform.position) < treeLODDistance)
            {
                if (o.GetComponent<ObjectScript>().LOD != 1)
                    o.GetComponent<ObjectScript>().SetLOD(1);
            }
            else
            {
                if (o.GetComponent<ObjectScript>().LOD != 2)
                    o.GetComponent<ObjectScript>().SetLOD(2);
            }
        }
    }

    //return first tree object that is available to spawn
    GameObject GetFreeTree()
    {
        foreach(GameObject o in trees)
        {
            if (o.GetComponent<ObjectScript>().availableToSpawn)
                return o;
        }
        return null;
    }
}
