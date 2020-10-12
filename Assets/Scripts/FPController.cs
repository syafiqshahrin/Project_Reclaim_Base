using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPController : MonoBehaviour
{
    //Move Character with WASD
    //Turn Camera with mouse vertically
    //Turn Character with mouse horizontally
    /* Will need to refactor  (Currently very spaghett)
     * Move different processes into proper functions to better clarify process flow e.g.(Evaluate Speed func, Check Ground function, Check Obstacle func, ApplyMovement func etc)
     */
    [Header("Camera Turn Rate and Sensitivity")]
    [SerializeField] [Range(0.01f, 0.1f)] float verticalTurnRate = 0.1f;
    [SerializeField] [Range(0.01f, 0.1f)] float horizontalTurnRate = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] float mouseSensitivity = 1.0f;
    [SerializeField] float vertConstraint = 60.0f;
    [SerializeField] PlayerInput input;
    [SerializeField] Camera PlayerCamera;
    [Header("Character Movement Settings")]
    [SerializeField] AnimationCurve EaseInCurve;
    [SerializeField] AnimationCurve EaseInRunCurve;
    [SerializeField] float PlayerMaxSpeed = 2.0f;
    [SerializeField] float PlayerRunMaxSpeed = 8.0f;
    [SerializeField] float CurrentPlayerSpeed = 0.0f; //only currently visible for debugging purposes
    float DefaultMoveEaseInTime = 0.0f;
    float RunEaseInTime = 0.0f;
    float zMovement;
    float xMovement;
    [SerializeField] float Gravity = 9.8f;
    [SerializeField] float maxStepHeight = 0.75f;
    [SerializeField] float maxHeadAllowanceHeight = 1.6f;
    [SerializeField] float SlopeAngle = 30.0f;
    [Header("Character Dash Settings")]
    [SerializeField] float MaxDashDistance = 1.0f;
    [SerializeField] float DashTime = 0.75f;
    [SerializeField] float DashCooldown = 0.4f;
    float DashCooldownTimer;
    float DashTimer;
    Vector3 DashDirection;
    [Header("Character Capsule Dimensions")]
    [SerializeField] GameObject CapsuleObject;
    [SerializeField] float CapsuleHeight = 1.8f;
    [SerializeField] float CapsuleWidth = 1.0f;
    Vector2 inputVal = Vector2.zero;

    bool IsMoving = false;
    bool IsRunning = false;
    bool IsDashing = false;
    RaycastHit hitInfoBody;
    private void Start()
    {
        CurrentPlayerSpeed = 0.0f;
        DefaultMoveEaseInTime = 0.0f;
        RunEaseInTime = 0.0f;
        DashCooldownTimer = 0.0f;
        DashTimer = 0.0f;
        CapsuleObject.GetComponent<CapsuleCollider>().height = CapsuleHeight;
        CapsuleObject.GetComponent<CapsuleCollider>().radius = CapsuleWidth/2.0f;
        CapsuleObject.GetComponent<CapsuleCollider>().center = new Vector3(0, CapsuleHeight/2.0f, 0);
        PlayerCamera.gameObject.transform.localPosition = new Vector3(0, CapsuleHeight * 0.83f, -CapsuleWidth * 0.45f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        //Updating time variables for evaluation of the ease in curves for movement//
        UpdateMovementTimer();
        EvaluateCurrentPlayerSpeed();
        if(IsDashing)
        {
            UpdateDashTimer();
            UpdateDash();
        }
        else
        {
            UpdateDashCooldownTimer();
        }
    }
    private void FixedUpdate()
    {
        CheckGround();
        if (IsMoving)
        {
            ResolveMovement();
        }
        UpdateCapsule();
        

    }
    public void PlayerLook(InputAction.CallbackContext context)
    {
        //Debug.Log("Called");
        Vector2 mouseDelta = context.ReadValue<Vector2>();

        Vector3 currentVerRot = PlayerCamera.transform.rotation.eulerAngles;
        currentVerRot.x = (currentVerRot.x + 180f) % 360f - 180f;
        currentVerRot.x -= mouseDelta.y * verticalTurnRate * mouseSensitivity;
        currentVerRot.x = Mathf.Clamp(currentVerRot.x, -vertConstraint, vertConstraint);
        PlayerCamera.transform.rotation = Quaternion.Euler(currentVerRot);


        Vector3 currentHorRot = gameObject.transform.rotation.eulerAngles;
        currentHorRot.y = (currentHorRot.y + 180f) % 360f - 180f;
        currentHorRot.y += mouseDelta.x * horizontalTurnRate * mouseSensitivity;
        transform.rotation = Quaternion.Euler(currentHorRot);
        CapsuleObject.transform.rotation = Quaternion.Euler(currentHorRot);
    }
    public void PlayerMove(InputAction.CallbackContext context)
    {
        inputVal = context.ReadValue<Vector2>();
        if (context.performed)
        {

            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }
    }
    public void PlayerRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            IsRunning = true;
        }
        else
        {
            IsRunning = false;
        }
    }
    public void DashStep(InputAction.CallbackContext context)
    {
        //Dash In Movement Direction
        //Dash has a max distance and speed
        //If there is obstacle in path of dash stop before obstacle
        //Dash has a cooldown
        //It's a dash not a blink!!
        if (context.performed)
        {
            StartDash();
        }
    }


    private void EvaluateCurrentPlayerSpeed()
    {
        CurrentPlayerSpeed = (PlayerMaxSpeed * EaseInCurve.Evaluate(DefaultMoveEaseInTime)) + ((PlayerRunMaxSpeed - PlayerMaxSpeed) * EaseInRunCurve.Evaluate(RunEaseInTime));
    }
    private void CheckGround()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        RaycastHit hitInfo;

        Vector3 centerPos = transform.position;
        centerPos.y += CapsuleHeight / 2.0f;

        Ray rayGround = new Ray(centerPos, Vector3.down);
        Debug.DrawRay(centerPos, Vector3.down * (CapsuleHeight / 2.0f), Color.cyan);
        if (Physics.Raycast(rayGround, out hitInfo, Mathf.Infinity, layerMask, QueryTriggerInteraction.UseGlobal))
        {
            if (hitInfo.point.y > (transform.position.y))
            {
                //Debug.Log("Hit Ground");
                Debug.DrawRay(centerPos, Vector3.down * (CapsuleHeight / 2.0f), Color.cyan);
                //Vector3 newHeightPos = transform.position;
                float diffY = hitInfo.point.y - transform.position.y;
                //newHeightPos.y = hitInfo.point.y;
                transform.Translate(new Vector3(0.0f, diffY * Gravity * Time.deltaTime, 0.0f), Space.World);
            }
            else if (hitInfo.point.y < (transform.position.y))
            {
                float diffY = Mathf.Clamp(transform.position.y - hitInfo.point.y, 0.0f, 1.0f);

                transform.Translate(new Vector3(0.0f, -diffY * Gravity * Time.deltaTime, 0.0f), Space.World);
            }

        }
        else
        {
            transform.Translate(new Vector3(0.0f, -1.0f * Gravity * Time.deltaTime, 0.0f), Space.World);
        }
    }

    private bool CheckObstacle(Vector3 direction)
    {
        
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        bool Obstacle = false;
        Vector3 prevPos = transform.position;

        Vector3 startBot = prevPos;
        Vector3 startTop = prevPos;
        startBot.y += maxStepHeight;
        startTop.y += maxHeadAllowanceHeight;
        Vector3 newPos = transform.InverseTransformPoint(startBot) + new Vector3(direction.x, 0.0f, direction.z);
        Vector3 newPosTop = transform.InverseTransformPoint(startTop) + new Vector3(direction.x, 0.0f, direction.z);
        Vector3 rayDirection = newPos - transform.InverseTransformPoint(startBot);
        Vector3 rayDirectionTop = newPosTop - transform.InverseTransformPoint(startTop);
        rayDirection = transform.TransformDirection(rayDirection);
        rayDirectionTop = transform.TransformDirection(rayDirectionTop);

        //RaycastHit hitInfo;
        Ray rayBot = new Ray(startBot, rayDirection);
        Ray rayTop = new Ray(startTop, rayDirectionTop);
        Debug.DrawRay(startBot, rayDirection * 100.0f, Color.red);
        Debug.DrawRay(startTop, rayDirectionTop * 100.0f, Color.magenta);
        if (Physics.SphereCast(rayBot, 0.25f, out hitInfoBody, 0.3f, layerMask, QueryTriggerInteraction.UseGlobal) || Physics.SphereCast(rayTop, 0.25f, 0.3f, layerMask, QueryTriggerInteraction.UseGlobal))
        {
            Obstacle = true;
        }else
        {
            Obstacle = false;
        }
        return Obstacle;
    }

    private void ResolveMovement()
    {
        zMovement = inputVal.y * CurrentPlayerSpeed * Time.deltaTime;
        xMovement = inputVal.x * CurrentPlayerSpeed * Time.deltaTime;
        Vector3 direction = new Vector3(xMovement, 0.0f, zMovement);
        if (CheckObstacle(direction))
        {
            Vector3 collisionNormal = hitInfoBody.normal;
            Debug.Log("Normal: " + collisionNormal);
            if (collisionNormal.y != 0.0f)
            {
                collisionNormal.y = 0.0f;

            }
            Vector3 prevPos = transform.position;
            Vector3 startBot = prevPos;
            Vector3 newPos = transform.InverseTransformPoint(startBot) + new Vector3(xMovement, 0.0f, zMovement);
            Vector3 rayDirection = newPos - transform.InverseTransformPoint(startBot);
            Vector3 newMovementDir = collisionNormal + rayDirection;
            newMovementDir = transform.InverseTransformDirection(newMovementDir);
            newMovementDir.Normalize();

            transform.Translate(new Vector3(newMovementDir.x * CurrentPlayerSpeed * Time.deltaTime, 0.0f, newMovementDir.y * PlayerMaxSpeed * Time.deltaTime), Space.Self);
        }
        else
        {
           
            transform.Translate(new Vector3(xMovement, 0.0f, zMovement), Space.Self);
        }
    }
    private void UpdateCapsule()
    {
        Vector3 newTransform = new Vector3(transform.position.x, CapsuleObject.transform.position.y, transform.position.z);
        CapsuleObject.transform.position = newTransform;
        CapsuleObject.transform.rotation = transform.rotation;
    }
    private void UpdateMovementTimer()
    {
        if (IsMoving)
        {
            DefaultMoveEaseInTime += Time.deltaTime;
            DefaultMoveEaseInTime = Mathf.Clamp(DefaultMoveEaseInTime, 0.0f, 1.0f);
        }
        else
        {
            DefaultMoveEaseInTime -= Time.deltaTime;
            DefaultMoveEaseInTime = Mathf.Clamp(DefaultMoveEaseInTime, 0.0f, 1.0f);
        }

        if (IsRunning)
        {
            RunEaseInTime += Time.deltaTime;
            RunEaseInTime = Mathf.Clamp(RunEaseInTime, 0.0f, 1.0f);

        }
        else
        {
            RunEaseInTime -= Time.deltaTime;
            RunEaseInTime = Mathf.Clamp(RunEaseInTime, 0.0f, 1.0f);
        }
    }
    private void UpdateDashTimer()
    {
        DashTimer += Time.deltaTime;
        DashTimer = Mathf.Clamp(DashTimer, 0.0f, DashTime);
    }
    private void UpdateDashCooldownTimer()
    {
        if(DashCooldownTimer != 0.0f)
        {
            DashCooldownTimer -= Time.deltaTime;
            DashCooldownTimer = Mathf.Clamp(DashCooldownTimer, 0.0f, DashCooldown);
        }
        
    }
    private void UpdateDash()
    {
        
        if(DashTimer < DashTime)
        {
            if(!CheckObstacle(DashDirection))
            {
                transform.Translate(new Vector3(DashDirection.x, 0.0f, DashDirection.z), Space.Self);
            }
            
        }
        else
        {
            IsDashing = false;
        }
        
    }
    private void StartDash()
    {
        if(!IsDashing && DashCooldownTimer == 0.0f)
        {
            IsDashing = true;
            DashTimer = 0.0f;
            DashCooldownTimer = DashCooldown;
            DashDirection = Vector2.zero;
            float dashSpeed = 0.0f;
            if (inputVal.magnitude > 0.0f)
            {
                DashDirection.x = inputVal.x;
                DashDirection.z = inputVal.y;
                DashDirection.Normalize();
                dashSpeed = MaxDashDistance / DashTime;
                DashDirection *= dashSpeed * Time.deltaTime;
            }
            else
            {
                DashDirection = transform.InverseTransformDirection(transform.forward);
                Debug.Log(DashDirection);
                dashSpeed = MaxDashDistance / DashTime;
                DashDirection *= dashSpeed * Time.deltaTime;
                
            }
        }
    }
}

