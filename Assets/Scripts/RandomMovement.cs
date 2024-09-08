using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Import this to work with UI buttons

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; // radius of sphere
    public Transform centrePoint; // centre of the area the agent wants to move around in

    // Reference to the UI Button
    public Button moveToCameraButton;
    public Camera mainCamera; // Reference to the camera

    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Add listener to the button to call MoveToCamera() when clicked
        if (moveToCameraButton != null)
        {
            moveToCameraButton.onClick.AddListener(MoveToCamera);
        }
    }

    void Update()
    {
        if (!isWaiting && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
                agent.SetDestination(point);
            }
        }
    }

    void MoveToCamera()
    {
        // If already waiting, return
        if (isWaiting) return;

        // Set the destination to the camera's position
        agent.SetDestination(mainCamera.transform.position);

        // Start the coroutine to wait and resume wandering
        StartCoroutine(WaitAndResumeWandering());
    }

    IEnumerator WaitAndResumeWandering()
    {
        isWaiting = true;

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Resume wandering
        isWaiting = false;
    }

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
}
