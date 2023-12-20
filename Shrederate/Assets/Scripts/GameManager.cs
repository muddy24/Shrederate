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
    public Camera mainCam;
    public GameObject mountain;
    public Canvas pauseCanvas;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.GetComponent<PlayerMovement>();
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (gameState == "paused")
                ResumeGame();
            else
                PauseGame();
        }

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

                    //mountain.GetComponent<Mountain>().CreateSlopesAt(sceneCam.GetComponent<SceneCam>().reticle.transform.position);
                    
                }
                break;

            case "cannonLoading":
                //
                break;

            case "cannonLaunching":
                //
                break;

            case "paused":
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

    void ResumeGame()
    {
        Time.timeScale = 1.0f;
        gameState = "default";
        pauseCanvas.enabled = false;
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        gameState = "paused";
        pauseCanvas.enabled = true;
    }

    public void SetState(string s)
    {
        gameState = s;

        if(s == "default")
        {
            player.moveEnabled = true;
            sceneCam.enabled = false; 
            playerCam.enabled = true;
            sceneCam.tag = "Untagged";
            mainCam.tag = "MainCamera";
            mountain.GetComponent<Mountain>().HideSlopes();
            RenderSettings.fog = true;
            //set trees to use LOD normally
            foreach (GameObject tree in mountain.GetComponent<Mountain>().trees)
                tree.GetComponent<LODGroup>().ForceLOD(-1);
        }
        if(s == "map")
        {
            player.moveEnabled = false;
            sceneCam.enabled = true;
            sceneCam.GetComponent<SceneCam>().rotationEnabled = true;
            playerCam.enabled = false;
            sceneCam.tag = "MainCamera";
            mainCam.tag = "Untagged";
            sceneCam.GetComponent<SceneCam>().SetTarget(mountain.GetComponent<Mountain>().camTarget, mountain.GetComponent<Mountain>().camPosition.transform.position);
            mountain.GetComponent<Mountain>().ShowSlopes();
            RenderSettings.fog = false;
            //set trees to chunky green ones
            foreach (GameObject tree in mountain.GetComponent<Mountain>().trees)
                tree.GetComponent<LODGroup>().ForceLOD(3);
        }
        if(s == "cannonLoading")
        {
            sceneCam.GetComponent<SceneCam>().rotationEnabled = true;
            player.moveEnabled = false;
            sceneCam.enabled = true;
            playerCam.enabled = false;
            RenderSettings.fog = true;
        }
    }
    
    public void SetSceneCamTarget(GameObject t, Vector3 tPos)
    {
        sceneCam.GetComponent<SceneCam>().SetTarget(t, tPos);
    }
}
