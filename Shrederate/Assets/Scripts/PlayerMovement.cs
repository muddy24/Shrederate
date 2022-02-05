using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public SphereCollider col;
    public Animator anim;
    public GameObject graphics;
    public GameObject airPivot;
    public CinemachineFreeLook cam;

    //gravity stuff
    public bool isGrounded = false;
    public float gravity = 10;

    //input
    private float verticalInput, horizontalInput;
    public bool crouchInput = false;

    //movement stuff
    public float skateSpeedMax = 5;
    public float maxSpeed = 20f;
    public float turnSmoothTime = 0.3f;
    public float turnTargetAngle = 0;
    public float currentTurnVelocity;
    public float turnStrength = 1;
    public float forwardStrength = 3;
    public float jumpForce = 10;
    public float turnReductionAtFullForward = 0.5f; //how much harder it is to turn when pushing fwd. 1 means you can't turn, 0 mean there's no impact

    //head look
    public float headLookH = 0;
    public float headLookV = 0.5f;

    //air control
    public float airControl = 1;
    public float airRotateTime = 1;
    public float airRotateAngleV = 0;
    public float airRotateAngleH = 0;
    float currentAirRotateV;
    float currentAirRotateH;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        col = GetComponentInChildren<SphereCollider>();
        anim = GetComponentInChildren<Animator>();

        rb.transform.parent = null;
    }

    private void FixedUpdate()
    {
        if (!isGrounded)
            return;

        if(verticalInput > 0)
        {
            rb.AddForce(transform.forward * verticalInput * forwardStrength);
        }
        if(horizontalInput != 0)
        {
            rb.AddForce(transform.right * turnTargetAngle * rb.velocity.magnitude * turnStrength * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //get input
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
        crouchInput = Input.GetButton("Jump");


        checkGrounded();
        anim.SetBool("On Ground", isGrounded);

        //movement
        if (isGrounded)
        {
            //if you're on the ground and stop crouching, jump
            if(anim.GetBool("Crouch") && !crouchInput)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
            
            airRotateAngleH = 0;
            airRotateAngleV = 0;

            //face direction
            RaycastHit hit;
            Physics.Raycast(col.transform.position, Vector3.down, out hit, Mathf.Infinity);
            transform.up = GetMeshColliderNormal(hit);
            turnTargetAngle = Mathf.SmoothDampAngle(turnTargetAngle, (90 * horizontalInput) * (1- verticalInput*turnReductionAtFullForward), ref currentTurnVelocity, turnSmoothTime);
            transform.Rotate(new Vector3(0, Vector3.SignedAngle(transform.forward, rb.velocity, transform.up) + turnTargetAngle, 0));

            //animation
            anim.SetFloat("Speed", rb.velocity.magnitude/maxSpeed);
            anim.SetFloat("Turn", turnTargetAngle/90);

            //go to rigidbody contact with ground
            transform.position = rb.transform.position - (GetMeshColliderNormal(hit) * col.radius);

            if (rb.velocity.magnitude < skateSpeedMax && rb.velocity.magnitude > 0.1f)
            {
                anim.SetFloat("Skate Speed", (rb.velocity.magnitude / skateSpeedMax) * verticalInput);
            }
            else
                anim.SetFloat("Skate Speed", 0);

            //head look
            /*headLookH = (transform.eulerAngles.y / 360) *(cam.m_XAxis.Value / 90)+1;
            headLookV = (transform.eulerAngles.x / 360) * cam.m_YAxis.Value;
            anim.SetFloat("Head Horizontal", headLookH);
            anim.SetFloat("Head Vertical", headLookV);*/
      
        }
        else
        {
            headLookH = 0;
            headLookV = 0;

            //if spacebar is pressed flip faster
            int flipSpeedMult = 1;
            if (crouchInput)
            {
                flipSpeedMult = 2;
            }

            //follow rigidbody
            transform.position = rb.transform.position + Vector3.down * col.radius;

            airRotateAngleH = Mathf.SmoothDamp(airRotateAngleH, horizontalInput * airControl * flipSpeedMult, ref currentAirRotateH, airRotateTime);
            airRotateAngleV = Mathf.SmoothDamp(airRotateAngleV, verticalInput * airControl * flipSpeedMult, ref currentAirRotateV, airRotateTime);

            transform.RotateAround(airPivot.transform.position, transform.right, airRotateAngleV);
            transform.RotateAround(airPivot.transform.position, transform.up, airRotateAngleH);

            

            //animate flipsssss
            anim.SetFloat("Turn", airRotateAngleH / airControl);
            anim.SetFloat("Spin Vertical", airRotateAngleV / airControl);

        }

        //this makes it so you can crouch in the air or on the ground, idk if that's what we want
        anim.SetBool("Crouch", crouchInput);

    }

    private void checkGrounded()
    {
        //cast down to find approximate ground normal under self
        RaycastHit vertHit;
        Physics.Raycast(col.transform.position, Vector3.down, out vertHit, Mathf.Infinity);

        //cast in normal direction to detect ground 
        RaycastHit normalHit;
        Physics.Raycast(col.transform.position, -1 *  GetMeshColliderNormal(vertHit), out normalHit, col.radius + .2f);

        if(normalHit.collider != null)
        {
            isGrounded = true;
        }
        else isGrounded = false;      
    }

    private Vector3 GetMeshColliderNormal(RaycastHit hit)
    {
        //get mesh data
        MeshCollider col = (MeshCollider)hit.collider;
        Mesh m = col.sharedMesh;         
        

        Vector3[] normals = m.normals;
        int[] triangles = m.triangles;

        //get mesh normal at hit point
        Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
        Vector3 baryCenter = hit.barycentricCoordinate;
        Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
        interpolatedNormal.Normalize();
        interpolatedNormal = hit.transform.TransformDirection(interpolatedNormal);

        return interpolatedNormal;
    }

    private Vector3 GetMeshColliderSlope (RaycastHit hit)
    {
        //get normal
        Vector3 normal = GetMeshColliderSlope(hit);

        //get slope from normal
        return new Vector3(normal.x, -1 * (1 - normal.y), normal.z);

    }
}
