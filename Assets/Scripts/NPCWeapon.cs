using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// NPC weapon controller that handles shooting mechanics with line-of-sight checks
    /// and probability-based damage control. Integrates with NPCController FSM.
    /// </summary>
    [RequireComponent(typeof(NPCController))]
    [RequireComponent(typeof(NPCSensor))]
    public class NPCWeapon : MonoBehaviour
    {
        [Header("Firing Settings")]
        [Tooltip("Projectile prefab to spawn when firing")]
        public GameObject projectilePrefab;

        [Tooltip("Fire rate (shots per second)")]
        public float fireRate = 1f;

        [Tooltip("Minimum range to start firing")]
        public float minFiringRange = 5f;

        [Tooltip("Maximum firing range")]
        public float maxFiringRange = 30f;

        [Tooltip("Enable firing (set false to disable ranged attacks)")]
        public bool enableFiring = true;

        [Header("Damage Model")]
        [Tooltip("Damage model for NPC projectiles")]
        public NPCDamageController.DamageModel damageModel = NPCDamageController.DamageModel.Asymmetric;

        [Header("Damage Settings")]
        [Tooltip("Minimum damage per shot")]
        public float minDamage = 5f;

        [Tooltip("Maximum damage per shot")]
        public float maxDamage = 15f;

        [Header("Symmetric Model Settings")]
        [Tooltip("Damage variance range (0.7 = 70% to 100% of base damage)")]
        [Range(0.1f, 1f)]
        public float symmetricVarianceMin = 0.7f;

        [Header("Asymmetric Model Settings")]
        [Tooltip("Probability of dealing high damage")]
        [Range(0f, 1f)]
        public float highDamageProbability = 0.2f;

        [Tooltip("Probability of dealing medium damage")]
        [Range(0f, 1f)]
        public float mediumDamageProbability = 0.5f;

        [Tooltip("Low damage multiplier")]
        public float lowDamageMultiplier = 0.5f;

        [Tooltip("Medium damage multiplier")]
        public float mediumDamageMultiplier = 1.0f;

        [Tooltip("High damage multiplier")]
        public float highDamageMultiplier = 1.5f;

        [Header("Spawn Settings")]
        [Tooltip("Offset from NPC position where projectile spawns")]
        public Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

        [Header("Accuracy Settings")]
        [Tooltip("Accuracy percentage (1.0 = perfect aim, 0.5 = 50% accurate)")]
        [Range(0f, 1f)]
        public float accuracy = 0.7f;

        [Tooltip("Enable leading target (predictive aiming)")]
        public bool enableLeadTarget = true;

        [Header("Visual Settings")]
        [Tooltip("Enable muzzle flash effect")]
        public bool enableMuzzleFlash = true;

        [Tooltip("Muzzle flash duration")]
        public float muzzleFlashDuration = 0.1f;

        [Tooltip("Projectile color")]
        public Color projectileColor = Color.red;

        // Internal state
        private float lastFireTime = -999f;
        private NPCController npcController;
        private NPCSensor npcSensor;
        private Transform player;
        private Light muzzleFlashLight;

        void Start()
        {
            npcController = GetComponent<NPCController>();
            npcSensor = GetComponent<NPCSensor>();

            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // Create default projectile if none assigned
            if (projectilePrefab == null)
            {
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
                muzzleFlashLight.color = Color.red;
                muzzleFlashLight.range = 4f;
                muzzleFlashLight.intensity = 1.5f;
                muzzleFlashLight.enabled = false;
            }
        }

        void Update()
        {
            // Only fire if enabled and not a weak NPC
            if (!enableFiring || npcController.isWeakNPC)
                return;

            // Allow firing in specific states only
            // This enables NPCs to fire at play start if player is in range
            NPCState[] allowedFiringStates = { NPCState.Idle, NPCState.Patrol, NPCState.Chase };
            bool canFireInCurrentState = System.Array.Exists(allowedFiringStates, 
                state => state == npcController.currentState);
            
            if (!canFireInCurrentState)
                return;

            // Check if can fire (prevent division by zero)
            if (fireRate <= 0f || Time.time - lastFireTime < 1f / fireRate)
                return;

            // Check if player is in firing range
            if (player == null)
                return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer >= minFiringRange && distanceToPlayer <= maxFiringRange)
            {
                // Check line of sight
                if (npcSensor.CanSeePlayer())
                {
                    TryFire();
                }
            }
        }

        /// <summary>
        /// Attempt to fire at the player
        /// </summary>
        private void TryFire()
        {
            if (player == null)
                return;

            // Calculate fire direction with accuracy
            Vector3 fireDirection = GetFireDirection();

            // Fire projectile
            Fire(fireDirection);
            lastFireTime = Time.time;
        }

        /// <summary>
        /// Get the direction to fire with accuracy and optional target leading
        /// </summary>
        private Vector3 GetFireDirection()
        {
            Vector3 targetPosition = player.position;

            // Lead the target (predictive aiming)
            if (enableLeadTarget)
            {
                // Get player velocity
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                CharacterController playerCC = player.GetComponent<CharacterController>();
                
                Vector3 playerVelocity = Vector3.zero;
                if (playerRb != null)
                {
                    playerVelocity = playerRb.velocity;
                }
                else if (playerCC != null)
                {
                    playerVelocity = playerCC.velocity;
                }

                // Calculate projectile travel time
                float distance = Vector3.Distance(transform.position, targetPosition);
                Projectile proj = projectilePrefab.GetComponent<Projectile>();
                float projectileSpeed = proj != null ? proj.speed : 20f;
                float travelTime = distance / projectileSpeed;

                // Lead the target
                targetPosition += playerVelocity * travelTime;
            }

            // Calculate base direction
            Vector3 spawnPos = GetSpawnPosition();
            Vector3 direction = (targetPosition - spawnPos).normalized;

            // Apply accuracy deviation
            if (accuracy < 1f)
            {
                // Add random spread based on accuracy
                float maxDeviation = (1f - accuracy) * 15f; // Max 15 degrees deviation at 0 accuracy
                float horizontalDeviation = Random.Range(-maxDeviation, maxDeviation);
                float verticalDeviation = Random.Range(-maxDeviation, maxDeviation);

                // Apply deviation
                direction = Quaternion.Euler(verticalDeviation, horizontalDeviation, 0) * direction;
            }

            return direction;
        }

        /// <summary>
        /// Fire a projectile
        /// </summary>
        private void Fire(Vector3 direction)
        {
            // Spawn projectile
            Vector3 spawnPos = GetSpawnPosition();
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
                projectileScript.symmetricVarianceMin = symmetricVarianceMin;
                projectileScript.highDamageProbability = highDamageProbability;
                projectileScript.mediumDamageProbability = mediumDamageProbability;
                projectileScript.lowDamageMultiplier = lowDamageMultiplier;
                projectileScript.mediumDamageMultiplier = mediumDamageMultiplier;
                projectileScript.highDamageMultiplier = highDamageMultiplier;
                projectileScript.trailColor = projectileColor;

                // Initialize with direction and owner
                projectileScript.Initialize(direction, gameObject, false);
            }

            // Muzzle flash effect
            if (enableMuzzleFlash && muzzleFlashLight != null)
            {
                muzzleFlashLight.enabled = true;
                Invoke(nameof(DisableMuzzleFlash), muzzleFlashDuration);
            }

            Debug.Log($"{gameObject.name} fired projectile using {damageModel} model");
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
            Debug.LogWarning($"NPCWeapon on {gameObject.name}: Creating default projectile at runtime. For production, assign a projectile prefab in Inspector.");
            
            projectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectilePrefab.name = "NPCDefaultProjectile";
            projectilePrefab.transform.localScale = Vector3.one * 0.2f;
            
            // Add rigidbody
            Rigidbody rb = projectilePrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add projectile script
            Projectile projScript = projectilePrefab.AddComponent<Projectile>();
            projScript.speed = 15f; // NPCs have slower projectiles

            // Set material color
            Renderer renderer = projectilePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = projectileColor;
            }

            // Don't destroy template
            projectilePrefab.SetActive(false);
        }

        /// <summary>
        /// Configure weapon based on NPC archetype
        /// </summary>
        public void ConfigureArchetype(NPCDamageController.NPCArchetype archetype)
        {
            switch (archetype)
            {
                case NPCDamageController.NPCArchetype.WeakGrunt:
                    minDamage = 3f;
                    maxDamage = 8f;
                    fireRate = 0.5f;
                    accuracy = 0.5f;
                    break;

                case NPCDamageController.NPCArchetype.Normal:
                    minDamage = 5f;
                    maxDamage = 15f;
                    fireRate = 1f;
                    accuracy = 0.7f;
                    break;

                case NPCDamageController.NPCArchetype.Elite:
                    minDamage = 10f;
                    maxDamage = 25f;
                    fireRate = 1.5f;
                    accuracy = 0.85f;
                    highDamageProbability = 0.3f;
                    mediumDamageProbability = 0.5f;
                    break;

                case NPCDamageController.NPCArchetype.Boss:
                    minDamage = 20f;
                    maxDamage = 50f;
                    fireRate = 2f;
                    accuracy = 0.9f;
                    highDamageProbability = 0.4f;
                    mediumDamageProbability = 0.4f;
                    break;
            }
        }

        // Visualization
        void OnDrawGizmosSelected()
        {
            // Draw min firing range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minFiringRange);

            // Draw max firing range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxFiringRange);

            // Draw spawn position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetSpawnPosition(), 0.3f);
        }
    }
}
