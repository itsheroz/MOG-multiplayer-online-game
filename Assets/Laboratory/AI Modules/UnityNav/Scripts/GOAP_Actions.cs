using System.Xml.Linq;
using UnityEngine;

// --- ACTION: CHASE WITH TIMEOUT ---
public class Action_Chase : GOAP_Action
{
    private AITargetSelector selector;
    private float maxTime = 5f;
    private float cooldownTime = 2f;

    // Action State
    private float timer;
    private bool isChasing = false;
    private float nextAvailableTime = 0f; // Cooldown timestamp

    public Action_Chase(AIActionController a, AINetworkHandler n, AITargetSelector s) : base(a, n)
    {
        selector = s;
    }

    public override void Setup()
    {
        Name = "Chase";
        Cost = 2f;
        Preconditions.Add("CanSeePlayer", true);
        Effects.Add("PlayerCaught", true);
    }

    public override bool IsValid()
    {
        // Invalid if no target OR if we are on cooldown
        if (Time.time < nextAvailableTime) return false;
        return selector.GetCurrentTarget() != null;
    }

    public override bool Perform()
    {
        Transform target = selector.GetCurrentTarget();

        // 1. Target Lost logic
        if (target == null)
        {
            StopChasing();
            return true; // Action finished (failed)
        }

        // 2. Timer Logic
        if (!isChasing)
        {
            isChasing = true;
            timer = maxTime;
        }

        timer -= Time.deltaTime;

        // 3. Timeout logic
        if (timer <= 0)
        {
            StopChasing();
            actionController.Stop();
            return true; // Action finished (time up)
        }

        // 4. Caught logic
        if (Vector3.Distance(actionController.transform.position, target.position) < 1.5f)
        {
            isChasing = false;
            actionController.Stop();
            return true; // Success
        }

        // 5. Execution
        actionController.SetRunMode(true);
        actionController.MoveTo(target.position);
        networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Run);
        return false; // Still running
    }

    private void StopChasing()
    {
        isChasing = false;
        selector.ClearTarget(); // 1. Remove target
        nextAvailableTime = Time.time + cooldownTime; // 2. Set Cooldown
    }
}

// --- ACTION: FLEE ---
public class Action_Flee : GOAP_Action
{
    private Transform self;
    private LayerMask aiLayer;

    public Action_Flee(AIActionController a, AINetworkHandler n, Transform s, LayerMask l) : base(a, n)
    {
        self = s; aiLayer = l;
    }

    public override void Setup()
    {
        Name = "Flee";
        Cost = 1f;
        Preconditions.Add("IsThreatened", true);
        Effects.Add("IsSafe", true);
    }

    public override bool IsValid() => true;

    public override bool Perform()
    {
        Collider[] hits = Physics.OverlapSphere(self.position, 3f, aiLayer);
        if (hits.Length > 0)
        {
            Vector3 dir = self.position - hits[0].transform.position;
            actionController.SetRunMode(false);
            actionController.MoveTo(self.position + dir.normalized * 2f);
            networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
            return false;
        }
        return true;
    }
}

// --- ACTION: WANDER ---
public class Action_Wander : GOAP_Action
{
    private float timer = 0;
    public Action_Wander(AIActionController a, AINetworkHandler n) : base(a, n) { }

    public override void Setup()
    {
        Name = "Wander";
        Cost = 5f;
        Effects.Add("IsPatrolling", true);
    }

    public override bool IsValid() => true;

    public override bool Perform()
    {
        if (actionController.HasReachedDestination())
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = Random.Range(1f, 3f);
                Vector3 rnd = Random.insideUnitSphere * 10f + actionController.transform.position;
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(rnd, out hit, 10f, 1))
                    actionController.MoveTo(hit.position);
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Walk);
            }
            else
            {
                actionController.Stop();
                networkHandler.SyncVisualState(AINetworkHandler.AIAnimState.Idle);
            }
        }
        // Wander ไม่มีวันจบในลูปนี้ (Return false ตลอดเพื่อให้ทำไปเรื่อยๆ จนกว่าจะมี Plan อื่นสำคัญกว่าแทรก)
        return false;
    }
}