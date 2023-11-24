using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Mountain : MonoBehaviour
{
    TerrainMesh terrain;
    float mapSize;

    public GameObject greenMarker;
    public GameObject blueMarker;
    public GameObject blackMarker;
    public GameObject redMarker;
    public GameObject slopePrefab;
    public GameObject cannon;
    public GameObject camTarget;
    public GameObject camPosition;
    public List<GameObject> minima = new List<GameObject>();

    //slope stuff
    List<List<Vector3>> slopePoints = new List<List<Vector3>>();
    List<List<bool>> ridgePoints = new List<List<bool>>();
    List<string> slopeGrades = new List<string>();
    List<GameObject> slopes = new List<GameObject>();
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

        Vector2 peakPoint = new Vector2(terrain.peak.x, terrain.peak.z);

        //make slopes along ridge from peak
        Vector2 ridgeDirection;
        for (int i = 0; i < 360; i += 45)
        {
            ridgeDirection = rotateV2(Vector2.left, i);
            CreateRidgeSlopeAt(GetPointAtPosition(peakPoint), ridgeDirection);
        }

        //spawn slopes branching off of ridge slopes
        int numRidgeSlopes = slopes.Count;
        for(int i = 0; i <= numRidgeSlopes; i++)
        {
            SplineComputer spline = slopes[i].GetComponent<Slope>().spline;


            float splineLength = spline.CalculateLength();
            float branchSpacing = 100;
            for(float distance = branchSpacing; distance < splineLength; distance += branchSpacing)
            {
                double travel = spline.Travel(0.0, distance, Spline.Direction.Forward);
                //right side
                Vector3 startPoint = spline.EvaluatePosition(travel) + (spline.Evaluate(travel).right * 50);               
                CreateSlopesAt(GetPointAtPosition(new Vector2(startPoint.x, startPoint.z)), spline.EvaluatePosition(travel));
                //left side
                startPoint = spline.EvaluatePosition(travel) + (spline.Evaluate(travel).right * -50);
                CreateSlopesAt(GetPointAtPosition(new Vector2(startPoint.x, startPoint.z)), spline.EvaluatePosition(travel));

            }
        }

        //Samples ridge points
        /*
        for(int i = 500; i < 3500; i += 50)
        {
            for(int j = 500; j < 3500; j += 50)
            {
                if(IsRidge(new Vector2(i, j), 5f))
                {
                    Instantiate(redMarker, GetPointAtPosition(new Vector2(i, j)), Quaternion.identity);
                }
            }
        }*/

        HideSlopes();
        
        //spawn sphere colliders and add terrain point indices to slope
        foreach (GameObject g in slopes)
        {
            g.GetComponent<Slope>().SpawnColliders();
            g.GetComponent<Slope>().terrainPointIndices = new List<int>();
            foreach(GameObject sphere in g.GetComponent<Slope>().spheres)
            {
                foreach(int i in SphereColliderTerrainIndices(sphere.GetComponent<SphereCollider>()))
                {
                    if (!g.GetComponent<Slope>().terrainPointIndices.Contains(i))
                        g.GetComponent<Slope>().terrainPointIndices.Add(i);
                }
                
            }
        }

        terrain.LevelSlopes(slopes);

        
        //generate tree locations
        float samplePoint = Random.Range(0, 10000);       
        for (int x = 0; x < mapSize; x += treeSpacing)
        {
            for(int z = 0; z < mapSize; z += treeSpacing)
            {
                RaycastHit hit;
                Physics.Raycast(new Vector3(x,9999,z), Vector3.down, out hit, Mathf.Infinity);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.tag != "SlopeCollider")
                    {


                        treePositions.Add(hit.point);

                        //assign tree position to chunk
                        foreach (GameObject chunk in terrain.GetChunks())
                        {
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
        
        foreach(GameObject g in slopes)
        {
            g.GetComponent<Slope>().DespawnColliders();
        }
        
        //GPU instancing for distant trees
        treeSpawner = transform.GetComponent<MeshSpawner>();
        treeSpawner.SetPositions(treePositions, new Vector3(-90,0,0));

        //find local maxima and minima
        /*int initialSearchSize = 124;
        Vector2 searchStartPoint = new Vector2(300, 300);
        //int refinements = 3;
        
        foreach(Vector2 v in findLocalMinima(searchStartPoint, initialSearchSize, (int)(terrain.mapWidth*terrain.vectorSpacing/searchStartPoint.x)))
        {
            minima.Add(Instantiate(cannon, new Vector3(v.x * terrain.vectorSpacing, terrain.GetHeightAtXY(v), v.y * terrain.vectorSpacing), Quaternion.identity));
        }*/
        
        camTarget.transform.position = new Vector3(.5f, 0.0f, .5f) * terrain.mapWidth * terrain.vectorSpacing;
    }

    //returns indices of terrain verts within a sphere collider
    private List<int> SphereColliderTerrainIndices(SphereCollider col)
    {
        int mapWidth = terrain.mapWidth+1;
        int colliderVertRadius = (int)(col.radius* col.transform.localScale.x / terrain.vectorSpacing);
        int centerXVert = (int)(col.transform.position.x / terrain.vectorSpacing);
        int centerYVert = (int)(col.transform.position.z / terrain.vectorSpacing);

        List<int> ret = new List<int>();

        //loop over verts in a square around the sphere
        for(int x = centerXVert - colliderVertRadius; x < centerXVert + colliderVertRadius; x++)
        {
            for(int y = centerYVert - colliderVertRadius; y < centerYVert + colliderVertRadius; y++)
            {
                //add index to return list if it's within the radius of the sphere
                if(Vector2.Distance(new Vector2(x,y), new Vector2(centerXVert,centerYVert)) < colliderVertRadius)
                {
                    ret.Add(y * mapWidth + x);
                }
                    
            }
        }

        return ret;
    }

    public void HideSlopes()
    {
        foreach(GameObject g in slopes)
        {
            g.GetComponent<Renderer>().enabled = false;
        }
    }

    public void ShowSlopes()
    {
        foreach(GameObject g in slopes)
        {
            g.GetComponent<Renderer>().enabled = true;
        }
    }
    
    //creates a slope along the ridge
    public void CreateRidgeSlopeAt(Vector3 startPos, Vector2 pathStartDirection)
    {
        List<Vector3> ret = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        ret.Add(startPos);
        normals.Add(GetNormalAtPosition(startPos));

        Vector3 currentPos = startPos;
        Vector3 nextPos = Vector3.zero;
        Vector2 moveDirection = pathStartDirection;
        float stepSize = 50f;

        int i = 0;

        //get second point
        while (i < 360)
        {
            nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection * stepSize);
            if (IsRidge(new Vector2(nextPos.x, nextPos.z), 5))
            {
                ret.Add(nextPos);
                normals.Add(GetNormalAtPosition(nextPos));
                currentPos = nextPos;
                break;
            }

            //increment move angle
            if (i > 0)
                i = (i * -1) - 10;
            else
                i = (i * -1) + 10;

            moveDirection = rotateV2(moveDirection, i);
        }

        bool foundPoint = true;
        //get continuing points
        for(int j = 0; j < 50; j++)
        {
            foundPoint = false;
            //start checking in continuing direction
            Vector3 dif = ret[ret.Count - 1] - ret[ret.Count - 2]; //Vector3.MoveTowards(ret[ret.Count - 2], ret[ret.Count - 1], stepSize).normalized;
            dif = dif.normalized;
            moveDirection = new Vector2(dif.x, dif.z);// new Vector2(ret[ret.Count - 1].x, ret[ret.Count-1].z) - new Vector2(ret[ret.Count - 2].x, ret[ret.Count-2].z);
            i = 0;
            while (i < 90)
            {
                //check if nextPos is on a ridge, if so add it
                nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection * stepSize);
                if (IsRidge(new Vector2(nextPos.x, nextPos.z), 5))
                {
                    ret.Add(nextPos);
                    normals.Add(GetNormalAtPosition(nextPos));
                    currentPos = nextPos;
                    foundPoint = true;
                    break;
                }

                //increment move angle to sweep
                if (i > 0)
                    i = (i * -1) - 10;
                else
                    i = (i * -1) + 10;
                moveDirection = rotateV2(moveDirection, i);
            }

            //end slope if no ridge found or too close to another slope
            if (foundPoint == false || DistanceToClosestSlope(nextPos) < stepSize)
                break;
        }

        //do not add slope if it's too small
        if (Vector3.Distance(ret[0], ret[ret.Count - 1]) < 200)
            return;

        //create slope game object
        GameObject newSlope = Instantiate(slopePrefab, Vector3.zero, Quaternion.identity);
        newSlope.GetComponent<Slope>().SetSlopePoints(ret,normals);
        newSlope.GetComponent<Slope>().SetGrade(AverageSlopeGrade(ret));
        newSlope.transform.parent = transform;
        slopes.Add(newSlope);

    }

    //overload with no branch position
    public void CreateSlopesAt(Vector3 startPos)
    {
        CreateSlopesAt(startPos, Vector3.zero);
    }

    //recursively generates slopes from startpos
    //goes downhill untill the slope grade changes, then adds slopes from the bottom
    public void CreateSlopesAt(Vector3 startPos, Vector3 branchPos)
    {
        List<Vector3> ret = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        //add branchpos from parent spline if available
        if(branchPos != Vector3.zero)
        {
            ret.Add(branchPos);
            normals.Add(GetNormalAtPosition(branchPos));
        }
        ret.Add(startPos);
        normals.Add(GetNormalAtPosition(startPos));

        Vector3 currentPos = startPos;
        Vector3 nextPos;
        Vector3 normal = new Vector3();
        Vector2 moveDirection;
        float stepSize = 50f;

        string thisSlopeGrade = GetGradeAtPosition(new Vector2(startPos.x,startPos.z));

        //find points to add to this slope
        while(GetGradeAtPosition(new Vector2(currentPos.x, currentPos.z)) == thisSlopeGrade && ret.Count < 20 && terrain.DistanceToCenterAxis(currentPos) < (terrain.mapWidth/2)*terrain.vectorSpacing*.8f)
        {            
            normal = GetNormalAtPosition(currentPos);
            moveDirection = new Vector2(normal.x, normal.z).normalized * stepSize;
            nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);

            //if grade aint right sweep left and right to look for a spot that's good
            float rotateSteps = 0;
            while(GetGradeAtPosition(new Vector2(nextPos.x, nextPos.z)) != thisSlopeGrade && rotateSteps < 8)
            {
                rotateSteps++;
                moveDirection = rotateV2(moveDirection, 2 * rotateSteps * -1 * rotateSteps%2);
                nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);
            }

            //end slope if too close to another slope
            if (DistanceToClosestSlope(nextPos) < 100)
                break;

            ret.Add(nextPos);
            normals.Add(GetNormalAtPosition(nextPos));
            currentPos = ret[ret.Count - 1];
        }

        //do not add slope if it's too small
        if (Vector3.Distance(ret[0], ret[ret.Count-1]) < 200)
            return;

        slopeGrades.Add(thisSlopeGrade);

        //drop markers to show slope path
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

        //create slope game object
        GameObject newSlope = Instantiate(slopePrefab, Vector3.zero, Quaternion.identity);
        newSlope.GetComponent<Slope>().SetSlopePoints(ret,normals);
        newSlope.GetComponent<Slope>().SetGrade(thisSlopeGrade);
        newSlope.transform.parent = transform;
        slopes.Add(newSlope);

        //get new points and recurse
        normal = GetNormalAtPosition(currentPos);
        moveDirection = new Vector2(normal.x, normal.z).normalized * stepSize;
        nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);
        CreateSlopesAt(nextPos, currentPos);
        moveDirection = rotateV2(moveDirection, 90);
        nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);
        CreateSlopesAt(nextPos, currentPos);
        moveDirection = rotateV2(moveDirection, -180);
        nextPos = GetPointAtPosition(new Vector2(currentPos.x, currentPos.z) + moveDirection);
        CreateSlopesAt(nextPos, currentPos);
    }

    //returns grade based on average normal of all slope points
    //used for ridge slopes
    public string AverageSlopeGrade(List<Vector3> slopePoints) 
    {
        Vector3 averageNorm = Vector3.zero;
        foreach(Vector3 point in slopePoints)
        {
            averageNorm += GetNormalAtPosition(point);
        }
        averageNorm = averageNorm.normalized;

        if (averageNorm.y >= greenSlopeRange.x)
            return "green";
        if (averageNorm.y >= blueSlopeRange.x)
            return "blue";
        else
            return "black";
    }
    //returns the distance to the nearest slope point in all generated slopes
    public float DistanceToClosestSlope(Vector3 pos)
    {
        float ret = 99999;
        foreach(GameObject g in slopes)
        {
            foreach(Vector3 slopePoint in g.GetComponent<Slope>().GetSplinePoints())
            {
                if (Vector3.Distance(pos, slopePoint) < ret)
                    ret = Vector3.Distance(pos, slopePoint);
            }
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
            }
        }

        return ret;
    }

    private List<Vector2> findLocalMaxima(Vector2 startPoint, int stepSize, int gridSize)
    {
        List<Vector2> ret = new List<Vector2>();

        //get points to check
        float[,] positions = new float[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                positions[i, j] = terrain.GetHeightAtXY(new Vector2(startPoint.x + (i * stepSize), startPoint.y + (j * stepSize)));
            }
        }

        //check for local maxima
        for (int i = 1; i < gridSize - 1; i++)
        {
            for (int j = 1; j < gridSize - 1; j++)
            {
                if (positions[i, j] > positions[i - 1, j] && positions[i, j] > positions[i + 1, j] && positions[i, j] > positions[i, j - 1] && positions[i, j] > positions[i, j + 1])
                    ret.Add(startPoint + new Vector2(i * stepSize, j * stepSize));
            }
        }

        return ret;
    }

    //returns true if point is on a ridgeline
    //ridgeline is defined as having two opposing neighbors lower than point, with lower average than the other two neighbors
    private bool IsRidge(Vector2 point, float minHeightDif)
    {
        float pointHeight = GetPointAtPosition(point).y;

        //get adjacent points
        Vector2 direction = Vector2.left;
        float[] heights = new float[4];
        for(int i = 0; i < 4; i++)
        {
            heights[i] = GetPointAtPosition(point + direction * 50).y;
            direction = rotateV2(direction, 90);
        }
        //check for ridge
        if (heights[0] < pointHeight-minHeightDif && heights[2] < pointHeight-minHeightDif && (heights[1] + heights[3]) / 2 > (heights[0] + heights[2]) / 2)
            return true;
        else if (heights[1] < pointHeight-minHeightDif && heights[3] < pointHeight-minHeightDif && (heights[0] + heights[2]) / 2 > (heights[1] + heights[3]) / 2)
            return true;

        //check again at 45 deg offset
        direction = rotateV2(direction, 45);
        for (int i = 0; i < 4; i++)
        {
            heights[i] = GetPointAtPosition(point + direction * 50).y;
            direction = rotateV2(direction, 90);
        }
        if (heights[0] < pointHeight-minHeightDif && heights[2] < pointHeight-minHeightDif && (heights[1] + heights[3]) / 2 > (heights[0] + heights[2]) / 2)
            return true;
        else if (heights[1] < pointHeight-minHeightDif && heights[3] < pointHeight-minHeightDif && (heights[0] + heights[2]) / 2 > (heights[1] + heights[3]) / 2)
            return true;

        return false;
    }

    //returns two points on a ridge given a peak
    private List<Vector3> GetRidgePoints(Vector3 peak)
    {
        Vector3 nextPos;
        Vector2 moveDirection;
        int ridgeDirection = 0;
        List<Vector3> ret = new List<Vector3>();
        ret.Add(Vector3.zero);

        //ret[0] gets set to the highest point around the peak
        for (int i = 0; i < 360; i += 5)
        {
            moveDirection = rotateV2(Vector2.left, i) * 100;
            nextPos = GetPointAtPosition(new Vector2(peak.x, peak.z) + moveDirection);

            if (nextPos.y > ret[0].y)
            {
                ret[0] = nextPos;
                ridgeDirection = i;
            }
                
        }

        //ret[1] is set to the point 180 degrees around the peak from ret[0]
        moveDirection = rotateV2(Vector3.left, ridgeDirection + 180);
        ret.Add(GetPointAtPosition(new Vector2(peak.x, peak.z) + moveDirection));

        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        /*
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
            if(chunk.GetComponent<TerrainChunk>().LOD == 1)
            {
                foreach (Vector3 pos in chunk.GetComponent<TerrainChunk>().treePositions)
                {
                    Debug.Log("DrawDist: " + treeDrawDistance + "PlayerDist : " + Vector3.Distance(player.transform.position, pos));
                    if (Vector3.Distance(player.transform.position, pos) < treeDrawDistance)
                    {
                        Debug.Log("Valid tree pos");
                        //check if there's already a tree there
                        bool alreadySpawned = false;

                        foreach (GameObject tree in trees)
                        {
                            if (tree.transform.position == pos)
                            {
                                alreadySpawned = true;
                                break;
                            }
                        }
                        if (!alreadySpawned)
                        {
                            Debug.Log("Spawning Tree");
                            GetFreeTree().GetComponent<ObjectScript>().SpawnAt(pos);
                        }
                            
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
        }*/
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
