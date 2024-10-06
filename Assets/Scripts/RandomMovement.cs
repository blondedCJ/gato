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
    public float wanderSpeed = 3.5f;  // Speed when wandering
    public float runSpeed = 6.0f;     // Speed when running
    PetAI petAI;
    public bool isWaiting = false;
    public bool isMovingToTreat = false; // Flag to track if the pet is moving to a treat
    public bool isMovingToCamera = false; // Flag to check if moving to camera

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
    }

    void Update()
    {
        // Ensure the correct speed is set each frame
        if (!isMovingToTreat && !isMovingToCamera && !petAI.isMovingToFeed && !isWaiting)
        {
            // Randomize speed for wandering between walk and run
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 point;
                if (RandomPoint(centrePoint.position, range, out point))
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);

                    // Set a random speed: either walking or running
                    float randomSpeed = Random.Range(0f, 1f);
                    if (randomSpeed < 0.5f)
                    {
                        agent.speed = wanderSpeed;  // Walking
                    }
                    else
                    {
                        agent.speed = runSpeed;  // Running
                    }

                    agent.SetDestination(point);
                }
            }
        }

        // Prevent random movement when moving to a treat, feed, camera, consuming, or when waiting
        if (isWaiting || isMovingToTreat || petAI.isMovingToTreat || petAI.isMovingToFeed || isMovingToCamera)
        {
            agent.ResetPath(); // Stop the NavMeshAgent from moving
            return;
        }
    }

    public float GetCurrentSpeed()
    {
        return agent.velocity.magnitude; // Assuming you are using a NavMeshAgent
    }

    void MoveToCamera()
    {
        if (isWaiting || isMovingToTreat || petAI.isMovingToTreat || petAI.isMovingToFeed)
            return;

        isMovingToCamera = true;
        agent.speed = runSpeed;  // Use running speed for moving to the camera
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
        agent.speed = runSpeed;  // Set to running speed
        agent.SetDestination(treatPosition);
    }

    // Call this when the treat is consumed or after reaching the destination
    public void ResumeWandering()
    {
        isWaiting = false;
        isMovingToTreat = false;
    }
}
