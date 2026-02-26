using System.Collections.Generic;
using UnityEngine;

// 1. WORLD STATE: Describes the current situation (e.g., "HasWeapon", "CanSeePlayer")
public class WorldState
{
    private Dictionary<string, bool> states = new Dictionary<string, bool>();

    public void Set(string key, bool value)
    {
        if (states.ContainsKey(key)) states[key] = value;
        else states.Add(key, value);
    }

    public bool Has(string key) => states.ContainsKey(key);
    public bool Get(string key) => states.ContainsKey(key) ? states[key] : false;
}

// 2. GOAP ACTION: Base class for anything the AI can DO
public abstract class GOAP_Action
{
    public string Name;
    public float Cost = 1.0f;

    // What is required to run this action? (e.g., Need "HasAmmo" to "Shoot")
    public Dictionary<string, bool> Preconditions = new Dictionary<string, bool>();

    // What happens after this action? (e.g., "Shoot" results in "PlayerDead")
    public Dictionary<string, bool> Effects = new Dictionary<string, bool>();

    // References to our controllers
    protected AIActionController actionController;
    protected AINetworkHandler networkHandler;

    public GOAP_Action(AIActionController _actions, AINetworkHandler _net)
    {
        actionController = _actions;
        networkHandler = _net;
        Setup();
    }

    // Configure Preconditions and Effects here
    public abstract void Setup();

    // Check if the action can physically be performed right now
    public abstract bool IsValid();

    // Execute the logic (return true if finished)
    public abstract bool Perform();
}

// 3. GOAP GOAL: What we want to achieve
public class GOAP_GOAL
{
    public string Name;
    public int Priority; // Higher is better
    public Dictionary<string, bool> DesiredStates = new Dictionary<string, bool>();

    public GOAP_GOAL(string name, int priority)
    {
        Name = name;
        Priority = priority;
    }
}