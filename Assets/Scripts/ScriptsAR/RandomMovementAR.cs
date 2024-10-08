using System.Collections;
using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using static ObjectPlacer;

public class RandomMovementAR : MonoBehaviour
{
    private LightshipNavMeshAgent navMeshAgent;
    private LightshipNavMeshManager lightshipNavMeshManager; // Reference to the NavMesh Manager
    private Animator animator; // Reference to the Animator component
    private float wanderInterval; // Randomized duration
    private float wanderRadius = 3.0f;
    private bool isWandering = true;
    private float idleAnimationCooldown = 5f; // Time to wait before playing a new idle animation
    private float lastIdleAnimationTime = 0f; // Time when the last idle animation was played

    public bool IsWandering => isWandering;

    // Store the last destination
    private Vector3? lastDestination = null;

    // List of idle animation names
    private string[] idleAnimations = { "Idle_1", "Idle_2", "Idle_3", "Idle_4", "Idle_5", "Idle_6", "Idle_7", "Idle_8", "SharpenClaws_Horiz" };

    private string currentAnimation = ""; // Track the current animation

    private void Start()
    {
        navMeshAgent = GetComponent<LightshipNavMeshAgent>();
        animator = GetComponent<Animator>(); // Get the Animator component
        lightshipNavMeshManager = FindObjectOfType<LightshipNavMeshManager>(); // Assuming a singleton or a single instance in the scene
        StartCoroutine(WanderRoutine());
    }

    private void Update()
    {
        // Continuously log the distance to the destination
        if (lastDestination != null)
        {
            float distanceToDestination = Vector3.Distance(transform.position, lastDestination.Value);
            Debug.Log($"Distance to destination: {distanceToDestination}");
        }

        // Log the state of the cat
        if (IsMoving)
        {
            Debug.Log("The cat is moving.");
            PlayWalkingAnimation(); // Play the walking animation when moving
        }
        else if (IsIdling)
        {
            Debug.Log("The cat is idling.");
            PlayRandomIdleAnimation(); // Play a random idle animation when idling
        }
        else
        {
            Debug.Log("The cat has stopped moving.");
        }
    }

    // Method to play the walking animation
    private void PlayWalkingAnimation()
    {
        string walkingAnimation = "Walk_F_IP"; // Define the walking animation name

        // Check if the current animation is not already playing
        if (currentAnimation != walkingAnimation)
        {
            // Play the walking animation using crossfade for smooth transition
            animator.CrossFade(walkingAnimation, 0.2f); // 0.2f is the duration of the crossfade

            currentAnimation = walkingAnimation;

            // Log the selected animation
            Debug.Log($"Playing walking animation: {walkingAnimation}");
        }
    }


    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            wanderInterval = Random.Range(5f, 30f); // Randomize the wander interval
            yield return new WaitForSeconds(wanderInterval);

            if (isWandering)
            {
                SetRandomDestination();
            }
        }
    }

    private void SetRandomDestination()
    {
        if (navMeshAgent == null)
        {
            Debug.LogWarning("NavMeshAgent is not set up properly.");
            return;
        }

        Vector3 randomPosition = GetRandomPositionWithinRadius(transform.position, wanderRadius);

        // Validate position on NavMesh
        if (IsPositionOnNavMesh(randomPosition))
        {
            navMeshAgent.SetDestination(randomPosition);
            lastDestination = randomPosition; // Update last destination
        }
        else
        {
            Debug.LogWarning("Random position not on NavMesh, trying again.");
            SetRandomDestination();
        }
    }

    private Vector3 GetRandomPositionWithinRadius(Vector3 center, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPosition = new Vector3(center.x + randomDirection.x, center.y, center.z + randomDirection.y);
        return randomPosition;
    }

    private bool IsPositionOnNavMesh(Vector3 position)
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

    // Property to determine if the pet is moving
    public bool IsMoving
    {
        get
        {
            if (navMeshAgent == null || lastDestination == null)
                return false;

            // Check if the agent has a valid destination and if it's within a small distance
            float distanceToDestination = Vector3.Distance(transform.position, lastDestination.Value);
            return distanceToDestination > 0.1f; // Adjust the threshold as needed
        }
    }

    // Property to determine if the pet is idling (has a destination but is at the destination)
    public bool IsIdling
    {
        get
        {
            return lastDestination != null && !IsMoving;
        }
    }

    // Method to stop the cat
    public void StopCat()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(transform.position); // Stop moving by setting the destination to current position
            // Do not clear lastDestination to check idle state accurately
            Debug.Log("The cat has stopped moving.");
        }
    }

    // Method to play a random idle animation
    private void PlayRandomIdleAnimation()
    {
        // Check if enough time has passed since the last idle animation was played
        if (Time.time - lastIdleAnimationTime >= idleAnimationCooldown)
        {
            // Select a random idle animation
            string selectedAnimation = idleAnimations[Random.Range(0, idleAnimations.Length)];

            // Check if the current animation is not already playing
            if (currentAnimation != selectedAnimation)
            {
                // Play the selected animation using crossfade for smooth transition
                animator.CrossFade(selectedAnimation, 0.2f); // 0.2f is the duration of the crossfade

                currentAnimation = selectedAnimation;

                // Log the selected animation
                Debug.Log($"Playing idle animation: {selectedAnimation}");

                // Update the time when the last animation was played
                lastIdleAnimationTime = Time.time;
            }
        }
    }
}