using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class VRInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty primaryButtonAction;
    [SerializeField] private InputActionProperty secondaryButtonAction;
    [SerializeField] private InputActionProperty thumbstickAction;

    [Header("Controller References")]
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;

    private bool isInitialized = false;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            SetupInputActions();
            isInitialized = true;
        }
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void OnDestroy()
    {
        DisableInputActions();
        CleanupInputActions();
    }

    private void ValidateReferences()
    {
        if (leftController == null)
        {
            Debug.LogError($"{nameof(leftController)} reference is missing in {nameof(VRInputHandler)}");
            enabled = false;
            return;
        }

        if (rightController == null)
        {
            Debug.LogError($"{nameof(rightController)} reference is missing in {nameof(VRInputHandler)}");
            enabled = false;
            return;
        }
    }

    private void EnableInputActions()
    {
        try
        {
            if (gripAction.action != null) gripAction.action.Enable();
            if (triggerAction.action != null) triggerAction.action.Enable();
            if (primaryButtonAction.action != null) primaryButtonAction.action.Enable();
            if (secondaryButtonAction.action != null) secondaryButtonAction.action.Enable();
            if (thumbstickAction.action != null) thumbstickAction.action.Enable();

            SubscribeToInputEvents();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to enable input actions: {e.Message}");
            enabled = false;
        }
    }

    private void DisableInputActions()
    {
        try
        {
            if (gripAction.action != null) gripAction.action.Disable();
            if (triggerAction.action != null) triggerAction.action.Disable();
            if (primaryButtonAction.action != null) primaryButtonAction.action.Disable();
            if (secondaryButtonAction.action != null) secondaryButtonAction.action.Disable();
            if (thumbstickAction.action != null) thumbstickAction.action.Disable();

            UnsubscribeFromInputEvents();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to disable input actions: {e.Message}");
        }
    }

    private void SubscribeToInputEvents()
    {
        if (gripAction.action != null)
        {
            gripAction.action.performed += OnGripPerformed;
            gripAction.action.canceled += OnGripCanceled;
        }
        if (triggerAction.action != null)
        {
            triggerAction.action.performed += OnTriggerPerformed;
            triggerAction.action.canceled += OnTriggerCanceled;
        }
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.performed += OnPrimaryButtonPerformed;
        }
        if (secondaryButtonAction.action != null)
        {
            secondaryButtonAction.action.performed += OnSecondaryButtonPerformed;
        }
        if (thumbstickAction.action != null)
        {
            thumbstickAction.action.performed += OnThumbstickPerformed;
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        if (gripAction.action != null)
        {
            gripAction.action.performed -= OnGripPerformed;
            gripAction.action.canceled -= OnGripCanceled;
        }
        if (triggerAction.action != null)
        {
            triggerAction.action.performed -= OnTriggerPerformed;
            triggerAction.action.canceled -= OnTriggerCanceled;
        }
        if (primaryButtonAction.action != null)
        {
            primaryButtonAction.action.performed -= OnPrimaryButtonPerformed;
        }
        if (secondaryButtonAction.action != null)
        {
            secondaryButtonAction.action.performed -= OnSecondaryButtonPerformed;
        }
        if (thumbstickAction.action != null)
        {
            thumbstickAction.action.performed -= OnThumbstickPerformed;
        }
    }

    private void CleanupInputActions()
    {
        gripAction.action?.Dispose();
        triggerAction.action?.Dispose();
        primaryButtonAction.action?.Dispose();
        secondaryButtonAction.action?.Dispose();
        thumbstickAction.action?.Dispose();
    }

    private void OnGripPerformed(InputAction.CallbackContext context)
    {
        float gripValue = context.ReadValue<float>();
        // Handle grip action
        Debug.Log($"Grip pressed with value: {gripValue}");
    }

    private void OnGripCanceled(InputAction.CallbackContext context)
    {
        // Handle grip release
        Debug.Log("Grip released");
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        float triggerValue = context.ReadValue<float>();
        // Handle trigger action
        Debug.Log($"Trigger pressed with value: {triggerValue}");
    }

    private void OnTriggerCanceled(InputAction.CallbackContext context)
    {
        // Handle trigger release
        Debug.Log("Trigger released");
    }

    private void OnPrimaryButtonPerformed(InputAction.CallbackContext context)
    {
        // Handle primary button press (usually A/X button)
        Debug.Log("Primary button pressed");
    }

    private void OnSecondaryButtonPerformed(InputAction.CallbackContext context)
    {
        // Handle secondary button press (usually B/Y button)
        Debug.Log("Secondary button pressed");
    }

    private void OnThumbstickPerformed(InputAction.CallbackContext context)
    {
        Vector2 thumbstickValue = context.ReadValue<Vector2>();
        // Handle thumbstick movement
        Debug.Log($"Thumbstick moved: {thumbstickValue}");
    }

    public void SetupInputActions()
    {
        try
        {
            if (gripAction.action == null)
            {
                gripAction.action = new InputAction("Grip", binding: "<XRController>{LeftHand}/grip");
            }
            if (triggerAction.action == null)
            {
                triggerAction.action = new InputAction("Trigger", binding: "<XRController>{LeftHand}/trigger");
            }
            if (primaryButtonAction.action == null)
            {
                primaryButtonAction.action = new InputAction("PrimaryButton", binding: "<XRController>{LeftHand}/primaryButton");
            }
            if (secondaryButtonAction.action == null)
            {
                secondaryButtonAction.action = new InputAction("SecondaryButton", binding: "<XRController>{LeftHand}/secondaryButton");
            }
            if (thumbstickAction.action == null)
            {
                thumbstickAction.action = new InputAction("Thumbstick", binding: "<XRController>{LeftHand}/thumbstick");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to setup input actions: {e.Message}");
            enabled = false;
        }
    }
} 