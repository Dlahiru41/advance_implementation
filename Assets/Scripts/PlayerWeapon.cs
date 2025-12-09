using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Player weapon controller that handles shooting mechanics with line-of-sight checks
    /// and probability-based damage control.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Firing Settings")]
        [Tooltip("Projectile prefab to spawn when firing")]
        public GameObject projectilePrefab;

        [Tooltip("Fire rate (shots per second)")]
        public float fireRate = 2f;

        [Tooltip("Maximum firing range")]
        public float maxRange = 50f;

        [Tooltip("Fire button (default: left mouse button)")]
        public KeyCode fireButton = KeyCode.Mouse0;

        [Tooltip("Alternative fire button (default: spacebar)")]
        public KeyCode alternativeFireButton = KeyCode.Space;

        [Header("Damage Model")]
        [Tooltip("Damage model for player projectiles")]
        public NPCDamageController.DamageModel damageModel = NPCDamageController.DamageModel.Symmetric;

        [Header("Damage Settings")]
        [Tooltip("Minimum damage per shot")]
        public float minDamage = 8f;

        [Tooltip("Maximum damage per shot")]
        public float maxDamage = 20f;

        [Header("Symmetric Model Settings")]
        [Tooltip("Damage variance range (0.7 = 70% to 100% of base damage)")]
        [Range(0.1f, 1f)]
        public float symmetricVarianceMin = 0.8f;

        [Header("Asymmetric Model Settings")]
        [Tooltip("Probability of dealing high damage")]
        [Range(0f, 1f)]
        public float highDamageProbability = 0.25f;

        [Tooltip("Probability of dealing medium damage")]
        [Range(0f, 1f)]
        public float mediumDamageProbability = 0.5f;

        [Tooltip("Low damage multiplier")]
        public float lowDamageMultiplier = 0.6f;

        [Tooltip("Medium damage multiplier")]
        public float mediumDamageMultiplier = 1.0f;

        [Tooltip("High damage multiplier")]
        public float highDamageMultiplier = 1.4f;

        [Header("Spawn Settings")]
        [Tooltip("Offset from player position where projectile spawns")]
        public Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

        [Header("Visual Settings")]
        [Tooltip("Enable muzzle flash effect")]
        public bool enableMuzzleFlash = true;

        [Tooltip("Muzzle flash duration")]
        public float muzzleFlashDuration = 0.1f;

        [Header("Line of Sight Settings")]
        [Tooltip("Enable line of sight check before firing")]
        public bool requireLineOfSight = true;

        [Tooltip("Layer mask for obstacles that block shots")]
        public LayerMask obstacleLayerMask;

        // Internal state
        private float lastFireTime = -999f;
        private Camera mainCamera;
        private Light muzzleFlashLight;

        void Start()
        {
            mainCamera = Camera.main;

            // Create default projectile if none assigned
            if (projectilePrefab == null)
            {
                Debug.LogWarning("PlayerWeapon: No projectile prefab assigned. Creating default sphere projectile.");
                CreateDefaultProjectile();
            }

            // Setup muzzle flash light
            if (enableMuzzleFlash)
            {
                GameObject lightObj = new GameObject("MuzzleFlashLight");
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = spawnOffset;
                
                muzzleFlashLight = lightObj.AddComponent<Light>();
                muzzleFlashLight.type = LightType.Point;
                muzzleFlashLight.color = Color.yellow;
                muzzleFlashLight.range = 5f;
                muzzleFlashLight.intensity = 2f;
                muzzleFlashLight.enabled = false;
            }
        }

        void Update()
        {
            // Check for fire input
            if (Input.GetKey(fireButton) || Input.GetKey(alternativeFireButton))
            {
                TryFire();
            }
        }

        /// <summary>
        /// Attempt to fire a projectile
        /// </summary>
        private void TryFire()
        {
            // Check fire rate
            if (Time.time - lastFireTime < 1f / fireRate)
                return;

            // Determine fire direction
            Vector3 fireDirection = GetFireDirection();

            // Check line of sight if required
            if (requireLineOfSight && !HasLineOfSight(fireDirection))
            {
                Debug.Log("PlayerWeapon: No clear line of sight to target");
                return;
            }

            // Fire projectile
            Fire(fireDirection);
            lastFireTime = Time.time;
        }

        /// <summary>
        /// Get the direction to fire based on mouse position or forward direction
        /// </summary>
        private Vector3 GetFireDirection()
        {
            // Use mouse position to determine direction
            if (mainCamera != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                
                // Raycast to find where the mouse is pointing
                if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
                {
                    // Fire towards the hit point
                    Vector3 direction = (hit.point - GetSpawnPosition()).normalized;
                    return direction;
                }
                else
                {
                    // Fire in the direction of the ray if no hit
                    return ray.direction;
                }
            }

            // Fallback: fire forward
            return transform.forward;
        }

        /// <summary>
        /// Check if there's a clear line of sight in the fire direction
        /// </summary>
        private bool HasLineOfSight(Vector3 direction)
        {
            Vector3 spawnPos = GetSpawnPosition();
            
            // Raycast to check for obstacles
            if (Physics.Raycast(spawnPos, direction, out RaycastHit hit, maxRange, obstacleLayerMask))
            {
                // Check if we hit an obstacle before reaching max range
                float hitDistance = Vector3.Distance(spawnPos, hit.point);
                
                // Allow shooting if we hit something within range (NPC or player)
                if (hit.collider.GetComponent<NPCController>() != null || 
                    hit.collider.CompareTag("Player"))
                {
                    return true;
                }
                
                // Blocked by obstacle
                return false;
            }

            // No obstacles in the way
            return true;
        }

        /// <summary>
        /// Fire a projectile
        /// </summary>
        private void Fire(Vector3 direction)
        {
            // Spawn projectile
            Vector3 spawnPos = GetSpawnPosition();
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Initialize projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Configure projectile with weapon settings
                projectileScript.damageModel = damageModel;
                projectileScript.minDamage = minDamage;
                projectileScript.maxDamage = maxDamage;
                projectileScript.symmetricVarianceMin = symmetricVarianceMin;
                projectileScript.highDamageProbability = highDamageProbability;
                projectileScript.mediumDamageProbability = mediumDamageProbability;
                projectileScript.lowDamageMultiplier = lowDamageMultiplier;
                projectileScript.mediumDamageMultiplier = mediumDamageMultiplier;
                projectileScript.highDamageMultiplier = highDamageMultiplier;

                // Initialize with direction and owner
                projectileScript.Initialize(direction, gameObject, true);
            }
            else
            {
                Debug.LogWarning("PlayerWeapon: Projectile prefab is missing Projectile component!");
            }

            // Muzzle flash effect
            if (enableMuzzleFlash && muzzleFlashLight != null)
            {
                muzzleFlashLight.enabled = true;
                Invoke(nameof(DisableMuzzleFlash), muzzleFlashDuration);
            }

            Debug.Log($"Player fired projectile using {damageModel} model");
        }

        /// <summary>
        /// Get the spawn position for projectiles
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            return transform.position + spawnOffset;
        }

        /// <summary>
        /// Disable muzzle flash light
        /// </summary>
        private void DisableMuzzleFlash()
        {
            if (muzzleFlashLight != null)
            {
                muzzleFlashLight.enabled = false;
            }
        }

        /// <summary>
        /// Create a default projectile prefab at runtime
        /// </summary>
        private void CreateDefaultProjectile()
        {
            projectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectilePrefab.name = "DefaultProjectile";
            projectilePrefab.transform.localScale = Vector3.one * 0.2f;
            
            // Add rigidbody
            Rigidbody rb = projectilePrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add projectile script
            projectilePrefab.AddComponent<Projectile>();

            // Set material color
            Renderer renderer = projectilePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }

            // Don't destroy template
            projectilePrefab.SetActive(false);
        }

        // Visualization
        void OnDrawGizmosSelected()
        {
            // Draw max firing range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxRange);

            // Draw spawn position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetSpawnPosition(), 0.3f);
        }

        // UI Display
        void OnGUI()
        {
            // Display firing instructions
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.LowerLeft;

            string instructions = $"Left Mouse / Space: Fire ({damageModel} damage)\n";
            instructions += $"Damage: {minDamage:F0}-{maxDamage:F0} | Rate: {fireRate:F1} shots/sec";
            
            GUI.Label(new Rect(10, Screen.height - 80, 400, 60), instructions, style);
        }
    }
}
