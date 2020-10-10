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
        

    }
    private void Update()
    {
        if(Pressed)
        {
            float zMovement;
            float xMovement;
            zMovement = inputVal.y * PlayerSpeed * Time.deltaTime;
            xMovement = inputVal.x * PlayerSpeed * Time.deltaTime;

            transform.Translate(new Vector3(xMovement, 0.0f, zMovement), Space.Self);
        }
    }
    public void PlayerMove(InputAction.CallbackContext context)
    {
        inputVal = context.ReadValue<Vector2>();
        if (context.action.triggered)
        {
            Debug.Log(context.valueType.Name);
        }
        if(context.performed)
        {
            Pressed = true;
            Debug.Log("x: " + inputVal.x + " y: " + inputVal.y);
        }else
        {
            Pressed = false;
        }
    }
}

