using System.Collections;
using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using static ObjectPlacer;
using Unity.VisualScripting;

public class RandomMovementAR : MonoBehaviour
{
    private LightshipNavMeshAgent navMeshAgent;
    private LightshipNavMeshManager lightshipNavMeshManager; // Reference to the NavMesh Manager
    private Animator animator; // Reference to the Animator component
    private TreatControllerAR treatController;

    // Movement speed settings
    private float walkSpeed = 3.0f; // Walking speed
    private float runSpeed = 8.0f; // Running speed
    private float fastRunSpeed = 12.0f; // Fast running speed

    private float wanderInterval; // Randomized duration
    private float wanderRadius; // Randomized radius for wandering
    private bool isWandering = true;
    private float idleAnimationCooldown = 5f; // Time to wait before playing a new idle animation
    private float lastIdleAnimationTime = 0f; // Time when the last idle animation was played
    private string walkAnimation = "Walk_F_IP"; // Walking animation name
    private string runAnimation = "Run_F_IP"; // Running animation name
    private string fastRunAnimation = "RunFast_F_IP"; // Fast running animation name
    private bool isMovingToItem = false;
    private bool isEating = false;
    private PetStatus petStatus;
    private bool idleAnimationInProgress = false;

    public bool IsWandering => isWandering;

    // Store the last destination
    private Vector3? lastDestination = null;
    private Vector3? randomDestination = null; // Separate random destination
    private Vector3? itemDestination = null;   // Separate destination for treat/feed

    // List of idle animation names
    private string[] idleAnimations = { "Idle_1", "Idle_2", "Idle_3", "Idle_4", "Idle_5", "Idle_6", "Idle_7", "Idle_8", "SharpenClaws_Horiz" };
    private string currentAnimation = ""; // Track the current animation

    // Sit animation sequence
    private string sitStart = "Sit_start";
    private string[] sitLoops = { "Sit_loop_1", "Sit_loop_2", "Sit_loop_3", "Sit_loop_4" };
    private string sitEnd = "Sit_end";

    // Sleep animation sequences
    private string[] sleepStarts = { "Lie_back_sleep_start", "Lie_belly_sleep_start", "Lie_side_sleep_start" };
    private string[] sleepLoops = { "Lie_back_sleep", "Lie_belly_sleep", "Lie_side_sleep" };
    private string[] sleepEnds = { "Lie_back_sleep_end", "Lie_belly_sleep_end", "Lie_side_sleep_end" };

    [SerializeField]
    public GameObject treat;
    public GameObject feed;
    private void Start()
    {
        navMeshAgent = GetComponent<LightshipNavMeshAgent>();
        animator = GetComponent<Animator>(); // Get the Animator component
        lightshipNavMeshManager = FindObjectOfType<LightshipNavMeshManager>(); // Assuming a singleton or a single instance in the scene
        treatController = FindObjectOfType<TreatControllerAR>(); // Find the TreatController in the scene
        treatController.OnTreatPlaced += (position, treatObject) => MoveToItem(position, treatObject, "Treat");
        treatController.OnFeedPlaced += (position, feedObject) => MoveToItem(position, feedObject, "Feed");

        StartCoroutine(WanderRoutine());
    }

    private void Update()
    {
        // Check if there is a destination to move to
        if (itemDestination != null || randomDestination != null)
        {
            // Determine which destination to prioritize (item destination takes priority)
            Vector3 currentDestination = itemDestination ?? randomDestination.Value; // Use itemDestination if it's set
            float distanceToDestination = Vector3.Distance(transform.position, currentDestination);
            Debug.Log($"Distance to destination: {distanceToDestination}");
            UpdateMovementSpeed(distanceToDestination);

            // Check if the cat has reached its destination
            // Check if the cat has reached its destination
            if (distanceToDestination <= 0.55f) // Check if reached the destination
            {
                navMeshAgent.walkingSpeed = 0f; // Reset movement speed to 0
                PlayRandomIdleAnimation(); // Play an idle animation

                // Handle reaching item destination
                if (itemDestination != null)
                {
                    Debug.Log("The cat has reached the treat/feed.");
                    itemDestination = null; // Clear the item destination
                    isMovingToItem = false;  // Reset flag for moving to item
                    isWandering = false;      // Resume wandering
                }
                // Handle reaching random destination
                else if (randomDestination != null)
                {
                    Debug.Log("The cat has reached its random destination.");
                    randomDestination = null; // Clear the random destination
                    lastDestination = null;   // Reset the last destination
                }
            }

        }

        // Log the state of the cat and play the appropriate animations
        if (IsMoving || !isEating)
        {
            //Debug.Log("The cat is moving.");
            PlayAnimation(currentAnimation); // Play the current movement animation
        }
        else if (IsIdling || !isEating) // Changed to else if
        {
          //  Debug.Log("The cat is idling.");
            PlayRandomIdleAnimation(); // Play a random idle animation when idling
        } 

        else
        {
           // Debug.Log("The cat has stopped moving.");
            // Optionally, you could play a default idle animation here if desired
        }
    }



