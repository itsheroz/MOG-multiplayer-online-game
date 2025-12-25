using UnityEngine;
using Photon.Pun;

/// <summary>
/// Buffered interpolation (Jitter Buffer). Stores received states and plays them back with a slight delay.
/// Provides smooth movement for characters at the cost of slight latency. Includes initial spawn snapping.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PUN_SyncInterpolationBuffer : MonoBehaviourPun, IPunObservable
{
    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }

    private State[] m_BufferedState = new State[20];
    private int m_TimestampCount;
    private bool _hasReceivedInitialState = false;

    [Tooltip("Time delay for playback. Higher = smoother but more latency.")]
    public double InterpolationDelay = 0.15;

    public void Awake()
    {
        // Initialize buffer to avoid flying from (0,0,0).
        State initialState;
        initialState.timestamp = 0;
        initialState.pos = transform.position;
        initialState.rot = transform.rotation;

        for (int i = 0; i < m_BufferedState.Length; i++)
        {
            m_BufferedState[i] = initialState;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Master Client: Send data.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Remote Client: Buffer the data.
            Vector3 pos = (Vector3)stream.ReceiveNext();
            Quaternion rot = (Quaternion)stream.ReceiveNext();
            double timestamp = info.SentServerTimestamp / 1000.0; // Convert ms to seconds.

            // Handle the very first packet (Spawn Snap).
            if (!_hasReceivedInitialState)
            {
                _hasReceivedInitialState = true;
                transform.position = pos;
                transform.rotation = rot;

                // Overwrite buffer history with this initial state.
                State startState;
                startState.timestamp = timestamp;
                startState.pos = pos;
                startState.rot = rot;

                for (int i = 0; i < m_BufferedState.Length; i++)
                    m_BufferedState[i] = startState;

                m_TimestampCount = 1;
                return;
            }

            // Shift buffer and add new state at index 0.
            for (int i = m_BufferedState.Length - 1; i >= 1; i--)
            {
                m_BufferedState[i] = m_BufferedState[i - 1];
            }

            // Add the new state to the front of the buffer.
            State state;
            state.timestamp = timestamp;
            state.pos = pos;
            state.rot = rot;
            m_BufferedState[0] = state;

            // Update the count of buffered states.
            m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);
        }
    }

    /// <summary>
    /// Finds the correct "playback" time and applies the interpolated transform.
    /// </summary>
    public void Update()
    {
        // Master Client controls logic directly.
        if (PhotonNetwork.IsMasterClient || !_hasReceivedInitialState) return;

        // Calculate the target "playback" time, which is in the past.
        double currentTime = PhotonNetwork.Time;
        double interpolationTime = currentTime - InterpolationDelay;

        // Check if we have a state older than the playback time.
        if (m_BufferedState[0].timestamp > interpolationTime)
        {
            // Find the two states in our buffer that surround the interpolationTime.
            for (int i = 0; i < m_TimestampCount; i++)
            {
                // Find the surrounding states (Older and Newer).
                if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                {
                    State rhs = m_BufferedState[Mathf.Max(i - 1, 0)]; // Newer
                    State lhs = m_BufferedState[i];                   // Older

                    // Calculate the time difference between these two states.
                    double diff = rhs.timestamp - lhs.timestamp;
                    float t = 0.0F;

                    // Calculate the interpolation factor (t) based on where our playback time falls.
                    if (diff > 0.0001)
                        t = (float)((interpolationTime - lhs.timestamp) / diff);

                    // Interpolate between history states.
                    transform.position = Vector3.Lerp(lhs.pos, rhs.pos, t);
                    transform.rotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                    return;
                }
            }
        }
        else
        {
            // Lagging behind buffer; catch up to the latest known state.
            State latest = m_BufferedState[0];
            transform.position = Vector3.Lerp(transform.position, latest.pos, Time.deltaTime * 20);
            transform.localRotation = latest.rot;
        }
    }
}