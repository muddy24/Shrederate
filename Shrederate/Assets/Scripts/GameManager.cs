using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public string gameState = "default";
    PlayerMovement player;
    public CinemachineFreeLook playerCam;
    public Camera sceneCam;
    public GameObject mountain;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //state machine
        switch (gameState)
        {
            case "map":
                if (Input.GetButtonDown("Map"))
                {
                    SetState("default");
                }
                //spawn at new location
                if (Input.GetMouseButtonDown(0))
                {
                    /*
                    if (sceneCam.GetComponent<SceneCam>().reticle.activeSelf)
                    {
                        transform.position = sceneCam.GetComponent<SceneCam>().reticle.transform.position + Vector3.up * 30f;
                        player.rb.transform.position = transform.position;
                        player.rb.velocity = Vector3.zero;
                    }
                    SetState("default");*/

                    mountain.GetComponent<Mountain>().CreateSlopesAt(sceneCam.GetComponent<SceneCam>().reticle.transform.position);
                    
                }
                break;

            case "cannonLoading":
                //
                break;

            case "cannonLaunching":
                //
                break;

            case "default":
                if (Input.GetButtonDown("Map"))
                {
                    SetState("map");
                    sceneCam.GetComponent<SceneCam>().SnapToMainCam();
                    sceneCam.GetComponent<SceneCam>().SetTarget(mountain.GetComponent<Mountain>().camTarget, mountain.GetComponent<Mountain>().camPosition.transform.position);                  
                }
                break;
        }
    }

    public void SetState(string s)
    {
        gameState = s;

        if(s == "default")
        {
            player.moveEnabled = true;
            sceneCam.enabled = false;
            playerCam.enabled = true;
            mountain.GetComponent<Mountain>().HideSlopes();
        }
        if(s == "map")
        {
            player.moveEnabled = false;
            sceneCam.enabled = true;
            sceneCam.GetComponent<SceneCam>().rotationEnabled = true;
            playerCam.enabled = false;
            sceneCam.GetComponent<SceneCam>().SetTarget(mountain.GetComponent<Mountain>().camTarget, mountain.GetComponent<Mountain>().camPosition.transform.position);
            mountain.GetComponent<Mountain>().ShowSlopes();
        }
        if(s == "cannonLoading")
        {
            sceneCam.GetComponent<SceneCam>().rotationEnabled = true;
            player.moveEnabled = false;
            sceneCam.enabled = true;
            playerCam.enabled = false;
        }
    }
    
    public void SetSceneCamTarget(GameObject t, Vector3 tPos)
    {
        sceneCam.GetComponent<SceneCam>().SetTarget(t, tPos);
    }
}