    private void MoveToItem(Vector3 itemPosition, GameObject itemObject, string itemType)
    {
        Debug.Log($"MoveToItem called with position: {itemPosition} and item type: {itemType}"); // Debug log
        isWandering = false;      // Stop wandering
        isMovingToItem = true;    // Set the flag for moving to item
        itemDestination = itemPosition; // Set the treat/feed as the destination
        navMeshAgent.SetDestination(itemPosition); // Move towards the treat/feed
        Debug.Log($"Moving to item at position: {itemPosition}");

        StartCoroutine(ConsumeItem(itemObject, itemType)); // Start consuming the item once arrived
    }


    private IEnumerator ConsumeItem(GameObject itemObject, string itemType)
    {
        isEating = true; // Set the eating flag to 

        // Wait until the cat has reached the item
        while (itemDestination != null)
        {
            yield return null; // Wait for the next frame
        }

        // Log that the cat is starting to eat
        Debug.Log($"The cat is now eating {itemType}.");

        // Play the eating animation
        PlayAnimation("Eating"); // Ensure "Eating" is defined in your animator

        // Introduce a short delay to ensure the eating animation has time to start
        yield return new WaitForSeconds(0.5f); // Adjust this time as necessary

        // Wait for the appropriate amount of time based on item type
        float consumeTime = itemType == "Treat" ? 3f : 10f; // 3 seconds for Treat, 10 seconds for Feed
        Debug.Log($"Consuming {itemType} for {consumeTime} seconds...");
        yield return new WaitForSeconds(consumeTime);

        if (itemType == "Treat")
        {
            // Increase hunger by 10
            FindObjectOfType<PetStatus>().FeedPet(10f); // Assuming FeedPet increases hunger
        }
        else if (itemType == "Feed")
        {
            // Increase hunger by 50
            FindObjectOfType<PetStatus>().FeedPet(50f);
        }

        // Destroy the item after consumption
        Destroy(itemObject); // Now this correctly references the item object
        treatController.isItemPlaced = false;

        // Transition back to an idle animation after consuming
        //PlayRandomIdleAnimation(); // Ensure to play a random idle animation instead of a specific one
        Debug.Log($"The cat has consumed the {itemType}.");
        
        // Set the wandering state back to true
        isWandering = true;
        itemDestination = null; // Clear the item destination
        isEating = false; // Reset the eating 
        PlayAnimation("Idle_7");
    }

    private IEnumerator PlaySingleIdleAnimation(string animationName)
    {
        // Play the selected animation using crossfade for smooth transition
        animator.CrossFade(animationName, 0.2f);
        currentAnimation = animationName;

        // Wait until the animation completes
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        idleAnimationInProgress = false; // Mark that the animation has finished
    }



    private IEnumerator PlaySittingAnimation()
    {
        // Play sit start animation
        animator.CrossFade(sitStart, 0.2f);
        currentAnimation = sitStart;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Play a random sit loop animation
        string sitLoop = sitLoops[Random.Range(0, sitLoops.Length)];
        animator.CrossFade(sitLoop, 0.2f);
        currentAnimation = sitLoop;
        yield return new WaitForSeconds(Random.Range(3f, 10f)); // Random loop duration

        // Play sit end animation
        animator.CrossFade(sitEnd, 0.2f);
        currentAnimation = sitEnd;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Ensure idle animation flag is reset
        idleAnimationInProgress = false;

        // Immediately trigger the next idle animation
        PlayRandomIdleAnimation();
    }



