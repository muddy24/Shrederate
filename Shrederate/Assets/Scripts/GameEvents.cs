using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
   public static GameEvents current;
   public static int lapCount;
   public static float jumpStartTime;

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

   public event Action onAirtimeStart;
   public void AirtimeStart()
   {
       jumpStartTime = Time.time;
       if (onAirtimeStart != null) {
           onAirtimeStart();
       }
   }

   public event Action<float> onLand;
   public void Land()
   {
       float totalJumpTime = Time.time - jumpStartTime;
       
       if (onLand != null) {
           onLand(totalJumpTime);
       }
   }
}
