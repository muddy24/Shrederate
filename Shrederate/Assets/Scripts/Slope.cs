using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;


public class Slope : MonoBehaviour
{
    public SplinePoint[] pathPoints;
    public List<int> terrainPointIndices;
    public string grade;
    public SplineComputer spline;

    public GameObject sphere;
    public List<GameObject> spheres = new List<GameObject>();

    public SplineComputer colliderSpline;

    public Material greenMat;
    public Material blueMat;
    public Material blackMat;

    // Start is called before the first frame update
    void Start()
    {
        //spline = GetComponentInChildren<SplineComputer>();
        colliderSpline.RebuildImmediate();
        spline.RebuildImmediate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGrade(string s)
    {
        grade = s;

        switch (grade)
        {
            case "green":
                GetComponent<Renderer>().material = greenMat;
                break;
            case "blue":
                GetComponent<Renderer>().material = blueMat;
                break;
            case "black":
                GetComponent<Renderer>().material = blackMat;
                break;
            default:
                break;
        }
    }

    public void SetSlopePoints(List<Vector3> points, List<Vector3> normals)
    {
        pathPoints = new SplinePoint[points.Count];
        for(int i = 0; i < points.Count; i++)
        {
            pathPoints[i] = new SplinePoint();
            pathPoints[i].position = points[i];
            pathPoints[i].normal = normals[i];
            pathPoints[i].size = 1f;
            pathPoints[i].color = Color.white;            
        }

        spline.SetPoints(pathPoints);
        spline.RebuildImmediate();

        //MeshCollider mc = colMesh.gameObject.AddComponent<MeshCollider>();
        //mc.sharedMesh = null;
        //mc.sharedMesh = colMesh.mesh;
    }

    public void SpawnColliders()
    {
        foreach(SplinePoint sp in pathPoints)
        {
            spheres.Add(Instantiate(sphere, sp.position, Quaternion.identity));
            spheres[spheres.Count - 1].tag = "SlopeCollider";
        }
    }

    public void DespawnColliders()
    {
        foreach(GameObject go in spheres)
        {
            Destroy(go);
        }
    }

    public List<Vector3> GetSplinePoints()
    {
        List<Vector3> ret = new List<Vector3>();
        foreach(SplinePoint sp in pathPoints)
        {
            ret.Add(sp.position);
        }

        return ret;
    }
}
