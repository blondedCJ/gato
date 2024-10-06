using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FeedingSystem : MonoBehaviour
{
    public GameObject treatPrefab;
    public GameObject catFoodPrefab;
    public Button treatButton;
    public Button feedButton;

    private bool canSpawnTreat = false;
    private bool canSpawnCatFood = false;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // Time threshold for double-click

    private InputAction clickAction;

    void Awake()
    {
        // Create a new InputAction for detecting clicks or taps
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
        Debug.Log("InputAction created");
    }

    void OnEnable()
    {
        if (clickAction != null)
        {
            // Enable the input action
            clickAction.Enable();
            clickAction.performed += OnClickPerformed;
            Debug.Log("InputAction enabled and event subscribed");
        }
        else
        {
            Debug.LogError("clickAction is null in OnEnable");
        }
    }

    void OnDisable()
    {
        if (clickAction != null)
        {
            // Disable the input action
            clickAction.performed -= OnClickPerformed;
            clickAction.Disable();
            Debug.Log("InputAction disabled and event unsubscribed");
        }
        else
        {
            Debug.LogError("clickAction is null in OnDisable");
        }
    }

    void Start()
    {
        // Assign button click listeners
        if (treatButton != null && feedButton != null)
        {
            treatButton.onClick.AddListener(() => SetSpawnMode(true, false));
            feedButton.onClick.AddListener(() => SetSpawnMode(false, true));
            Debug.Log("Button listeners assigned");
        }
        else
        {
            Debug.LogError("Buttons are not assigned in the inspector");
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            SpawnFood();
        }
        lastClickTime = Time.time;
    }

    void SetSpawnMode(bool treatMode, bool feedMode)
    {
        canSpawnTreat = treatMode;
        canSpawnCatFood = feedMode;
    }

    void SpawnFood()
    {
        Vector3 spawnPosition = GetMouseWorldPosition();

        if (canSpawnTreat)
        {
            Instantiate(treatPrefab, spawnPosition, Quaternion.identity);
        }
        else if (canSpawnCatFood)
        {
            Instantiate(catFoodPrefab, spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Camera.main.nearClipPlane; // Set a distance from the camera
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}