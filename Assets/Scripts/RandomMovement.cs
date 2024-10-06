using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // radius for random wandering
    public Transform centrePoint; // center of the area for wandering
    public Button moveToCameraButton; // Button to trigger move to camera
    public Camera mainCamera; // Reference to the camera
    public Animator animator;
    public float wanderSpeed = 3.5f; // Public speed variable for wandering
    PetAI petAI;
    public bool isWaiting = false;
    private bool isMovingToTreat = false; // Flag to track if the pet is moving to a treat
    private bool isMovingToCamera = false; // Flag to check if moving to camera
    private List<string> idleAnimations;
    private List<string> movementAnimations;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        petAI = GetComponent<PetAI>();

        // Add listener to the button to call MoveToCamera() when clicked
        if (moveToCameraButton != null)
        {
            moveToCameraButton.onClick.AddListener(MoveToCamera);
        }
        animator = GetComponent<Animator>();

        // Set the initial speed of the NavMeshAgent
        agent.speed = wanderSpeed;

        // Initialize the list of idle animations
        idleAnimations = new List<string>
        {
            "Idle_1",
            "Idle_2",
            "Idle_3",
            "Idle_4",
            "Idle_5",
            "Idle_6",
            "Idle_7",
            "Idle_8",
            // Add more idle animation names as needed
        };

        // Initialize the list of movement animations
        movementAnimations = new List<string>
        {
            "Walk_F_IP",
            "Run_F_IP",
            "RunFast_F_IP",
            "Jump_Run_IP",
            // Add more movement animation names as needed
        };

        // Start the random animation coroutine
        StartCoroutine(RandomAnimationRoutine());

    }

    void Update()
    {
        // Ensure the speed is set correctly each frame
        agent.speed = wanderSpeed;
        Debug.Log("Current Wander Speed: " + agent.speed);

        // Prevent random movement when moving to a treat, feed, camera, consuming, or when waiting
        if (isWaiting || isMovingToTreat || petAI.isMovingToTreat || petAI.isMovingToFeed || petAI.IsConsuming || isMovingToCamera)
        {
            agent.ResetPath(); // Stop the NavMeshAgent from moving
            return;
        }

        // If not waiting and the agent has reached its destination, perform random wandering
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                agent.SetDestination(point);
                PlayAnimation("Walk_F_IP");
            }
        }
    }

    IEnumerator RandomAnimationRoutine()
    {
        while (true)
        {
            // Wait for a random interval between 5 and 30 seconds
            float waitTime = Random.Range(5f, 5f);
            yield return new WaitForSeconds(waitTime);

            // Check if the current animation is idle, sitting, or laying down
            if (!IsIdleAnimationPlaying())
            {
                // Play a random movement animation
                PlayRandomMovementAnimation();
            }
        }
    }

    bool IsIdleAnimationPlaying()
    {
        // Check if the current animation is one of the idle animations
        foreach (string idleAnimation in idleAnimations)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimation))
            {
                return true;
            }
        }
        return false;
    }

    void PlayRandomMovementAnimation()
    {
        if (movementAnimations.Count == 0)
        {
            Debug.LogWarning("No movement animations available to play.");
            return;
        }

        // Select a random index from the movement animations list
        int randomIndex = Random.Range(0, movementAnimations.Count);

        // Get the animation name at the random index
        string randomAnimation = movementAnimations[randomIndex];

        // Play the random movement animation
        animator.Play(randomAnimation);
    }

    void PlayAnimation(string animationName)
    {
        // Trigger the animation by setting a trigger parameter
        animator.Play(animationName);
    }

    // Move the pet towards the camera position (triggered by button)
    void MoveToCamera()
    {
        if (isWaiting || isMovingToTreat || petAI.isMovingToTreat || petAI.isMovingToFeed)
            return;

        isMovingToCamera = true; // Set flag to prevent other actions
        agent.SetDestination(mainCamera.transform.position);

        // Temporarily stop wandering
        StartCoroutine(WaitAndResumeWandering(5f));
    }

    // Wait for a specified duration before resuming wandering
    IEnumerator WaitAndResumeWandering(float waitTime)
    {
        isWaiting = true;

        // Wait for the specified time
        yield return new WaitForSeconds(waitTime);

        isWaiting = false;
        isMovingToCamera = false;
    }

    // Generate a random point within the specified range on the NavMesh
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    // Move to treat method (can be called from PetAI or other scripts)
    public void MoveToTreat(Vector3 treatPosition)
    {
        isMovingToTreat = true;
        agent.SetDestination(treatPosition);
    }
    // Call this when the treat is consumed or after reaching the destination
    public void ResumeWandering()
    {
        isWaiting = false;
        isMovingToTreat = false;
        // Immediately trigger the animation cycle after consumption
        PetAnimationController petAnimationController = GetComponent<PetAnimationController>();
        if (petAnimationController != null)
        {
            petAnimationController.StartAnimationCycle(); // Start cycling animations directly
        }
    }
}