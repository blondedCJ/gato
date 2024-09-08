using UnityEngine;

public class PetAI : MonoBehaviour
{
    public float movementSpeed = 2f; // Speed at which the pet moves
    private GameObject currentTreatTarget; // The treat the pet is moving toward

    private bool isMovingToTreat = false; // Check if the pet is moving towards a treat

    private PetStatus petStatus; // Reference to PetStatus for updating hunger

    void Start()
    {
        petStatus = GetComponent<PetStatus>();
    }

    void Update()
    {
        if (isMovingToTreat && currentTreatTarget != null)
        {
            MoveTowardsTreat();
        }
    }

    public void SetTreatTarget(GameObject treat)
    {
        currentTreatTarget = treat; // Assign the new treat as the target
        isMovingToTreat = true;
    }

    private void MoveTowardsTreat()
    {
        Vector3 direction = (currentTreatTarget.transform.position - transform.position).normalized;
        float step = movementSpeed * Time.deltaTime;

        // Smoothly move the pet towards the treat
        transform.position = Vector3.MoveTowards(transform.position, currentTreatTarget.transform.position, step);

        // Ensure the pet is facing the direction it's moving
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Check if the pet is close enough to the treat
        if (Vector3.Distance(transform.position, currentTreatTarget.transform.position) < 0.1f)
        {
            ConsumeTreat();
        }
    }

    private void ConsumeTreat()
    {
        Debug.Log("Pet consumed the treat!");

        // Increase hunger
        petStatus.IncreaseHungerBy(10f);

        // Log the hunger value to verify
        Debug.Log($"Current Hunger Value: {petStatus.hunger}");

        // Destroy the treat after 2 seconds
        Destroy(currentTreatTarget, 2f);

        // Stop moving
        isMovingToTreat = false;
        currentTreatTarget = null;
    }
}
