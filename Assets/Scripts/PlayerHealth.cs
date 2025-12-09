using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Manages player health and damage reception.
    /// Works with NPCDamageController to handle incoming damage.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health of the player")]
        public float maxHealth = 100f;

        [Tooltip("Current health of the player")]
        [SerializeField]
        private float currentHealth = 100f;

        [Header("Damage Feedback")]
        [Tooltip("Enable visual damage feedback")]
        public bool enableDamageFeedback = true;

        [Tooltip("Duration of damage feedback effect (seconds)")]
        public float damageFeedbackDuration = 0.2f;

        [Tooltip("Color to flash when taking damage")]
        public Color damageColor = new Color(1f, 0f, 0f, 0.3f);

        [Header("Regeneration")]
        [Tooltip("Enable health regeneration")]
        public bool enableRegeneration = false;

        [Tooltip("Health regenerated per second")]
        public float regenerationRate = 2f;

        [Tooltip("Delay before regeneration starts after taking damage")]
        public float regenerationDelay = 5f;

        [Header("Death Settings")]
        [Tooltip("Enable respawn on death")]
        public bool enableRespawn = true;

        [Tooltip("Respawn delay (seconds)")]
        public float respawnDelay = 3f;

        [Tooltip("Respawn location (leave null to respawn at start position)")]
        public Transform respawnPoint;

        // Private state
        private float lastDamageTime = -999f;
        private Vector3 startPosition;
        private bool isDead = false;
        private Renderer playerRenderer;
        private Color originalColor;
        private bool isShowingDamageFeedback = false;
        private float damageFeedbackStartTime;

        void Start()
        {
            currentHealth = maxHealth;
            startPosition = transform.position;

            // Get renderer for damage feedback
            playerRenderer = GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                originalColor = playerRenderer.material.color;
            }
        }

        void Update()
        {
            // Handle health regeneration
            if (enableRegeneration && !isDead && currentHealth < maxHealth)
            {
                if (Time.time - lastDamageTime >= regenerationDelay)
                {
                    currentHealth = Mathf.Min(currentHealth + regenerationRate * Time.deltaTime, maxHealth);
                }
            }

            // Handle damage feedback
            if (isShowingDamageFeedback)
            {
                if (Time.time - damageFeedbackStartTime >= damageFeedbackDuration)
                {
                    // Reset color
                    if (playerRenderer != null)
                    {
                        playerRenderer.material.color = originalColor;
                    }
                    isShowingDamageFeedback = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to the player
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="sourcePosition">Position of damage source (for directional feedback)</param>
        public void TakeDamage(float damage, Vector3 sourcePosition)
        {
            if (isDead)
                return;

            // Apply damage
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            lastDamageTime = Time.time;

            // Log damage
            Debug.Log($"Player took {damage:F1} damage. Health: {currentHealth:F1}/{maxHealth:F1}");

            // Visual feedback
            if (enableDamageFeedback && playerRenderer != null)
            {
                playerRenderer.material.color = damageColor;
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
        /// Heal the player
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            Debug.Log($"Player healed {amount:F1}. Health: {currentHealth:F1}/{maxHealth:F1}");
        }

        /// <summary>
        /// Handle player death
        /// </summary>
        private void Die()
        {
            if (isDead)
                return;

            isDead = true;
            Debug.Log("Player died!");

            // Handle respawn
            if (enableRespawn)
            {
                Invoke(nameof(Respawn), respawnDelay);
            }
        }

        /// <summary>
        /// Respawn the player
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
            if (playerRenderer != null)
            {
                playerRenderer.material.color = originalColor;
            }

            Debug.Log("Player respawned!");
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
        /// Check if player is dead
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

        // UI visualization helper
        void OnGUI()
        {
            // Draw simple health bar in top-left corner
            float barWidth = 200f;
            float barHeight = 20f;
            float margin = 10f;

            // Background
            GUI.Box(new Rect(margin, margin, barWidth, barHeight), "");

            // Health bar
            float healthWidth = barWidth * (currentHealth / maxHealth);
            Color healthColor = currentHealth > maxHealth * 0.5f ? Color.green : 
                               currentHealth > maxHealth * 0.25f ? Color.yellow : Color.red;
            
            GUI.color = healthColor;
            GUI.Box(new Rect(margin, margin, healthWidth, barHeight), "");
            GUI.color = Color.white;

            // Text
            string healthText = $"Health: {currentHealth:F0}/{maxHealth:F0}";
            GUI.Label(new Rect(margin + 5, margin + 2, barWidth, barHeight), healthText);

            // Death message
            if (isDead)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 30;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.red;
                
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100), 
                         "YOU DIED", style);
            }
        }
    }
}
