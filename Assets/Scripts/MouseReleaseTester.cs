using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseReleaseTester : MonoBehaviour
{
    private UIInput _input;
    private void Awake()
    {
        _input = new UIInput();
    }

    private void OnEnable()
    {
        _input.Input.Enable();
        _input.Input.PressRelease.performed += Test;
    }

    private void OnDisable()
    {
        _input.Input.Disable();
        _input.Input.PressRelease.performed -= Test;
    }

    private void Test(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse was released!");
    }
}
