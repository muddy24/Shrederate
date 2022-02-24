using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JumpStatsHandler : MonoBehaviour
{
    private Text currentText;
    private float bestJumpTime;
    
    // Start is called before the first frame update
    void Start()
    {
        currentText = gameObject.GetComponent<Text>();
        GameEvents.current.onLand += OnPlayerLand;
        bestJumpTime = 0;
    }

    private void OnPlayerLand(float totalJumpTime)
    {
        if (totalJumpTime > bestJumpTime) 
       {
           bestJumpTime = totalJumpTime;
       }

        //currentText.text = "Sickest Jump? " + (bestJumpTime * 0.1f);
        currentText.text = "last jump was  " + (totalJumpTime);
        // this should be incorporated into a trick tracker somehow
    }

    private void OnDestroy()
    {
        GameEvents.current.onLand -= OnPlayerLand;
    }
}
