using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GauntletMode
{
    Homing,
    Charge,
    Shield
}
public class GauntletController : MonoBehaviour
{
    //Manages Gauntlet States
    //Change between Gauntlet Modes: Charge Ballista Shot and Homing Projectile
    //
    HomingGauntletMode HomingMode;
    ChargeGauntletMode ChargeMode;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
