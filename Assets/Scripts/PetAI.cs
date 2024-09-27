using UnityEngine;
using System.Collections;

public class PetAI : MonoBehaviour
{
    public float movementSpeed; // Speed at which the pet moves
    private GameObject currentTreatTarget; // The treat the pet is moving toward
    private GameObject currentFeedTarget;  // The feed the pet is moving toward
    public Animator animator;
    public bool isMovingToTreat = false; // Check if the pet is moving towards a treat
    public bool isMovingToFeed = false;  // Check if the pet is moving towards feed
    public bool IsConsuming { get; private set; } = false;  // Public property to track consumption

    private PetStatus petStatus; // Reference to PetStatus for updating hunger

    [SerializeField] private GameObject treatPrefab; // Serialized field for the treat prefab
    [SerializeField] private GameObject feedPrefab;  // Serialized field for the feed prefab

    // Adjusted minimum distance for consuming the treat
    private float treatConsumeDistance = 1.6f;  // Now set to 1.6 to trigger consumption when the pet gets closer

    private float feedConsumeDuration = 5f;  // Time to consume the feed
    private float feedHungerIncrease = 50f;  // Amount to increase hunger by half



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
        else if (isMovingToFeed && currentFeedTarget != null)
        {
            MoveTowardsFeed();
        }
    }

    public void SetTreatTarget(GameObject treat)
    {
        Debug.Log("Treat target set: " + treat.name);
        currentTreatTarget = treat; // Assign the new treat as the target
        isMovingToTreat = true;
    }

    public void SetFeedTarget(GameObject feed)
    {
        Debug.Log("Feed target set: " + feed.name);
        currentFeedTarget = feed; // Assign the new feed as the target
        isMovingToFeed = true;
    }

    private void MoveTowardsTreat()
    {
        if (currentTreatTarget == null)
        {
            Debug.LogWarning("No treat target!");
            return;
        }
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", true);

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
            StartCoroutine(WaitAndConsumeTreat());
            randomMovement.ResumeWandering();
        }
    }

    private void MoveTowardsFeed()
    {
        if (currentFeedTarget == null)
        {
            Debug.LogWarning("No feed target!");
            return;
        }

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", true);

        // Define the radius around the feed object
        float radius = 3.8f; // Adjust the radius as needed

        // Generate a random point within the specified radius
        Vector3 randomOffset = Random.insideUnitSphere * radius;

        // Ensure the random point is on the same plane as the feed object (optional)
        randomOffset.y = 0; // Set Y to 0 if you want to keep it on the same plane

        // Calculate the target position with the random offset
        Vector3 targetPosition = currentFeedTarget.transform.position + randomOffset;

        RandomMovement randomMovement = GetComponent<RandomMovement>();
        randomMovement.MoveToTreat(targetPosition); // Use the modified target position

        // Calculate the direction to the feed object
        Vector3 directionToFeed = (currentFeedTarget.transform.position - transform.position).normalized;
        float step = movementSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        float distanceToFeed = Vector3.Distance(transform.position, targetPosition);

        // Rotate towards the feed only if not too close, and apply more control on rotation
        if (distanceToFeed > treatConsumeDistance + 0.5f) // Add a slight buffer to stop rotating when close
        {
            if (directionToFeed != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToFeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else
        {
            // Freeze rotation when close to the feed to avoid jitter
            transform.rotation = Quaternion.LookRotation(currentFeedTarget.transform.position - transform.position);
        }

        // Once within the consumption range, start consuming the feed
        if (distanceToFeed <= treatConsumeDistance)
        {
            StartCoroutine(ConsumeFeed());
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdling", true);
            // No need to resume wandering here, let the coroutine handle it
        }
    }





    private IEnumerator WaitAndConsumeTreat()
    {
        isMovingToTreat = false;

        Debug.Log("Pet arrived at the treat. Waiting for 2 seconds to eat...");
        yield return new WaitForSeconds(2f);
        animator.SetBool("isRunning", false);
        animator.SetBool("isIdling", true);
        ConsumeTreat();
    }

    private void ConsumeTreat()
    {
        if (currentTreatTarget == null)
        {
            Debug.LogWarning("No treat to consume!");
            return;
        }

        

        Debug.Log("Pet consumed the treat: " + currentTreatTarget.name);
        petStatus.IncreaseHungerBy(10f);
        animator.SetBool("isRunning", false);


        Destroy(currentTreatTarget);
        currentTreatTarget = null;

        // Notify TreatController that the treat has been consumed
        FindObjectOfType<TreatController>().ItemConsumed();
    }

    private IEnumerator ConsumeFeed()
    {
        isMovingToFeed = false;  // Stop moving towards the feed
        IsConsuming = true;      // Start consumption

        Debug.Log("Pet arrived at the feed. Consuming for 5 seconds...");
        yield return new WaitForSeconds(feedConsumeDuration);

        if (currentFeedTarget != null)
        {
            Debug.Log("Pet consumed the feed: " + currentFeedTarget.name);
            petStatus.IncreaseHungerBy(feedHungerIncrease);
            animator.SetBool("isRunning", false);

            // Assuming the feed prefab has two children: "CatFood" and "Bowl"
            Transform catFood = currentFeedTarget.transform.Find("CatFood");
            Transform bowl = currentFeedTarget.transform.Find("Bowl");

            // Destroy the cat food immediately
            if (catFood != null)
            {
                Debug.Log("Destroying cat food...");
                Destroy(catFood.gameObject);
                IsConsuming = false;
                // Resume wandering after consuming food
                RandomMovement randomMovement = GetComponent<RandomMovement>();
                randomMovement.ResumeWandering();
            }

            // Wait for 10 seconds before destroying the bowl
            if (bowl != null)
            {
                Debug.Log("Waiting 10 seconds to destroy the bowl...");
                yield return new WaitForSeconds(10f);
                Destroy(bowl.gameObject);
            }

            // Clear the feed target and notify TreatController that the feed has been consumed
            currentFeedTarget = null; // Clear the feed target
            FindObjectOfType<TreatController>().ItemConsumed(); // Notify TreatController
        }

        // Ensure the pet is ready to move to a new feed target if one is set
        if (currentFeedTarget != null)
        {
            SetFeedTarget(currentFeedTarget); // Reset to the current feed target for movement
        }
    }


}



