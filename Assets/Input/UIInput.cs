//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Input/UIInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @UIInput : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @UIInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""UIInput"",
    ""maps"": [
        {
            ""name"": ""Input"",
            ""id"": ""938eaf69-95e0-45cb-95cf-b33ea68ed567"",
            ""actions"": [
                {
                    ""name"": ""Position"",
                    ""type"": ""Value"",
                    ""id"": ""f1fd97e5-f859-48f0-bdee-3ca47478c7bd"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""PressRelease"",
                    ""type"": ""Button"",
                    ""id"": ""5ad6c6d9-896c-4100-b269-c1eb90f8b675"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""PositionDelta"",
                    ""type"": ""Value"",
                    ""id"": ""f3d61140-e30c-4993-9d39-1fe028e4fbf9"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Press"",
                    ""type"": ""Button"",
                    ""id"": ""5cb1104f-9e2e-430e-b41a-fdf090bbe0ca"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""913c46c8-635c-4212-b245-5f6425a33315"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a5b454cf-9fde-449c-828d-aadea8af0182"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1704ec47-492b-4a5d-b1dd-ebb928f4b7b5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PressRelease"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4707119c-57d9-4e40-a83a-a04d42643c43"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PressRelease"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""71870b36-71e1-4ae2-aa29-886403ee96e5"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PositionDelta"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a3721367-374f-4bd5-b590-f04431f15125"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PositionDelta"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03177a1d-c5c7-43b3-89c1-2ad9374d1816"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Press"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f664b50-4d92-46df-b0e4-b9621a519e24"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Press"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""MouseZoom"",
            ""id"": ""967676d8-a0b6-473b-84ee-9c53e6fbccc2"",
            ""actions"": [
                {
                    ""name"": ""Zoom"",
                    ""type"": ""Value"",
                    ""id"": ""31daed2f-4804-427a-9d40-e5652eac7bba"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""cfd01534-22f6-410d-9804-f8c914cbfe11"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""27bad8fd-d9e8-4174-a5b2-47432a6af401"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1d1e7344-9989-4b61-86a4-1aa656d2dfac"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""TouchZoom"",
            ""id"": ""966bde99-81a3-46ba-9b63-2364fc5d683b"",
            ""actions"": [
                {
                    ""name"": ""PrimaryFingerPosition"",
                    ""type"": ""Value"",
                    ""id"": ""62bbac97-e196-4aba-a388-1467ebe816d5"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SecondaryFingerPosition"",
                    ""type"": ""Value"",
                    ""id"": ""e6681bc9-0dd2-4c2c-ad6e-f71149b8fe42"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SecondaryFingerPressed"",
                    ""type"": ""Button"",
                    ""id"": ""da8f7bd5-6621-4c7e-bbcb-8444792c88a9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f0c6b527-f1fe-44dc-85eb-4f4fdb4787cb"",
                    ""path"": ""<Touchscreen>/touch0/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""PrimaryFingerPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""268d26ae-d7ed-4661-94d0-4a1fde370e8a"",
                    ""path"": ""<Touchscreen>/touch1/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryFingerPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f7c62a6e-8baf-4c16-9990-136604e5a994"",
                    ""path"": ""<Touchscreen>/touch1/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SecondaryFingerPressed"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Input
        m_Input = asset.FindActionMap("Input", throwIfNotFound: true);
        m_Input_Position = m_Input.FindAction("Position", throwIfNotFound: true);
        m_Input_PressRelease = m_Input.FindAction("PressRelease", throwIfNotFound: true);
        m_Input_PositionDelta = m_Input.FindAction("PositionDelta", throwIfNotFound: true);
        m_Input_Press = m_Input.FindAction("Press", throwIfNotFound: true);
        // MouseZoom
        m_MouseZoom = asset.FindActionMap("MouseZoom", throwIfNotFound: true);
        m_MouseZoom_Zoom = m_MouseZoom.FindAction("Zoom", throwIfNotFound: true);
        m_MouseZoom_MousePosition = m_MouseZoom.FindAction("MousePosition", throwIfNotFound: true);
        // TouchZoom
        m_TouchZoom = asset.FindActionMap("TouchZoom", throwIfNotFound: true);
        m_TouchZoom_PrimaryFingerPosition = m_TouchZoom.FindAction("PrimaryFingerPosition", throwIfNotFound: true);
        m_TouchZoom_SecondaryFingerPosition = m_TouchZoom.FindAction("SecondaryFingerPosition", throwIfNotFound: true);
        m_TouchZoom_SecondaryFingerPressed = m_TouchZoom.FindAction("SecondaryFingerPressed", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Input
    private readonly InputActionMap m_Input;
    private IInputActions m_InputActionsCallbackInterface;
    private readonly InputAction m_Input_Position;
    private readonly InputAction m_Input_PressRelease;
    private readonly InputAction m_Input_PositionDelta;
    private readonly InputAction m_Input_Press;
    public struct InputActions
    {
        private @UIInput m_Wrapper;
        public InputActions(@UIInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Position => m_Wrapper.m_Input_Position;
        public InputAction @PressRelease => m_Wrapper.m_Input_PressRelease;
        public InputAction @PositionDelta => m_Wrapper.m_Input_PositionDelta;
        public InputAction @Press => m_Wrapper.m_Input_Press;
        public InputActionMap Get() { return m_Wrapper.m_Input; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InputActions set) { return set.Get(); }
        public void SetCallbacks(IInputActions instance)
        {
            if (m_Wrapper.m_InputActionsCallbackInterface != null)
            {
                @Position.started -= m_Wrapper.m_InputActionsCallbackInterface.OnPosition;
                @Position.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnPosition;
                @Position.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnPosition;
                @PressRelease.started -= m_Wrapper.m_InputActionsCallbackInterface.OnPressRelease;
                @PressRelease.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnPressRelease;
                @PressRelease.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnPressRelease;
                @PositionDelta.started -= m_Wrapper.m_InputActionsCallbackInterface.OnPositionDelta;
                @PositionDelta.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnPositionDelta;
                @PositionDelta.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnPositionDelta;
                @Press.started -= m_Wrapper.m_InputActionsCallbackInterface.OnPress;
                @Press.performed -= m_Wrapper.m_InputActionsCallbackInterface.OnPress;
                @Press.canceled -= m_Wrapper.m_InputActionsCallbackInterface.OnPress;
            }
            m_Wrapper.m_InputActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Position.started += instance.OnPosition;
                @Position.performed += instance.OnPosition;
                @Position.canceled += instance.OnPosition;
                @PressRelease.started += instance.OnPressRelease;
                @PressRelease.performed += instance.OnPressRelease;
                @PressRelease.canceled += instance.OnPressRelease;
                @PositionDelta.started += instance.OnPositionDelta;
                @PositionDelta.performed += instance.OnPositionDelta;
                @PositionDelta.canceled += instance.OnPositionDelta;
                @Press.started += instance.OnPress;
                @Press.performed += instance.OnPress;
                @Press.canceled += instance.OnPress;
            }
        }
    }
    public InputActions @Input => new InputActions(this);

    // MouseZoom
    private readonly InputActionMap m_MouseZoom;
    private IMouseZoomActions m_MouseZoomActionsCallbackInterface;
    private readonly InputAction m_MouseZoom_Zoom;
    private readonly InputAction m_MouseZoom_MousePosition;
    public struct MouseZoomActions
    {
        private @UIInput m_Wrapper;
        public MouseZoomActions(@UIInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Zoom => m_Wrapper.m_MouseZoom_Zoom;
        public InputAction @MousePosition => m_Wrapper.m_MouseZoom_MousePosition;
        public InputActionMap Get() { return m_Wrapper.m_MouseZoom; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MouseZoomActions set) { return set.Get(); }
        public void SetCallbacks(IMouseZoomActions instance)
        {
            if (m_Wrapper.m_MouseZoomActionsCallbackInterface != null)
            {
                @Zoom.started -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnZoom;
                @MousePosition.started -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_MouseZoomActionsCallbackInterface.OnMousePosition;
            }
            m_Wrapper.m_MouseZoomActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
            }
        }
    }
    public MouseZoomActions @MouseZoom => new MouseZoomActions(this);

    // TouchZoom
    private readonly InputActionMap m_TouchZoom;
    private ITouchZoomActions m_TouchZoomActionsCallbackInterface;
    private readonly InputAction m_TouchZoom_PrimaryFingerPosition;
    private readonly InputAction m_TouchZoom_SecondaryFingerPosition;
    private readonly InputAction m_TouchZoom_SecondaryFingerPressed;
    public struct TouchZoomActions
    {
        private @UIInput m_Wrapper;
        public TouchZoomActions(@UIInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @PrimaryFingerPosition => m_Wrapper.m_TouchZoom_PrimaryFingerPosition;
        public InputAction @SecondaryFingerPosition => m_Wrapper.m_TouchZoom_SecondaryFingerPosition;
        public InputAction @SecondaryFingerPressed => m_Wrapper.m_TouchZoom_SecondaryFingerPressed;
        public InputActionMap Get() { return m_Wrapper.m_TouchZoom; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TouchZoomActions set) { return set.Get(); }
        public void SetCallbacks(ITouchZoomActions instance)
        {
            if (m_Wrapper.m_TouchZoomActionsCallbackInterface != null)
            {
                @PrimaryFingerPosition.started -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnPrimaryFingerPosition;
                @PrimaryFingerPosition.performed -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnPrimaryFingerPosition;
                @PrimaryFingerPosition.canceled -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnPrimaryFingerPosition;
                @SecondaryFingerPosition.started -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPosition;
                @SecondaryFingerPosition.performed -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPosition;
                @SecondaryFingerPosition.canceled -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPosition;
                @SecondaryFingerPressed.started -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPressed;
                @SecondaryFingerPressed.performed -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPressed;
                @SecondaryFingerPressed.canceled -= m_Wrapper.m_TouchZoomActionsCallbackInterface.OnSecondaryFingerPressed;
            }
            m_Wrapper.m_TouchZoomActionsCallbackInterface = instance;
            if (instance != null)
            {
                @PrimaryFingerPosition.started += instance.OnPrimaryFingerPosition;
                @PrimaryFingerPosition.performed += instance.OnPrimaryFingerPosition;
                @PrimaryFingerPosition.canceled += instance.OnPrimaryFingerPosition;
                @SecondaryFingerPosition.started += instance.OnSecondaryFingerPosition;
                @SecondaryFingerPosition.performed += instance.OnSecondaryFingerPosition;
                @SecondaryFingerPosition.canceled += instance.OnSecondaryFingerPosition;
                @SecondaryFingerPressed.started += instance.OnSecondaryFingerPressed;
                @SecondaryFingerPressed.performed += instance.OnSecondaryFingerPressed;
                @SecondaryFingerPressed.canceled += instance.OnSecondaryFingerPressed;
            }
        }
    }
    public TouchZoomActions @TouchZoom => new TouchZoomActions(this);
    public interface IInputActions
    {
        void OnPosition(InputAction.CallbackContext context);
        void OnPressRelease(InputAction.CallbackContext context);
        void OnPositionDelta(InputAction.CallbackContext context);
        void OnPress(InputAction.CallbackContext context);
    }
    public interface IMouseZoomActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
    }
    public interface ITouchZoomActions
    {
        void OnPrimaryFingerPosition(InputAction.CallbackContext context);
        void OnSecondaryFingerPosition(InputAction.CallbackContext context);
        void OnSecondaryFingerPressed(InputAction.CallbackContext context);
    }
}
