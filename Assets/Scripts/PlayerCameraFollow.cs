using UnityEngine;

/// <summary>
/// Simple and effective camera follow script for top-down games.
/// Positions the camera above the player and follows smoothly.
/// Perfect for Clash of Clans, Diablo, or RTS-style gameplay.
/// </summary>
public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player GameObject to follow. If null, will auto-find GameObject tagged 'Player'")]
    public Transform target;

    [Header("Camera Position")]
    [Tooltip("Offset from player position. X=side, Y=height, Z=forward/back")]
    public Vector3 offset = new Vector3(0f, 15f, -10f);

    [Tooltip("Camera viewing angle (degrees from horizontal). 45° = isometric, 60° = semi-top-down, 90° = pure top-down")]
    [Range(30f, 90f)]
    public float viewAngle = 50f;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (lower = smoother but more lag, higher = more responsive)")]
    [Range(0.01f, 1f)]
    public float followSmoothness = 0.125f;

    [Tooltip("How smoothly the camera rotates when following")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Header("Optional: Zoom Control")]
    [Tooltip("Enable mouse wheel zoom")]
    public bool enableZoom = false;

    [Tooltip("Minimum camera height when zooming")]
    public float minHeight = 8f;

    [Tooltip("Maximum camera height when zooming")]
    public float maxHeight = 30f;

    [Tooltip("Zoom speed multiplier")]
    public float zoomSpeed = 2f;

    // Private state
    private Vector3 velocity = Vector3.zero;
    private float currentHeight;

    void Start()
    {
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("PlayerCameraFollow: Auto-found player GameObject");
            }
            else
            {
                Debug.LogError("PlayerCameraFollow: No target assigned and no GameObject with 'Player' tag found!");
            }
        }

        // Initialize current height from offset
        currentHeight = offset.y;

        // Position camera initially
        if (target != null)
        {
            PositionCamera(true);
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Handle zoom if enabled
        if (enableZoom)
        {
            HandleZoom();
        }

        // Follow the player
        PositionCamera(false);
    }

    /// <summary>
    /// Position the camera relative to the player
    /// </summary>
    /// <param name="instant">If true, snap instantly without smoothing</param>
    void PositionCamera(bool instant)
    {
        // Calculate desired position
        Vector3 currentOffset = offset;
        currentOffset.y = currentHeight; // Use current height (may be modified by zoom)

        Vector3 desiredPosition = target.position + currentOffset;

        // Move camera smoothly or instantly
        if (instant)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothness);
        }

        // Calculate rotation to look at player
        Vector3 lookDirection = target.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        // Apply rotation smoothly
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness / Time.deltaTime);
    }

    /// <summary>
    /// Handle mouse wheel zoom
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentHeight -= scroll * zoomSpeed;
            currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

            // Adjust forward/back offset proportionally to maintain view angle
            float heightRatio = currentHeight / offset.y;
            offset.z = offset.z * heightRatio;
        }
    }

    /// <summary>
    /// Focus camera on player instantly (useful for respawn or teleport)
    /// </summary>
    public void SnapToPlayer()
    {
        if (target != null)
        {
            PositionCamera(true);
        }
    }

    /// <summary>
    /// Set a new target to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            SnapToPlayer();
        }
    }

    // Draw gizmos in editor to visualize camera follow
    void OnDrawGizmosSelected()
    {
        if (target == null)
            return;

        // Draw line from camera to player
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);

        // Draw sphere at target
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, 0.5f);

        // Draw camera position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
