using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //public CharacterController controller;
    public Rigidbody rb;
    public CapsuleCollider col;

    //gravity stuff
    public bool isGrounded = false;
    public bool hasGravity = true;
    public float gravity = 10;

    //movement stuff
    private Vector3 velocity;
    public float speed = .05f;
    public float maxSpeed = 20f;
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    public float turnSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;
        //controller = gameObject.GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized;
            rb.velocity *= maxSpeed;
        }
        //direction
        float targetAngle;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        transform.Rotate(new Vector3(0, horizontal * (360 / turnSpeed) * Time.deltaTime, 0));
        
        
        checkGrounded();

        //movement
        if (isGrounded)
        {
            rb.AddForce(new Vector3(transform.forward.x, 0, transform.forward.z) * vertical * speed, ForceMode.Acceleration);

            RaycastHit hit;
            Physics.Raycast(col.center, Vector3.down, out hit, Mathf.Infinity);

            float dragForce = 1-Vector3.Dot(rb.velocity, transform.right);
            rb.AddForce(transform.right * dragForce * rb.velocity.magnitude);
        }

        Debug.DrawRay(transform.position, rb.velocity, Color.green, 0.1f);
        Debug.DrawRay(transform.position, transform.right, Color.blue, 0.1f);
        
    }

    private void checkGrounded()
    {
        RaycastHit hit;
        Physics.Raycast(col.transform.position, Vector3.down, out hit, col.height + .1f);

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
