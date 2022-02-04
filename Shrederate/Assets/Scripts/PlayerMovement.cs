using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public SphereCollider col;
    public Animator anim;
    public GameObject graphics;

    //gravity stuff
    public bool isGrounded = false;
    public float gravity = 10;

    //movement stuff
    private float verticalInput, horizontalInput;
    public float maxSpeed = 20f;
    public float turnSmoothTime = 0.3f;
    public float turnTargetAngle = 0;
    public float currentTurnVelocity;
    public float turnStrength = 1;

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
        if(verticalInput > 0)
        {
            rb.AddForce(transform.forward * verticalInput * 10);
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

        checkGrounded();

        //movement
        if (isGrounded)
        {
            //face direction
            RaycastHit hit;
            Physics.Raycast(col.transform.position, Vector3.down, out hit, Mathf.Infinity);
            transform.up = GetMeshColliderNormal(hit);
            turnTargetAngle = Mathf.SmoothDampAngle(turnTargetAngle, 90 * horizontalInput, ref currentTurnVelocity, turnSmoothTime);
            transform.Rotate(new Vector3(0, Vector3.SignedAngle(transform.forward, rb.velocity, transform.up) + turnTargetAngle, 0));

            //animation
            anim.SetFloat("Speed", rb.velocity.magnitude/maxSpeed);
            anim.SetFloat("Turn", turnTargetAngle/90);

            //go to rigidbody contact with ground
            transform.position = rb.transform.position - (GetMeshColliderNormal(hit) * col.radius);
      
        }
        else
        {
            //follow rigidbody
            transform.position = rb.transform.position + Vector3.down * col.radius;


        }

    }

    private void checkGrounded()
    {
        RaycastHit hit;
        Physics.Raycast(col.transform.position, Vector3.down, out hit, col.radius + .1f);

        if(hit.collider != null)
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
