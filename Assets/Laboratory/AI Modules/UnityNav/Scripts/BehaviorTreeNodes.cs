using System.Collections.Generic;
using UnityEngine;

// 1. Enum for the result of a node's process
public enum NodeState
{
    RUNNING, // Still working (e.g., moving to target)
    SUCCESS, // Finished work successfully
    FAILURE  // Cannot do the work (e.g., no target found)
}

// 2. Base Class for all nodes
public abstract class Node
{
    protected NodeState state;
    public NodeState nodeState => state;
    public abstract NodeState Evaluate();
}

// 3. Composite Node: Selector (OR Logic)
// Tries children one by one. If one Succeeds or is Running, it returns that state.
// Acts like a Priority Selector.
public class Selector : Node
{
    private List<Node> nodes = new List<Node>();

    public Selector(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    continue; // Try next child
                case NodeState.SUCCESS:
                    state = NodeState.SUCCESS;
                    return state;
                case NodeState.RUNNING:
                    state = NodeState.RUNNING;
                    return state;
            }
        }
        state = NodeState.FAILURE;
        return state;
    }
}

// 4. Composite Node: Sequence (AND Logic)
// Runs children in order. If one fails, the whole sequence fails.
public class Sequence : Node
{
    private List<Node> nodes = new List<Node>();

    public Sequence(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;

        foreach (var node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    state = NodeState.FAILURE;
                    return state;
                case NodeState.SUCCESS:
                    continue; // Do next step
                case NodeState.RUNNING:
                    anyChildRunning = true;
                    state = NodeState.RUNNING;
                    return state;
            }
        }

        state = anyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return state;
    }
}