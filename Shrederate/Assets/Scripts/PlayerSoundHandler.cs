using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundHandler : MonoBehaviour
{
    private AudioSource[] playerSounds;

    private AudioSource startSound;
    private AudioSource landingSound;
    private AudioSource turnSound;

    // Start is called before the first frame update
    void Start()
    {
        playerSounds = GetComponents<AudioSource>();

        startSound = playerSounds[2];
        landingSound = playerSounds[0];
        turnSound = playerSounds[1];

    }

    public void PlayLanding() {
        landingSound.Play();
    }

    public void PlayTurn() {
        turnSound.Play();
    }
}
