using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public GameObject cannonTube;
    public GameObject cannonRing;
    public GameObject chairLift;

    public float cannonRaiseHeight = 15.6f;
    Transform initialPosition;
    public float moveSpeed;
    public float rotateSpeed;

    public GameObject cameraPosition;
    public float activateRadius;
    GameObject player;
    GameManager gm;

    Transform launchTarget;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        gm = player.GetComponent<GameManager>();
        initialPosition = cannonTube.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(gm.gameState == "default")
        {
            //player is in activation radius
            //TODO: Have a UI popup to let the player know they can launch
            if(Vector3.Distance(player.transform.position, transform.position) < activateRadius)
            {
                if (Input.GetButtonDown("Interact"))
                {
                    gm.sceneCam.transform.GetComponent<SceneCam>().SnapToMainCam();
                    gm.SetState("cannonLoading");
                    gm.SetSceneCamTarget(gameObject, cameraPosition.transform.position);
                    StartCoroutine(Fire());
                }
            }
        }
    }

    IEnumerator Fire()
    {
        Vector3 targetPos = cannonTube.transform.position + new Vector3(0, cannonRaiseHeight, 0);

        while(Vector3.Distance(cannonTube.transform.position, targetPos) > 0.1f)
        {
            cannonTube.transform.position = Vector3.MoveTowards(cannonTube.transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        gm.SetState("map");

        for(int i = 0; i < 1000000; i++)
        {
            cannonRing.transform.RotateAround(transform.position, Vector3.up, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
