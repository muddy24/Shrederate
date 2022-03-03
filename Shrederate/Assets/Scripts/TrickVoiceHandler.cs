using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickVoiceHandler : MonoBehaviour
{

    private AudioSource[] voiceSounds;
    private float timeLastSound = 0.0f;
    public float timeToWaitBetweenPlays = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        voiceSounds = GetComponents<AudioSource>();

    }

    public void PlayRandomVoiceLine()
    {   
        if (Time.time - timeLastSound > timeToWaitBetweenPlays) 
        {
            voiceSounds[Random.Range(0, voiceSounds.Length)].Play();
            timeLastSound = Time.time;
        }
        
    }
}
