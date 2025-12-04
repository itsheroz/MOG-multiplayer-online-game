using StarterAssets;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private IShooter _shooter;
    //heal action
    private IHealer _healer;
    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _shooter = GetComponent<IShooter>();
        _healer = GetComponent<IHealer>();
    }

    /// <summary>
    /// This method allows a shooter component to register itself with the controller.
    /// This is part of the Dependency Injection pattern.
    /// </summary>
    public void SetShooter(IShooter newShooter)
    {
        _shooter = newShooter;
        Debug.Log($"PlayerController has registered a shooter: {newShooter.GetType().Name}", this);
    }

    public void SetHealer(IHealer newHealer)
    {
        _healer = newHealer;
        Debug.Log($"PlayerController has registered a healer: {newHealer.GetType().Name}", this);
    }

    void Update()
    {
        if (_input != null && _shooter != null && _input.fire)
        {
            _input.fire = false;
            _shooter.Fire();
        }

        //After add implement heal input on StarterAssetsInputs
        if (_input != null && _healer != null && _input.heal)
        {
            _input.heal = false;
            _healer.LaunchHeal();
        }
    }
}
