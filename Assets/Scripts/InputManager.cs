using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Helper;
using UI.State;
using UI.Transition;

public class InputManager : MonoBehaviour
{
    [SerializeField] private bool useTouch;

    public static Action<StateUIElement> StateElementTapped;
    public static Action<StateUIElement> StateElementDragStarted;

    public static Action StateChartPanelTapped;
    public static Action StateChartPanelDragStarted;

    public static Action<TransitionSelectElement> TransitionElementSelected;
    public static Action TransitionDeselected;

    public static Action DragEnded;

    public static Action<float, Vector2> ZoomInputChanged;

    private UIInput _uiInput;
    private bool _inputReleased;

    private void Awake()
    {
        _uiInput = new UIInput();
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
        else
        {
            _uiInput.TouchZoom.Enable();
            _uiInput.TouchZoom.SecondaryFingerPressed.performed += EnterZoomMode;
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
        else
        {
            _uiInput.TouchZoom.Disable();
            _uiInput.TouchZoom.SecondaryFingerPressed.performed -= EnterZoomMode;
        }
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        _inputReleased = false;
        var wasPossibleDrawInteraction = false;
        ProcessInputOverElement(
            _uiInput.Input.Position.ReadValue<Vector2>(),
            state =>
            {
                StartCoroutine(ProcessPressInput(state));
                wasPossibleDrawInteraction = true;
            },
            transitionSelect =>
            {
                TransitionElementSelected?.Invoke(transitionSelect);
                wasPossibleDrawInteraction = true;
            },
            () =>
            {
                StartCoroutine(ProcessPressInput());
            });
        
        if(!wasPossibleDrawInteraction)
            TransitionDeselected?.Invoke();
    }

    private IEnumerator ProcessPressInput(StateUIElement selectedStateElement = null)
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

    private void EnterZoomMode(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Second Finger detected");
        DragEnded?.Invoke();
        _uiInput.Input.Disable();
        _uiInput.Input.Press.started -= HandlePress;
        _uiInput.Input.PressRelease.performed -= HandlePressRelease;

        var fingerOnePosition = GetFingerOnePosition();
        var fingerTwoPosition = GetFingerTwoPosition();
        var midPoint = HelperFunctions.GetMidpointOfVectors(fingerOnePosition, fingerTwoPosition);
        ProcessInputOverElement(
            midPoint,
            state => { },
            transition => { },
            () => { StartCoroutine(HandleTouchZoom(midPoint)); });
    }

    private void ExitZoomMode()
    {
        _uiInput.Input.Enable();
        _uiInput.Input.Press.started += HandlePress;
        _uiInput.Input.PressRelease.performed += HandlePressRelease;
    }

    private IEnumerator HandleTouchZoom(Vector2 zoomCenter)
    {
        float secondaryFingerPress;
        Debug.Log($"will start to zoom around center: {zoomCenter}");
        var previousFingerDistance = Vector2.Distance(GetFingerOnePosition(), GetFingerTwoPosition());
        do
        {
            var currentFingerDistance = Vector2.Distance(GetFingerOnePosition(), GetFingerTwoPosition());
            var zoomDistance = currentFingerDistance - previousFingerDistance;

            var zoomInputThreshold = 1f;
            var zoomSpeedModifier = 0.005f;
            if (Mathf.Abs(zoomDistance) > zoomInputThreshold)
                ZoomInputChanged?.Invoke(zoomDistance * zoomSpeedModifier, zoomCenter);

            previousFingerDistance = currentFingerDistance;
            yield return null;
            secondaryFingerPress =
                _uiInput.TouchZoom.SecondaryFingerPressed
                    .ReadValue<float>(); // If = 1, then secondary finger is pressed down
        } while (Mathf.Approximately(secondaryFingerPress, 1f));

        Debug.Log("Zoom canceled");
        ExitZoomMode();
    }

    private Vector2 GetFingerOnePosition()
    {
        return _uiInput.TouchZoom.PrimaryFingerPosition.ReadValue<Vector2>();
    }

    private Vector2 GetFingerTwoPosition()
    {
        return _uiInput.TouchZoom.SecondaryFingerPosition.ReadValue<Vector2>();
    }

    private void HandleMouseZoom(InputAction.CallbackContext context)
    {
        var zoomDelta = context.ReadValue<float>() > 0 ? 0.15f : -0.15f;
        ZoomInputChanged?.Invoke(zoomDelta, _uiInput.MouseZoom.MousePosition.ReadValue<Vector2>());
    }

    private void ProcessInputOverElement(
        Vector2 inputPosition,
        Action<StateUIElement> inputOverStateAction,
        Action<TransitionSelectElement> inputOverTransitionAction,
        Action inputOverPanelAction)
    {
        var raycastResults = HelperFunctions.GetRaycastResultsOnPosition(inputPosition);
        var inputWasOnPanel = false;

        foreach (var raycastResult in raycastResults)
        {
            if (raycastResult.gameObject.CompareTag("StateUIElement"))
            {
                var stateUIElement = raycastResult.gameObject.GetComponentInParent<StateUIElement>();
                inputOverStateAction(stateUIElement);
                return;
            }

            if (raycastResult.gameObject.TryGetComponent<TransitionSelectElement>(out var transitionSelectElement))
            {
                inputOverTransitionAction(transitionSelectElement);
                return;
            }

            if (raycastResult.gameObject.GetComponent<StateChartPanel>() != null)
            {
                inputWasOnPanel = true;
            }
        }

        if (inputWasOnPanel)
            inputOverPanelAction();
    }
}