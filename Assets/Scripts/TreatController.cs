using UnityEngine;
using UnityEngine.InputSystem;

public class TreatController : MonoBehaviour
{
    [SerializeField] private GameObject treatPrefab; // Reference to the treat prefab
    [SerializeField] private GameObject feedPrefab; // Reference to the feed prefab

    public Camera mainCamera;
    public float spawnOffsetY = 1.0f; // Offset above the ground
    public float doubleClickTime = 0.3f; // Time window for double-click detection

    private bool isTreatButtonEnabled = false;
    private bool isFeedButtonEnabled = false; // New state for feed button
    private float lastMouseClickTime = 0f;
    private float lastTouchTapTime = 0f;
    private bool isItemPlaced = false; // Track if an item (treat or feed) is already placed

    public bool isSpawningTreat { get; private set; } // Flag to indicate spawning treat

    public PetAI petController; // Reference to the PetAI

    void Update()
    {
        // Reset the flag every frame
        isSpawningTreat = false;

        // Handle mouse double-click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastMouseClickTime <= doubleClickTime)
            {
                if (isTreatButtonEnabled)
                {
                    SpawnTreat(GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
                    isSpawningTreat = true; // Treat is being spawned
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnFeed(GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
                }
            }
            lastMouseClickTime = Time.time;
        }

        // Handle touch double-tap
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            if (Time.time - lastTouchTapTime <= doubleClickTime)
            {
                if (isTreatButtonEnabled)
                {
                    SpawnTreat(GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                    isSpawningTreat = true; // Treat is being spawned
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnFeed(GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
            }
            lastTouchTapTime = Time.time;
        }
    }

    // Treat Button
    public void OnTreatButtonClick()
    {
        isTreatButtonEnabled = !isTreatButtonEnabled; // Toggle treat spawning
        isFeedButtonEnabled = false; // Ensure feed button is disabled
    }

    // New Feed Button
    public void OnFeedButtonClick()
    {
        isFeedButtonEnabled = !isFeedButtonEnabled; // Toggle feed spawning
        isTreatButtonEnabled = false; // Ensure treat button is disabled
    }

    // Spawn Treat Logic
    private void SpawnTreat(Vector2 inputPosition)
    {
        if (inputPosition != Vector2.zero && !isItemPlaced) // Check if no item is currently placed
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnOffsetY, hit.point.z);
                GameObject treatInstance = Instantiate(treatPrefab, spawnPosition, Quaternion.identity); // No rotation
                Debug.Log($"Treat Spawned - Position: {spawnPosition}");

                // Notify the pet about the spawned treat
                petController.SetTreatTarget(treatInstance);
                isItemPlaced = true; // Set the flag to true when a treat is placed
            }
        }
        else
        {
            Debug.Log("An item is already placed! Please consume it before placing another.");
        }
    }

    // Spawn Feed Logic (similar to SpawnTreat)
    private void SpawnFeed(Vector2 inputPosition)
    {
        if (inputPosition != Vector2.zero && !isItemPlaced) // Check if no item is currently placed
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnOffsetY, hit.point.z);
                GameObject feedInstance = Instantiate(feedPrefab, spawnPosition, Quaternion.identity); // No rotation
                Debug.Log($"Feed Spawned - Position: {spawnPosition}");

                // Notify the pet about the spawned feed
                petController.SetFeedTarget(feedInstance);
                isItemPlaced = true; // Set the flag to true when feed is placed
            }
        }
        else
        {
            Debug.Log("An item is already placed! Please consume it before placing another.");
        }
    }

    // Call this method when the treat or feed is consumed
    public void ItemConsumed()
    {
        isItemPlaced = false; // Reset the flag to allow new placements
    }

    private Vector2 GetMouseOrTouchPosition(Vector2 inputPosition)
    {
        return inputPosition;
    }
}
