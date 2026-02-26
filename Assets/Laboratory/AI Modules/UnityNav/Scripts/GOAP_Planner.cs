using System.Collections.Generic;
using System.Linq;

public class GOAP_Planner
{
    public Queue<GOAP_Action> Plan(List<GOAP_Action> availableActions, WorldState currentWorldState, GOAP_GOAL goal)
    {
        // Simple Backward Chaining:
        // Start from Goal -> Find Action that satisfies Goal -> Find Action that satisfies Precondition...

        foreach (var action in availableActions)
        {
            if (!action.IsValid()) continue;

            // Check if this action achieves the Goal
            bool achievesGoal = true;
            foreach (var kvp in goal.DesiredStates)
            {
                if (!action.Effects.ContainsKey(kvp.Key) || action.Effects[kvp.Key] != kvp.Value)
                {
                    achievesGoal = false;
                    break;
                }
            }

            if (achievesGoal)
            {
                // Simple Planner: Assume 1-step plan for this tutorial demo.
                // (Full GOAP would recursively check action.Preconditions against WorldState)

                // If action needs nothing special (Preconditions met by WorldState), we can do it!
                if (CheckPreconditions(action, currentWorldState))
                {
                    Queue<GOAP_Action> plan = new Queue<GOAP_Action>();
                    plan.Enqueue(action);
                    return plan;
                }
            }
        }
        return null; // No plan found
    }

    private bool CheckPreconditions(GOAP_Action action, WorldState state)
    {
        foreach (var pre in action.Preconditions)
        {
            if (!state.Has(pre.Key) || state.Get(pre.Key) != pre.Value)
                return false;
        }
        return true;
    }
}