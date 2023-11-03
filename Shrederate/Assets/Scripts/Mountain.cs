using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour
{
    TerrainMesh terrain;
    float mapSize;

    public GameObject greenMarker;
    public GameObject redMarker;
    public GameObject camTarget;
    public List<GameObject> minima = new List<GameObject>();

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

        //generate tree locations
        float samplePoint = Random.Range(0, 10000);
        for (int x = 0; x < mapSize; x += treeSpacing)
        {
            for(int z = 0; z < mapSize; z += treeSpacing)
            {
                if (Mathf.PerlinNoise(samplePoint + (x / mapSize) * treeNoiseScale, (z / mapSize) * treeNoiseScale) < Random.Range(0f,treeNoiseCuttoff))
                {
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(x,99999,z), Vector3.down, out hit, Mathf.Infinity, ~3);

                    if(hit.collider != null)
                    {
                        treePositions.Add(hit.point);

                        //assign tree position to chunk
                        foreach(GameObject chunk in terrain.GetChunks())
                        {
                            chunk.GetComponent<TerrainChunk>().boxBounds.enabled = true;
                            if (chunk.GetComponent<TerrainChunk>().boxBounds.bounds.Contains(hit.point))
                            {
                                chunk.GetComponent<TerrainChunk>().treePositions.Add(hit.point);
                                break;
                            }
                            chunk.GetComponent<TerrainChunk>().boxBounds.enabled = false;
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
            minima.Add(Instantiate(greenMarker, new Vector3(v.x * terrain.vectorSpacing, terrain.GetHeightAtXY(v), v.y * terrain.vectorSpacing), Quaternion.identity));
        }

        camTarget.transform.position = new Vector3(.5f, 0.0f, .5f) * terrain.mapWidth * terrain.vectorSpacing;
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

        //old way where it checks through every treepos each frame
        /*foreach (Vector3 pos in treePositions)
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
        }*/

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
