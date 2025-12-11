using UnityEngine;
/// An offline engine that moves an object back and forth
/// between its start position (A) and an end position (B).
public class PingPongMove : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The 'End' (B) position, relative to the start position.")]
    [SerializeField] private Vector3 _endPositionOffset = new Vector3(0, 5, 0);
    [Tooltip("Movement speed in meters per second.")]
    [SerializeField] private float _speed = 2.0f;
    // --- Internal State ---
    private Vector3 _startPosition; // The calculated Start (A) position.
    private Vector3 _endPosition; // The calculated End (B) position.
                                  // The current destination (either A or B).
    private Vector3 _currentTarget;
    void Awake()
    {
        // 1. Save the starting position (A).
        _startPosition = transform.position;
        // 2. Calculate the end position (B) based on the offset.
        _endPosition = _startPosition + _endPositionOffset;
        // 3. Set the first target to B.
        _currentTarget = _endPosition;
    }
    void Update()
    {
        // 1. Move towards the current target at a constant speed.
        transform.position = Vector3.MoveTowards(
        transform.position,
        _currentTarget,
        _speed * Time.deltaTime
        );
        // 2. Check if we have reached the target.
        if (Vector3.Distance(transform.position, _currentTarget) < 0.01f)
        {
            // 3. If reached, swap the target.
            // (If target was A, set to B. If B, set to A).
            _currentTarget = (_currentTarget == _startPosition) ? _endPosition :
            _startPosition;
        }
    }
    /// Draws Gizmos in the Scene Editor for easy setup.
    private void OnDrawGizmosSelected()
    {
        // Calculate positions (use current pos if not playing).
        Vector3 startPos = (Application.isPlaying) ? _startPosition : transform.position;
        Vector3 endPos = startPos + _endPositionOffset;
        // Draw the path line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPos, endPos);
        // Draw spheres at start and end points
        Gizmos.DrawWireSphere(startPos, 0.2f);
        Gizmos.DrawWireSphere(endPos, 0.2f);
    }
}
