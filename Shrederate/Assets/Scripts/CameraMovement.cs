using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    GameObject player;

    public float cameraDistance = 10;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocityDirection = player.GetComponent<Rigidbody>().velocity.normalized;

        if(velocityDirection.magnitude > 0.01f)
        {
            transform.position = player.transform.position - (velocityDirection * cameraDistance);
            transform.forward = (player.transform.position - transform.position);
        }
    }
}
