using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Generic weapon script that can be equipped by NPCs or players.
    /// Handles projectile spawning and firing mechanics.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip("Projectile prefab to spawn when firing")]
        public GameObject projectilePrefab;

        [Tooltip("Muzzle point where projectiles spawn")]
        public Transform muzzle;

        [Header("Firing Settings")]
        [Tooltip("Fire rate (shots per second)")]
        public float fireRate = 1f;

        [Tooltip("Enable automatic firing")]
        public bool automaticFire = false;

        [Header("Damage Settings")]
        [Tooltip("Damage model for projectiles")]
        public NPCDamageController.DamageModel damageModel = NPCDamageController.DamageModel.Symmetric;

        [Tooltip("Minimum damage per shot")]
        public float minDamage = 5f;

        [Tooltip("Maximum damage per shot")]
        public float maxDamage = 15f;

        [Header("Visual Settings")]
        [Tooltip("Projectile color")]
        public Color projectileColor = Color.yellow;

        // Internal state
        private float lastFireTime = -999f;
        private GameObject owner;
        private bool isPlayerWeapon = false;

        /// <summary>
        /// Initialize the weapon with an owner
        /// </summary>
        public void Initialize(GameObject ownerObject, bool forPlayer = false)
        {
            owner = ownerObject;
            isPlayerWeapon = forPlayer;

            // Create default muzzle if not assigned
            if (muzzle == null)
            {
                GameObject muzzleObj = new GameObject("Muzzle");
                muzzleObj.transform.SetParent(transform);
                muzzleObj.transform.localPosition = new Vector3(0f, 0f, 0.5f); // Slightly in front
                muzzle = muzzleObj.transform;
            }

            // Create default projectile if not assigned
            if (projectilePrefab == null)
            {
                Debug.LogWarning("Weapon: No projectile prefab assigned. Weapon will not be able to fire.");
            }
        }

        /// <summary>
        /// Attempt to fire the weapon
        /// </summary>
        /// <param name="direction">Direction to fire in</param>
        /// <returns>True if weapon fired successfully</returns>
        public bool Fire(Vector3 direction)
        {
            // Check fire rate
            if (fireRate <= 0f || Time.time - lastFireTime < 1f / fireRate)
                return false;

            // Check if we have a projectile to spawn
            if (projectilePrefab == null)
            {
                Debug.LogWarning("Weapon: Cannot fire - no projectile prefab assigned!");
                return false;
            }

            // Validate direction
            if (direction.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("Weapon: Invalid fire direction (zero or near-zero)");
                return false;
            }

            // Get spawn position (muzzle is created in Initialize if null)
            Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Set projectile color
            Renderer renderer = projectile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = projectileColor;
            }

            // Initialize projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Configure projectile with weapon settings
                projectileScript.damageModel = damageModel;
                projectileScript.minDamage = minDamage;
                projectileScript.maxDamage = maxDamage;
                projectileScript.trailColor = projectileColor;

                // Initialize with direction and owner
                projectileScript.Initialize(direction.normalized, owner, isPlayerWeapon);
            }
            else
            {
                Debug.LogWarning("Weapon: Projectile prefab is missing Projectile component!");
            }

            lastFireTime = Time.time;
            return true;
        }

        /// <summary>
        /// Check if weapon can fire
        /// </summary>
        public bool CanFire()
        {
            if (projectilePrefab == null)
                return false;

            if (fireRate <= 0f)
                return false;

            return Time.time - lastFireTime >= 1f / fireRate;
        }

        /// <summary>
        /// Get time until weapon can fire again
        /// </summary>
        public float GetCooldownTime()
        {
            if (fireRate <= 0f)
                return float.MaxValue;

            float timeSinceLastFire = Time.time - lastFireTime;
            float cooldown = 1f / fireRate;
            return Mathf.Max(0f, cooldown - timeSinceLastFire);
        }

        // Visualization
        void OnDrawGizmosSelected()
        {
            if (muzzle != null)
            {
                // Draw muzzle position
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(muzzle.position, 0.1f);

                // Draw fire direction
                Gizmos.color = Color.red;
                Vector3 forward = muzzle != null ? muzzle.forward : transform.forward;
                Gizmos.DrawRay(muzzle.position, forward * 2f);
            }
        }
    }
}
