using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    //gravity stuff
    public bool hasGravity = true;
    public float gravity = 10;
    public float fallSpeed;
    public float maxFallSpeed = 10;

    //movement stuff
    public float speed = 6f;
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    public float turnSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        fallSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //direction
        float targetAngle;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        transform.Rotate(new Vector3(0, horizontal * (360 / turnSpeed) * Time.deltaTime, 0));

        controller.Move(transform.forward * speed * Time.deltaTime * vertical);


        //fall if not on ground
        if (hasGravity && !controller.isGrounded && fallSpeed < maxFallSpeed)
        {
            fallSpeed += gravity * Time.deltaTime;
        }
        else
        {
            fallSpeed = 0;
        }

        controller.Move(new Vector3(0, -fallSpeed, 0));
    }
}
