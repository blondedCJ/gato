using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatBehaviorAR : MonoBehaviour
{
    public enum CatState { Idle, Walking, MediumRun, FastRun, Sitting, Sleeping, Consuming }
    public CatState currentState = CatState.Idle;

    public NavMeshAgent agent;
    public Animator animator;
    public RandomMovementAR randomMovementAR;
    public float wanderRadius = 10f;
    public float defaultStoppingDistance = 0.5f; // Default stopping distance
    public float consumingStoppingDistance = 1.5f; // Stopping distance when consuming

    private float stateTimer = 0f;
    private float directionTimer = 0f; // Timer for directional animations
    private Dictionary<string, AnimationClip> animationClips; // Store animation clips
    public PetStatus petStatus;

    private string currentAnimation = ""; // Track the current animation
    private Vector3? targetPosition = null; // Current target position
    private Vector3? queuedTargetPosition = null; // Queued target position for when waking up
    private GameObject targetObject = null; // The treat or feed object
    private TreatController treatController; // Store the TreatController reference


    // List of idle animation names
    private string[] idleAnimations = { "Idle_1", "Idle_2", "Idle_3", "Idle_4", "Idle_5", "Idle_6", "Idle_7", "Idle_8", "SharpenClaws_Horiz" };

    // Sit animation sequence
    private string sitStart = "Sit_start";
    private string[] sitLoops = { "Sit_loop_1", "Sit_loop_2", "Sit_loop_3", "Sit_loop_4" };
    private string sitEnd = "Sit_end";

    // Sleep animation sequences
    private string[] sleepStarts = { "Lie_back_sleep_start", "Lie_belly_sleep_start", "Lie_side_sleep_start" };
    private string[] sleepLoops = { "Lie_back_sleep", "Lie_belly_sleep", "Lie_side_sleep" };
    private string[] sleepEnds = { "Lie_back_sleep_end", "Lie_belly_sleep_end", "Lie_side_sleep_end" };

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        randomMovementAR = GetComponent<RandomMovementAR>();
        // Initialize animation clips dictionary
        animationClips = new Dictionary<string, AnimationClip>();
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            animationClips[clip.name] = clip;
        }

        TransitionToState(CatState.Idle);
    }

    public void MoveTo(Vector3 targetPosition, GameObject targetObject, TreatController treatController)
    {
        this.treatController = treatController; // Store the reference
        this.targetObject = targetObject;

        // Disable random wandering when moving to a target
        StopRandomWandering(); // Call method to disable wandering

        if (currentState == CatState.Sleeping)
        {
            queuedTargetPosition = targetPosition; // Queue the target position if sleeping
        }
        else
        {
            // Move to the treat or feed as you already do
            GameObject alignmentTarget = new GameObject("AlignmentTarget");
            float desiredDistance = 3f; // Adjust based on model size
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            alignmentTarget.transform.position = targetPosition - directionToTarget * desiredDistance;

            this.targetPosition = alignmentTarget.transform.position;
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(alignmentTarget.transform.position);
            agent.isStopped = false;

            // Transition to FastRun if targeting treat/feed
            if (targetObject.CompareTag("Treat") || targetObject.CompareTag("Feed"))
            {
                TransitionToState(CatState.FastRun);
            }

            StartCoroutine(DestroyAlignmentTargetWhenReached(alignmentTarget));
            StartCoroutine(NotifyTreatController(treatController));
        }
    }

    private void StopRandomWandering()
    {
        // Stop wandering by clearing any current wandering target
        targetPosition = null;
        agent.isStopped = true;
    }



    // Coroutine to check if the agent has reached the alignment target and destroy it
    private IEnumerator DestroyAlignmentTargetWhenReached(GameObject alignmentTarget)
    {
        // Wait until the agent reaches the destination
        while (Vector3.Distance(agent.transform.position, alignmentTarget.transform.position) > agent.stoppingDistance)
        {
            // Wait until the next frame
            yield return null;
        }

        // Once the agent has reached the target, destroy the alignment target
        Destroy(alignmentTarget);
    }


    IEnumerator NotifyTreatController(TreatController treatController)
    {
        yield return new WaitForSeconds(2f); // Wait for the cat to consume the treat
        treatController.ResetItemPlacedFlag();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        directionTimer -= Time.deltaTime;
       
        if (randomMovementAR.IsWandering)
        {
            // Handle animations based on the wandering state
            HandleWanderingState();
        }
        
        if (targetPosition.HasValue && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            targetPosition = null; // Clear targetPosition to avoid re-triggering the transition
            TransitionToState(CatState.Consuming); // Start consuming when reaching the target
        }

        // Detect when the wandering destination is reached and set a new one immediately
        if (!targetPosition.HasValue && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            if (currentState == CatState.Walking || currentState == CatState.MediumRun || currentState == CatState.FastRun)
            {
                SetNewDestination(); // Immediately set a new destination
            }
        }

        if (!targetPosition.HasValue)
        {
            switch (currentState)
            {
                case CatState.Idle:
                case CatState.Walking:
                case CatState.MediumRun:
                case CatState.FastRun:
                case CatState.Sitting:
                case CatState.Sleeping:
                    HandleWanderingState(); // Use a common method for all wandering states
                    break;
                case CatState.Consuming:
                    HandleConsumingState();
                    break;
            }
        }
    }


    void HandleWanderingState()
    {
        // Only decrement timer for walking, idle, and running
        if (currentState != CatState.Sitting && currentState != CatState.Sleeping)
        {
            if (stateTimer <= 0f)
            {
                TransitionToRandomWanderingState();
            }
        }
    }


    void TransitionToRandomWanderingState()
    {
        CatState[] possibleStates = { CatState.Idle, CatState.Walking, CatState.MediumRun, CatState.FastRun, CatState.Sitting, CatState.Sleeping };
        CatState randomState = possibleStates[Random.Range(0, possibleStates.Length)];
        TransitionToState(randomState);
    }

    void TransitionToState(CatState newState)
    {
        if (currentState == newState)
        {
            return; // Prevent re-entering the same state
        }

        currentState = newState;
        Debug.Log($"Transitioned to {newState} state");

        switch (newState)
        {
            case CatState.Idle:
                agent.isStopped = true;
                stateTimer = Random.Range(3f, 8f);
                string randomIdleAnimation = idleAnimations[Random.Range(0, idleAnimations.Length)];
                ChangeAnimation(randomIdleAnimation);
                break;

            case CatState.Walking:
                agent.speed = 4f;
                agent.isStopped = false; // Allow movement
                SetNewDestination();
                stateTimer = Random.Range(3f, 7f);
                ChangeAnimation("Walk_F_IP");
                break;

            case CatState.MediumRun:
                agent.speed = 12f;
                agent.isStopped = false; // Allow movement
                SetNewDestination();
                stateTimer = Random.Range(3f, 7f);
                ChangeAnimation("Run_F_IP");
                break;

            case CatState.FastRun:
                agent.speed = 16f;
                agent.isStopped = false; // Allow movement
                ChangeAnimation("RunFast_F_IP");
                break;

            case CatState.Sitting:
                agent.isStopped = true; // Stop movement
                stateTimer = Random.Range(1, 1); // Sitting for a longer period
                StartSitSequence();
                break;

            case CatState.Sleeping:
                agent.isStopped = true; // Stop movement
                stateTimer = Random.Range(1, 1); // Sleeping for a longer period
                StartCoroutine(SleepSequence());
                break;

            case CatState.Consuming:
                // Handle consumption logic here, as before
                agent.isStopped = true;
                agent.stoppingDistance = defaultStoppingDistance; // Reset stopping distance
                stateTimer = targetObject.CompareTag("Treat") ? 3f : 10f; // Treat or Feed consumption time
                ChangeAnimation("Eating");
                break;
        }

        // If transitioning from Sleeping and a queued target exists, move to it
        if (newState != CatState.Sleeping && queuedTargetPosition.HasValue)
        {
            MoveTo(queuedTargetPosition.Value, targetObject, treatController);
            queuedTargetPosition = null;
        }
    }

    void HandleConsumingState()
    {
        if (stateTimer <= 0f)
        {
            Debug.Log("Consuming state timer reached 0");
            if (targetObject != null)
            {
                Destroy(targetObject); // Destroy the treat or feed object
                Debug.Log("Destroyed target object");

                if (petStatus != null)
                {
                    if (targetObject.CompareTag("Treat"))
                    {
                        petStatus.IncreaseHungerBy(10f); // Increase hunger by 10 for treats
                    }
                    else if (targetObject.CompareTag("Feed"))
                    {
                        petStatus.IncreaseHungerBy(50f); // Increase hunger by 50 for feed
                    }
                }

                TransitionToRandomWanderingState(); // Random wandering after consumption
            }
            else
            {
                Debug.LogError("PetStatus is null. Cannot increase hunger.");
            }
        }
    }


    void HandleIdleState()
    {
        if (stateTimer <= 0f)
        {
            TransitionToState(CatState.Walking);
        }
    }

    void HandleMovementState()
    {
        if (stateTimer <= 0f)
        {
            TransitionToState(CatState.Idle);
        }
    }

    void HandleSittingState()
    {
        if (stateTimer <= 0f)
        {
            TransitionToState(CatState.Idle);
        }
    }

    void HandleSleepingState()
    {
        if (stateTimer <= 0f)
        {
            TransitionToState(CatState.Idle);
        }
    }

    void SetNewDestination()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        Vector3 newDestination = transform.position + randomDirection * wanderRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(newDestination, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void StartSitSequence()
    {
        ChangeAnimation(sitStart);
        Invoke("SitLoop", animationClips[sitStart].length);
    }

    void SitLoop()
    {
        string randomSitLoop = sitLoops[Random.Range(0, sitLoops.Length)];
        ChangeAnimation(randomSitLoop);
        Invoke("SitEnd", animationClips[randomSitLoop].length);
    }

    void SitEnd()
    {
        ChangeAnimation(sitEnd);
        Invoke("IdleAfterSit", animationClips[sitEnd].length);
    }

    void IdleAfterSit()
    {
        // Transition back to Idle after sitting is fully complete
        TransitionToState(CatState.Idle);
    }


    IEnumerator SleepSequence()
    {
        // Randomly choose an index for the sleep sequence
        int randomIndex = Random.Range(0, sleepStarts.Length);

        // Play the corresponding sleep start, loop, and end animations in order
        string sleepStart = sleepStarts[randomIndex];
        string sleepLoop = sleepLoops[randomIndex];
        string sleepEnd = sleepEnds[randomIndex];

        // Start the sleep sequence
        ChangeAnimation(sleepStart);
        yield return new WaitForSeconds(animationClips[sleepStart].length);

        // Play the sleep loop
        ChangeAnimation(sleepLoop);
        yield return new WaitForSeconds(animationClips[sleepLoop].length);

        // Play the sleep end animation
        ChangeAnimation(sleepEnd);
        yield return new WaitForSeconds(animationClips[sleepEnd].length);

        // Transition back to idle after the sleep sequence is complete
        TransitionToState(CatState.Idle);
    }


    void ChangeAnimation(string animationName)
    {
        if (currentAnimation != animationName)
        {
            Debug.Log($"Changing animation to {animationName}");
            animator.CrossFade(animationName, 0.2f);
            currentAnimation = animationName;
        }
    }
}