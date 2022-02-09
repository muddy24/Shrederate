using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mountain : MonoBehaviour
{
    TerrainMesh terrain;
    float mapSize;

    //tree stuff
    public GameObject tree;
    public int treeSpacing = 5;
    List<GameObject> trees;

    // Start is called before the first frame update
    void Start()
    {
        terrain = gameObject.GetComponent<TerrainMesh>();

        mapSize = terrain.mapWidth * terrain.vectorSpacing;

        trees = new List<GameObject>();
        
        //spawn trees
        for(int x = 0; x < mapSize; x += treeSpacing)
        {
            for(int z = 0; z < mapSize; z += treeSpacing)
            {
                RaycastHit hit;
                Physics.Raycast(new Vector3(x,99999,z), Vector3.down, out hit, Mathf.Infinity);

                if(hit.collider != null)
                {
                    trees.Add(Instantiate(tree, hit.point, Quaternion.identity));
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