    private IEnumerator PlaySleepingAnimation()
    {
        // Select a random sleep start, loop, and end animation
        int sleepIndex = Random.Range(0, sleepStarts.Length);
        string sleepStart = sleepStarts[sleepIndex];
        string sleepLoop = sleepLoops[sleepIndex];
        string sleepEnd = sleepEnds[sleepIndex];

        // Play sleep start animation
        animator.CrossFade(sleepStart, 0.2f);
        currentAnimation = sleepStart;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Play sleep loop animation
        animator.CrossFade(sleepLoop, 0.2f);
        currentAnimation = sleepLoop;
        yield return new WaitForSeconds(Random.Range(5f, 15f)); // Random loop duration

        // Play sleep end animation
        animator.CrossFade(sleepEnd, 0.2f);
        currentAnimation = sleepEnd;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Set wandering state back to true and start wandering again
        idleAnimationInProgress = false; // Mark that the sleep animation has finished
        isWandering = true; // Ensure the cat resumes wandering after sleeping
        SetRandomDestination(); // Start wandering again
    }



    private void UpdateMovementSpeed(float distanceToDestination)
    {
        if (distanceToDestination <= wanderRadius / 3)
        {
            navMeshAgent.walkingSpeed = runSpeed;
            PlayAnimation(runAnimation);
            //Debug.Log("The cat is walking");
        }
        else
        {
            navMeshAgent.walkingSpeed = fastRunSpeed;
            PlayAnimation(fastRunAnimation);
            //Debug.Log("The cat is running fast");
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
            wanderRadius = Random.Range(6f, 10f); // Randomize the wander radius
            yield return new WaitForSeconds(wanderInterval);

            if (isWandering && !isMovingToItem) // Only wander if not moving to a treat/feed
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
            randomDestination = randomPosition; // Update random destination
            navMeshAgent.SetDestination(randomPosition); // Move to random position
            Debug.Log($"Setting random destination at position: {randomPosition}");
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
            if (navMeshAgent == null || (randomDestination == null && itemDestination == null))
                return false;

            Vector3 currentDestination = itemDestination != null ? itemDestination.Value : randomDestination.Value;
            float distanceToDestination = Vector3.Distance(transform.position, currentDestination);
            Debug.Log($"IsMoving check: Distance to destination = {distanceToDestination}"); // Log distance
            return distanceToDestination > 0.1f; // Adjust the threshold as needed
        }
    }

    public bool IsIdling
    {
        get
        {
            // Check if both destinations are null and not moving
            bool isIdling = randomDestination == null && itemDestination == null && !IsMoving;
            if (isEating)
            {
                return false;
            }
            //Debug.Log($"IsIdling check: {isIdling}"); // Log idle state
            return isIdling;
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
        // Only play a new idle animation if none is currently in progress and the cat is not eating
        if (!idleAnimationInProgress && !isEating && Time.time - lastIdleAnimationTime >= idleAnimationCooldown)
        {
            idleAnimationInProgress = true; // Mark that an animation is now playing

            // Randomly decide between a standard idle, sitting, or sleeping animation
            int idleType = Random.Range(0, 3); // 0 for idle, 1 for sit, 2 for sleep

            if (idleType == 0) // Play a regular idle animation
            {
                string selectedAnimation = idleAnimations[Random.Range(0, idleAnimations.Length)];
                StartCoroutine(PlaySingleIdleAnimation(selectedAnimation));
            }
            else if (idleType == 1) // Play a sitting animation sequence
            {
                StartCoroutine(PlaySittingAnimation());
            }
            else if (idleType == 2) // Play a sleeping animation sequence
            {
                StartCoroutine(PlaySleepingAnimation());
            }

            // Update the time when the last animation was started
            lastIdleAnimationTime = Time.time;
        }
    }


    private void PlayAnimation(string animationName)
    {
        // Check if the current animation is not already playing
        if (currentAnimation != animationName)
        {
            // Play the selected animation using crossfade for smooth transition
            animator.CrossFade(animationName, 0.2f); // 0.2f is the duration of the crossfade

            currentAnimation = animationName;

            // Log the selected animation
            Debug.Log($"Playing animation: {animationName}");
        }
    }

}