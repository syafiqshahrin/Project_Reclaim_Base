using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityScript.Scripting.Pipeline;

public class HomingGauntletMode : MonoBehaviour
{
    //Spawn Projectiles
    //Play Reload Animation when mode is switched (animation is bullets appearing one by one and rotating into place)
    //When left click is held projectiles go into primed mode (Primed mode is when the players are about to shoot and acquiring targets)
    //While in primed mode the projectiles offset increases and spins around socket
    //Finds Targets in the player's camera view within a certain distance (targets armor pieces)
    //
    //When left click is released or its reach the max primed duration the bullets will shoot
    [SerializeField] FPController PlayerController;
    [SerializeField] GameObject ProjectileObject;
    [SerializeField] GameObject SpawnSocket;
    [SerializeField] AnimationCurve AwayWeightCurve;
    [SerializeField] AnimationCurve TargetWeightCurve;
    RaycastHit[] AcquiredTargets;
    GameObject[] HomingProjectiles;
    HomingProjectileData[] HomingProjectilesData;
    
    float CurveTimer;
    float ProjectileTimer;
    [SerializeField]float ProjectileSpeed = 5.0f;
    [SerializeField]float MaxProjectileDuration = 2.0f;
    [SerializeField]float TargetWeight;
    [SerializeField]float AwayWeight;
    [SerializeField] float MaxDistance = 20.0f;
    float SpawnOffset = 0.07f;
    float PrimedOffset = 0.1f;
    float CurrentOffset;
    bool Primed;
    bool LeftHeldDown;
    bool Fired;
    int targets;
    [SerializeField] int ProjectileAmount;
    void Start()
    {
        ProjectileTimer = 0.0f;
        AcquiredTargets = new RaycastHit[ProjectileAmount];
        targets = 0;
        CurveTimer = 0.0f;
        LeftHeldDown = false;
        Fired = false;
        Primed = false;
        CurrentOffset = SpawnOffset;
        HomingProjectiles = new GameObject[ProjectileAmount];
        HomingProjectilesData = new HomingProjectileData[ProjectileAmount];
        float angleIncrement = 360.0f / HomingProjectiles.Length;
        angleIncrement *= Mathf.Deg2Rad;
        for(int i = 0; i < HomingProjectiles.Length; i++)
        {
            HomingProjectiles[i] = Instantiate(ProjectileObject, SpawnSocket.transform.position, SpawnSocket.transform.rotation, SpawnSocket.transform);
            HomingProjectilesData[i] = HomingProjectiles[i].GetComponent<HomingProjectileData>();
            HomingProjectilesData[i].SpawnOffset = (new Vector3(Mathf.Cos(angleIncrement * i), Mathf.Sin(angleIncrement * i), 0.0f));
            HomingProjectiles[i].transform.Translate(HomingProjectilesData[i].SpawnOffset * CurrentOffset, Space.Self);
            //HomingProjectiles[i].transform.position += HomingProjectilesData[i].SpawnOffset.normalized * CurrentOffset;
            // Vector3 temp = HomingProjectiles[i].transform.localPosition;
            //temp.z = 0.0f;
            //HomingProjectiles[i].transform.position = SpawnSocket.transform.TransformPoint(temp);
        }

    }

    void Update()
    {
        if(Primed)
        {
            //SpawnSocket.transform.Rotate(new Vector3(0.0f, 0.0f, 20.0f * Time.deltaTime), Space.Self);
            if(LeftHeldDown)
            {
                AcquireTargets();
            }
        }
        if (Fired)
        {
            CurveTimer += Time.deltaTime;
            CurveTimer = Mathf.Clamp(CurveTimer, 0.0f, 1.0f);
            ProjectileTimer += Time.deltaTime;
            ProjectileTimer = Mathf.Clamp(ProjectileTimer, 0.0f, MaxProjectileDuration);
            UpdateProjectiles();

        }
       
    }

