using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;


public class Slope : MonoBehaviour
{
    public SplinePoint[] pathPoints;
    public List<int> terrainPointIndices;
    public string grade;
    public string slopeName = "name";
    public SplineComputer spline;
    public Vector3 slopeStartPos;

    public GameObject sphere;
    public List<GameObject> spheres = new List<GameObject>();

    public GameObject signPrefab;
    public GameObject startSign;

    public SplineComputer colliderSpline;

    public Material greenMat;
    public Material blueMat;
    public Material blackMat;

    public Trial trial;

    //Random words for slope names
    List<string> slopeName1 = new List<string> { "Peak", "Ridge", "Summit", "Alpine", "Glacier", "Frost", "Crystal", "Powder", "Avalanche", "Blizzard", "Frostbite", "Slope", "Hillside", "Crest", "Drift", "Iceberg", "Frosty", "Frostbite", "Cold", "Whiteout", "Frosting", "Polar", "Arctic", "Freeze", "Blizzardy", "Icy", "Glacial", "Winter", "Tundra", "Hailstone", "Chilled", "Arctic", "Icicle", "Glaze", "Storm", "Icebound", "Chill", "Slush", "White", "Frozen", "Biting", "Glitter", "Bank", "Hoarfrost", "Wintry", "Subzero", "Shiver", "Sleet", "Glissade", "Icefall", "Bound", "Snowy", "Snowpack", "Mass", "Snowmelt", "Drift", "Line", "Sport", "Cat", "Capped", "Shoe", "Suit", "Mobile", "Bird", "Board", "Cone", "Plow" };
    List<string> slopeName2 = new List<string> { "Run", "Slope", "Trail", "Slide", "Drop", "Descent", "Glide", "Path", "Way", "Course", "Track", "Route", "Line", "Passage", "Runway", "Journey", "Passage", "Downhill", "Decline", "Incline", "Ascent", "Climb", "Rise", "Descent", "Descent", "Trailblaze", "Trajectory", "Trajectory", "Route", "Traverse", "Course", "Journey", "Pathway", "Slope", "Downhill", "Incline", "Ascent", "Slide", "Glide", "Descent", "Drop", "Run", "Slope", "Trail", "Slide", "Drop", "Descent", "Glide", "Path", "Way", "Course", "Track", "Route", "Line", "Passage", "Runway", "Journey", "Passage", "Downhill", "Decline", "Incline", "Ascent", "Climb", "Rise", "Descent", "Descent", "Trailblaze", "Trajectory", "Trajectory", "Route", "Traverse", "Course", "Journey", "Pathway", "Slope", "Downhill", "Incline", "Ascent", "Slide", "Glide", "Descent", "Drop" };

    // Start is called before the first frame update
    void Start()
    {
        //spline = GetComponentInChildren<SplineComputer>();
        //colliderSpline.RebuildImmediate();
        spline.RebuildImmediate();
        slopeName = slopeName1[Random.Range(0, slopeName1.Count)] + " " + slopeName2[Random.Range(0, slopeName2.Count)];
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

        slopeStartPos = spline.EvaluatePosition(spline.Travel(0, 25));
        startSign = Instantiate(signPrefab, slopeStartPos, Quaternion.identity);
        startSign.gameObject.transform.parent = transform;

        SplineSample ss = new SplineSample();
        spline.Evaluate(1.0, ref ss);
        

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
            DestroyImmediate(go);
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
