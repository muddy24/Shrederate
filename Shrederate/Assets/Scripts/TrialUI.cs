using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrialUI : MonoBehaviour
{

    public Text slopeName;
    public Text trialType;
    public Text statusText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetTrial(Slope s)
    {
        slopeName.text = s.slopeName;
        trialType.text = s.trial.trialType;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
