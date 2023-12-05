using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSpawner : MonoBehaviour
{
    List<Vector3> positions;
    List<int> closestTerrainIndices = new List<int>(); //closest terrain indices for each position. Used to search through more quickly
    public Material material;
    public Vector3 meshScale;
    List<Vector3> eulerAngles;
    public int batchSize = 1000;
    public Mesh meshLOD3;
    public Mesh meshLOD2;
    public GameObject obj;
    List<GameObject> objectPool;
    public int maxObjects = 10;

    public float LOD2Range = 100;
    public float objRange = 50;

    public TerrainMesh terrain;
    public GameObject player;
    Vector3 prevPlayerPos = Vector3.zero;

    private List<Matrix4x4[]> positionBatches = new List<Matrix4x4[]>();
    private List<Matrix4x4> m4x4s = new List<Matrix4x4>();
    private List<Matrix4x4[]> LOD3Batches;
    private List<Matrix4x4[]> LOD2Batches;
    private List<int> objPositionIndices;

    bool renderMeshes = false;

    // Start is called before the first frame update
    void Start()
    {
        //terrain = transform.GetComponent<TerrainMesh>();
        player = GameObject.FindWithTag("Player");
        objectPool = new List<GameObject>();
        for(int i = 0; i < maxObjects; i++)
        {
            objectPool.Add(Instantiate(obj, Vector3.zero, Quaternion.identity));
            objectPool[objectPool.Count - 1].transform.parent = transform.parent;
        }
    }

    //overload with single rotation and no scale
    public void SetPositions(List<Vector3> posList, Vector3 modelRotation, Vector3 scale)
    {
        eulerAngles = new List<Vector3>();
        foreach (Vector3 pos in posList)
            eulerAngles.Add(modelRotation);

        SetPositions(posList, eulerAngles, scale);
    }
    //Sets positions and default rotation of models to be rendered, turns on rendering
    public void SetPositions(List<Vector3> posList, List<Vector3> modelRotation, Vector3 scale)
    {
        positions=posList;
        eulerAngles = modelRotation;
        renderMeshes = true;
        meshScale= scale;
        SetBatches();
        closestTerrainIndices = new List<int>();
        //set closestTerrainIndices based on positions
        foreach(Vector3 pos in positions)
        {
            closestTerrainIndices.Add((int)(pos.z / terrain.vectorSpacing)*(terrain.mapWidth+1) + (int)(pos.x / terrain.vectorSpacing));
        }

        /*m4x4s = new List<Matrix4x4>();
        for(int i = 0; i < positions.Count; i++)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(positions[i], Quaternion.Euler(eulerAngles[i]), Vector3.one * scale);
            m4x4s.Add(m);
        }
        */
    }

    void SetBatches()
    {
        
        //list of terrain indices that correspond to LODs
        List<int> LOD2Indices = new List<int>();
        List<int> objIndices = new List<int>();

        int playerXVert = (int)(player.transform.position.x / terrain.vectorSpacing);
        int playerYVert = (int)(player.transform.position.z / terrain.vectorSpacing);
        int LOD2NumVerts = (int)(LOD2Range/terrain.vectorSpacing/2);
        int objNumVerts = (int)(objRange/terrain.vectorSpacing/2);

        //set indices to spawn objects and render LOD2 based on player position
        for(int x = playerXVert - objNumVerts; x < playerXVert + objNumVerts; x++)
        {
            for(int y = playerYVert - objNumVerts; y < playerYVert + objNumVerts; y++)
            {
                objIndices.Add(y * (terrain.mapWidth + 1) + x);
            }
        }
        /*for (int x = playerXVert - LOD2NumVerts; x < playerXVert + LOD2NumVerts; x++)
        {
            for (int y = playerYVert - LOD2NumVerts; y < playerYVert + LOD2NumVerts; y++)
            {
                if(!objIndices.Contains(y*(terrain.mapWidth + 1) + x))
                    LOD2Indices.Add(y * (terrain.mapWidth + 1) + x);
            }
        }*/

        //set object positions
        objPositionIndices = new List<int>();
        foreach(int x in closestTerrainIndices)
        {
            if (objIndices.Contains(x))
            {
                objPositionIndices.Add(closestTerrainIndices.IndexOf(x));
            }
        }

        /*Matrix4x4[] thisBatch;
        //set LOD2 Matrix4x4s
        LOD2Batches = new List<Matrix4x4[]>();
        thisBatch = new Matrix4x4[batchSize];
        int batchCount = 0;
        foreach(int i in closestTerrainIndices)
        {
            if (LOD2Indices.Contains(i))
            {
                thisBatch[batchCount] = m4x4s[closestTerrainIndices.IndexOf(i)];
                batchCount++;
                if(batchCount >= batchSize)
                {
                    LOD2Batches.Add(thisBatch);
                    batchCount = 0;
                    thisBatch = new Matrix4x4[batchSize];
                }
            }
        }
        if (batchCount > 0)
            LOD2Batches.Add(thisBatch);

        //set LOD3 Batches
        LOD3Batches = new List<Matrix4x4[]>();
        thisBatch = new Matrix4x4[batchSize];
        batchCount = 0;
        //set LOD3 Matrix4x4s
        foreach (int i in closestTerrainIndices)
        {
            if(!objIndices.Contains(i) && !LOD2Indices.Contains(i))
            {
                thisBatch[batchCount] = m4x4s[closestTerrainIndices.IndexOf(i)];
                batchCount++;
                if (batchCount >= batchSize)
                {
                    LOD3Batches.Add(thisBatch);
                    batchCount = 0;
                    thisBatch = new Matrix4x4[batchSize];
                }
            }
        }
        if (batchCount > 0)
            LOD3Batches.Add(thisBatch);
        
        yield return null;
        */

        positionBatches.Clear();
        int i = 0;
        int j = 0;
        Matrix4x4[] thisBatch = new Matrix4x4[batchSize];
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
                    m.SetTRS(positions[i * batchSize + j], Quaternion.Euler(eulerAngles[i * batchSize + j]), meshScale);
                    thisBatch[j] = m;
                }

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
        //reset lod lists
        //if (Vector3.Distance(prevPlayerPos, player.transform.position) > terrain.vectorSpacing)
        //{
           // prevPlayerPos = player.transform.position;
            SetBatches();
       //}

        /*if (LOD3Batches == null)
            return;
        //instancing
        foreach (Matrix4x4[] batch in LOD3Batches)
        {
            Graphics.DrawMeshInstanced(meshLOD3, 0, material, batch);
        }
        if (LOD2Batches == null)
            return;
        foreach(Matrix4x4[] batch in LOD2Batches)
        {
            Graphics.DrawMeshInstanced(meshLOD2, 0, material, batch);
        }*/

        //object management//
        //despawn distant objects
        foreach (GameObject go in objectPool)
        {
            //dont check objects that aren't spawned
            if (!go.GetComponent<ObjectScript>().availableToSpawn)
            {
                bool needsToDespawn = true;
                foreach(int i in objPositionIndices)
                {
                    if(positions[i] == go.transform.position)
                    {
                        needsToDespawn = false;
                    }
                }
                if (needsToDespawn)
                    go.GetComponent<ObjectScript>().ReturnToObjectPool();
            }
        }
        //spawn close trees
        foreach(int index in objPositionIndices)
        {
            bool alreadySpawned = false;
            foreach(GameObject o in objectPool)
            {
                if(o.transform.position == positions[index])
                {
                    alreadySpawned = true;
                    break;
                }
            }
            if (!alreadySpawned)
            {
                GetFreeTree().GetComponent<ObjectScript>().SpawnAt(positions[index]);
            }
        }

        
        foreach(Matrix4x4[] batch in positionBatches)
        {
            Graphics.DrawMeshInstanced(meshLOD3, 0, material, batch);
        }
    }

    GameObject GetFreeTree()
    {
        foreach (GameObject o in objectPool)
        {
            if (o.GetComponent<ObjectScript>().availableToSpawn)
                return o;
        }
        return null;
    }
}