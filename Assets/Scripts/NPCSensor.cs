using UnityEngine;
using System.Collections.Generic;

namespace NPCAISystem
{
    /// <summary>
    /// Handles NPC sensing capabilities including vision (FOV cone) and hearing (sound events)
    /// </summary>
    public class NPCSensor : MonoBehaviour
    {
        [Header("Vision Settings")]
        [Tooltip("Field of view angle in degrees (e.g., 90-120)")]
        [Range(0f, 360f)]
        public float visionAngle = 110f;

        [Tooltip("Maximum distance the NPC can see")]
        public float visionRange = 15f;

        [Tooltip("Layer mask for obstacles that block vision")]
        public LayerMask visionObstacleMask;

        [Header("Hearing Settings")]
        [Tooltip("Maximum distance the NPC can hear sounds")]
        public float hearingRange = 20f;

        [Header("Detection Settings")]
        [Tooltip("How often to check for player (in seconds)")]
        public float detectionInterval = 0.2f;

        [Tooltip("Target layer to detect (typically player)")]
        public LayerMask targetMask;

        // Internal state
        private Transform player;
        private float lastDetectionTime;
        private Vector3 lastKnownPlayerPosition;
        private bool hasLastKnownPosition = false;

        // Events for detection
        public System.Action<Transform> OnPlayerDetected;
        public System.Action OnPlayerLost;
        public System.Action<Vector3> OnSoundHeard;

        void Start()
        {
            // Find player by tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"NPCSensor on {gameObject.name}: No GameObject with 'Player' tag found!");
            }
        }

        void Update()
        {
            // Periodic detection check
            if (Time.time - lastDetectionTime >= detectionInterval)
            {
                lastDetectionTime = Time.time;
                CheckForPlayer();
            }
        }

        /// <summary>
        /// Checks if the player is within vision range and FOV cone
        /// </summary>
        private void CheckForPlayer()
        {
            if (player == null) return;

            Vector3 directionToPlayer = player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            // Check if player is within vision range
            if (distanceToPlayer <= visionRange)
            {
                // Check if player is within FOV cone
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angleToPlayer <= visionAngle / 2f)
                {
                    // Perform raycast to check line of sight
                    if (HasLineOfSight(player.position))
                    {
                        lastKnownPlayerPosition = player.position;
                        hasLastKnownPosition = true;
                        OnPlayerDetected?.Invoke(player);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Performs a raycast to verify line of sight to target position
        /// </summary>
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;

            // Raycast from NPC eye level to target
            Vector3 rayStart = transform.position + Vector3.up * 1.5f;
            
            // Use combined mask of vision obstacles and target
            int combinedMask = visionObstacleMask | targetMask;
            
            if (Physics.Raycast(rayStart, direction.normalized, out RaycastHit hit, distance, combinedMask))
            {
                // Check if we hit the player or an obstacle
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Simulates hearing a sound at a specific position
        /// </summary>
        public void HearSound(Vector3 soundPosition, float soundRadius)
        {
            float distanceToSound = Vector3.Distance(transform.position, soundPosition);
            
            if (distanceToSound <= hearingRange && distanceToSound <= soundRadius)
            {
                lastKnownPlayerPosition = soundPosition;
                hasLastKnownPosition = true;
                OnSoundHeard?.Invoke(soundPosition);
            }
        }

        /// <summary>
        /// Returns the last known player position
        /// </summary>
        public bool TryGetLastKnownPosition(out Vector3 position)
        {
            position = lastKnownPlayerPosition;
            return hasLastKnownPosition;
        }

        /// <summary>
        /// Clears the last known player position
        /// </summary>
        public void ClearLastKnownPosition()
        {
            hasLastKnownPosition = false;
        }

        /// <summary>
        /// Checks if player is currently visible
        /// </summary>
        public bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector3 directionToPlayer = player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= visionRange)
            {
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                if (angleToPlayer <= visionAngle / 2f)
                {
                    return HasLineOfSight(player.position);
                }
            }

            return false;
        }

        // Visualization in editor
        void OnDrawGizmosSelected()
        {
            // Draw vision range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);

            // Draw hearing range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            // Draw FOV cone
            Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward * visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward * visionRange;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

            // Draw last known position if available
            if (hasLastKnownPosition)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 1f);
            }
        }
    }
}
