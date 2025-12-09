using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Implements probability-controlled damage system for NPCs with support for
    /// symmetric, asymmetric, and dynamic damage models.
    /// </summary>
    public class NPCDamageController : MonoBehaviour
    {
        /// <summary>
        /// Damage model types
        /// </summary>
        public enum DamageModel
        {
            Symmetric,      // Same rules as player - fair and balanced
            Asymmetric,     // Different probability curves - adds unpredictability
            Dynamic         // Adapts based on NPC state and context
        }

        /// <summary>
        /// NPC archetype determines damage characteristics
        /// </summary>
        public enum NPCArchetype
        {
            WeakGrunt,      // Low damage, low variance
            Normal,         // Medium damage, standard variance
            Elite,          // High damage, bell curve distribution
            Boss            // Very high damage, high burst probability
        }

        [Header("Damage Model Configuration")]
        [Tooltip("The damage calculation model to use")]
        public DamageModel damageModel = DamageModel.Symmetric;

        [Tooltip("NPC archetype - determines base damage characteristics")]
        public NPCArchetype archetype = NPCArchetype.Normal;

        [Header("Base Damage Settings")]
        [Tooltip("Minimum damage this NPC can deal")]
        public float minDamage = 5f;

        [Tooltip("Maximum damage this NPC can deal")]
        public float maxDamage = 15f;

        [Header("Symmetric Model Settings")]
        [Tooltip("Damage variance range (0.7 = 70% to 100% of base damage)")]
        [Range(0.1f, 1f)]
        public float symmetricVarianceMin = 0.7f;

        [Header("Asymmetric Model Settings")]
        [Tooltip("Probability of dealing high damage (150% of base)")]
        [Range(0f, 1f)]
        public float highDamageProbability = 0.2f;

        [Tooltip("Probability of dealing medium damage (100% of base)")]
        [Range(0f, 1f)]
        public float mediumDamageProbability = 0.5f;

        [Tooltip("Low damage dealt if not high or medium (50% of base)")]
        public float lowDamageMultiplier = 0.5f;

        [Tooltip("Medium damage multiplier")]
        public float mediumDamageMultiplier = 1.0f;

        [Tooltip("High damage multiplier")]
        public float highDamageMultiplier = 1.5f;

        [Header("Dynamic Model Settings")]
        [Tooltip("Enable health-based damage scaling")]
        public bool useHealthScaling = true;

        [Tooltip("Health threshold (0-1) below which damage increases")]
        [Range(0f, 1f)]
        public float lowHealthThreshold = 0.3f;

        [Tooltip("Bonus damage probability when health is low")]
        [Range(0f, 0.5f)]
        public float lowHealthDamageBonus = 0.2f;

        [Tooltip("Enable aggression-based damage scaling")]
        public bool useAggressionScaling = true;

        [Tooltip("Additional damage probability for aggressive NPCs")]
        [Range(0f, 0.3f)]
        public float aggressionDamageBonus = 0.1f;

        [Tooltip("Enable distance-based damage scaling")]
        public bool useDistanceScaling = true;

        [Tooltip("Distance (meters) considered 'close' for bonus damage")]
        public float closeCombatDistance = 3f;

        [Tooltip("Damage bonus when close to target")]
        [Range(0f, 0.3f)]
        public float closeCombatDamageBonus = 0.15f;

        [Tooltip("Enable group size damage scaling")]
        public bool useGroupScaling = true;

        [Tooltip("Damage reduction per additional NPC in group")]
        [Range(0f, 0.3f)]
        public float groupDamageReduction = 0.1f;

        [Header("Attack Settings")]
        [Tooltip("Minimum time between attacks (seconds)")]
        public float attackCooldown = 2f;

        [Tooltip("Range at which NPC can attack player")]
        public float attackRange = 2f;

        [Header("Critical Hit Settings")]
        [Tooltip("Enable critical hit system")]
        public bool enableCriticalHits = true;

        [Tooltip("Base critical hit chance")]
        [Range(0f, 0.5f)]
        public float criticalHitChance = 0.1f;

        [Tooltip("Critical hit damage multiplier")]
        [Range(1.5f, 3f)]
        public float criticalHitMultiplier = 2f;

        // Private state
        private float lastAttackTime = -999f;
        private NPCController npcController;
        private Transform player;
        private float currentHealth = 100f;
        private float maxHealth = 100f;

        void Start()
        {
            npcController = GetComponent<NPCController>();
            
            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // Configure archetype-specific settings
            ConfigureArchetype();
        }

        void Update()
        {
            // Check if we can attack
            if (player == null || npcController == null)
                return;

            // Only attack during chase state
            if (npcController.currentState != NPCState.Chase)
                return;

            // Check if enough time has passed since last attack
            if (Time.time - lastAttackTime < attackCooldown)
                return;

            // Check if player is in range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                PerformAttack();
            }
        }

        /// <summary>
        /// Configure damage settings based on NPC archetype
        /// </summary>
        private void ConfigureArchetype()
        {
            switch (archetype)
            {
                case NPCArchetype.WeakGrunt:
                    minDamage = 3f;
                    maxDamage = 8f;
                    criticalHitChance = 0.05f;
                    attackCooldown = 2.5f;
                    break;

                case NPCArchetype.Normal:
                    minDamage = 5f;
                    maxDamage = 15f;
                    criticalHitChance = 0.1f;
                    attackCooldown = 2f;
                    break;

                case NPCArchetype.Elite:
                    minDamage = 10f;
                    maxDamage = 25f;
                    criticalHitChance = 0.15f;
                    attackCooldown = 1.5f;
                    highDamageProbability = 0.3f;
                    mediumDamageProbability = 0.5f;
                    break;

                case NPCArchetype.Boss:
                    minDamage = 20f;
                    maxDamage = 50f;
                    criticalHitChance = 0.2f;
                    attackCooldown = 1.8f;
                    highDamageProbability = 0.4f;
                    mediumDamageProbability = 0.4f;
                    break;
            }
        }

        /// <summary>
        /// Execute an attack on the player
        /// </summary>
        private void PerformAttack()
        {
            lastAttackTime = Time.time;

            // Calculate damage based on selected model
            float damage = CalculateDamage();

            // Apply damage to player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                Debug.Log($"{gameObject.name} dealt {damage:F1} damage to player using {damageModel} model");
            }
        }

        /// <summary>
        /// Calculate damage based on the selected damage model
        /// </summary>
        private float CalculateDamage()
        {
            float damage = 0f;

            switch (damageModel)
            {
                case DamageModel.Symmetric:
                    damage = CalculateSymmetricDamage();
                    break;

                case DamageModel.Asymmetric:
                    damage = CalculateAsymmetricDamage();
                    break;

                case DamageModel.Dynamic:
                    damage = CalculateDynamicDamage();
                    break;
            }

            // Apply critical hit
            if (enableCriticalHits && Random.value < criticalHitChance)
            {
                damage *= criticalHitMultiplier;
                Debug.Log($"{gameObject.name} scored a CRITICAL HIT!");
            }

            return damage;
        }

        /// <summary>
        /// Symmetric damage: Same rules as player - fair and predictable
        /// Uses uniform probability distribution within range
        /// </summary>
        private float CalculateSymmetricDamage()
        {
            float baseDamage = (minDamage + maxDamage) / 2f;
            float randomFactor = Random.Range(symmetricVarianceMin, 1f);
            return baseDamage * randomFactor;
        }

        /// <summary>
        /// Asymmetric damage: Weighted probability curves for unpredictability
        /// Creates distinct damage tiers with different probabilities
        /// </summary>
        private float CalculateAsymmetricDamage()
        {
            float roll = Random.value;
            float baseDamage = (minDamage + maxDamage) / 2f;

            if (roll < highDamageProbability)
            {
                // High damage
                return baseDamage * highDamageMultiplier;
            }
            else if (roll < highDamageProbability + mediumDamageProbability)
            {
                // Medium damage
                return baseDamage * mediumDamageMultiplier;
            }
            else
            {
                // Low damage
                return baseDamage * lowDamageMultiplier;
            }
        }

        /// <summary>
        /// Dynamic damage: Adapts based on NPC state, health, distance, and context
        /// Combines base probability with multiple scaling factors
        /// </summary>
        private float CalculateDynamicDamage()
        {
            // Start with base probability
            float baseProbability = 0.5f;

            // Health-based scaling
            if (useHealthScaling)
            {
                float healthPercentage = currentHealth / maxHealth;
                if (healthPercentage < lowHealthThreshold)
                {
                    baseProbability += lowHealthDamageBonus;
                }
            }

            // Aggression scaling (for non-weak NPCs)
            if (useAggressionScaling && !npcController.isWeakNPC)
            {
                baseProbability += aggressionDamageBonus;
            }

            // Distance scaling
            if (useDistanceScaling && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= closeCombatDistance)
                {
                    baseProbability += closeCombatDamageBonus;
                }
            }

            // Group size scaling (reduce individual damage in groups)
            if (useGroupScaling)
            {
                int nearbyNPCs = CountNearbyNPCs(10f);
                if (nearbyNPCs > 1)
                {
                    float reduction = (nearbyNPCs - 1) * groupDamageReduction;
                    baseProbability = Mathf.Max(0.2f, baseProbability - reduction);
                }
            }

            // Clamp probability
            baseProbability = Mathf.Clamp01(baseProbability);

            // Lerp between min and max damage based on probability
            return Mathf.Lerp(minDamage, maxDamage, baseProbability);
        }

        /// <summary>
        /// Count nearby NPCs within specified radius
        /// </summary>
        private int CountNearbyNPCs(float radius)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius);
            int count = 0;

            foreach (Collider col in nearbyColliders)
            {
                if (col.gameObject != gameObject && col.GetComponent<NPCController>() != null)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Set NPC health for dynamic damage calculations
        /// </summary>
        public void SetHealth(float current, float max)
        {
            currentHealth = current;
            maxHealth = max;
        }

        /// <summary>
        /// Get the calculated attack range
        /// </summary>
        public float GetAttackRange()
        {
            return attackRange;
        }

        /// <summary>
        /// Check if NPC can attack (cooldown check)
        /// </summary>
        public bool CanAttack()
        {
            return Time.time - lastAttackTime >= attackCooldown;
        }

        // Visualization
        void OnDrawGizmosSelected()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw close combat distance
            if (useDistanceScaling)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, closeCombatDistance);
            }

            // Draw group radius
            if (useGroupScaling)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 10f);
            }
        }
    }
}
