using StarterAssets;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private StarterAssetsInputs _input;
    private IShooter _shooter;

    private void Awake() {
        _input = GetComponent<StarterAssetsInputs>();
        _shooter = GetComponent<IShooter>();
    }

    /// <summary>
    /// This method allows a shooter component to register itself with the controller.
    /// This is part of the Dependency Injection pattern.
    /// </summary>
    public void SetShooter(IShooter newShooter) {
        _shooter = newShooter;
        Debug.Log($"This has registered a shooter: {newShooter.GetType().Name}", this);
    }

    void Update() {
        if (_input != null && _shooter != null && _input.fire) {
            _input.fire = false;
            _shooter.Fire();
        }
    }
}