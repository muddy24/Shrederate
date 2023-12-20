using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlalomScript : MonoBehaviour
{
    public TrialUIManager uiManager;

    public GameObject gateScreen;

    // Start is called before the first frame update
    void Start()
    {
        uiManager = GameObject.FindWithTag("UICanvas").GetComponent<TrialUIManager>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerSphere")
        {
            uiManager.NotifyMissedGate(this);
            //GameObject.FindWithTag("Player").GetComponent<PlayerInfo>().AddBucks(10);
            //other.gameObject.GetComponent<PlayerInfo>().AddBucks(10);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
