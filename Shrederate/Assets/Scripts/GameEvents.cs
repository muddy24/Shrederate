using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
   public static GameEvents current;
   public static int lapCount;

   private void Awake()
   {
       current = this;
       lapCount = 0;
   }

   public event Action<int> onLapStart;
   public void LapStart()
   {
       lapCount++;
       if (onLapStart != null)
       {
           onLapStart(lapCount);
       }
   }

}
