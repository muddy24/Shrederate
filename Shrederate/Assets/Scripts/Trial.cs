using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class Trial : MonoBehaviour
{
    List<string> trialTypes;
    public string trialType;

    public SplineComputer slopePath;

    public GameObject slalomFlag;
    public List<GameObject> slalomFlags;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetTrial()
    {
        slopePath = gameObject.GetComponent<Slope>().spline;

        trialTypes = new List<string>();
        trialTypes.Add("slalom");
        trialTypes.Add("trickPark");

        trialType = trialTypes[Random.Range(0, trialTypes.Count)];

        switch (trialType)
        {
            case "slalom":
                CreateSlalom();
                break;

            default:
                break;
        }
    }

    public void CreateSlalom()
    {
        slalomFlags = new List<GameObject>();

        float d = 100;
        SplineSample ss = new SplineSample();
        int direction = 1;
        while(d < slopePath.CalculateLength())
        {
            ss = slopePath.Evaluate(slopePath.Travel(0f, d, Spline.Direction.Forward));

            slalomFlags.Add(Instantiate(slalomFlag, ss.position + (ss.right * 20 * direction) - Vector3.up * 2, Quaternion.LookRotation(ss.right, ss.up)));

            direction *= -1;

            d += 60;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
