using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatBehavior : MonoBehaviour
{
    public enum CatState { Idle, Walking, MediumRun, FastRun, Sitting, Sleeping, Consuming }
    public CatState currentState = CatState.Idle;

    public NavMeshAgent agent;
    public Animator animator;
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

        if (currentState == CatState.Sleeping)
        {
            queuedTargetPosition = targetPosition; // Queue the target position if sleeping
        }
        else
        {
            // Create an empty GameObject as a target point
            GameObject alignmentTarget = new GameObject("AlignmentTarget");
            float desiredDistance = 3f; // Adjust this distance based on your model's size
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            alignmentTarget.transform.position = targetPosition - directionToTarget * desiredDistance;

            // Move the cat to the alignment target
            this.targetPosition = alignmentTarget.transform.position;
            agent.stoppingDistance = 0.1f; // Ensure the agent stops close to the target
            agent.SetDestination(alignmentTarget.transform.position);
            agent.isStopped = false;
            TransitionToState(CatState.FastRun);

            // Clean up the alignment target after use
            Destroy(alignmentTarget, 5f); // Destroy after a delay to ensure it's used

            // Notify the TreatController when the cat has consumed the treat
            StartCoroutine(NotifyTreatController(treatController));
        }
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

        if (targetPosition.HasValue && agent.remainingDistance <= agent.stoppingDistance)
        {
            targetPosition = null; // Clear targetPosition to avoid re-triggering the transition
            TransitionToState(CatState.Consuming); // Start consuming when reaching the target
        }

        if (!targetPosition.HasValue)
        {
            switch (currentState)
            {
                case CatState.Idle:
                    HandleIdleState();
                    break;
                case CatState.Walking:
                case CatState.MediumRun:
                case CatState.FastRun:
                    HandleMovementState();
                    break;
                case CatState.Sitting:
                    HandleSittingState();
                    break;
                case CatState.Sleeping:
                    HandleSleepingState();
                    break;
                case CatState.Consuming:
                    HandleConsumingState();
                    break;
            }
        }
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
                agent.stoppingDistance = defaultStoppingDistance; // Reset stopping distance
                stateTimer = Random.Range(3f, 8f);
                string randomIdleAnimation = idleAnimations[Random.Range(0, idleAnimations.Length)];
                ChangeAnimation(randomIdleAnimation);
                break;
            case CatState.Walking:
                agent.speed = 4f;
                agent.isStopped = false;
                SetNewDestination();
                stateTimer = Random.Range(3f, 7f);
                ChangeAnimation("Walk_F_IP");
                break;
            case CatState.MediumRun:
                agent.speed = 8f;
                agent.isStopped = false;
                SetNewDestination();
                stateTimer = Random.Range(3f, 7f);
                ChangeAnimation("Run_F_IP");
                break;
            case CatState.FastRun:
                agent.speed = 16f;
                agent.isStopped = false;
                ChangeAnimation("RunFast_F_IP");
                break;
            case CatState.Sitting:
                agent.isStopped = true;
                stateTimer = 30f;
                StartSitSequence();
                break;
            case CatState.Sleeping:
                agent.isStopped = true;
                stateTimer = 60f;
                StartCoroutine(SleepSequence());
                break;
            case CatState.Consuming:
                agent.isStopped = true;
                agent.stoppingDistance = defaultStoppingDistance; // Reset stopping distance
                stateTimer = 2f; // Time to consume the object
                ChangeAnimation("Eating"); // Use a longer crossfade duration

                if (targetObject.CompareTag("Treat"))
                {
                    stateTimer = 3f; // Treat consumption time           
                }
                else if (targetObject.CompareTag("Feed"))
                {
                    stateTimer = 10f; // Feed consumption time
                }
                else
                {
                    stateTimer = 2f; // Default consumption time
                }

                animator.CrossFade("Eating", 1f); // Use a longer crossfade duration
                break;
        }

        // Check for queued target position when transitioning from sleeping
        if (newState != CatState.Sleeping && queuedTargetPosition.HasValue)
        {
            MoveTo(queuedTargetPosition.Value, targetObject, treatController); // Use the stored reference
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
                else
                {
                    Debug.LogError("PetStatus is null. Cannot increase hunger.");
                }

            }
            TransitionToState(CatState.Idle);
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
        TransitionToState(CatState.Idle);
    }

    IEnumerator SleepSequence()
    {
        string randomSleepStart = sleepStarts[Random.Range(0, sleepStarts.Length)];
        ChangeAnimation(randomSleepStart);
        yield return new WaitForSeconds(animationClips[randomSleepStart].length);

        string randomSleepLoop = sleepLoops[Random.Range(0, sleepLoops.Length)];
        ChangeAnimation(randomSleepLoop);
        yield return new WaitForSeconds(animationClips[randomSleepLoop].length);

        string randomSleepEnd = sleepEnds[Random.Range(0, sleepEnds.Length)];
        ChangeAnimation(randomSleepEnd);
        yield return new WaitForSeconds(animationClips[randomSleepEnd].length);

        TransitionToState(CatState.Idle);
    }

    void ChangeAnimation(string animationName)
    {
        if (currentAnimation != animationName)
        {
            animator.CrossFade(animationName, 0.2f);
            currentAnimation = animationName;
        }
    }
}