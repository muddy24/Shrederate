using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowBucks : MonoBehaviour
{
    private Text currentText;

    // Start is called before the first frame update
    void Start()
    {
        currentText = gameObject.GetComponent<Text>();
    }

    void Update()
    {

    }

    public void SetBucks(int buckAmount)
    {
        currentText.text = "$ " + buckAmount.ToString();
    }
}
