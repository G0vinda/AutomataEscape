using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZoomInputHandler : MonoBehaviour
{
    [SerializeField] private bool useTouch;
    
    private UIInput _uiInput;
    private UIManager _uiManager;

    private void Awake()
    {
        _uiInput = new UIInput();
    }

    private void Start()
    {
        _uiManager = GameManager.Instance.GetUIManager();
    }

    private void OnEnable()
    {
        _uiInput.MouseZoom.Enable();
        if (!useTouch)
            _uiInput.MouseZoom.Zoom.performed += HandleMouseZoom;
    }

    private void OnDisable()
    {
        _uiInput.MouseZoom.Disable();
        if (!useTouch)
            _uiInput.MouseZoom.Zoom.performed -= HandleMouseZoom;
    }

    private void HandleTouchZoom()
    {
        
    }

    private void HandleMouseZoom(InputAction.CallbackContext context)
    {
        var zoomDelta = context.ReadValue<float>() > 0 ? 0.15f : -0.15f;
        _uiManager.ProcessZoom(zoomDelta, _uiInput.MouseZoom.MousePosition.ReadValue<Vector2>());
    }
}
