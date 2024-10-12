using UnityEngine;
using UnityEngine.InputSystem;

public class CatPetting : MonoBehaviour
{
    private Camera arCamera;
    private Animator catAnimator;
    private PetStatus petStatus;

    // LayerMask for raycasting (only hits the interactive layer)
    public LayerMask interactionLayerMask;

    private float affectionIncreaseRate = 5f; // Rate at which affection increases per second
    public float pettingDistanceThreshold = 2.0f; // Distance threshold for petting

    private bool isPetting = false; // To track if the player is currently petting

    void Start()
    {
        arCamera = Camera.main;
        catAnimator = GetComponent<Animator>();
        petStatus = FindObjectOfType<PetStatus>();
    }

    void Update()
    {
        // Detect touch input for mobile or mouse click for PC
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            HandleTouch(Touchscreen.current.primaryTouch.position.ReadValue());
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            HandleTouch(Mouse.current.position.ReadValue());
        }
        else
        {
            // Stop petting when the input is released
            if (isPetting)
            {
                StopPetting();
            }
        }

        // Gradually increase affection while petting
        if (isPetting)
        {
            GraduallyIncreaseAffection();
        }
    }

    private void HandleTouch(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            float distanceToCat = Vector3.Distance(arCamera.transform.position, transform.position);
            if (distanceToCat <= pettingDistanceThreshold)
            {
                if (!isPetting) // Only start the petting process once
                {
                    StartPetting();
                }
            }
            else
            {
                Debug.Log("Too far to pet the cat!");
                StopPetting(); // Ensure petting stops if player moves too far
            }
        }
    }

    private void StartPetting()
    {
        isPetting = true;
        catAnimator.CrossFade("Caress_idle", 0.2f); // Start petting animation
        Debug.Log("Started petting the cat.");
    }

    private void StopPetting()
    {
        isPetting = false;
        catAnimator.CrossFade("Idle_1", 0.2f); // Transition back to idle or another default animation
        Debug.Log("Stopped petting the cat.");
    }

    private void GraduallyIncreaseAffection()
    {
        petStatus.PlayWithPet(affectionIncreaseRate * Time.deltaTime); // Gradually increase affection
        Debug.Log("Increasing affection...");
    }
}
