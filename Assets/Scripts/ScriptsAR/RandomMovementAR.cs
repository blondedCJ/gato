using System.Collections;
using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;

public class RandomMovementAR : MonoBehaviour
{
    private LightshipNavMeshAgent navMeshAgent;
    private TreatControllerAR treatController;
    private PetStatus petStatus; // Reference to the PetStatus component
    private float wanderInterval = 5.0f;
    private float wanderRadius = 3.0f;
    private bool isWandering = true;
    private bool isConsuming = false;

    // Distance to stop from the item
    private float stopDistance = 0.2f; // Adjust this value as needed


    private void Start()
    {
        navMeshAgent = GetComponent<LightshipNavMeshAgent>();
        treatController = GameObject.FindObjectOfType<TreatControllerAR>();
        petStatus = GameObject.FindObjectOfType<PetStatus>(); // Find the PetStatus component

        if (treatController != null)
        {
            // Subscribe to the events
            treatController.OnTreatPlaced += GoToItem;
            treatController.OnFeedPlaced += GoToItem;
        }

        StartCoroutine(WanderRoutine());
    }

    private void OnDestroy()
    {
        if (treatController != null)
        {
            // Unsubscribe from the events to prevent memory leaks
            treatController.OnTreatPlaced -= GoToItem;
            treatController.OnFeedPlaced -= GoToItem;
        }
    }

    private void GoToItem(Vector3 itemPosition)
    {
        if (isConsuming)
            return;

        // Calculate direction and stop position
        Vector3 directionToItem = (itemPosition - transform.position).normalized;
        Vector3 targetPosition = itemPosition - directionToItem * stopDistance;

        // Stop wandering and go to the item
        isWandering = false;
        navMeshAgent.SetDestination(targetPosition);

        // Rotate the cat to face the item
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToItem.x, 0, directionToItem.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(wanderInterval);

            if (isWandering && !isConsuming)
            {
                SetRandomDestination();
            }
        }
    }

    private void Update()
    {
        if (navMeshAgent.State == LightshipNavMeshAgent.AgentNavigationState.Idle && !isConsuming)
        {
            if (!isWandering)
            {
                // Check if the destination was a treat or feed
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Treat"))
                    {
                        StartCoroutine(ConsumeItem(hitCollider.gameObject, 3f, 10f)); // 3 seconds for treat, increases hunger by 10
                        return;
                    }
                    else if (hitCollider.CompareTag("Feed"))
                    {
                        StartCoroutine(ConsumeItem(hitCollider.gameObject, 5f, 50f)); // 10 seconds for feed, increases hunger by 50
                        return;
                    }
                }
            }
            else
            {
                SetRandomDestination();
            }
        }
    }

    private IEnumerator ConsumeItem(GameObject item, float consumeTime, float hungerIncrease)
    {
        isConsuming = true;
        Debug.Log($"Consuming {item.name} for {consumeTime} seconds.");

        // Wait for the specified consume time to simulate consuming the feed
        yield return new WaitForSeconds(consumeTime);

        // Check if the item itself is a treat
        if (item.CompareTag("Treat"))
        {
            Destroy(item);
            Debug.Log("Treat consumed.");
        }
        else
        {
            // Find the CatFood and Bowl as child objects
            Transform catFood = item.transform.Find("CatFood");
            Transform bowl = item.transform.Find("Bowl");

            // Destroy the CatFood after consumeTime
            if (catFood != null)
            {
                Destroy(catFood.gameObject);
                Debug.Log("CatFood consumed.");
            }
        }

        // Increase the pet's hunger after consuming the feed
        if (petStatus != null)
        {
            petStatus.IncreaseHungerBy(hungerIncrease);
        }

        // Allow the pet to start wandering again
        isWandering = true;
        isConsuming = false; // Set isConsuming to false so the pet can wander

        // Start a new coroutine to handle the destruction of the Bowl
        Transform bowlTransform = item.transform.Find("Bowl");
        if (bowlTransform != null)
        {
            StartCoroutine(DestroyBowlAfterDelay(bowlTransform, 5f));
        }

        // Reset the item placement flag in TreatControllerAR
        if (treatController != null)
        {
            treatController.isItemPlaced = false;
        }
    }

    private IEnumerator DestroyBowlAfterDelay(Transform bowl, float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Destroy the Bowl
        if (bowl != null)
        {
            Destroy(bowl.gameObject);
            Debug.Log("Bowl consumed.");
        }
    }

    private void SetRandomDestination()
    {
        if (navMeshAgent == null || treatController == null)
        {
            Debug.LogWarning("NavMeshAgent or TreatControllerAR is not set up properly.");
            return;
        }

        Vector3 randomPosition = GetRandomPositionWithinRadius(transform.position, wanderRadius);

        if (treatController.IsPositionOnNavMesh(randomPosition))
        {
            navMeshAgent.SetDestination(randomPosition);
        }
        else
        {
            SetRandomDestination();
        }
    }

    private Vector3 GetRandomPositionWithinRadius(Vector3 center, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPosition = new Vector3(center.x + randomDirection.x, center.y, center.z + randomDirection.y);

        return randomPosition;
    }
}