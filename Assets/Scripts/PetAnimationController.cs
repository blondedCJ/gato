using UnityEngine;
using System.Collections;

public class PetAnimationController : MonoBehaviour
{
    public Animator animator;
    public float minStateDuration = 1f; // Minimum duration for each animation state
    public float maxStateDuration = 1f; // Maximum duration for each animation state

    private RandomMovement randomMovement;
    private PetAI petAI;
    private Coroutine animationCycleCoroutine;

    void Start()
    {
        randomMovement = GetComponent<RandomMovement>();
        petAI = GetComponent<PetAI>();

        // Start cycling through animations
        animationCycleCoroutine = StartCoroutine(CycleAnimations());
    }

    void Update()
    {
        // Do not cycle animations if the pet is moving to a treat, feed, or consuming
        if (petAI.isMovingToTreat || petAI.isMovingToFeed || petAI.IsConsuming)
        {
            StopCurrentAnimation();
            return;
        }
    }

    // Cycle through random animations: sit, idle, walk, and run
    private IEnumerator CycleAnimations()
    {
        while (true)
        {
            if (!petAI.isMovingToTreat && !petAI.isMovingToFeed && !petAI.IsConsuming)
            {
                // Randomly choose an animation to play
                int animationState = Random.Range(0, 2); // 0: sit, 1: idle, 2: walk, 3: run
                PlayAnimation(animationState);

                // Wait for a random duration before changing to another animation
                float waitTime = Random.Range(minStateDuration, maxStateDuration);
                yield return new WaitForSeconds(waitTime);
            }

            yield return null; // Wait until next frame to recheck conditions
        }
    }

    // Play a specific animation based on the random selection
    private void PlayAnimation(int state)
    {
        petAI.ResetAnimations();
        switch (state)
        {
            case 0: // Sit
                animator.SetBool("isSitting", true);
                randomMovement.isWaiting = true; // Prevent movement while sitting
                break;
            case 1: // Idle
                animator.SetBool("isIdling", true);
                randomMovement.isWaiting = true; // Prevent movement while idling
                break;
            case 2: // Walk
                animator.SetBool("isWalking", true);
                randomMovement.isWaiting = false; // Allow movement while walking
                break;
        }
    }

    // Stop the current animation (used when the pet is moving to treat/feed)
    private void StopCurrentAnimation()
    {
        if (animationCycleCoroutine != null)
        {
            StopCoroutine(animationCycleCoroutine);
            animationCycleCoroutine = StartCoroutine(CycleAnimations()); // Restart after the action is done
        }

        petAI.ResetAnimations(); // Reset all animation states
    }
}
