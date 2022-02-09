using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public List<GameObject> LODs;
    public int LOD = 1;
    public float distancePerLOD = 100;

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        SetLOD(1);
    }

    // Update is called once per frame
    void Update()
    {
        int newLOD = (int)Mathf.Ceil(Vector3.Distance(transform.position, player.transform.position) / distancePerLOD);
        //if(newLOD > LODs.Count) newLOD = LODs.Count;

        if (LOD != newLOD)
            SetLOD(newLOD);
    }

    public void SetLOD(int lod)
    {
        LOD = lod;

        for(int i = 1; i <= LODs.Count; i++)
        {
            if (i == LOD)
                LODs[i-1].SetActive(true);
            else LODs[i-1].SetActive(false);            
        }
    }

}
