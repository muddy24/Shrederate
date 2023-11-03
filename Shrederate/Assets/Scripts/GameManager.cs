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
                    if (sceneCam.GetComponent<SceneCam>().reticle.activeSelf)
                    {
                        transform.position = sceneCam.GetComponent<SceneCam>().reticle.transform.position + Vector3.up * 30f;
                        player.rb.transform.position = transform.position;
                        player.rb.velocity = Vector3.zero;
                    }
                    SetState("default");
                    
                }
                break;
            case "default":
                if (Input.GetButtonDown("Map"))
                {
                    SetState("map");
                }
                break;
        }
    }

    private void SetState(string s)
    {
        gameState = s;

        if(s == "default")
        {
            player.moveEnabled = true;
            sceneCam.enabled = false;
            playerCam.enabled = true;
        }
        if(s == "map")
        {
            player.moveEnabled = false;
            sceneCam.enabled = true;
            playerCam.enabled = false;
        }
    }
}
