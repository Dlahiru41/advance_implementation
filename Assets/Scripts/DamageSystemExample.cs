using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Example script showing how to set up and customize the probability-controlled damage system.
    /// Attach this to an NPC or use the code as a reference.
    /// </summary>
    public class DamageSystemExample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [Tooltip("Select which example setup to use")]
        public ExampleType exampleType = ExampleType.SymmetricBalanced;

        public enum ExampleType
        {
            SymmetricBalanced,      // Fair combat for normal enemies
            AsymmetricUnpredictable, // Exciting boss with damage spikes
            DynamicAdaptive,        // Intelligent enemy that adapts
            CustomConfiguration     // Manual configuration in inspector
        }

    void Start()
    {
        // Get or add required components
        NPCDamageController damageController = GetComponent<NPCDamageController>();
        if (damageController == null)
        {
            damageController = gameObject.AddComponent<NPCDamageController>();
        }

        // Apply example configuration
        ConfigureExample(damageController);
    }

    void ConfigureExample(NPCDamageController damageController)
    {
        switch (exampleType)
        {
            case ExampleType.SymmetricBalanced:
                ConfigureSymmetricExample(damageController);
                break;

            case ExampleType.AsymmetricUnpredictable:
                ConfigureAsymmetricExample(damageController);
                break;

            case ExampleType.DynamicAdaptive:
                ConfigureDynamicExample(damageController);
                break;

            case ExampleType.CustomConfiguration:
                // Don't change anything - let user configure in inspector
                Debug.Log("Using custom configuration from Inspector");
                break;
        }
    }

    /// <summary>
    /// Example 1: Symmetric Damage - Fair and Balanced
    /// Perfect for standard enemies in RPGs and adventure games
    /// </summary>
    void ConfigureSymmetricExample(NPCDamageController damageController)
    {
        Debug.Log("Configuring Symmetric Damage Example");

        // Use symmetric model - same rules as player
        damageController.damageModel = NPCDamageController.DamageModel.Symmetric;
        damageController.archetype = NPCDamageController.NPCArchetype.Normal;

        // Base damage settings
        damageController.minDamage = 5f;
        damageController.maxDamage = 15f;

        // Symmetric variance (70%-100% of average damage)
        damageController.symmetricVarianceMin = 0.7f;

        // Standard critical hits
        damageController.enableCriticalHits = true;
        damageController.criticalHitChance = 0.1f;  // 10% crit chance
        damageController.criticalHitMultiplier = 2f; // 2x damage on crit

        // Standard attack rate
        damageController.attackCooldown = 2f;
        damageController.attackRange = 2f;

        Debug.Log("Symmetric setup complete: Min=5, Max=15, Variance=70%-100%");
    }

    /// <summary>
    /// Example 2: Asymmetric Damage - Unpredictable Boss
    /// Creates exciting combat with damage spikes
    /// </summary>
    void ConfigureAsymmetricExample(NPCDamageController damageController)
    {
        Debug.Log("Configuring Asymmetric Damage Example");

        // Use asymmetric model - weighted probabilities
        damageController.damageModel = NPCDamageController.DamageModel.Asymmetric;
        damageController.archetype = NPCDamageController.NPCArchetype.Boss;

        // Higher base damage for boss
        damageController.minDamage = 20f;
        damageController.maxDamage = 50f;

        // Asymmetric probability distribution
        // 40% chance for devastating attacks (150% damage)
        damageController.highDamageProbability = 0.4f;
        damageController.highDamageMultiplier = 1.5f;

        // 40% chance for normal attacks (100% damage)
        damageController.mediumDamageProbability = 0.4f;
        damageController.mediumDamageMultiplier = 1.0f;

        // 20% chance for weak attacks (50% damage) - remaining probability
        damageController.lowDamageMultiplier = 0.5f;

        // Higher critical chance for boss
        damageController.enableCriticalHits = true;
        damageController.criticalHitChance = 0.2f;  // 20% crit chance
        damageController.criticalHitMultiplier = 2.5f; // 2.5x damage on crit

        // Faster attacks
        damageController.attackCooldown = 1.5f;
        damageController.attackRange = 3f; // Longer reach

        Debug.Log("Asymmetric setup complete: 40% high, 40% medium, 20% low damage");
    }

    /// <summary>
    /// Example 3: Dynamic Damage - Intelligent Adaptive Enemy
    /// Damage changes based on context and state
    /// </summary>
    void ConfigureDynamicExample(NPCDamageController damageController)
    {
        Debug.Log("Configuring Dynamic Damage Example");

        // Use dynamic model - adapts to situation
        damageController.damageModel = NPCDamageController.DamageModel.Dynamic;
        damageController.archetype = NPCDamageController.NPCArchetype.Elite;

        // Elite enemy damage
        damageController.minDamage = 10f;
        damageController.maxDamage = 25f;

        // Enable all dynamic modifiers
        
        // 1. Health-based scaling (enrage when low health)
        damageController.useHealthScaling = true;
        damageController.lowHealthThreshold = 0.3f;  // Below 30% health
        damageController.lowHealthDamageBonus = 0.2f; // +20% damage probability

        // 2. Aggression scaling (elite is aggressive)
        damageController.useAggressionScaling = true;
        damageController.aggressionDamageBonus = 0.15f; // +15% damage probability

        // 3. Distance scaling (bonus in close combat)
        damageController.useDistanceScaling = true;
        damageController.closeCombatDistance = 3f;
        damageController.closeCombatDamageBonus = 0.15f; // +15% damage when close

        // 4. Group scaling (reduce overwhelm)
        damageController.useGroupScaling = true;
        damageController.groupDamageReduction = 0.1f; // -10% per additional NPC

        // Elite critical hits
        damageController.enableCriticalHits = true;
        damageController.criticalHitChance = 0.15f;  // 15% crit chance
        damageController.criticalHitMultiplier = 2.2f;

        // Fast attacks
        damageController.attackCooldown = 1.5f;
        damageController.attackRange = 2.5f;

        Debug.Log("Dynamic setup complete: All modifiers enabled");
    }

    // Helper method to demonstrate health tracking for dynamic damage
    void UpdateHealthForDynamicDamage()
    {
        // If this NPC has a health system, update the damage controller
        // This is needed for dynamic damage health-based scaling
        NPCDamageController damageController = GetComponent<NPCDamageController>();
        if (damageController != null && damageController.damageModel == NPCDamageController.DamageModel.Dynamic)
        {
            // Example: If you have an NPC health component
            // float currentHealth = npcHealthComponent.currentHealth;
            // float maxHealth = npcHealthComponent.maxHealth;
            // damageController.SetHealth(currentHealth, maxHealth);
            
            // For demonstration purposes (if no health system exists):
            float exampleCurrentHealth = 30f;  // 30% health (low)
            float exampleMaxHealth = 100f;
            damageController.SetHealth(exampleCurrentHealth, exampleMaxHealth);
        }
    }
}
