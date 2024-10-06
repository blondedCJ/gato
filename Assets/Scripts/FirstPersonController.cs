using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class FirstPersonController : MonoBehaviour
{
    // References
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private RectTransform movementPanel;
    [SerializeField] private RectTransform cameraPanel;// Reference to TreatController

    // Player settings
    [SerializeField] private float cameraSensitivity = 1f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float moveInputDeadZone = 0.1f;
    [SerializeField] private float smoothTime = 1f;
    [SerializeField] private float lookSmoothFactor = 0.1f;
    [SerializeField] private float dragDistanceThreshold = 100f;

    // Camera bobbing settings
    [SerializeField] private float walkBobbingSpeed = 14f;
    [SerializeField] private float walkBobbingAmount = 0.05f;
    [SerializeField] private float idleBobbingSpeed = 2f;
    [SerializeField] private float idleBobbingAmount = 0.02f;

    // Touch detection
    private int leftFingerId = -1, rightFingerId = -1;

    // Camera control
    private Vector2 lookInput;
    private Vector2 smoothLookInput;
    private float cameraPitch;

    // Player movement
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;
    private float currentSpeed;

    // Bobbing
    private float defaultCameraYPos;
    private float bobbingTimer = 0;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
        leftFingerId = -1;
        rightFingerId = -1;
        moveInputDeadZone = Mathf.Pow(Screen.height / moveInputDeadZone, 2);
        defaultCameraYPos = cameraTransform.localPosition.y;
    }

    void Update()
    {

        GetTouchInput();
        lookInput = Vector2.Lerp(lookInput, Vector2.zero, lookSmoothFactor);

        if (rightFingerId != -1)
        {
            LookAround();
        }

        if (leftFingerId != -1)
        {
            UpdatePlayerSpeed();
            Move();  // Move the player only if no treat is being spawned
            CameraBobbing();
        }
        else
        {
            ApplyBreathingEffect();
        }
    }

    void GetTouchInput()
    {
        foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            Vector2 touchPosition = touch.screenPosition;
            bool isWithinMovementPanel = IsTouchWithinPanel(movementPanel, touchPosition);
            bool isWithinCameraPanel = IsTouchWithinPanel(cameraPanel, touchPosition);

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    if (isWithinMovementPanel && leftFingerId == -1)
                    {
                        leftFingerId = touch.finger.index;
                        moveTouchStartPosition = touchPosition;
                    }
                    else if (isWithinCameraPanel && rightFingerId == -1)
                    {
                        rightFingerId = touch.finger.index;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    if (touch.finger.index == leftFingerId)
                    {
                        leftFingerId = -1;
                    }
                    else if (touch.finger.index == rightFingerId)
                    {
                        rightFingerId = -1;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                    if (touch.finger.index == rightFingerId && isWithinCameraPanel)
                    {
                        lookInput = touch.delta * cameraSensitivity * Time.deltaTime;
                    }
                    else if (touch.finger.index == leftFingerId && isWithinMovementPanel)
                    {
                        moveInput = touchPosition - moveTouchStartPosition;

                        // Only move when the drag is significant
                        if (moveInput.sqrMagnitude > moveInputDeadZone)
                        {
                            moveInput.Normalize();
                        }
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    if (touch.finger.index == rightFingerId && isWithinCameraPanel)
                    {
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }

    bool IsTouchWithinPanel(RectTransform panel, Vector2 touchPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(panel, touchPosition, Camera.main);
    }

    void LookAround()
    {
        if (rightFingerId != -1)
        {
            // Smoothly interpolate the look input using Lerp
            smoothLookInput = Vector2.Lerp(smoothLookInput, lookInput, smoothTime);

            // Vertical (pitch) rotation
            cameraPitch = Mathf.Clamp(cameraPitch - smoothLookInput.y, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

            // Horizontal (yaw) rotation
            transform.Rotate(Vector3.up, smoothLookInput.x);
        }
    }

    void UpdatePlayerSpeed()
    {
        float dragDistance = moveInput.magnitude;

        if (dragDistance > dragDistanceThreshold)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    void Move()
    {
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;

        Vector3 movement = (forward + right) * currentSpeed * Time.deltaTime;

        characterController.Move(movement);
    }


    void CameraBobbing()
    {
        if (characterController.velocity.magnitude > 0)
        {
            bobbingTimer += Time.deltaTime * walkBobbingSpeed;
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x,
                defaultCameraYPos + Mathf.Sin(bobbingTimer) * walkBobbingAmount,
                cameraTransform.localPosition.z);
        }
    }

    void ApplyBreathingEffect()
    {
        bobbingTimer += Time.deltaTime * idleBobbingSpeed;
        cameraTransform.localPosition = new Vector3(
            cameraTransform.localPosition.x,
            defaultCameraYPos + Mathf.Sin(bobbingTimer) * idleBobbingAmount,
            cameraTransform.localPosition.z);
    }
}
