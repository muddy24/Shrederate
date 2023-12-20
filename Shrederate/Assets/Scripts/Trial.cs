using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Trial : MonoBehaviour
{
    Mountain mountain;

    List<string> trialTypes;
    public string trialType;

    public SplineComputer slopePath;

    public GameObject slalomFlag;
    public List<GameObject> slalomFlags;
    public List<float> flagTravelAmounts;

    public GameObject finishLinePrefab;
    public GameObject finishLine;

    public bool[] trialCompletions = new bool[3];

    public string bestScore = "N/A";

    public float[] scoreGoals = new float[3];

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetTrial()
    {
        mountain = GameObject.FindWithTag("Mountain").GetComponent<Mountain>();

        slopePath = gameObject.GetComponent<Slope>().spline;

        trialTypes = new List<string>();
        trialTypes.Add("Slalom");
        trialTypes.Add("Trick Park");

        for(int i = 0; i < 3; i++)
        {
            trialCompletions[i] = false;
        }

        trialType = trialTypes[Random.Range(0, trialTypes.Count)];

        switch (trialType)
        {
            case "Slalom":
                CreateSlalom();
                break;

            case "Trick Park":
                CreateTrickPark();
                break;

            default:
                break;
        }
    }

    public void CreateSlalom()
    {
        slalomFlags = new List<GameObject>();
        flagTravelAmounts = new List<float>();

        float d = 100;
        SplineSample ss = new SplineSample();
        int direction = 1;
        while(d < slopePath.CalculateLength()-100)
        {
            ss = slopePath.Evaluate(slopePath.Travel(0f, d, Spline.Direction.Forward));
            flagTravelAmounts.Add((float)ss.percent);
            slalomFlags.Add(Instantiate(slalomFlag, ss.position + (ss.right * 15 * direction), Quaternion.LookRotation(ss.right, Vector3.up)));
            slalomFlags[slalomFlags.Count-1].transform.parent = gameObject.transform;
            slalomFlags[slalomFlags.Count-1].GetComponent<SlalomScript>().gateScreen.GetComponent<Renderer>().enabled = false;
            direction *= -1;

            d += 60;
        }

        ss = slopePath.Evaluate(1.0f);
        finishLine = Instantiate(finishLinePrefab, ss.position, Quaternion.LookRotation(ss.right, ss.up));

        scoreGoals[0] = slopePath.CalculateLength() / 30;
        scoreGoals[1] = scoreGoals[0] * 1.5f;
        scoreGoals[2] = scoreGoals[0] * 2.0f; 
    }

    public void CreateTrickPark()
    {
        scoreGoals[0] = slopePath.CalculateLength() * 10;
        scoreGoals[1] = scoreGoals[0] / 1.5f;
        scoreGoals[2] = scoreGoals[0] / 2.0f;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
