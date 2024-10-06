using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatBehavior : MonoBehaviour
{
    public enum CatState { Idle, Walking, MediumRun, FastRun, Sitting, Sleeping }
    public CatState currentState = CatState.Idle;

    public NavMeshAgent agent;
    public Animator animator;
    public float wanderRadius = 10f;

    private float stateTimer = 0f;
    private float directionTimer = 0f; // Timer for directional animations
    private Dictionary<string, AnimationClip> animationClips; // Store animation clips

    private string currentAnimation = ""; // Track the current animation

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

    void Update()
    {
        stateTimer -= Time.deltaTime;
        directionTimer -= Time.deltaTime;

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
        }

        UpdateAnimation();
    }

    void TransitionToState(CatState newState)
    {
        currentState = newState;

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
                 agent.speed = 12f;
                 agent.isStopped = false;
                 SetNewDestination();
                 stateTimer = Random.Range(3f, 7f);
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
        }
    }

    void HandleIdleState()
    {
        if (stateTimer <= 0f)
        {
            float randomValue = Random.value; // Random value between 0 and 1

            if (randomValue < 0.2f) // 20% chance to sit
            {
                TransitionToState(CatState.Sitting);
            }
            else if (randomValue < 0.8f) // 10% chance to sleep (cumulative 30% with sitting)
            {
                TransitionToState(CatState.Sleeping);
            }
            else
            {
                TransitionToRandomMovementState();
            }
        }
    }

    void HandleMovementState()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude <= 0.1f)
        {
            TransitionToState(CatState.Idle);
        }
        else if (stateTimer <= 0f)
        {
            TransitionToRandomMovementState();
        }
    }

    void HandleSittingState()
    {
        // Sitting state is managed by the animation sequence
    }

    void HandleSleepingState()
    {
        // Sleeping state is managed by the animation sequence
    }

    void TransitionToRandomMovementState()
    {
        CatState[] movementStates = { CatState.Walking, CatState.MediumRun, CatState.FastRun };
        TransitionToState(movementStates[Random.Range(0, movementStates.Length)]);
    }

    void SetNewDestination()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        Vector3 destination = transform.position + randomDirection * wanderRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void StartSitSequence()
    {
        ChangeAnimation(sitStart);
        Invoke("StartSitLoop", animationClips[sitStart].length);
    }

    void StartSitLoop()
    {
        string randomSitLoop = sitLoops[Random.Range(0, sitLoops.Length)];
        ChangeAnimation(randomSitLoop);
        Invoke("EndSitSequence", animationClips[randomSitLoop].length);
    }

    void EndSitSequence()
    {
        ChangeAnimation(sitEnd);
        Invoke("TransitionToIdle", animationClips[sitEnd].length);
    }

    void TransitionToIdle()
    {
        TransitionToState(CatState.Idle);
    }

    IEnumerator SleepSequence()
    {
        // Randomly select a sleep animation sequence
        int sleepIndex = Random.Range(0, sleepStarts.Length);
        string sleepStart = sleepStarts[sleepIndex];
        string sleepLoop = sleepLoops[sleepIndex];
        string sleepEnd = sleepEnds[sleepIndex];

        // Play the sleep start animation
        ChangeAnimation(sleepStart);
        yield return new WaitForSeconds(animationClips[sleepStart].length);

        // Calculate the remaining time for the sleep loop
        float remainingSleepTime = stateTimer - animationClips[sleepStart].length - animationClips[sleepEnd].length;

        // Play the sleep loop animation for the remaining sleep time
        while (remainingSleepTime > 0)
        {
            ChangeAnimation(sleepLoop);
            yield return new WaitForSeconds(animationClips[sleepLoop].length);
            remainingSleepTime -= animationClips[sleepLoop].length;
        }

        // Play the sleep end animation
        ChangeAnimation(sleepEnd);
        yield return new WaitForSeconds(animationClips[sleepEnd].length);

        // Transition back to a random movement state
        TransitionToRandomMovementState();
    }

    void UpdateAnimation()
    {
        if (directionTimer <= 0f)
        {
            // Update the direction timer based on the current animation clip length
            if (animationClips.TryGetValue(currentAnimation, out AnimationClip clip))
            {
                directionTimer = clip.length;
            }
            else
            {
                Debug.LogWarning($"Animation clip not found for {currentAnimation}");
            }
        }
    }

    void ChangeAnimation(string newAnimation)
    {
        if (currentAnimation != newAnimation)
        {
            animator.CrossFade(newAnimation, 0.2f);
            currentAnimation = newAnimation;
            Debug.Log($"Changed Animation to {newAnimation}");

            // Set the directionTimer based on the animation clip length
            if (animationClips.TryGetValue(newAnimation, out AnimationClip clip))
            {
                directionTimer = clip.length;
            }
            else
            {
                Debug.LogWarning($"Animation clip not found for {newAnimation}");
            }
        }
    }
}