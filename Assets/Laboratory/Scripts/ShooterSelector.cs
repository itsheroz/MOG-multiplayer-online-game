using UnityEngine;

/// <summary>
/// This is the ABSTRACT base class for a shooter selector.
/// It uses the Template Method Pattern to define the setup algorithm in Awake(),
/// while leaving library-specific details to be implemented by child classes.
/// </summary>
[RequireComponent(typeof(Shooter))]
[RequireComponent(typeof(PlayerController))]
public abstract class ShooterSelector : MonoBehaviour
{
    // --- Template Methods to be implemented by child classes ---

    /// <summary>
    /// Child classes must implement this to check if their specific network library is active.
    /// </summary>
    protected abstract bool IsNetworkActive();


    /// Child classes must implement this to provide their specific online shooter component.
    protected abstract IShooter GetOnlineShooter();
    /// Child classes must implement this to provide their specific online Healer component.
    protected abstract IHealer GetOnlineHealer();

    // --- The main algorithm, defined once in the base class ---
    private void Awake()
    {
        PlayerController controller = GetComponent<PlayerController>();
        IShooter offlineShooter = GetComponent<Shooter>();
        IHealer offlineHealer = GetComponent<Healer>();

        // Let the child class provide the correct online shooter.
        IShooter onlineShooter = GetOnlineShooter();
        IHealer onlineHealer = GetOnlineHealer();

        // Let the child class determine if we are in an online mode.
        if (IsNetworkActive())
        {
            // --- ONLINE MODE ---
            (offlineShooter as MonoBehaviour).enabled = false;
            (onlineShooter as MonoBehaviour).enabled = true;
            controller.SetShooter(onlineShooter);

            /// Healer Setting
            (offlineHealer as MonoBehaviour).enabled = false;
            (onlineHealer as MonoBehaviour).enabled = true;
            controller.SetHealer(onlineHealer);
        }
        else
        {
            // --- OFFLINE MODE ---
            (onlineShooter as MonoBehaviour).enabled = false;
            (offlineShooter as MonoBehaviour).enabled = true;
            controller.SetShooter(offlineShooter);

            /// Healer Setting
            (onlineHealer as MonoBehaviour).enabled = false;
            (offlineHealer as MonoBehaviour).enabled = true;
            controller.SetHealer(offlineHealer);
        }
    }
}