    void PrimeProjectiles()
    {
        float angleIncrement = 360.0f / HomingProjectiles.Length;
        angleIncrement *= Mathf.Deg2Rad;
        CurrentOffset = PrimedOffset;
        for (int i = 0; i < HomingProjectiles.Length; i++)
        {
            
            HomingProjectilesData[i].SpawnOffset = (new Vector3(Mathf.Cos(angleIncrement * i), Mathf.Sin(angleIncrement * i), 0.0f));
            HomingProjectiles[i].transform.Translate(HomingProjectilesData[i].SpawnOffset * CurrentOffset, Space.Self);
            //HomingProjectiles[i].transform.position += HomingProjectilesData[i].SpawnOffset.normalized * CurrentOffset;
            //Vector3 temp = HomingProjectiles[i].transform.localPosition;
            //temp.z = 0.0f;
            //[i].transform.position = SpawnSocket.transform.TransformPoint(temp);
        }
        Primed = true;
    }
    void DePrimeProjectiles()
    {
        float angleIncrement = 360.0f / HomingProjectiles.Length;
        angleIncrement *= Mathf.Deg2Rad;
        CurrentOffset = SpawnOffset;
        for (int i = 0; i < HomingProjectiles.Length; i++)
        {
            
            HomingProjectilesData[i].SpawnOffset = (new Vector3(Mathf.Cos(angleIncrement * i), Mathf.Sin(angleIncrement * i), 0.0f));
            HomingProjectiles[i].transform.position = SpawnSocket.transform.position;
            HomingProjectiles[i].transform.Translate(HomingProjectilesData[i].SpawnOffset * CurrentOffset, Space.Self);
            //HomingProjectiles[i].transform.position += HomingProjectilesData[i].SpawnOffset.normalized * CurrentOffset;
            //Vector3 temp = HomingProjectiles[i].transform.localPosition;
            //temp.z = 0.0f;
            //HomingProjectiles[i].transform.position = SpawnSocket.transform.TransformPoint(temp);
        }
        Primed = false;
    }
    void AcquireTargets()
    {

        int layerMask = 0; 
        for(int i = 0; i < 9; i++)
        {
            int mask = 1 << (i);
            layerMask = mask | layerMask;
        }
        layerMask = ~layerMask;
        Transform CamTransform = PlayerController.GetPlayerCamera().transform;
        RaycastHit hit;
        
        if(Physics.SphereCast(CamTransform.transform.position, 0.1f, CamTransform.forward, out hit, MaxDistance, layerMask, QueryTriggerInteraction.UseGlobal))
        {
            if(targets == 0)
            {
                targets++;
                AcquiredTargets[targets - 1] = hit;
                AcquiredTargets[targets - 1].collider.gameObject.GetComponent<Dummy>().DummyHit();
                
            }
            else if(targets > 0 && hit.collider.gameObject.GetInstanceID() != AcquiredTargets[targets-1].collider.gameObject.GetInstanceID())
            {
                
                if (targets < AcquiredTargets.Length - 1)
                {
                    targets++;
                    AcquiredTargets[targets - 1] = hit;
                    AcquiredTargets[targets - 1].collider.gameObject.GetComponent<Dummy>().DummyHit();
                }
            }
            else
            {
                Debug.Log("Duplicate");
            }
           
            
        }
        
        
    }
    void Shoot()
    {
        for (int i = 0; i < HomingProjectiles.Length; i++)
        {
            HomingProjectilesData[i].AwayVector = HomingProjectiles[i].transform.position - SpawnSocket.transform.position;
            HomingProjectilesData[i].AwayVector.Normalize();
            HomingProjectilesData[i].TargetDirectionVector = AcquiredTargets[i % targets].collider.transform.position - HomingProjectiles[i].transform.position;
            HomingProjectilesData[i].Velocity = HomingProjectilesData[i].TargetDirectionVector;
            HomingProjectilesData[i].IsFired = true;
            HomingProjectilesData[i].transform.parent = null;
        }
        Fired = true;
        CurveTimer = 0.0f;
        ProjectileTimer = 0.0f;
        
    }
    void UpdateProjectiles()
    {
        float tWeight = CalculateWeight(TargetWeight, TargetWeightCurve);
        float aWeight = CalculateWeight(AwayWeight, AwayWeightCurve);
        for(int i = 0; i < HomingProjectiles.Length; i++)
        {
            if(HomingProjectiles[i].activeSelf)
            {
                CalculateTagetVelocity(i, tWeight);
                HomingProjectilesData[i].Velocity = HomingProjectilesData[i].Velocity + HomingProjectilesData[i].TargetVector + CalculateAwayVelocity(i, aWeight);
                float velocityMagnitude = HomingProjectilesData[i].Velocity.sqrMagnitude;
                //std::cout << velocityMagnitude << std::endl;
                if (velocityMagnitude > ProjectileSpeed * ProjectileSpeed)
                {
                    HomingProjectilesData[i].Velocity = HomingProjectilesData[i].Velocity.normalized * ProjectileSpeed;
                }
                HomingProjectiles[i].transform.Translate(HomingProjectilesData[i].Velocity.x * Time.deltaTime, HomingProjectilesData[i].Velocity.y * Time.deltaTime, HomingProjectilesData[i].Velocity.z * Time.deltaTime, Space.World);
                HomingProjectiles[i].transform.forward = HomingProjectilesData[i].Velocity;
                if (CheckIfProjectileHit(i))
                {
                    HomingProjectilesData[i].IsFired = false;
                    HomingProjectiles[i].SetActive(false);
                    

                }
            }
          
        }
        CheckProjectiles();
    }

