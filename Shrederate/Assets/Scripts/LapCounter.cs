using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapCounter : MonoBehaviour
{
    private Text currentText;
    
    // Start is called before the first frame update
    void Start()
    {
        currentText = gameObject.GetComponent<Text>();
        GameEvents.current.onLapStart += OnLapStart;
    }

    private void OnLapStart(int lap)
    {
        currentText.text = "Lap Number " + lap;
    }
}
