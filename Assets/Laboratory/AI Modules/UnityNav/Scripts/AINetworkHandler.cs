using UnityEngine;
// using Unity.Netcode; // Uncomment if using NetCode for GameObjects
// using Photon.Pun;    // Uncomment if using PUN2

public class AINetworkHandler : MonoBehaviour // Inherit from NetworkBehaviour in NetCode
{
    // Enum to represent visual states for clients
    public enum AIAnimState
    {
        Idle = 0,
        Walk = 1,
        Run = 2
    }

    // Reference to logic to determine ownership
    public bool IsOwnerOrServer => true; // Replace with IsServer or photonView.IsMine

    // Current state variable to sync across network
    // [NetworkVariable] // For NetCode
    public AIAnimState currentAnimState;

    private void Update()
    {
        // Clients only: Read the synced state and update animations
        if (!IsOwnerOrServer)
        {
            UpdateVisuals(currentAnimState);
        }
    }

    // Called by the Brain (Logic Layer) to update the network state
    public void SyncVisualState(AIAnimState newState)
    {
        if (IsOwnerOrServer)
        {
            currentAnimState = newState;
            // If using RPC, call RpcUpdateState(newState) here
            UpdateVisuals(newState); // Update local host visuals immediately
        }
    }

    private void UpdateVisuals(AIAnimState state)
    {
        // Here you would trigger actual Animator parameters
        // Example: animator.SetInteger("State", (int)state);

        // For debugging (if no animations yet):
        // Debug.Log($"Visual Update: {state}");
    }
}