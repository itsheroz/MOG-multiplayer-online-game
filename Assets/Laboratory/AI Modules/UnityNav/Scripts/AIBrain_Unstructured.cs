using UnityEngine;

public class AIBrain_Unstructured : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private AIActionController actionController;
    [SerializeField] private AINetworkHandler networkHandler;
    [SerializeField] private AITargetSelector targetSelector;

    [Header("Sensors")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float separationRadius = 3f;
    [SerializeField] private LayerMask aiLayer;

    [Header("Chase Settings")]
    [SerializeField] private float maxChaseTime = 5.0f;
    [SerializeField] private float chaseCooldown = 2.0f; // Time to wait before chasing again

    [Header("Patrol Logic")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    // Internal state variables
    private float idleTimer = 0f;
    private bool isIdling = false;

    // Chase state variables
    private bool isChasing = false;
    private float currentChaseTimer = 0f;
    private float nextChaseTime = 0f; // Timestamp when AI can chase again

    private void Update()
    {
        if (!networkHandler.IsOwnerOrServer) return;

        // 1. GATHER DATA
        Transform nearbyAI = GetClosestAI();
        Transform target = targetSelector.GetCurrentTarget();
        float distToTarget = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;

        // 2. DECISION MAKING

        // PRIORITY 1: AVOIDANCE
        if (nearbyAI != null)
        {
            HandleAvoidance(nearbyAI);
            isChasing = false;
            isIdling = false;
        }
        // PRIORITY 2: CHASE PLAYER
        // Condition: Target exists AND (Close enough OR Already chasing) AND (Cooldown ready)
        else if (target != null && (distToTarget <= detectionRadius || isChasing) && Time.time >= nextChaseTime)
        {
            if (CheckChaseLogic(distToTarget))
            {
                HandleChase(target);
            }
            else
            {
                // Timeout logic handled inside CheckChaseLogic -> StopChasing
                HandlePatrol();
            }
        }
        // PRIORITY 3: PATROL
        else
        {
            HandlePatrol();
            // Ensure chase flag is reset if we fall through to patrol
            if (isChasing) StopChasing();
        }
    }

    // --- BEHAVIOR IMPLEMENTATIONS ---

    private bool CheckChaseLogic(float distToTarget)
    {
        // Start Chasing
        if (!isChasing)
        {
            if (distToTarget <= detectionRadius)
            {
                isChasing = true;
                currentChaseTimer = maxChaseTime;
                return true;
            }
            return false;
        }

        // Countdown
        currentChaseTimer -= Time.deltaTime;
        if (currentChaseTimer <= 0)
        {
            StopChasing(); // Timeout: Give up
            return false;
        }

        return true; // Continue chasing
    }

    private void StopChasing()
    {
        isChasing = false;
        targetSelector.ClearTarget(); // Force sensor to drop target
        nextChaseTime = Time.time + chaseCooldown; // Set cooldown
    }

    private void HandleChase(Transform target)
    {
        actionController.SetRunMode(true);
        actionController.MoveTo(target.position);
        networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Run);
        isIdling = false;
    }

    private void HandleAvoidance(Transform otherAI)
    {
        Vector3 directionAway = transform.position - otherAI.position;
        Vector3 avoidPos = transform.position + directionAway.normalized * 2f;
        actionController.SetRunMode(false);
        actionController.MoveTo(avoidPos);
        networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
    }

    private void HandlePatrol()
    {
        actionController.SetRunMode(false);

        if (actionController.HasReachedDestination())
        {
            if (!isIdling)
            {
                isIdling = true;
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                actionController.Stop();
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Idle);
            }
            else
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    Vector3 randomPoint = GetRandomPointOnNavMesh();
                    actionController.MoveTo(randomPoint);
                    isIdling = false;
                    networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
                }
            }
        }
        else
        {
            networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
        }
    }

    private Transform GetClosestAI()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, aiLayer);
        foreach (var hit in hits) { if (hit.transform != transform) return hit.transform; }
        return null;
    }

    private Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1)) return hit.position;
        return transform.position;
    }
}