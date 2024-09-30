using System.Collections;
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
    PetAI petai;
    public bool isWaiting = false;
    private bool isMovingToTreat = false; // Flag to track if the pet is moving to a treat
    private bool isMovingToCamera = false; // Flag to check if moving to camera

    private PetAI petAI; // Reference to the PetAI script

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
            }
        }
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
