using UnityEngine;
using UnityEngine.InputSystem;
using Niantic.Lightship.AR.NavigationMesh;
using static ObjectPlacer;

public class TreatControllerAR : MonoBehaviour
{
    [SerializeField] private GameObject treatPrefab; // Reference to the treat prefab
    [SerializeField] private GameObject feedPrefab;  // Reference to the feed prefab
    [SerializeField] private LightshipNavMeshManager lightshipNavMeshManager; // Reference to the Lightship NavMesh Manager

    public delegate void ItemPlacedHandler(Vector3 position);
    public event ItemPlacedHandler OnTreatPlaced;
    public event ItemPlacedHandler OnFeedPlaced;

    public Camera mainCamera;
    public float spawnOffsetY = 1.0f;                // Offset above the ground
    public float doubleClickTime = 0.3f;             // Time window for double-click detection

    private bool isTreatButtonEnabled = false;       // Treat button state
    private bool isFeedButtonEnabled = false;        // Feed button state
    private float lastMouseClickTime = 0f;           // Track last mouse click time
    private float lastTouchTapTime = 0f;             // Track last touch tap time
    public bool isItemPlaced = false;               // Track if an item is already placed
               // Reference to the PetAI for notifying treat/feed placement

    // Update is called once per frame
    void Update()
    {
        // Handle mouse double-click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastMouseClickTime <= doubleClickTime)
            {
                if (isTreatButtonEnabled)
                {
                    SpawnItemWithinNavMesh(treatPrefab, GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnItemWithinNavMesh(feedPrefab, GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
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
                    SpawnItemWithinNavMesh(treatPrefab, GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnItemWithinNavMesh(feedPrefab, GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
            }
            lastTouchTapTime = Time.time;
        }
    }

    // Handle Treat Button Click
    public void OnTreatButtonClick()
    {
        isTreatButtonEnabled = !isTreatButtonEnabled; // Toggle treat placement
        isFeedButtonEnabled = false;                  // Disable feed placement
        Debug.Log("Treat placement enabled: " + isTreatButtonEnabled);
    }

    // Handle Feed Button Click
    public void OnFeedButtonClick()
    {
        isFeedButtonEnabled = !isFeedButtonEnabled;   // Toggle feed placement
        isTreatButtonEnabled = false;                 // Disable treat placement
        Debug.Log("Feed placement enabled: " + isFeedButtonEnabled);
    }

    // Spawn item only if it's within the NavMesh (Treat or Feed)
    private void SpawnItemWithinNavMesh(GameObject prefab, Vector2 inputPosition)
    {
        if (inputPosition != Vector2.zero && !isItemPlaced)
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPosition = hit.point;

                // Check if hit position is within the Lightship NavMesh
                if (IsPositionOnNavMesh(hitPosition))
                {
                    Vector3 spawnPosition = new Vector3(hitPosition.x, hitPosition.y + spawnOffsetY, hitPosition.z);
                    GameObject itemInstance = Instantiate(prefab, spawnPosition, Quaternion.identity); // Instantiate item
                    Debug.Log($"{prefab.name} spawned at position: {spawnPosition}");

                    isItemPlaced = true; // Mark item as placed

                    // Trigger the appropriate event
                    if (prefab == treatPrefab)
                    {
                        OnTreatPlaced?.Invoke(spawnPosition);
                    }
                    else if (prefab == feedPrefab)
                    {
                        OnFeedPlaced?.Invoke(spawnPosition);
                    }
                }
                else
                {
                    Debug.Log("Cannot place item outside the NavMesh!");
                }
            }
        }
        else
        {
            Debug.Log("An item is already placed! Please consume it before placing another.");
        }
    }


// Utility function to get mouse/touch position
private Vector2 GetMouseOrTouchPosition(Vector2 inputPosition)
    {
        return inputPosition;
    }

    // Check if the position is within the Lightship NavMesh
    public bool IsPositionOnNavMesh(Vector3 position)
    {
        // Use LightshipNavMeshManager to check if position is within a valid surface
        var surfaces = lightshipNavMeshManager.LightshipNavMesh.Surfaces;
        foreach (var surface in surfaces)
        {
            foreach (var element in surface.Elements)
            {
                Vector3 tileCenter = NavMeshUtils.TileToPosition(element.Coordinates, surface.Elevation, lightshipNavMeshManager.LightshipNavMesh.Settings.TileSize);
                float halfTileSize = lightshipNavMeshManager.LightshipNavMesh.Settings.TileSize / 2f;

                // Check if the position falls within the boundaries of this tile
                if (position.x >= tileCenter.x - halfTileSize && position.x <= tileCenter.x + halfTileSize &&
                    position.z >= tileCenter.z - halfTileSize && position.z <= tileCenter.z + halfTileSize)
                {
                    return true; // Position is within this tile of the NavMesh
                }
            }
        }
        return false; // Position is not within the navigation mesh
    }
}
