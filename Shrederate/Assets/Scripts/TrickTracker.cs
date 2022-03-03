using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrickTracker : MonoBehaviour
{
    public GameObject player;
    public GameObject trickPrefab;
    public GameObject uiCanvas;

    public float uprightLandingThreshold = 0.6f;
    public float trickUILinger = 0.75f; // how long a trcik stay son the ui before dissapearing

    List<GameObject> trickPrefabs;
    List<TrickData> trickHistory;

    private TrickVoiceHandler voice;

    public class TrickData
    {
        public float startTime;
        public Transform startTransform;
        public bool isComplete;
        public bool stuckLanding;
        public float endTime;
        public Transform endTransform;
        public int prefabID;
        public bool soundPlayed;

        public TrickData(Transform initTransform, int fabID)
        {
            startTime = Time.time;
            startTransform = initTransform;
            prefabID = fabID;
            isComplete = false;
            soundPlayed = false;
        }

        public void EndTrick(bool isUpright)
        {
            endTime = Time.time;
            isComplete = true;
            stuckLanding = isUpright;
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onAirtimeStart += OnAirtime;
        GameEvents.current.onLand += OnLanding;

        trickPrefabs = new List<GameObject>();
        trickHistory = new List<TrickData>();

        voice = GetComponent<TrickVoiceHandler>();

    }

    private void OnAirtime() {
        
        GameObject newTrick = Instantiate(trickPrefab, new Vector3(160,0,0), Quaternion.identity);
        newTrick.transform.SetParent(uiCanvas.transform, false);

        trickPrefabs.Add(newTrick);
        trickHistory.Add(new TrickData(player.transform, newTrick.GetInstanceID()));

        // shoudl call update function that bumps all old ones up a little bit
    }

    private void OnLanding(float totalJumpTime) {
        // Debug.Log("frog clogs");
        if (trickHistory.Count <= 0) {
            return;
        } 

        // check if player stuck the landing --  are they upright?
        // TODO: make this take the slope of the ground into account
        bool isUpright = player.transform.up.y > uprightLandingThreshold;
        if (isUpright) 
        {
            trickHistory[trickHistory.Count - 1].EndTrick(true);
        } else 
        {
            trickHistory[trickHistory.Count - 1].EndTrick(false);
            GameObject finishedTrick = trickPrefabs[trickHistory.Count - 1];
            Text trickText = finishedTrick.transform.GetChild(0).gameObject.GetComponent<Text>();;
            trickText.text = "ayo you biffed";
            Text trickValue = finishedTrick.transform.GetChild(1).gameObject.GetComponent<Text>();;
            trickValue.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
         // check if trick is expired then use Destroy(gameObj)  
        foreach (GameObject fab in trickPrefabs) 
        {
            // find matching data for prefab
            TrickData trick = trickHistory.Find(x => x.prefabID == fab.GetInstanceID());

            if (trick == null) 
            {
                Destroy(fab); // don't need any no-good, no-data-having trick in these parts
            }

            // update values for exisitng tricks
            if (!trick.isComplete) 
            {
                // updates the ui text for the trick
                Text trickText = fab.transform.GetChild(0).gameObject.GetComponent<Text>();
                Text trickValueText = fab.transform.GetChild(1).gameObject.GetComponent<Text>();
                float currentAirtime = Time.time - trick.startTime;
                trickValueText.text = currentAirtime.ToString("F1") + "s";
                if (currentAirtime > 2.0f && currentAirtime < 5.00f) 
                {
                    trickText.text = "Nice Air";
                    // play sound too?
                } 
                else if (currentAirtime > 5.0f && currentAirtime < 10.0f) 
                {
                    trickText.text = "Mega Air";
                } 
                else if (currentAirtime > 10.0f && currentAirtime < 20.0f) 
                {
                    trickText.text = "Giga Air";
                } 
                else if (currentAirtime > 20.0f) 
                {
                    trickText.text = "Ultra Air";
                } 
            }
            // remove 'old tricks'
            if (trick.isComplete && (Time.time > (trick.endTime + trickUILinger)))
            {
                // trick has been dispalyed for long enough, plz remove
                Destroy(fab);

                // was trick good?
                if (trick.endTime - trick.startTime > 2.0f && trick.stuckLanding) 
                {
                    // play random motivational sound line (if we haven't already)
                    if (!trick.soundPlayed) 
                    {
                        voice.PlayRandomVoiceLine();
                        trick.soundPlayed = true;
                    }
                    
                }
                
            }
        }
    }

    private void OnDestroy()
    {
        GameEvents.current.onAirtimeStart -= OnAirtime;
        GameEvents.current.onLand -= OnLanding;
    }
}
