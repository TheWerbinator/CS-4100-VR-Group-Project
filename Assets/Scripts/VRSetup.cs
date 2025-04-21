using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRSetup : MonoBehaviour
{
    [Header("XR References")]
    [SerializeField] private XRInteractionManager interactionManager;
    [SerializeField] private XRRig xrRig;
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;
    [SerializeField] private GameObject ballPrefab;

    [Header("Controller Settings")]
    [SerializeField] private float controllerRotationSpeed = 100f;
    [SerializeField] private float controllerPositionSpeed = 2f;

    private void Start()
    {
        SetupXR();
        SetupControllers();
        SetupBall();
    }

    private void SetupXR()
    {
        if (interactionManager == null)
        {
            interactionManager = FindObjectOfType<XRInteractionManager>();
            if (interactionManager == null)
            {
                GameObject managerObj = new GameObject("XR Interaction Manager");
                interactionManager = managerObj.AddComponent<XRInteractionManager>();
            }
        }

        if (xrRig == null)
        {
            xrRig = FindObjectOfType<XRRig>();
            if (xrRig == null)
            {
                Debug.LogError("XRRig not found in scene. Please add an XR Rig to your scene.");
                return;
            }
        }
    }

    private void SetupControllers()
    {
        if (leftController == null || rightController == null)
        {
            var controllers = FindObjectsOfType<ActionBasedController>();
            foreach (var controller in controllers)
            {
                if (controller.controllerNode == UnityEngine.XR.XRNode.LeftHand)
                {
                    leftController = controller;
                }
                else if (controller.controllerNode == UnityEngine.XR.XRNode.RightHand)
                {
                    rightController = controller;
                }
            }
        }

        if (leftController != null)
        {
            ConfigureController(leftController);
        }
        if (rightController != null)
        {
            ConfigureController(rightController);
        }
    }

    private void ConfigureController(ActionBasedController controller)
    {
        // Configure controller rotation
        controller.rotateAnchorAction.action.expectedControlType = "Vector2";
        controller.rotateAnchorAction.action.started += ctx => RotateController(controller, ctx.ReadValue<Vector2>());
        controller.rotateAnchorAction.action.canceled += ctx => RotateController(controller, Vector2.zero);

        // Configure controller position
        controller.translateAnchorAction.action.expectedControlType = "Vector2";
        controller.translateAnchorAction.action.started += ctx => MoveController(controller, ctx.ReadValue<Vector2>());
        controller.translateAnchorAction.action.canceled += ctx => MoveController(controller, Vector2.zero);
    }

    private void RotateController(ActionBasedController controller, Vector2 input)
    {
        if (input.magnitude > 0.1f)
        {
            Vector3 rotation = new Vector3(input.y, input.x, 0) * controllerRotationSpeed * Time.deltaTime;
            controller.transform.Rotate(rotation);
        }
    }

    private void MoveController(ActionBasedController controller, Vector2 input)
    {
        if (input.magnitude > 0.1f)
        {
            Vector3 movement = new Vector3(input.x, 0, input.y) * controllerPositionSpeed * Time.deltaTime;
            controller.transform.Translate(movement);
        }
    }

    private void SetupBall()
    {
        if (ballPrefab != null)
        {
            // Add XR Grab Interactable to ball prefab if it doesn't have one
            XRGrabInteractable grabInteractable = ballPrefab.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = ballPrefab.AddComponent<XRGrabInteractable>();
            }

            // Configure grab interactable
            grabInteractable.interactionManager = interactionManager;
            grabInteractable.throwOnDetach = true;
            grabInteractable.throwSmoothingDuration = 0.1f;
            grabInteractable.throwVelocityScale = 1.5f;
            grabInteractable.throwAngularVelocityScale = 1.5f;

            // Add collider if not present
            if (ballPrefab.GetComponent<Collider>() == null)
            {
                SphereCollider collider = ballPrefab.AddComponent<SphereCollider>();
                collider.radius = 0.5f; // Adjust based on your ball size
            }

            // Configure rigidbody
            Rigidbody rb = ballPrefab.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = ballPrefab.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    public void ResetVRPosition()
    {
        if (xrRig != null)
        {
            xrRig.MatchRigUpCameraForward(Vector3.up, Vector3.forward);
        }
    }

    public void RecenterVR()
    {
        if (xrRig != null)
        {
            xrRig.MatchRigUp(Vector3.up);
            xrRig.MatchRigForward(Vector3.forward);
        }
    }
} 