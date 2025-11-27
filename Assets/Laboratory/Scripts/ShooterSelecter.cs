using UnityEngine;

/// <summary>
/// This is the ABSTRACT base class for a shooter selector.
/// It uses the Template Method Pattern to define the setup algorithm in Awake(),
/// while leaving library-specific details to be implemented by child classes.
/// </summary>
[RequireComponent(typeof(Shooter))]
[RequireComponent(typeof(PlayerController))]
public abstract class ShooterSelector : MonoBehaviour {
    protected abstract bool IsNetworkActive();
    protected abstract IShooter GetOnlineShooter();

    // The main algorithm, defined once in the base class
    private void Awake() {
        PlayerController controller = GetComponent<PlayerController>();
        IShooter offlineShooter = GetComponent<Shooter>();

        // Let the child class provide the correct online shooter.
        IShooter onlineShooter = GetOnlineShooter();

        // Let the child class determine if we are in an online mode.
        if (IsNetworkActive()) {
            // ONLINE MODE
            (offlineShooter as MonoBehaviour).enabled = false;
            (onlineShooter as MonoBehaviour).enabled = true;
            controller.SetShooter(onlineShooter);
        } else {
            // OFFLINE MODE
            (onlineShooter as MonoBehaviour).enabled = false;
            (offlineShooter as MonoBehaviour).enabled = true;
            controller.SetShooter(offlineShooter);
        }
    }
}