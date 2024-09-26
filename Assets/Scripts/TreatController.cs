using UnityEngine;
using UnityEngine.InputSystem;

public class TreatController : MonoBehaviour
{
    public GameObject treatPrefab;
    public Camera mainCamera;
    public float spawnOffsetY = 1.0f; // Offset above the ground
    public float doubleClickTime = 0.3f; // Time window for double-click detection

    private bool isTreatButtonEnabled = false;
    private float lastMouseClickTime = 0f;
    private float lastTouchTapTime = 0f;

    public bool isSpawningTreat { get; private set; } // Flag to indicate spawning treat

    public PetAI petController; // Reference to the PetController

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
            }
            lastTouchTapTime = Time.time;
        }
    }

    public void OnTreatButtonClick()
    {
        isTreatButtonEnabled = !isTreatButtonEnabled; // Toggle treat spawning
    }

private void SpawnTreat(Vector2 inputPosition)
{
    if (inputPosition != Vector2.zero)
    {
        Ray ray = mainCamera.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Spawn at the hit point with a Y offset of 1.5
            Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + 1.5f, hit.point.z); // Add offset to Y
            GameObject treatInstance = Instantiate(treatPrefab, spawnPosition, Quaternion.identity); // No rotation
            Debug.Log($"Treat Spawned - Position: {spawnPosition}");

            // Notify the pet about the spawned treat
            petController.SetTreatTarget(treatInstance);
        }
    }
}

    private Vector2 GetMouseOrTouchPosition(Vector2 inputPosition)
    {
        return inputPosition;
    }
}
