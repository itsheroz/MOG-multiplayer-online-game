using UnityEngine;
using UnityEngine.AI;

// This component handles the physical movement and low-level actions.
// It knows 'HOW' to move, but not 'WHY' or 'WHERE'.
[RequireComponent(typeof(NavMeshAgent))]
public class AIActionController : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5.0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    // Command: Move to a specific position
    public void MoveTo(Vector3 targetPosition)
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
    }

    // Command: Stop moving immediately
    public void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    // Command: Change movement speed based on behavior (Patrol vs Chase)
    public void SetRunMode(bool isRunning)
    {
        agent.speed = isRunning ? chaseSpeed : moveSpeed;
    }

    // Query: Check if the agent has reached its destination
    public bool HasReachedDestination()
    {
        if (agent.pathPending) return false;

        // Check remaining distance against stopping distance
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    // Query: Check current velocity for animation purposes
    public float GetCurrentSpeed()
    {
        return agent.velocity.magnitude;
    }
}