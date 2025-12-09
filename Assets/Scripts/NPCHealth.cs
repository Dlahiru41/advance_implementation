using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Manages NPC health and damage reception.
    /// Works with Projectile collision to handle incoming damage from player.
    /// </summary>
    [RequireComponent(typeof(NPCController))]
    public class NPCHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health of the NPC")]
        public float maxHealth = 50f;

        [Tooltip("Current health of the NPC")]
        [SerializeField]
        private float currentHealth = 50f;

        [Header("Damage Feedback")]
        [Tooltip("Enable visual damage feedback")]
        public bool enableDamageFeedback = true;

        [Tooltip("Duration of damage feedback effect (seconds)")]
        public float damageFeedbackDuration = 0.2f;

        [Tooltip("Color to flash when taking damage")]
        public Color damageColor = new Color(1f, 0f, 0f, 0.3f);

        [Header("Death Settings")]
        [Tooltip("Enable respawn on death")]
        public bool enableRespawn = false;

        [Tooltip("Respawn delay (seconds)")]
        public float respawnDelay = 5f;

        [Tooltip("Respawn location (leave null to respawn at start position)")]
        public Transform respawnPoint;

        [Tooltip("Destroy NPC on death instead of respawning")]
        public bool destroyOnDeath = true;

        // Private state
        private Vector3 startPosition;
        private bool isDead = false;
        private Renderer npcRenderer;
        private Material npcMaterial;  // Cached material reference
        private Color originalColor;
        private bool isShowingDamageFeedback = false;
        private float damageFeedbackStartTime;
        private NPCController npcController;

        void Start()
        {
            currentHealth = maxHealth;
            startPosition = transform.position;
            npcController = GetComponent<NPCController>();

            // Get renderer for damage feedback and cache material
            npcRenderer = GetComponent<Renderer>();
            if (npcRenderer != null)
            {
                // Create a unique material instance for this NPC
                npcMaterial = npcRenderer.material;
                originalColor = npcMaterial.color;
            }
        }

        void Update()
        {
            // Handle damage feedback
            if (isShowingDamageFeedback)
            {
                if (Time.time - damageFeedbackStartTime >= damageFeedbackDuration)
                {
                    // Reset color
                    if (npcMaterial != null)
                    {
                        npcMaterial.color = originalColor;
                    }
                    isShowingDamageFeedback = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to the NPC
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="sourcePosition">Position of damage source (for directional feedback)</param>
        public void TakeDamage(float damage, Vector3 sourcePosition)
        {
            if (isDead)
                return;

            // Apply damage
            currentHealth = Mathf.Max(0f, currentHealth - damage);

            // Log damage
            Debug.Log($"{gameObject.name} took {damage:F1} damage. Health: {currentHealth:F1}/{maxHealth:F1}");

            // Visual feedback
            if (enableDamageFeedback && npcMaterial != null)
            {
                npcMaterial.color = damageColor;
                isShowingDamageFeedback = true;
                damageFeedbackStartTime = Time.time;
            }

            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the NPC
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"{gameObject.name} healed {amount:F1}. Health: {currentHealth:F1}/{maxHealth:F1}");
        }

        /// <summary>
        /// Handle NPC death
        /// </summary>
        private void Die()
        {
            if (isDead)
                return;

            isDead = true;
            Debug.Log($"{gameObject.name} died!");

            // Disable NPC controller to stop AI behavior
            if (npcController != null)
            {
                npcController.enabled = false;
            }

            // Handle death based on settings
            if (destroyOnDeath)
            {
                // Destroy the NPC GameObject
                Destroy(gameObject, 0.5f); // Small delay for any death effects
            }
            else if (enableRespawn)
            {
                // Respawn after delay
                Invoke(nameof(Respawn), respawnDelay);
            }
        }

        /// <summary>
        /// Respawn the NPC
        /// </summary>
        private void Respawn()
        {
            // Reset health
            currentHealth = maxHealth;
            isDead = false;

            // Reset position
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                transform.rotation = respawnPoint.rotation;
            }
            else
            {
                transform.position = startPosition;
            }

            // Reset visual state
            if (npcMaterial != null)
            {
                npcMaterial.color = originalColor;
            }

            // Re-enable NPC controller
            if (npcController != null)
            {
                npcController.enabled = true;
            }

            Debug.Log($"{gameObject.name} respawned!");
        }

        /// <summary>
        /// Get current health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        /// <summary>
        /// Get current health
        /// </summary>
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// Check if NPC is dead
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }

        /// <summary>
        /// Set health directly (for debugging/testing)
        /// </summary>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            if (currentHealth <= 0f && !isDead)
            {
                Die();
            }
        }
    }
}
