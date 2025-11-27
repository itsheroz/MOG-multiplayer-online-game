using System;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// This is a PURE, OFFLINE projectile script. It knows nothing about networking.
/// It handles its own movement, collision, and lifetime.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Tooltip("The force applied to the bullet on instantiation.")]
    public float Force = 20f;
    [Tooltip("How many seconds before the bullet destroys itself if it hits nothing.")]
    public float Lifetime = 10.0f;
    public event Action OnHitAction;
    private void Start()
    {
        // This logic runs for the owner online, and for everyone offline.
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * Force;
        // Destroy(gameObject,Lifetime);
    }
    private void OnTriggerEnter(Collider other)
    {
        // This logic runs for the owner online, and for everyone offline.
        Debug.Log($"Projectile hit {other.name}");
        // For simplicity, we destroy the projectile when it hits anything.
        // In a real game, you might check tags or layers.
        OnHitAction?.Invoke();
    }
    /// <summary>
    /// This is the "injection" method. The creator of this projectile
    /// will call this to tell it what to do upon collision.
    /// </summary>
    /// <param name="hitAction">The action to perform when hitting an object.</param>
    public void SetupHitAction(Action hitAction)
    {
        OnHitAction = hitAction;
    }
}
