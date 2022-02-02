using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    PlayerMovement pm;

    public float cameraDistance = 5;

    // Start is called before the first frame update
    void Start()
    {
        pm = transform.parent.gameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocityDirection = pm.rb.velocity.normalized;

        if(velocityDirection.magnitude > 0.01f)
        {
            transform.position = pm.transform.position - (velocityDirection * cameraDistance) + pm.transform.up * cameraDistance/3;
            transform.forward = (pm.transform.position - transform.position);
        }
    }
}
