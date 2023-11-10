using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCam : MonoBehaviour
{
    public Vector3 mountainHomePos;
    public Vector3 initialTargetPosition;
    public GameObject target;
    public bool rotationEnabled = true;
    public bool reachedTargetPos = false;
    public float camMoveSpeed = 5f;
    public float camRotateSpeed = 5f;
    Camera cam;
    Camera mainCam;
    float inputH = 0;
    float rotateSpeed = 90f;
    public GameObject reticle;

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetComponent<Camera>();
        transform.position = mountainHomePos;
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.enabled)
        {
            transform.LookAt(target.transform);

            //move cam towards target
            if(!reachedTargetPos)
            {
                //TODO:Get camera rotation smoothing working properly
                //Quaternion lookRotation = Quaternion.LookRotation(target.transform.position, initialTargetPosition);
                //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, camRotateSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, initialTargetPosition, camMoveSpeed * Time.deltaTime);

                if(Vector3.Distance(transform.position, initialTargetPosition) < .5f)
                    reachedTargetPos = true;
            }

            else
            {
                //allow rotating cam view around target
                if (rotationEnabled)
                {
                    inputH = Input.GetAxis("Horizontal");
                    transform.RotateAround(target.transform.position, Vector3.up, inputH * rotateSpeed * Time.deltaTime);
                }               
            }
            


            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int mask = 1 << 2;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~mask))
            {
                reticle.SetActive(true);
                reticle.transform.position = hit.point;
            }
            else
                reticle.SetActive(false);
        }

        else
        {
            reticle.SetActive(false);
        }
    }

    //sets target and offsets
    public void SetTarget(GameObject t, Vector3 tPos)
    {
        reachedTargetPos = false;
        target = t;
        initialTargetPosition = tPos;
    }

    public void SnapToMainCam()
    {
        transform.position = mainCam.transform.position;
        transform.rotation = mainCam.transform.rotation;
    }

    //goes to target camera position
    public void SnapToTarget()
    {
        transform.position = initialTargetPosition;
        transform.LookAt(target.transform);
    }
}
