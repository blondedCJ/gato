using UnityEngine;
using UnityEngine.InputSystem;

public class TreatController : MonoBehaviour
{
    [SerializeField] private GameObject treatPrefab;
    [SerializeField] private GameObject feedPrefab;

    public Camera mainCamera;
    public float spawnOffsetY = 1.0f;
    public float doubleClickTime = 0.3f;

    private bool isTreatButtonEnabled = false;
    private bool isFeedButtonEnabled = false;
    private float lastMouseClickTime = 0f;
    private float lastTouchTapTime = 0f;
    private bool isItemPlaced = false;
    public bool isSpawningTreat { get; private set; }

    void Update()
    {
        isSpawningTreat = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastMouseClickTime <= doubleClickTime)
            {
                if (isTreatButtonEnabled)
                {
                    SpawnTreat(GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
                    isSpawningTreat = true;
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnFeed(GetMouseOrTouchPosition(Mouse.current.position.ReadValue()));
                }
            }
            lastMouseClickTime = Time.time;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            if (Time.time - lastTouchTapTime <= doubleClickTime)
            {
                if (isTreatButtonEnabled)
                {
                    SpawnTreat(GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                    isSpawningTreat = true;
                }
                else if (isFeedButtonEnabled)
                {
                    SpawnFeed(GetMouseOrTouchPosition(Touchscreen.current.primaryTouch.position.ReadValue()));
                }
            }
            lastTouchTapTime = Time.time;
        }
    }

    public void OnTreatButtonClick()
    {
        isTreatButtonEnabled = !isTreatButtonEnabled;
        isFeedButtonEnabled = false;
    }

    public void OnFeedButtonClick()
    {
        isFeedButtonEnabled = !isFeedButtonEnabled;
        isTreatButtonEnabled = false;
    }

    private void SpawnTreat(Vector2 inputPosition)
    {
        if (inputPosition != Vector2.zero && !isItemPlaced && !IsAnyCatConsuming())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnOffsetY, hit.point.z);
                GameObject treatInstance = Instantiate(treatPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"Treat Spawned - Position: {spawnPosition}");

                MoveClosestCatTo(spawnPosition, treatInstance);
                isItemPlaced = true;
            }
        }
        else
        {
            Debug.Log("An item is already placed or a cat is consuming! Please wait before placing another.");
        }
    }

    private void SpawnFeed(Vector2 inputPosition)
    {
        if (inputPosition != Vector2.zero && !isItemPlaced && !IsAnyCatConsuming())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y + spawnOffsetY, hit.point.z);
                GameObject feedInstance = Instantiate(feedPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"Feed Spawned - Position: {spawnPosition}");

                MoveClosestCatTo(spawnPosition, feedInstance);
                isItemPlaced = true;
            }
        }
        else
        {
            Debug.Log("An item is already placed or a cat is consuming! Please wait before placing another.");
        }
    }

    private void MoveClosestCatTo(Vector3 targetPosition, GameObject targetObject)
    {
        CatBehavior[] cats = FindObjectsOfType<CatBehavior>();
        CatBehavior closestCat = null;
        float closestDistance = float.MaxValue;

        foreach (CatBehavior cat in cats)
        {
            float distance = Vector3.Distance(cat.transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCat = cat;
            }
        }

        if (closestCat != null)
        {
            closestCat.MoveTo(targetPosition, targetObject, this); // Pass the TreatController reference
            Debug.Log($"Closest cat moving to position: {targetPosition}");
        }
    }

    private Vector2 GetMouseOrTouchPosition(Vector2 inputPosition)
    {
        return inputPosition;
    }

    private bool IsAnyCatConsuming()
    {
        CatBehavior[] cats = FindObjectsOfType<CatBehavior>();
        foreach (CatBehavior cat in cats)
        {
            if (cat.currentState == CatBehavior.CatState.Consuming)
            {
                return true;
            }
        }
        return false;
    }

    public void ResetItemPlacedFlag()
    {
        isItemPlaced = false;
    }
}