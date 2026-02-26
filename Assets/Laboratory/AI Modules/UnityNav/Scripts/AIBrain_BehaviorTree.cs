using System.Collections.Generic;
using UnityEngine;

public class AIBrain_BehaviorTree : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private AIActionController actionController;
    [SerializeField] private AINetworkHandler networkHandler;
    [SerializeField] private AITargetSelector targetSelector;

    [Header("Settings")]
    [SerializeField] private LayerMask aiLayer;
    [SerializeField] private float avoidRange = 3f;
    [SerializeField] private float chaseTimeout = 5f;
    [SerializeField] private float chaseCooldown = 2f; // New Setting

    private Node topNode;

    // Blackboard Data
    private float idleTimer = 0f;
    private bool isIdling = false;
    public float NextChaseTime { get; set; } = 0f; // Shared Cooldown variable

    private void Start()
    {
        if (networkHandler.IsOwnerOrServer) ConstructBehaviorTree();
    }

    private void Update()
    {
        if (networkHandler.IsOwnerOrServer && topNode != null) topNode.Evaluate();
    }

    private void ConstructBehaviorTree()
    {
        // 1. Nodes
        Node avoidNode = new ActionNode_Avoid(actionController, networkHandler, transform, aiLayer, avoidRange);

        // Pass 'this' (the brain) to nodes so they can read/write NextChaseTime
        Node canSeePlayer = new ConditionNode_CanSeePlayer(targetSelector, this);
        Node chaseAction = new ActionNode_Chase(actionController, networkHandler, targetSelector, this, chaseTimeout, chaseCooldown);

        Sequence chaseSequence = new Sequence(new List<Node> { canSeePlayer, chaseAction });

        Node patrolNode = new ActionNode_Patrol(actionController, networkHandler, transform, this);

        // 2. Root Selector
        topNode = new Selector(new List<Node> { avoidNode, chaseSequence, patrolNode });
    }

    // Helpers for Blackboard
    public ref float GetIdleTimer() => ref idleTimer;
    public ref bool GetIsIdling() => ref isIdling;
}

// ================= NODES =================

// --- CONDITION: SEE PLAYER (With Cooldown Check) ---
public class ConditionNode_CanSeePlayer : Node
{
    private AITargetSelector selector;
    private AIBrain_BehaviorTree brain;

    public ConditionNode_CanSeePlayer(AITargetSelector _selector, AIBrain_BehaviorTree _brain)
    {
        selector = _selector;
        brain = _brain;
    }

    public override NodeState Evaluate()
    {
        // Fail if we are on cooldown
        if (Time.time < brain.NextChaseTime) return NodeState.FAILURE;

        if (selector.GetCurrentTarget() != null) return NodeState.SUCCESS;
        return NodeState.FAILURE;
    }
}

// --- ACTION: CHASE (With Timeout & Clear Target) ---
public class ActionNode_Chase : Node
{
    private AIActionController actions;
    private AINetworkHandler network;
    private AITargetSelector selector;
    private AIBrain_BehaviorTree brain;

    private float maxTime;
    private float cooldownTime;
    private float timer;
    private bool isChasing = false;

    public ActionNode_Chase(AIActionController actions, AINetworkHandler network, AITargetSelector selector, AIBrain_BehaviorTree brain, float maxTime, float cooldown)
    {
        this.actions = actions;
        this.network = network;
        this.selector = selector;
        this.brain = brain;
        this.maxTime = maxTime;
        this.cooldownTime = cooldown;
    }

    public override NodeState Evaluate()
    {
        Transform target = selector.GetCurrentTarget();

        // 1. Target Lost -> Fail
        if (target == null)
        {
            ApplyCooldown();
            return NodeState.FAILURE;
        }

        // 2. Init Timer
        if (!isChasing)
        {
            isChasing = true;
            timer = maxTime;
        }

        // 3. Timeout Check
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ApplyCooldown();
            return NodeState.FAILURE; // Timeout -> Fail -> Switch to Patrol
        }

        // 4. Move
        actions.SetRunMode(true);
        actions.MoveTo(target.position);
        network.SyncVisualState(AINetworkHandler.AIAnimState.Run);

        state = NodeState.RUNNING;
        return state;
    }

    private void ApplyCooldown()
    {
        isChasing = false;
        selector.ClearTarget(); // Clear the target reference
        brain.NextChaseTime = Time.time + cooldownTime; // Set cooldown
    }
}

// --- ACTION: AVOID (เหมือนเดิม) ---
public class ActionNode_Avoid : Node
{
    private AIActionController actions;
    private AINetworkHandler network;
    private Transform self;
    private LayerMask mask;
    private float range;

    public ActionNode_Avoid(AIActionController actions, AINetworkHandler network, Transform self, LayerMask mask, float range)
    {
        this.actions = actions;
        this.network = network;
        this.self = self;
        this.mask = mask;
        this.range = range;
    }

    public override NodeState Evaluate()
    {
        Collider[] hits = Physics.OverlapSphere(self.position, range, mask);
        Transform threat = null;
        foreach (var hit in hits) { if (hit.transform != self) { threat = hit.transform; break; } }

        if (threat == null) return NodeState.FAILURE;

        Vector3 dir = self.position - threat.position;
        actions.SetRunMode(false);
        actions.MoveTo(self.position + dir.normalized * 2f);
        network.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
        state = NodeState.RUNNING;
        return state;
    }
}

// --- ACTION: PATROL (เหมือนเดิม) ---
public class ActionNode_Patrol : Node
{
    private AIActionController actions;
    private AINetworkHandler network;
    private Transform self;
    private AIBrain_BehaviorTree ctx;

    public ActionNode_Patrol(AIActionController actions, AINetworkHandler network, Transform self, AIBrain_BehaviorTree ctx)
    {
        this.actions = actions;
        this.network = network;
        this.self = self;
        this.ctx = ctx;
    }

    public override NodeState Evaluate()
    {
        ref float timer = ref ctx.GetIdleTimer();
        ref bool isIdling = ref ctx.GetIsIdling();
        actions.SetRunMode(false);

        if (actions.HasReachedDestination())
        {
            if (!isIdling)
            {
                isIdling = true;
                timer = Random.Range(1f, 3f);
                actions.Stop();
                network.SyncVisualState(AINetworkHandler.AIAnimState.Idle);
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Vector3 rnd = Random.insideUnitSphere * 15f + self.position;
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(rnd, out hit, 15f, 1))
                        actions.MoveTo(hit.position);
                    isIdling = false;
                    network.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
                }
            }
        }
        else network.SyncVisualState(AINetworkHandler.AIAnimState.Walk);

        state = NodeState.RUNNING;
        return state;
    }
}