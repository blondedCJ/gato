using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class CatPetting : MonoBehaviour
{
    private Camera arCamera;
    private Animator catAnimator; // Reference to the cat's Animator
    private PetStatus petStatus;  // Reference to the PetStatus script

    // Threshold for how much affection increases per pet
    private float affectionIncreaseAmount = 5f;

    // This variable will be true when the player is in the petting area
    private bool isInPettingArea;

    void Start()
    {
        arCamera = Camera.main; // Use the main AR camera
        catAnimator = GetComponent<Animator>(); // Get the Animator component of the cat
        petStatus = FindObjectOfType<PetStatus>(); // Find the PetStatus component
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
    }

    private void HandleTouch(Vector2 touchPosition)
    {
        // Only attempt to pet if in the petting area
        if (isInPettingArea)
        {
            PetCat(); // Try to pet the cat
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other collider is the camera (player)
        if (other.CompareTag("Player")) // Ensure the player camera has this tag
        {
            isInPettingArea = true;
            Debug.Log("You can now pet the cat!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the player camera exited the trigger
        {
            isInPettingArea = false;
            Debug.Log("You left the petting area.");
        }
    }

    private void PetCat()
    {
        // Play petting animation
        catAnimator.CrossFade("PettingAnimation", 0.2f); // Ensure you have a petting animation in the Animator

        // Increase affection
        petStatus.IncreaseAffection(affectionIncreaseAmount);

        Debug.Log("Cat petted! Affection increased.");
    }
}
