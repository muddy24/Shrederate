using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoRotate : MonoBehaviour
{
    
    private Image logo;
    private GameObject logoObject;
    private GameObject player;
    public float xAngle, yAngle, zAngle;

    public Sprite logoMain;
    public Sprite logoBack;

    private bool mainLogoOn;

    // Start is called before the first frame update
    void Start()
    {
        mainLogoOn = true;
        logo = GetComponent<Image>();
        logoObject = GameObject.Find("Logo");
        player = GameObject.Find("Player");
    }
    // Update is called once per frame
    void Update()
    {
        logoObject.transform.Rotate(xAngle, yAngle, zAngle, Space.Self);  
        // Debug.Log(logoObject.transform.localRotation.eulerAngles.y);
        float currentAngle = logoObject.transform.localRotation.eulerAngles.y; 
        // if (currentAngle - Mathf.Floor(currentAngle) == 0) {
        //     Debug.Log("we done did it");
        //     Debug.Log(currentAngle);
        // } 
        if (
            Mathf.Approximately(Mathf.Floor(currentAngle * 10.0f), 900.0f)
            || Mathf.Approximately(Mathf.Floor(currentAngle * 10.0f), 2700.0f)
        ) {
            Debug.Log("HELL YEA");
            if (mainLogoOn) {
                logo.sprite = logoBack;
                mainLogoOn = false;
            } else { 
                logo.sprite = logoMain;
                mainLogoOn = true;
            }
        }

    }
}
