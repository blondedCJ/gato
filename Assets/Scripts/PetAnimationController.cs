using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetAnimationController : MonoBehaviour
{
    public Animator animator;
    public bool isIdling = false; // Flag to indicate if the pet is idling
    public float minIdleTime = 3f; // Minimum time to wait between idle animations
    public float maxIdleTime = 8f; // Maximum time to wait between idle animations
    public float transitionTime = 0.5f; // Time for smooth transition between animations

    private Coroutine idleCoroutine;

    // Reference to the RandomMovement script
    private RandomMovement randomMovement;

    // List of idle animations
    private List<string> idleAnimations = new List<string>
    {
        "Idle_1",
        "Idle_2",
        "Idle_3",
        "Idle_4",
        "Idle_5",
        "Idle_6",
        "Idle_7",
        "Idle_8"
    };

    // List of walking and running animations
    private List<string> walkAnimations = new List<string>
    {
        "Walk_F_IP"
    };

    private List<string> runAnimations = new List<string>
    {
        "RunFast_F_IP"
    };

    void Start()
    {
        animator = GetComponent<Animator>();
        randomMovement = GetComponent<RandomMovement>(); // Get the reference to RandomMovement
    }

    void Update()
    {
        if (randomMovement != null)
        {
            // Check if the cat is wandering or running and play appropriate animations
            if (randomMovement.isMovingToTreat || randomMovement.isMovingToCamera || !randomMovement.isWaiting)
            {
                SetIdle(false); // Not idling
                PlayMovementAnimation(); // Play walking or running animation based on movement
            }
            else
            {
                SetIdle(true); // Start idling if the cat is not moving
            }
        }

        // Handle idle animation cycling
        if (isIdling && idleCoroutine == null)
        {
            // Start cycling through idle animations if isIdling is true
            idleCoroutine = StartCoroutine(CycleIdleAnimations());
        }
        else if (!isIdling && idleCoroutine != null)
        {
            // Stop the idle cycle if isIdling becomes false
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    void PlayMovementAnimation()
    {
        // Determine if the cat is walking or running
        float speed = randomMovement.GetCurrentSpeed(); // Get the current speed from RandomMovement
        List<string> animationList;

        if (speed >= 0.5f) // Adjust this threshold as needed for running
        {
            // Play a running animation
            animationList = runAnimations;
        }
        else
        {
            // Play a walking animation
            animationList = walkAnimations;
        }

        // Randomly choose an animation from the appropriate list
        string randomMovementAnimation = animationList[Random.Range(0, animationList.Count)];

        // Smoothly transition to the chosen animation using CrossFade
        animator.Play(randomMovementAnimation);
    }

    IEnumerator CycleIdleAnimations()
    {
        while (isIdling)
        {
            // Randomly choose an idle animation from the list
            string randomIdle = idleAnimations[Random.Range(0, idleAnimations.Count)];

            // Smoothly transition to the chosen idle animation using CrossFade
            animator.CrossFade(randomIdle, transitionTime);

            // Wait for a random amount of time before cycling to the next idle animation
            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SetIdle(bool idle)
    {
        isIdling = idle;
        if (!isIdling && idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine); // Stop idle cycling when no longer idling
            idleCoroutine = null;
        }
    }
}
