using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    PlayerMovement pm;
    public ParticleSystem windTrails;
    public float maxWindTrailEmission = 50;
    public float windTrailCuttoff = 0.75f; //ratio of max speed when wind trails kick in. b/t 0 and 1

    // Start is called before the first frame update
    void Start()
    {
        windTrails.Stop();
        pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        var windTrailEmission = windTrails.emission;

        if(pm.rb.velocity.magnitude < pm.maxSpeed * windTrailCuttoff)
        {
            windTrails.Stop();
        }
        else
        {
            float lerpPoint = Mathf.InverseLerp(windTrailCuttoff, 1, pm.rb.velocity.magnitude / pm.maxSpeed);
            windTrailEmission.rateOverTime = Mathf.Lerp(0, maxWindTrailEmission, lerpPoint);
            windTrails.Play();
        }
    }
}
