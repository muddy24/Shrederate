using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCam : MonoBehaviour
{
    public Vector3 mountainHomePos;
    public GameObject target;
    Camera cam;
    float inputH = 0;
    float rotateSpeed = 90f;
    public GameObject reticle;

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetComponent<Camera>();
        transform.position = mountainHomePos;    
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.enabled)
        {
            transform.LookAt(target.transform);
            inputH = Input.GetAxis("Horizontal");
            transform.RotateAround(target.transform.position, Vector3.up, inputH * rotateSpeed * Time.deltaTime);


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
}
