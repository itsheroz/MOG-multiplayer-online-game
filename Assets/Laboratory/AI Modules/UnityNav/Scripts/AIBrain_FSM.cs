using UnityEngine;
using UnityEngine.AI;

// Define the possible states for our AI
public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Avoid
}

public class AIBrain_FSM : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private AIActionController actionController;
    [SerializeField] private AINetworkHandler networkHandler;

    [Header("Sensors & Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float separationRadius = 3f;
    [SerializeField] private LayerMask aiLayer;
    [SerializeField] private AITargetSelector targetSelector;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    // State Management Variables
    private AIState currentState;
    private float stateTimer; // General purpose timer (used for Idle)

    [Header("Chase Settings")]
    [SerializeField] private float maxChaseTime = 5.0f; // Give up after 5 seconds
    private float currentChaseTimer;

    [Header("Chase Settings")]
    [SerializeField] private float chaseCooldownTime = 2.0f; // [NEW] พัก 2 วินาทีก่อนจะไล่ใครใหม่
    private float nextChaseTime = 0f; // [NEW] เก็บเวลาที่จะไล่ล่าได้อีกครั้ง

    private void Start()
    {
        // Initialize logic only on the server/owner
        if (networkHandler.IsOwnerOrServer)
        {
            ChangeState(AIState.Patrol); // Start by patrolling
        }
    }

    private void Update()
    {
        if (!networkHandler.IsOwnerOrServer) return;

        // 1. GLOBAL TRANSITIONS (High Priority Overrides)
        // These checks happen every frame regardless of state
        // ---------------------------------------------------

        // Priority A: Avoidance (Self-preservation)
        Transform nearbyAI = GetClosestAI();
        if (nearbyAI != null && currentState != AIState.Avoid)
        {
            ChangeState(AIState.Avoid);
            return;
        }

        // Priority B: Chasing (Aggression) - Only if not avoiding
        if (Time.time >= nextChaseTime)
        {
            Transform target = targetSelector.GetCurrentTarget();
            if (target != null && currentState != AIState.Chase && currentState != AIState.Avoid)
            {
                ChangeState(AIState.Chase);
                return;
            }
        }

        // 2. STATE SPECIFIC LOGIC
        // ---------------------------------------------------
        switch (currentState)
        {
            case AIState.Idle:
                UpdateIdle();
                break;
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Avoid:
                UpdateAvoid(nearbyAI);
                break;
        }
    }

    // --- STATE MACHINE CORE ---

    // Handles the switching logic: Exit old state -> Enter new state
    private void ChangeState(AIState newState)
    {
        // Optional: Call Exit Logic for the old state if needed
        // ExitState(currentState); 

        currentState = newState;

        // Enter Logic: What happens ONCE when we start this state?
        switch (currentState)
        {
            case AIState.Idle:
                actionController.Stop();
                stateTimer = Random.Range(minIdleTime, maxIdleTime);
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Idle);
                break;

            case AIState.Patrol:
                actionController.SetRunMode(false); // Walk
                Vector3 randomPoint = GetRandomPointOnNavMesh();
                actionController.MoveTo(randomPoint);
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
                break;

            case AIState.Chase:
                // Reset timer when we START chasing
                currentChaseTimer = maxChaseTime;
                actionController.SetRunMode(true);
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Run);
                break;

            case AIState.Avoid:
                actionController.SetRunMode(false); // Walk away
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
                break;
        }
    }

    // --- STATE UPDATE METHODS (Run every frame) ---

    private void UpdateIdle()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            ChangeState(AIState.Patrol);
        }
    }

    private void UpdatePatrol()
    {
        if (actionController.HasReachedDestination())
        {
            ChangeState(AIState.Idle);
        }
    }

    private void UpdateChase()
    {
        Transform target = targetSelector.GetCurrentTarget();

        // Condition 1: หาไม่เจอ (Disconnect / หายตัว)
        if (target == null)
        {
            StopChasing(); // [NEW] เรียกฟังก์ชันจบการไล่ล่า
            return;
        }

        // Condition 2: หมดเวลาไล่ (Timeout)
        currentChaseTimer -= Time.deltaTime;
        if (currentChaseTimer <= 0)
        {
            StopChasing(); // [NEW] เรียกฟังก์ชันจบการไล่ล่า
            return;
        }

        actionController.MoveTo(target.position);
    }

    private void StopChasing()
    {
        // 1. สั่ง Selector ให้ลืมเป้าหมายซะ (ตามที่คุณต้องการ)
        targetSelector.ClearTarget();

        // 2. ตั้งเวลา Cooldown เพื่อไม่ให้หันกลับมาไล่ทันที (AI พักเหนื่อย)
        nextChaseTime = Time.time + chaseCooldownTime;

        // 3. กลับไปเดินเล่น
        ChangeState(AIState.Patrol);
    }

    private void UpdateAvoid(Transform otherAI)
    {
        // If the threat is gone, go back to patrol
        if (otherAI == null)
        {
            ChangeState(AIState.Patrol);
            return;
        }

        // Calculate vector away from threat
        Vector3 dirAway = transform.position - otherAI.position;
        Vector3 targetPos = transform.position + dirAway.normalized * 2.0f;

        actionController.MoveTo(targetPos);

        // Transition: If far enough, return to normal
        if (Vector3.Distance(transform.position, otherAI.position) > separationRadius * 1.5f)
        {
            ChangeState(AIState.Patrol);
        }
    }

    // --- HELPERS (Same as before) ---

    private Transform GetClosestAI()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, aiLayer);
        foreach (var hit in hits)
        {
            if (hit.transform != transform) return hit.transform;
        }
        return null;
    }

    private Vector3 GetRandomPointOnNavMesh()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            return hit.position;
        }
        return transform.position;
    }
}