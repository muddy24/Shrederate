using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickTracker : MonoBehaviour
{
    private float trickStartTime;
    private float trickStartRotation;
    public GameObject player;

    public float uprightLandingThreshold = 0.6f;

    
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onAirtimeStart += OnAirtime;
        GameEvents.current.onLand += OnLanding;

    }

    private void OnAirtime() {
        trickStartTime = Time.time;
    }

    private void OnLanding(float totalJumpTime) {
        // Debug.Log("frog clogs");
        // check if play stuck the landing --  are they upright?
        bool isUpright = player.transform.up.y > uprightLandingThreshold;
        if (isUpright) 
        {
            Debug.Log("ayo you did it boss");
        } else 
        {
            Debug.Log("bro you DID NOT stick it");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        GameEvents.current.onAirtimeStart -= OnAirtime;
        GameEvents.current.onLand -= OnLanding;
    }
}
