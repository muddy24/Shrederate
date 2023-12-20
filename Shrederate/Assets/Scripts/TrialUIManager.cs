using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;

public class TrialUIManager : MonoBehaviour
{
    List<Slope> slopes;
    List<Vector3> slopeStartPositions;
    public Canvas trialCanvas;
    public Canvas trialReticle;
    public float reticleRotationSpeed = 360f;
    public Camera cam;
    public float cuttoffDistance = 100f;
    public float cuttoffAngle = 45f;
    public float startDistance = 10f; //distance from start at which the A button can be pressed to start a challenge

    public Vector3 trialPanelHomePosition;
    
    public Text slopeName;
    public Text trialType;
    public Text highScore;
    public Text statusText;
    public Text[] goalTexts;
    public Text notificationText;
    public Text warningText;
    public Text warningTimeText;
    public Text addPointsText;
    public Image[] checkImages;

    GameManager gm;

    float slalomMissedGatePenalty = 3f;

    int currentSlopeIndex = -1;

    GameObject player;

    string trialState = "idle";

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        gm = player.GetComponent<GameManager>();
        trialPanelHomePosition = trialCanvas.GetComponent<RectTransform>().localPosition;
        slopes = new List<Slope>();
        slopeStartPositions = new List<Vector3>();
        cam = Camera.main;
        addPointsText.enabled = false;
    }
    
    void UpdateUIFromTrial()
    {
        Trial trial = slopes[currentSlopeIndex].GetComponent<Trial>();

        for(int i = 0; i < 3; i++)
        {
            //set check images
            if (trial.trialCompletions[i])
                checkImages[i].enabled = true;
            else
                checkImages[i].enabled = false;
            //set goal texts
            switch (trial.trialType)
            {
                case "Slalom":
                    goalTexts[i].text = trial.scoreGoals[i].ToString("F2");
                    break;
                default:
                    goalTexts[i].text = trial.scoreGoals[i].ToString("F0");
                    break;
            }
            
        }
        //set title stuff
        highScore.text = "Best Score: " + trial.bestScore;
        slopeName.text = slopes[currentSlopeIndex].slopeName;
        trialType.text = trial.trialType;


    }

    public void SetSlopes(List<GameObject> slopeList)
    {
        foreach(GameObject go in slopeList)
        {
            slopes.Add(go.GetComponent<Slope>());
            slopeStartPositions.Add(slopes[slopes.Count-1].startSign.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gm.gameState == "map")
        {
            trialReticle.enabled = false;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask mask = LayerMask.GetMask("SlopeCollider");
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.gameObject.tag == "SlopeCollider")
                    if(GetSlopeIndex(hit.collider.gameObject.GetComponent<Slope>()) != currentSlopeIndex && GetSlopeIndex(hit.collider.gameObject.GetComponent<Slope>()) >= 0)
                    {
                        SetSlopeCanvas(GetSlopeIndex(hit.collider.gameObject.GetComponent<Slope>()));
                    }
            }
                
        }
        else if(gm.gameState == "default")
            switch (trialState)
            {
                //no active trial. look for nearby slope signs
                case "idle":
                    DetectTargetedSlope();
                    break;

                //approaching and looking at a slope start. Display slope trial info and distance to start
                case "approach":
                
                    trialReticle.transform.Rotate(new Vector3(0, 0, reticleRotationSpeed * Time.deltaTime));
                    trialReticle.transform.position = cam.WorldToScreenPoint(slopeStartPositions[currentSlopeIndex] + Vector3.up * 6);
                    float dist = Vector3.Distance(player.transform.position, slopeStartPositions[currentSlopeIndex] + Vector3.up * 5);
                    if (dist > startDistance)
                    {
                        statusText.text = dist.ToString("F1") + "m";
                    }
                    else
                    {
                        statusText.text = "Press R to start";
                        if (Input.GetButtonDown("Interact"))
                        {
                            StartTrial();
                        }
                    }
                    DetectTargetedSlope();
                    break;

                case "active":
                    CheckAbandonTrial();

                    switch (trialType.text)
                    {
                        case "Slalom":
                            statusText.text = (float.Parse(statusText.text) + Time.deltaTime).ToString("F2");

                            //enable next gate screen
                            SplineSample ss = new SplineSample();
                            slopes[currentSlopeIndex].spline.Project(player.transform.position, ref ss);
                            float playerD = (float)ss.percent;

                            if(playerD == 1.0f)
                            {
                                EndTrial();
                                return;
                            }
                            List<float> flagTravelAmounts = slopes[currentSlopeIndex].GetComponent<Trial>().flagTravelAmounts;
                            for (int i = 0; i < flagTravelAmounts.Count; i++)
                            {
                                if (flagTravelAmounts[i] < playerD)
                                    slopes[currentSlopeIndex].GetComponent<Trial>().slalomFlags[i].GetComponent<SlalomScript>().gateScreen.GetComponent<Renderer>().enabled = false;
                                else
                                {
                                    slopes[currentSlopeIndex].GetComponent<Trial>().slalomFlags[i].GetComponent<SlalomScript>().gateScreen.GetComponent<Renderer>().enabled = true;
                                    break;
                                }
                                
                            }
                            break;
                        case "Trick Park":
                            break;
                        default:
                            break;
                    }
                    break;

                case "endTrial":
                    break;

                default:
                    break;

            }
    }

    //if player is far from current slope, display warning text
    //countdown from 5, if they don't return, abandon trial
    public void CheckAbandonTrial()
    {
        SplineSample ss = new SplineSample();
        slopes[currentSlopeIndex].spline.Project(player.transform.position, ref ss);

        if(Vector3.Distance(player.transform.position, ss.position) > 50)
        {
            if (warningText.enabled == false) //start warning
            {
                warningText.enabled = true;
                warningTimeText.enabled = true;
                warningText.text = "Return to trial!";
                warningTimeText.text = "5.00";
            }
            else if(float.Parse(warningTimeText.text) >= 0) //continue warning countdown
            {
                warningTimeText.text = (float.Parse(warningTimeText.text) - Time.deltaTime).ToString("F2");
            }
            else //abandon run
            {
                AbandonTrial();
            }
        }
        else //end warning if returned to slope
        {
            warningText.enabled = false;
            warningTimeText.enabled = false;
        }
    }

    //hides warning text and sets state to idle
    public void AbandonTrial()
    {
        warningText.enabled = false;
        warningTimeText.enabled = false;
        trialState = "idle";

        //turn off all slalom gate screens
        if(trialType.text == "Slalom")
        {
            foreach(GameObject go in slopes[currentSlopeIndex].GetComponent<Trial>().slalomFlags)
            {
                go.GetComponent<SlalomScript>().gateScreen.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void EndTrial()
    {
        trialState = "endTrial";
        //set highscore
        UpdateHighScore();
        
        StartCoroutine(UIIdleAfterSeconds(5));
    }

    void UpdateHighScore()
    {
        Trial trial = slopes[currentSlopeIndex].GetComponent<Trial>();
        trial.bestScore = statusText.text;

        //get number of previous completions
        int prevCompletions = 0;
        foreach(bool b in trial.trialCompletions)
            if(b)
                prevCompletions++;

        int currentCompletions = 0;

        switch (trialType.text)
        {
            //for slalom lower scores are better
            case "Slalom":
                for(int i = 0; i < 3; i++)
                {
                    if (float.Parse(trial.bestScore) <= trial.scoreGoals[i])
                    {
                        trial.trialCompletions[i] = true;
                        currentCompletions++;
                    }
                    else
                        trial.trialCompletions[i] = false;
                }
                break;
            //otherwise higher scores are better
            default:
                for (int i = 0; i < 3; i++)
                {
                    if (float.Parse(trial.bestScore) >= trial.scoreGoals[i])
                    {
                        trial.trialCompletions[i] = true;
                        currentCompletions++;
                    }                        
                    else
                        trial.trialCompletions[i] = false;
                }
                break;
        }

        UpdateUIFromTrial();

        if (currentCompletions - prevCompletions > 0)
            StartCoroutine(AddPlayerPoints(currentCompletions - prevCompletions));
    }

    public IEnumerator AddPlayerPoints(int points)
    {
        //display text
        addPointsText.text = "+" + points;
        addPointsText.enabled = true;
        //wait 1 sec
        for(float i = 0; i < 1f; i += Time.deltaTime)
            yield return null;
        //fade and lift
        for(float t = 0; t < 0.3f; t+= Time.deltaTime)
        {
            while (addPointsText.color.a > 0.0f)
            {
                addPointsText.color = new Color(addPointsText.color.r, addPointsText.color.g, addPointsText.color.b, addPointsText.color.a - (Time.deltaTime / .3f));
                yield return null;
            }
        }
        //add points to playerinfo
        player.GetComponent<PlayerInfo>().AddBucks(points);
        //reset and hide text
        addPointsText.enabled = false;
        addPointsText.color = new Color(addPointsText.color.r, addPointsText.color.g, addPointsText.color.b, 1);
    }

    IEnumerator UIIdleAfterSeconds(float t)
    {
        yield return new WaitForSeconds(t);
        trialState = "idle";
    }
    
    public void NotifyMissedGate(SlalomScript s)
    {
        if (currentSlopeIndex == -1)//ignore if no active slope
            return;

        if(s.transform.parent == slopes[currentSlopeIndex].transform)
        {
            statusText.text = (float.Parse(statusText.text) + slalomMissedGatePenalty).ToString("F2");
            notificationText.color = Color.red;
            notificationText.fontSize = 450;
            notificationText.text = "+" + slalomMissedGatePenalty.ToString("F2");
            StartCoroutine(FadeTextToZeroAlpha(.5f, notificationText));
        }
    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    void StartTrial()
    {
        trialReticle.enabled = false;
        switch (trialType.text)
        {
            case "Slalom":
                statusText.text = "0.00";
                break;
            case "Trick Park":
                break;
            default:
                break;
        }
        trialState = "active";
    }

    void DetectTargetedSlope()
    {
        float minCamAngle = cuttoffAngle;
        int targetSlopeIndex = -1;

        for (int i = 0; i < slopes.Count; i++)
        {
            if (Vector3.Distance(cam.transform.position, slopeStartPositions[i]) < cuttoffDistance)
            {
                float angle = Vector3.Angle(slopeStartPositions[i] - cam.transform.position, cam.transform.forward);

                if (angle < minCamAngle)
                {
                    minCamAngle = angle;
                    targetSlopeIndex = i;
                }
            }
        }

        SetSlopeCanvas(targetSlopeIndex);
    }
    
    int GetSlopeIndex(Slope slopeToFind)
    {
        for(int i = 0; i < slopes.Count; ++i)
        {
            if(slopes[i].slopeName == slopeToFind.slopeName) return i;
        }
        return -1;
    }

    void SetSlopeCanvas(int index)
    {
        //slope already targetted
        if (index == currentSlopeIndex)
            return;           
        //no slope targetted
        if (index == -1)
        {
            trialCanvas.GetComponent<Canvas>().enabled = false;
            currentSlopeIndex = -1;
            trialReticle.GetComponent<Canvas>().enabled = false;
            trialState = "idle";
        }
        //new slope target -> populate UI
        else
        {
            trialCanvas.GetComponent<Canvas>().enabled = true;
            currentSlopeIndex = index;
            if (gm.gameState == "default") //don't do these in map mode
            {
                trialReticle.GetComponent<Canvas>().enabled = true;
                notificationText.color = new Color(0, 0, 0, 0);
                trialState = "approach";
            }
                           
            UpdateUIFromTrial();

            StartCoroutine(MoveUIOnscreen());

            
        }
            
    }

    //moves the UI on screen from thr right
    IEnumerator MoveUIOnscreen()
    {
        trialCanvas.GetComponent<RectTransform>().localPosition = trialPanelHomePosition + new Vector3(200, 0,0);

        while(trialCanvas.GetComponent<RectTransform>().localPosition.x - trialPanelHomePosition.x > 0)
        {
            trialCanvas.GetComponent<RectTransform>().localPosition -= new Vector3(200 * 4 * Time.deltaTime, 0,0);
            yield return null;
        }
    }
}
