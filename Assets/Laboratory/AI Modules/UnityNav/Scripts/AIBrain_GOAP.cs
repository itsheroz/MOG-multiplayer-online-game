using System.Collections.Generic;
using UnityEngine;

public class AIBrain_GOAP : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private AIActionController actionController;
    [SerializeField] private AINetworkHandler networkHandler;
    [SerializeField] private AITargetSelector targetSelector; 
    [SerializeField] private LayerMask aiLayer;

    private GOAP_Planner planner;
    private List<GOAP_Action> availableActions;
    private List<GOAP_GOAL> goals;
    private GOAP_Action currentAction;

    // เก็บสถานะเก่าไว้เทียบว่ามีการเปลี่ยนแปลงไหม
    private bool previousHasTarget = false; 

    private void Start()
    {
        if (!networkHandler.IsOwnerOrServer) return;

        planner = new GOAP_Planner();
        availableActions = new List<GOAP_Action>();
        goals = new List<GOAP_GOAL>();

        // Register Actions
        availableActions.Add(new Action_Flee(actionController, networkHandler, transform, aiLayer));
        availableActions.Add(new Action_Chase(actionController, networkHandler, targetSelector)); 
        availableActions.Add(new Action_Wander(actionController, networkHandler));

        // Register Goals (Priority สูงอยู่บน)

        // 1. Safety First
        GOAP_GOAL safetyGoal = new GOAP_GOAL("Safety", 100);
        safetyGoal.DesiredStates.Add("IsSafe", true);
        goals.Add(safetyGoal);

        // 2. Kill Player
        GOAP_GOAL killGoal = new GOAP_GOAL("KillPlayer", 50);
        killGoal.DesiredStates.Add("PlayerCaught", true);
        goals.Add(killGoal);

        // 3. Patrol (งานอดิเรก ทำเมื่อว่าง)
        GOAP_GOAL patrolGoal = new GOAP_GOAL("Patrol", 10);
        patrolGoal.DesiredStates.Add("IsPatrolling", true);
        goals.Add(patrolGoal);
    }

    private void Update()
    {
        if (!networkHandler.IsOwnerOrServer) return;

        // 1. UPDATE WORLD STATE
        // ------------------------------------------------------------
        WorldState currentState = new WorldState();
        
        // Check Threat
        bool isThreatened = Physics.CheckSphere(transform.position, 3f, aiLayer);
        currentState.Set("IsThreatened", isThreatened);
        if (!isThreatened) currentState.Set("IsSafe", true);

        // Check Target
        bool hasTarget = targetSelector.GetCurrentTarget() != null;
        currentState.Set("CanSeePlayer", hasTarget);


        // 2. INTERRUPTION LOGIC (หัวใจสำคัญของการแก้บั๊กนี้)
        // ------------------------------------------------------------
        // ถ้าสถานะการเจอศัตรูเปลี่ยนไป (จากไม่เจอ -> เป็นเจอ)
        // เราต้อง "สั่งหยุด" Action ปัจจุบัน (เช่น Wander) ทันที เพื่อให้ Planner คิดใหม่
        if (hasTarget != previousHasTarget)
        {
            if (hasTarget) // ถ้าเพิ่งเจอสดๆ ร้อนๆ
            {
                // บังคับหยุด Action เดิม (เพื่อให้หลุดไปเข้า Loop ข้อ 4)
                currentAction = null; 
                actionController.Stop(); // หยุดเดินก่อนด้วย
            }
            previousHasTarget = hasTarget;
        }


        // 3. EXECUTE CURRENT ACTION
        // ------------------------------------------------------------
        if (currentAction != null)
        {
            // ทำ Action ต่อไป
            bool isFinished = currentAction.Perform();

            // ถ้ายังไม่เสร็จ (return false) และไม่ใช่ Action ที่เราอยากขัดจังหวะ -> ให้ return เลย (ไม่ต้องคิดแผนใหม่)
            // แต่ถ้า Action นั้นเป็น Wander เราอาจจะปล่อยผ่านไปให้คิดแผนใหม่ได้ ถ้าต้องการให้ AI ฉลาดมากๆ
            if (!isFinished)
            {
                return; 
            }
            
            // ถ้าทำเสร็จแล้ว (return true) ให้ล้างค่า เตรียมคิดงานต่อไป
            currentAction = null;
        }


        // 4. RE-PLANNING (คิดแผนใหม่)
        // ------------------------------------------------------------
        // จะมาถึงตรงนี้ได้ก็ต่อเมื่อ:
        // A. ยังไม่มี Action (เพิ่งเกิด)
        // B. Action เดิมทำเสร็จแล้ว
        // C. Action เดิมถูกสั่งยกเลิก (จาก Interruption Logic ข้อ 2)
        
        // เรียงลำดับความสำคัญของ Goal
        goals.Sort((x, y) => y.Priority.CompareTo(x.Priority));

        foreach (var goal in goals)
        {
            // ตรวจสอบว่า Goal นี้ทำสำเร็จไปหรือยัง?
            bool goalMet = true;
            foreach(var kvp in goal.DesiredStates) {
                // ถ้า State ปัจจุบันยังไม่ตรงกับ Goal แสดงว่ายังทำไม่เสร็จ
                if(!currentState.Get(kvp.Key)) goalMet = false;
            }
            
            // ถ้า Goal นี้สำเร็จแล้ว ข้ามไปดู Goal รองลงไป
            if(goalMet) continue;

            // หาแผนสำหรับ Goal นี้
            Queue<GOAP_Action> plan = planner.Plan(availableActions, currentState, goal);
            if (plan != null && plan.Count > 0)
            {
                currentAction = plan.Dequeue();
                
                // [Optional] Debug ดูว่า AI คิดอะไรออก
                // Debug.Log($"New Plan: {currentAction.GetType().Name} for Goal: {goal.Priority}");
                
                // เรียก Setup หรือ Start ของ Action ใหม่ (ถ้ามี)
                return; // ได้แผนแล้ว จบ Update รอบนี้
            }
        }
    }
}