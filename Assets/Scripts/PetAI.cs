using UnityEngine;
using System.Collections;

public class PetAI : MonoBehaviour
{
    public float movementSpeed = 2f; // Speed at which the pet moves
    private GameObject currentTreatTarget; // The treat the pet is moving toward

    public bool isMovingToTreat = false; // Check if the pet is moving towards a treat

    private PetStatus petStatus; // Reference to PetStatus for updating hunger

    [SerializeField] private GameObject treatPrefab; // Serialized field for the treat prefab

    // Adjusted minimum distance for consuming the treat
    private float treatConsumeDistance = 1.6f;  // Now set to 1.6 to trigger consumption when the pet gets closer

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
        Debug.Log("Treat target set: " + treat.name);
        currentTreatTarget = treat; // Assign the new treat as the target
        isMovingToTreat = true;
    }

    private void MoveTowardsTreat()
    {
        if (currentTreatTarget == null)
        {
            Debug.LogWarning("No treat target!");
            return;
        }

        // Disable random wandering in RandomMovement
        RandomMovement randomMovement = GetComponent<RandomMovement>();
        randomMovement.MoveToTreat(currentTreatTarget.transform.position);

        Vector3 direction = (currentTreatTarget.transform.position - transform.position).normalized;
        float step = movementSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentTreatTarget.transform.position, step);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float distanceToTreat = Vector3.Distance(transform.position, currentTreatTarget.transform.position);
        if (distanceToTreat <= treatConsumeDistance)
        {
            // Start the coroutine to wait and then consume the treat
            StartCoroutine(WaitAndConsumeTreat());
            // After consuming the treat, resume wandering
            randomMovement.ResumeWandering();
        }
    }

    // Coroutine to wait 2 seconds before consuming the treat
    private IEnumerator WaitAndConsumeTreat()
    {
        // Stop the pet from moving during the waiting time
        isMovingToTreat = false;

        Debug.Log("Pet arrived at the treat. Waiting for 2 seconds to eat...");

        // Wait for 2 seconds to mimic the pet eating
        yield return new WaitForSeconds(2f);

        ConsumeTreat(); // Consume the treat after waiting
    }

    private void ConsumeTreat()
    {
        if (currentTreatTarget == null)
        {
            Debug.LogWarning("No treat to consume!");
            return;
        }

        Debug.Log("Pet consumed the treat: " + currentTreatTarget.name);

        // Increase hunger
        petStatus.IncreaseHungerBy(10f);

        // Log the hunger value to verify
        Debug.Log($"Current Hunger Value: {petStatus.hunger}");

        // Destroy the treat immediately
        Destroy(currentTreatTarget);

        // Stop moving
        currentTreatTarget = null;
    }
}
