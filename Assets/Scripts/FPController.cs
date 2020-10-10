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
    [SerializeField] [Range(0.01f, 0.1f)] float verticalTurnRate = 0.1f;
    [SerializeField] [Range(0.01f, 0.1f)] float horizontalTurnRate = 0.1f;
    [SerializeField] [Range(0.1f, 2.0f)] float mouseSensitivity = 1.0f;
    [SerializeField] PlayerInput input;
    [SerializeField] Camera PlayerCamera;
    [SerializeField] float vertConstraint = 60.0f;
    [SerializeField] float PlayerSpeed = 2.0f;
    [SerializeField] GameObject CapsuleObject;
    Vector2 inputVal = Vector2.zero;

    bool Pressed = false;
    private void Start()
    {

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
        if (Pressed)
        {
            float zMovement;
            float xMovement;
            zMovement = inputVal.y * PlayerSpeed * Time.deltaTime;
            xMovement = inputVal.x * PlayerSpeed * Time.deltaTime;
            int layerMask = 1 << 8;
            layerMask = ~layerMask;
            Vector3 prevPos = transform.position;
            Vector3 startBot = prevPos;
            Vector3 startTop = prevPos;
            startBot.y = 0.0f;
            Vector3 newPos = transform.InverseTransformPoint(startBot) + new Vector3(xMovement, 0.0f, zMovement);
            Vector3 rayDirection = newPos - transform.InverseTransformPoint(startBot);
            rayDirection = transform.TransformDirection(rayDirection);

            Ray ray = new Ray(startBot, rayDirection); 
            if(Physics.SphereCast(ray, 0.25f, 0.3f, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                Debug.DrawRay(prevPos, rayDirection * 100.0f, Color.red);
                //Debug.Log("Hit");
            }
            else
            {
                Debug.DrawRay(prevPos, rayDirection * 100.0f, Color.red);
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