    void ResetProjectile(int i)
    {
        
        float angleIncrement = 360.0f / HomingProjectiles.Length;
        angleIncrement *= Mathf.Deg2Rad;
        CurrentOffset = SpawnOffset;
        HomingProjectiles[i].transform.parent = SpawnSocket.transform;
        HomingProjectilesData[i].SpawnOffset = (new Vector3(Mathf.Cos(angleIncrement * i), Mathf.Sin(angleIncrement * i), 0.0f));
        HomingProjectiles[i].transform.position = SpawnSocket.transform.position;
        HomingProjectiles[i].transform.rotation = SpawnSocket.transform.rotation;
        HomingProjectiles[i].transform.Translate(HomingProjectilesData[i].SpawnOffset * CurrentOffset, Space.Self);
        //HomingProjectiles[i].transform.position += HomingProjectilesData[i].SpawnOffset.normalized * CurrentOffset;
        //Vector3 temp = HomingProjectiles[i].transform.localPosition;
        //temp.z = 0.0f;
        //HomingProjectiles[i].transform.position = SpawnSocket.transform.TransformPoint(temp);

        Debug.Log("Projectile " + i + " is resetted");
        
    }
    void CheckProjectiles()
    {
        int projectileHit = 0; ;
        for(int i = 0; i < HomingProjectiles.Length; i++)
        {
            if(!HomingProjectilesData[i].IsFired)
            {
                projectileHit++;
            }
        }
        if(projectileHit == HomingProjectiles.Length || ProjectileTimer >= MaxProjectileDuration)
        {
            for (int i = 0; i < HomingProjectiles.Length; i++)
            {
                ResetProjectile(i);
                HomingProjectiles[i].SetActive(true);
            }
            Fired = false;
            targets = 0;
        }
        
    }
    bool CheckIfProjectileHit(int i)
    {
        
        int layerMask = 0;
        for (int j = 0; j < 9; j++)
        {
            int mask = 1 << (j);
            layerMask = mask | layerMask;
        }
        layerMask = ~layerMask;
        if (Physics.Raycast(HomingProjectiles[i].transform.position, HomingProjectiles[i].transform.forward, 0.2f, layerMask,QueryTriggerInteraction.UseGlobal))
        {
            Debug.Log("Projectile " + i + "struck target");
            return true;
        }
        return false;
    }
    void CalculateTagetVelocity(int i, float weight)
    {

        Vector3 vel = AcquiredTargets[i % targets].collider.transform.position - HomingProjectiles[i].transform.position;
        vel.Normalize();
        vel *= weight * Time.deltaTime;
        HomingProjectilesData[i].TargetVector = vel;

    }
    Vector3 CalculateAwayVelocity(int i, float weight)
    {
        Vector3 vel = HomingProjectilesData[i].AwayVector;
        vel *= weight * Time.deltaTime;
        return vel;
    }
    float CalculateWeight(float w, AnimationCurve curve)
    {
        float weight;
        weight = w * curve.Evaluate(CurveTimer);
        
        for(int i = 0; i < curve.keys.Length; i++)
        {
            if(!(AcquiredTargets[0].distance > MaxDistance))
            {
                curve.keys[i].time *= AcquiredTargets[0].distance / MaxDistance;
            }
            
                
        }

        return weight;
    }

    void Reload()
    {

    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (!Fired)
            {
                LeftHeldDown = true;
                PrimeProjectiles();
                Debug.Log("Pressed");
            }
        }
        else if(context.canceled)
        {
            if(targets > 0)
            {
                    
                    LeftHeldDown = false;
                    Shoot();
            }
            else
            {
                targets = 0;
                LeftHeldDown = false;
                DePrimeProjectiles();
            }
            Debug.Log("Released");
        }
    }
}
