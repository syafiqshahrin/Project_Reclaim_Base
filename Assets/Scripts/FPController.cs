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
    [Header("Camera Turn Rate and Sensitivity")]
    [SerializeField] [Range(0.01f, 0.1f)] float verticalTurnRate = 0.1f;
    [SerializeField] [Range(0.01f, 0.1f)] float horizontalTurnRate = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] float mouseSensitivity = 1.0f;
    [SerializeField] float vertConstraint = 60.0f;
    [SerializeField] PlayerInput input;
    [SerializeField] Camera PlayerCamera;
    [Header("Character Movement Settings")]
    [SerializeField] float PlayerSpeed = 2.0f;
    [SerializeField] float Gravity = 9.8f;
    [SerializeField] float maxStepHeight = 0.75f;
    [SerializeField] float maxHeadAllowanceHeight = 1.6f;
    [Header("Character Capsule Dimensions")]
    [SerializeField] GameObject CapsuleObject;
    [SerializeField] float CapsuleHeight = 1.8f;
    [SerializeField] float CapsuleWidth = 1.0f;
    Vector2 inputVal = Vector2.zero;

    bool Pressed = false;
    private void Start()
    {
        CapsuleObject.GetComponent<CapsuleCollider>().height = CapsuleHeight;
        CapsuleObject.GetComponent<CapsuleCollider>().radius = CapsuleWidth/2.0f;
        CapsuleObject.GetComponent<CapsuleCollider>().center = new Vector3(0, CapsuleHeight/2.0f, 0);
        PlayerCamera.gameObject.transform.localPosition = new Vector3(0, CapsuleHeight * 0.83f, -CapsuleWidth * 0.45f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    private void FixedUpdate()
    {
        //==================================Fake Gravity and Feet Elevation====================//
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
            else if(hitInfo.point.y < (transform.position.y))
            {
                float diffY = Mathf.Clamp(transform.position.y - hitInfo.point.y, 0.0f, 1.0f);

                transform.Translate(new Vector3(0.0f, -diffY * Gravity * Time.deltaTime, 0.0f), Space.World);
            }

        }
        else
        {
            transform.Translate(new Vector3(0.0f, -1.0f * Gravity * Time.deltaTime, 0.0f), Space.World);
        }

        //==================================Movement Logic and Detect Collisions in Path====================//
        if (Pressed)
        {
            float zMovement;
            float xMovement;
            zMovement = inputVal.y * PlayerSpeed * Time.deltaTime;
            xMovement = inputVal.x * PlayerSpeed * Time.deltaTime;
            //int layerMask = 1 << 8;
            //layerMask = ~layerMask;
            Vector3 prevPos = transform.position;
           
            Vector3 startBot = prevPos;
            Vector3 startTop = prevPos;
            startBot.y += maxStepHeight;
            startTop.y += maxHeadAllowanceHeight;
            Vector3 newPos = transform.InverseTransformPoint(startBot) + new Vector3(xMovement, 0.0f, zMovement);
            Vector3 newPosTop = transform.InverseTransformPoint(startTop) + new Vector3(xMovement, 0.0f, zMovement);
            Vector3 rayDirection = newPos - transform.InverseTransformPoint(startBot);
            Vector3 rayDirectionTop = newPosTop - transform.InverseTransformPoint(startTop);
            rayDirection = transform.TransformDirection(rayDirection);
            rayDirectionTop = transform.TransformDirection(rayDirectionTop);
            
            //RaycastHit hitInfo;
            Ray rayBot = new Ray(startBot, rayDirection); 
            Ray rayTop = new Ray(startTop, rayDirectionTop); 
            
            if (Physics.SphereCast(rayBot, 0.25f, 0.3f, layerMask, QueryTriggerInteraction.UseGlobal) || Physics.SphereCast(rayTop, 0.25f, 0.3f, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                Debug.DrawRay(startBot, rayDirection * 100.0f, Color.red);
                Debug.DrawRay(startTop, rayDirectionTop * 100.0f, Color.magenta);
                Debug.Log("Hit");
            }
            else
            {
                Debug.DrawRay(startBot, rayDirection * 100.0f, Color.red);
                Debug.DrawRay(startTop, rayDirectionTop * 100.0f, Color.magenta);
                transform.Translate(new Vector3(xMovement, 0.0f, zMovement), Space.Self);
            }
            Vector3 newTransform = new Vector3(transform.position.x, CapsuleObject.transform.position.y, transform.position.z);
            CapsuleObject.transform.position = newTransform;
            CapsuleObject.transform.rotation = transform.rotation;
        }
    
    }
    public void PlayerMove(InputAction.CallbackContext context)
    {
        inputVal = context.ReadValue<Vector2>();
        if (context.action.triggered)
        {
            //Debug.Log(context.valueType.Name);
        }
        if (context.performed)
        {
            Pressed = true;
           // Debug.Log("x: " + inputVal.x + " y: " + inputVal.y);
        }
        else
        {
            Pressed = false;
        }
    }
}

