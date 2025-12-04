using StarterAssets;
using UnityEngine;

public abstract class PlayerAction : MonoBehaviour, IShooter
{
    private Camera _playerCamera;

    private void Awake()
    {
        // This component is self-sufficient and finds its own camera reference.
        _playerCamera = GetComponent<FirstPersonController>().MainCamera;
    }

    protected void EnsureCamera()
    {
        // Ensure the controller and its camera reference are not null
        if (_playerCamera == null)
        {
            FirstPersonController _controllerLogic = GetComponent<FirstPersonController>();
            if (_controllerLogic.MainCamera == null)
            {
                Debug.LogError("Shooter cannot find the FirstPersonController or its MainCamera reference!");
                return;
            }
            else
            {
                // Use the camera reference from the controller logic
                _playerCamera = _controllerLogic.MainCamera;
            }
        }
    }

    public void Fire()
    {
        if (_playerCamera == null)
        {
            EnsureCamera();
            return;
        }

        HandleFire(_playerCamera.transform.position, _playerCamera.transform.rotation);
    }

    protected abstract void HandleFire(Vector3 position, Quaternion rotation);
}
