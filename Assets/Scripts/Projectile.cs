using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Projectile script that handles movement, collision, and damage application
    /// with probability-based damage control.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip("Speed of the projectile")]
        public float speed = 20f;

        [Tooltip("Lifetime of the projectile before auto-destruction")]
        public float lifetime = 5f;

        [Tooltip("Damage model for this projectile")]
        public NPCDamageController.DamageModel damageModel = NPCDamageController.DamageModel.Symmetric;

        [Header("Damage Settings")]
        [Tooltip("Minimum damage this projectile can deal")]
        public float minDamage = 5f;

        [Tooltip("Maximum damage this projectile can deal")]
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

        [Header("Visual Settings")]
        [Tooltip("Enable trail renderer")]
        public bool enableTrail = true;

        [Tooltip("Trail color")]
        public Color trailColor = Color.yellow;

        [Tooltip("Trail width")]
        public float trailWidth = 0.1f;

        [Tooltip("Trail time")]
        public float trailTime = 0.5f;

        // Internal state
        private Rigidbody rb;
        private GameObject owner;
        private bool isPlayerProjectile;
        private TrailRenderer trail;

        /// <summary>
        /// Initialize the projectile with direction and owner
        /// </summary>
        public void Initialize(Vector3 direction, GameObject shooter, bool fromPlayer = false)
        {
            owner = shooter;
            isPlayerProjectile = fromPlayer;

            // Validate direction
            if (direction.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("Projectile: Invalid direction vector (zero or near-zero). Using forward fallback.");
                direction = shooter != null ? shooter.transform.forward : Vector3.forward;
            }

            // Setup rigidbody
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.velocity = direction.normalized * speed;
            }

            // Setup visual trail
            if (enableTrail)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                trail.time = trailTime;
                trail.startWidth = trailWidth;
                trail.endWidth = 0.01f;
                
                // Use shared material to avoid allocations
                Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
                trail.sharedMaterial = trailMaterial;
                trail.startColor = trailColor;
                trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }

            // Destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        void OnCollisionEnter(Collision collision)
        {
            // Ignore collision with owner
            if (collision.gameObject == owner)
                return;

            // Check if hit player
            if (isPlayerProjectile == false && collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float damage = CalculateDamage();
                    playerHealth.TakeDamage(damage, transform.position);
                    Debug.Log($"Projectile hit player for {damage:F1} damage using {damageModel} model");
                }
                Destroy(gameObject);
                return;
            }

            // Check if hit NPC
            if (isPlayerProjectile && collision.gameObject.GetComponent<NPCController>() != null)
            {
                NPCHealth npcHealth = collision.gameObject.GetComponent<NPCHealth>();
                if (npcHealth != null)
                {
                    float damage = CalculateDamage();
                    npcHealth.TakeDamage(damage, transform.position);
                    Debug.Log($"Player projectile hit NPC for {damage:F1} damage using {damageModel} model");
                }
                else
                {
                    Debug.LogWarning($"Player projectile hit NPC {collision.gameObject.name} but it has no NPCHealth component!");
                }
                
                Destroy(gameObject);
                return;
            }

            // Hit something else (wall, terrain, etc)
            Destroy(gameObject);
        }

        /// <summary>
        /// Calculate damage based on the selected damage model
        /// </summary>
        private float CalculateDamage()
        {
            switch (damageModel)
            {
                case NPCDamageController.DamageModel.Symmetric:
                    return CalculateSymmetricDamage();
                
                case NPCDamageController.DamageModel.Asymmetric:
                    return CalculateAsymmetricDamage();
                
                case NPCDamageController.DamageModel.Dynamic:
                    // For projectiles, dynamic is similar to symmetric
                    return CalculateSymmetricDamage();
                
                default:
                    return (minDamage + maxDamage) / 2f;
            }
        }

        /// <summary>
        /// Symmetric damage: Same rules as player - fair and predictable
        /// </summary>
        private float CalculateSymmetricDamage()
        {
            float baseDamage = (minDamage + maxDamage) / 2f;
            float randomFactor = Random.Range(symmetricVarianceMin, 1f);
            return baseDamage * randomFactor;
        }

        /// <summary>
        /// Asymmetric damage: Weighted probability curves for unpredictability
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
    }
}
