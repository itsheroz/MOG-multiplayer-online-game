using StarterAssets;
using UnityEngine;
/// <summary>
/// OFFLINE shooter implementation. It knows how to instantiate a projectile locally.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(PlayerController))]
public class Shooter : PlayerAction {
    [SerializeField] protected GameObject bulletPrefab;
    
        protected override void HandleFire(Vector3 position, Quaternion rotation) {
        Vector3 spawnPosition = position + (rotation * Vector3.forward * 1.5f);
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, rotation);
        bullet.GetComponent<Projectile>().SetupHitAction(() => {
            Destroy(bullet);
        });
    }
}