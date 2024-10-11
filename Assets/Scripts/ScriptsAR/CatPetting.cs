using UnityEngine;
using UnityEngine.InputSystem;

public class CatPetting : MonoBehaviour
{
    private Camera arCamera;
    private Animator catAnimator;
    private PetStatus petStatus;

    // LayerMask for raycasting (only hits the interactive layer)
    public LayerMask interactionLayerMask;

    private float affectionIncreaseAmount = 5f;

    // Distance threshold for petting the cat
    public float pettingDistanceThreshold = 2.0f; // Adjust this value as needed

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
    }

    private void HandleTouch(Vector2 touchPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(touchPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction); // Use Physics2D.Raycast

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            // Check distance from the camera to the cat
            float distanceToCat = Vector3.Distance(arCamera.transform.position, transform.position);
            if (distanceToCat <= pettingDistanceThreshold)
            {
                PetCat(); // Only allow petting if within threshold
            }
            else
            {
                Debug.Log("Too far to pet the cat!");
            }
        }
    }

    private void PetCat()
    {
        catAnimator.CrossFade("Caress_idle", 0.2f);
        petStatus.IncreaseAffection(affectionIncreaseAmount);
        Debug.Log("Cat petted! Affection increased.");
    }
}
