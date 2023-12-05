using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public int powBucks = 0;

    public PowBucks pbText;

    public int GetBucks()
    {
        return powBucks;
    }

    public void AddBucks(int toAdd)
    {
        powBucks += toAdd;
        pbText.SetBucks(powBucks);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
