using UnityEngine;
/// <summary>
/// This is the OFFLINE implementation of the IHealer interface.
/// It directly instantiates a healing projectile using standard Instantiate.
/// </summary>
public class Healer : MonoBehaviour, IHealer
{
    [Tooltip("The healing projectile prefab to fire.")]
    [SerializeField] private GameObject healBoxPrefab;
    private Camera _playerCamera; // Reference to the player's camera
    private void Awake()
    {
        // A simple way to find the camera.
        // In a real project, this might be set by FirstPersonController or another manager.
        _playerCamera = Camera.main;
    }
    /// Public method from the IHealer interface, called by PlayerController.
    public void LaunchHeal()
    {
        if (_playerCamera == null)
        {
            Debug.LogError("Player camera not set on Healer.", this);
            return;
        }
        // Call the local instantiation method
        LocalLaunch(_playerCamera.transform.position, _playerCamera.transform.rotation);
    }
    /// Handles the actual instantiation of the projectile in an offline environment.
    private void LocalLaunch(Vector3 position, Quaternion rotation)
    {
        Vector3 spawnPosition = position + (rotation * Vector3.forward * 1.5f);
        // 1. Instantiate the projectile using standard Unity Instantiate.
        GameObject bulletGameObject = Instantiate(healBoxPrefab, spawnPosition, rotation);
        // 2. Get the core Projectile component.
        Projectile projectileLogic = bulletGameObject.GetComponent<Projectile>();
        // 3. Inject the OFFLINE destruction logic.
        // The projectile will simply destroy itself upon collision.
        projectileLogic.SetupHitAction(() =>
        {
            Destroy(bulletGameObject);
        });
    }
}