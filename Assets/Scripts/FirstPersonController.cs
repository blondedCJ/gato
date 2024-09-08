using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class FirstPersonController : MonoBehaviour
{
    // References
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private RectTransform movementPanel;
    [SerializeField] private RectTransform cameraPanel;

    // Player settings
    [SerializeField] private float cameraSensitivity = 1f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float moveInputDeadZone = 0.1f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float lookSmoothFactor = 0.1f;
    [SerializeField] private float dragDistanceThreshold = 100f;

    // Camera bobbing settings
    [SerializeField] private float walkBobbingSpeed = 14f;
    [SerializeField] private float walkBobbingAmount = 0.05f;
    [SerializeField] private float idleBobbingSpeed = 2f;
    [SerializeField] private float idleBobbingAmount = 0.02f;

    // Touch detection
    private int leftFingerId = -1, rightFingerId = -1;
    private float halfScreenWidth;

    // Camera control
    private Vector2 lookInput;
    private Vector2 smoothLookInput;
    private Vector2 lookInputVelocity;
    private float cameraPitch;

    // Player movement
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;
    private float currentSpeed;
    private bool isDragging = false;

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
        halfScreenWidth = Screen.width / 2;
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

        if (leftFingerId != -1 && isDragging)
        {
            UpdatePlayerSpeed();
            Move();
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
            bool isWithinMovementPanel = IsTouchWithinPanel(movementPanel);
            bool isWithinCameraPanel = IsTouchWithinPanel(cameraPanel);

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    if (touchPosition.x < Screen.width / 2 && isWithinMovementPanel && leftFingerId == -1)
                    {
                        leftFingerId = touch.finger.index;
                        moveTouchStartPosition = touchPosition;
                        isDragging = false; // Reset dragging flag
                    }
                    else if (touchPosition.x >= Screen.width / 2 && isWithinCameraPanel && rightFingerId == -1)
                    {
                        rightFingerId = touch.finger.index;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    if (touch.finger.index == leftFingerId)
                    {
                        leftFingerId = -1;
                        isDragging = false; // Reset dragging flag
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
                        // Set dragging to true only when moving
                        isDragging = true;
                    }
                    else if (touch.finger.index == leftFingerId && isWithinMovementPanel)
                    {
                        moveInput = touchPosition - moveTouchStartPosition;

                        // Set dragging to true only when moving significantly
                        if (moveInput.sqrMagnitude > moveInputDeadZone)
                        {
                            isDragging = true;
                        }
                    }
                    else if (touch.finger.index == leftFingerId)
                    {
                        // Reset dragging if the touch moves outside the panel
                        isDragging = false;
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


    bool IsTouchWithinPanel(RectTransform panel)
    {
        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        return RectTransformUtility.RectangleContainsScreenPoint(panel, touchPosition, Camera.main);
    }


    void LookAround()
    {
        if (rightFingerId != -1 && isDragging)
        {
            // Smoothly interpolate the look input using Lerp
            smoothLookInput = Vector2.Lerp(smoothLookInput, lookInput, smoothTime);

            // Vertical (pitch) rotation
            cameraPitch = Mathf.Clamp(cameraPitch - smoothLookInput.y, -90f, 90f);
            Quaternion targetRotation = Quaternion.Euler(cameraPitch, 0, 0);
            cameraTransform.localRotation = targetRotation;

            // Horizontal (yaw) rotation
            transform.Rotate(transform.up, smoothLookInput.x);
        }
        else
        {
            // Smoothly reset look input to zero
            smoothLookInput = Vector2.Lerp(smoothLookInput, Vector2.zero, smoothTime * 2f);
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
        if (moveInput.sqrMagnitude <= moveInputDeadZone) return;

        Debug.Log($"Movement Input: {moveInput}");

        Vector2 movementDirection = moveInput.normalized * currentSpeed * Time.deltaTime;
        characterController.Move(transform.right * movementDirection.x + transform.forward * movementDirection.y);
    }

    void CameraBobbing()
    {
        bobbingTimer += Time.deltaTime * walkBobbingSpeed;
        float newY = defaultCameraYPos + Mathf.Sin(bobbingTimer) * walkBobbingAmount;
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, newY, cameraTransform.localPosition.z);
    }

    void ApplyBreathingEffect()
    {
        bobbingTimer += Time.deltaTime * idleBobbingSpeed;
        float newY = defaultCameraYPos + Mathf.Sin(bobbingTimer) * idleBobbingAmount;
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, newY, cameraTransform.localPosition.z);
    }
}
