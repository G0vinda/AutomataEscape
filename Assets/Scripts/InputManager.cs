using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private bool useTouch;

    public static Action<StateUIPlaceElement> StateElementTapped;
    public static Action StateChartPanelTapped;

    public static Action<StateUIPlaceElement> StateElementDragStarted;
    public static Action StateChartPanelDragStarted;

    public static Action DragEnded;

    private UIInput _uiInput;
    private UIManager _uiManager;
    private bool _inputReleased;

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
        _uiInput.Input.Enable();
        _uiInput.Input.Press.started += HandlePress;
        _uiInput.Input.PressRelease.performed += HandlePressRelease;
        if (!useTouch)
        {
            _uiInput.MouseZoom.Enable();
            _uiInput.MouseZoom.Zoom.performed += HandleMouseZoom;   
        }
    }

    private void OnDisable()
    {
        _uiInput.Input.Disable();
        _uiInput.Input.Press.started -= HandlePress;
        _uiInput.Input.PressRelease.performed -= HandlePressRelease;
        if (!useTouch)
        {
            _uiInput.MouseZoom.Disable();
            _uiInput.MouseZoom.Zoom.performed -= HandleMouseZoom;
        }
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        var eventSystem = EventSystem.current;
        var raycastResults = new List<RaycastResult>();
        var pointerEventData = new PointerEventData(eventSystem);
        
        pointerEventData.position = _uiInput.Input.Position.ReadValue<Vector2>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        Debug.Log("Press detected!");

        if (raycastResults.Count == 0)
            return;

        var pressWasOnPanel = false;
        foreach (var raycastResult in raycastResults)
        {
            if(raycastResult.gameObject.CompareTag("StateUIElement"))
            {
                var stateUIPlaceElement = raycastResult.gameObject.GetComponentInParent<StateUIPlaceElement>();
                if(stateUIPlaceElement != null)
                    StartCoroutine(ProcessPressInput(stateUIPlaceElement));
                return;
            }

            if (raycastResult.gameObject.GetComponent<StateChartPanel>() != null)
            {
                pressWasOnPanel = true;
            }
        }

        if (pressWasOnPanel)
            StartCoroutine(ProcessPressInput());
    }

    private IEnumerator ProcessPressInput(StateUIPlaceElement selectedStateElement = null)
    {
        while (true)
        {
            if (_inputReleased) // Player input was tap
            {
                if (selectedStateElement != null)
                {
                    StateElementTapped?.Invoke(selectedStateElement);
                }
                else
                {
                    StateChartPanelTapped?.Invoke();    
                }

                _inputReleased = false;
                break;
            }

            if (_uiInput.Input.PositionDelta.ReadValue<Vector2>() != Vector2.zero) // Player input is drag
            {
                if (selectedStateElement != null)
                {
                    StateElementDragStarted?.Invoke(selectedStateElement);
                }
                else
                {
                    StateChartPanelDragStarted?.Invoke();    
                }
                
                break;
            }
            
            yield return null;
        }
    }

    private void HandlePressRelease(InputAction.CallbackContext context)
    {
        if (DragEnded != null)
        {
            _inputReleased = false;
            DragEnded.Invoke();
        }
        else
        {
            _inputReleased = true;       
        }
    }

    public Vector2 GetPointerPosition()
    {
        return _uiInput.Input.Position.ReadValue<Vector2>();
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
