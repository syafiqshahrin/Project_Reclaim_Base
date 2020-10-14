using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectileData : MonoBehaviour
{
    public Vector3 Velocity;
    public Vector3 TargetDirectionVector;
    public Vector3 TargetVector;
    public Vector3 AwayVector;
    public Vector3 SpawnOffset;

    public bool IsFired;
    public bool TargetHit;

    private void Start()
    {
        IsFired = false;
        Velocity = Vector3.zero;
        TargetDirectionVector = Vector3.zero;
        TargetVector = Vector3.zero;
        AwayVector = Vector3.zero;
    }
   


}
