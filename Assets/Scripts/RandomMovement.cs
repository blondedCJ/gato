using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // radius of the sphere for random wandering
    public Transform centrePoint; // centre of the area for wandering
    public Button moveToCameraButton; // Button to trigger move to camera
    public Camera mainCamera; // Reference to the camera
    public Animator animator;
    private bool isWaiting = false;
    private bool isMovingToTreat = false; // Flag to track if the pet is moving to a treat

    private PetAI petAI; // Reference to the PetAI script for managing treat movement

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        petAI = GetComponent<PetAI>();

        // Add listener to the button to call MoveToCamera() when clicked
        if (moveToCameraButton != null)
        {
            moveToCameraButton.onClick.AddListener(MoveToCamera);
        }
    }

    void Update()
    {
        // If the pet is moving to a treat or consuming feed, do not perform random wandering
        if (isMovingToTreat || petAI.isMovingToTreat || petAI.IsConsuming)
        {
            // Let the PetAI handle movement when chasing a treat or consuming feed
            return;
        }

        // Perform random wandering when not waiting or moving to a treat
        if (!isWaiting && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                agent.SetDestination(point);
                animator.SetBool("isIdling", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", true); 
            }
        }
    }


    // Move the pet towards the camera position (triggered by button)
    void MoveToCamera()
    {
        if (isWaiting || isMovingToTreat) return;

        // Set the destination to the camera's position
        agent.SetDestination(mainCamera.transform.position);

        // Temporarily stop wandering
        StartCoroutine(WaitAndResumeWandering());
    }

    // Wait before resuming wandering
    IEnumerator WaitAndResumeWandering()
    {
        isWaiting = true;

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Resume wandering after wait
        isWaiting = false;
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

    // Call this method from PetAI or other scripts to stop wandering and move to the treat
    public void MoveToTreat(Vector3 treatPosition)
    {
        isMovingToTreat = true;
        agent.SetDestination(treatPosition);
    }

    // Call this when treat is consumed or after reaching the destination
    public void ResumeWandering()
    {
    
        isWaiting = false;
        isMovingToTreat = false; // Resetting for treat movement
        petAI.isMovingToFeed = false; // Resetting for feed movement
    }
}
