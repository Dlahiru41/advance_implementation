# Probability-Controlled Damage System

## Overview

The Probability-Controlled Damage System introduces intelligent, dynamic damage calculation for NPCs, making combat more unpredictable, engaging, and tunable. This system implements three damage models with support for different NPC archetypes and context-aware damage modifiers.

---

## Features

✅ **Three Damage Models**
- **Symmetric**: Fair damage using the same rules as the player
- **Asymmetric**: Weighted probability curves for unpredictability
- **Dynamic**: Context-aware damage based on NPC state, health, distance, and group size

✅ **NPC Archetypes**
- **Weak Grunt**: Low damage, low variance
- **Normal**: Medium damage, standard variance  
- **Elite**: High damage, bell curve distribution
- **Boss**: Very high damage, high burst probability

✅ **Dynamic Modifiers**
- Health-based scaling (enraged when low health)
- Aggression-based scaling (non-weak NPCs deal more)
- Distance-based scaling (bonus damage in close combat)
- Group-based scaling (reduced damage when in groups)

✅ **Critical Hit System**
- Configurable critical hit chance per archetype
- Critical hits apply damage multiplier
- Visual feedback for critical hits

---

## Core Components

### 1. NPCDamageController

Main component that handles all damage calculations and attack logic.

**Inspector Settings:**

#### Damage Model Configuration
- `damageModel`: Choose between Symmetric, Asymmetric, or Dynamic
- `archetype`: Select NPC type (WeakGrunt, Normal, Elite, Boss)

#### Base Damage Settings
- `minDamage`: Minimum damage the NPC can deal
- `maxDamage`: Maximum damage the NPC can deal

#### Symmetric Model Settings
- `symmetricVarianceMin`: Variance range (0.7 = 70%-100% of base damage)

#### Asymmetric Model Settings
- `highDamageProbability`: Chance to deal 150% damage (default: 20%)
- `mediumDamageProbability`: Chance to deal 100% damage (default: 50%)
- `lowDamageMultiplier`: Multiplier for low damage (default: 0.5x)
- `mediumDamageMultiplier`: Multiplier for medium damage (default: 1.0x)
- `highDamageMultiplier`: Multiplier for high damage (default: 1.5x)

#### Dynamic Model Settings
- `useHealthScaling`: Enable damage increase when health is low
- `lowHealthThreshold`: Health % below which bonus applies (default: 30%)
- `lowHealthDamageBonus`: Additional damage probability when low health
- `useAggressionScaling`: Enable bonus for aggressive (non-weak) NPCs
- `aggressionDamageBonus`: Additional probability for aggressive NPCs
- `useDistanceScaling`: Enable close combat bonus
- `closeCombatDistance`: Distance considered "close" (default: 3m)
- `closeCombatDamageBonus`: Damage bonus when close
- `useGroupScaling`: Reduce damage when NPCs are grouped
- `groupDamageReduction`: Damage reduction per additional NPC

#### Attack Settings
- `attackCooldown`: Minimum time between attacks (default: 2s)
- `attackRange`: Range at which NPC can attack (default: 2m)

#### Critical Hit Settings
- `enableCriticalHits`: Enable/disable critical hits
- `criticalHitChance`: Base critical hit chance (0-50%)
- `criticalHitMultiplier`: Critical hit damage multiplier (1.5-3x)

### 2. PlayerHealth

Manages player health, damage reception, regeneration, and death/respawn.

**Inspector Settings:**

#### Health Settings
- `maxHealth`: Maximum player health (default: 100)
- `currentHealth`: Current health (read-only in inspector)

#### Damage Feedback
- `enableDamageFeedback`: Show visual feedback when damaged
- `damageFeedbackDuration`: How long feedback lasts (default: 0.2s)
- `damageColor`: Color to flash when damaged

#### Regeneration
- `enableRegeneration`: Auto-heal over time
- `regenerationRate`: Health per second regenerated
- `regenerationDelay`: Delay after damage before regen starts

#### Death Settings
- `enableRespawn`: Auto-respawn on death
- `respawnDelay`: Time before respawning (default: 3s)
- `respawnPoint`: Location to respawn (null = start position)

---

## Damage Models Explained

### ⭐ 1. Symmetric Damage Design

**Philosophy**: Fair and balanced - NPC uses same rules as player.

**How it works:**
- Damage is randomly selected from `minDamage` to `maxDamage`
- Uses variance factor (e.g., 70%-100% of base damage)
- Predictable but fair combat

**Best for:**
- Balanced gameplay
- Competitive/skill-based experiences
- RPGs and adventure games

**Example:**
```csharp
// Symmetric damage calculation
float baseDamage = (minDamage + maxDamage) / 2f;  // e.g., 10
float randomFactor = Random.Range(0.7f, 1f);       // 70%-100%
float damage = baseDamage * randomFactor;          // 7-10 damage
```

**Unity Inspector Setup:**
1. Add `NPCDamageController` to NPC GameObject
2. Set `Damage Model` to `Symmetric`
3. Configure `Min Damage` and `Max Damage`
4. Adjust `Symmetric Variance Min` (default: 0.7)

---

### ⭐ 2. Asymmetric Damage Design

**Philosophy**: Unpredictable - NPC uses different probability curves than player.

**How it works:**
- Three damage tiers: High (150%), Medium (100%), Low (50%)
- Weighted probabilities for each tier
- Default: 20% high, 50% medium, 30% low
- Creates tension through unpredictable damage spikes

**Best for:**
- Boss fights
- Adding tension and excitement
- Creating distinct enemy archetypes
- Tuning difficulty without changing mechanics

**Example:**
```csharp
// Asymmetric damage calculation
float roll = Random.value;  // 0-1
float baseDamage = 10f;

if (roll < 0.2f)           // 20% chance
    damage = baseDamage * 1.5f;   // 15 damage (HIGH)
else if (roll < 0.7f)      // 50% chance  
    damage = baseDamage * 1.0f;   // 10 damage (MEDIUM)
else                       // 30% chance
    damage = baseDamage * 0.5f;   // 5 damage (LOW)
```

**Unity Inspector Setup:**
1. Add `NPCDamageController` to NPC GameObject
2. Set `Damage Model` to `Asymmetric`
3. Configure probability distribution:
   - `High Damage Probability`: 0.2 (20%)
   - `Medium Damage Probability`: 0.5 (50%)
   - Low damage is automatic (remaining 30%)
4. Adjust multipliers:
   - `High Damage Multiplier`: 1.5
   - `Medium Damage Multiplier`: 1.0
   - `Low Damage Multiplier`: 0.5

---

### ⭐ 3. Dynamic Damage Design (Advanced)

**Philosophy**: Context-aware - damage adapts to NPC state and situation.

**How it works:**
- Base probability starts at 0.5 (50%)
- Multiple factors modify the probability:
  - **Low Health**: +20% when below 30% health (enraged)
  - **Aggression**: +10% for non-weak NPCs
  - **Close Range**: +15% when within 3 meters
  - **Group Size**: -10% per additional nearby NPC
- Final probability determines damage on scale from min to max

**Best for:**
- Realistic combat behavior
- Adaptive difficulty
- Strategic gameplay (encouraging positioning)
- Boss fights with phases

**Example:**
```csharp
// Dynamic damage calculation
float probability = 0.5f;  // Base 50%

// NPC is low health (below 30%)
if (health < maxHealth * 0.3f)
    probability += 0.2f;  // Now 70%

// NPC is aggressive type
if (isAggressive)
    probability += 0.1f;  // Now 80%

// Player is close (within 3m)
if (distance < 3f)
    probability += 0.15f;  // Now 95%

// 2 other NPCs nearby
int nearbyNPCs = 2;
probability -= nearbyNPCs * 0.1f;  // Back to 75%

// Final damage based on probability
damage = Mathf.Lerp(minDamage, maxDamage, probability);
```

**Unity Inspector Setup:**
1. Add `NPCDamageController` to NPC GameObject
2. Set `Damage Model` to `Dynamic`
3. Enable desired modifiers:
   - ☑ `Use Health Scaling`
     - `Low Health Threshold`: 0.3 (30%)
     - `Low Health Damage Bonus`: 0.2 (20%)
   - ☑ `Use Aggression Scaling`
     - `Aggression Damage Bonus`: 0.1 (10%)
   - ☑ `Use Distance Scaling`
     - `Close Combat Distance`: 3 (meters)
     - `Close Combat Damage Bonus`: 0.15 (15%)
   - ☑ `Use Group Scaling`
     - `Group Damage Reduction`: 0.1 (10% per NPC)

---

## NPC Archetypes

Pre-configured damage profiles for different enemy types:

### Weak Grunt
- **Min Damage**: 3
- **Max Damage**: 8
- **Critical Chance**: 5%
- **Attack Cooldown**: 2.5s
- **Use Case**: Low-tier enemies, cannon fodder

### Normal
- **Min Damage**: 5
- **Max Damage**: 15
- **Critical Chance**: 10%
- **Attack Cooldown**: 2s
- **Use Case**: Standard enemies, balanced combat

### Elite
- **Min Damage**: 10
- **Max Damage**: 25
- **Critical Chance**: 15%
- **Attack Cooldown**: 1.5s
- **Asymmetric**: 30% high, 50% medium, 20% low
- **Use Case**: Tough enemies, mini-bosses

### Boss
- **Min Damage**: 20
- **Max Damage**: 50
- **Critical Chance**: 20%
- **Attack Cooldown**: 1.8s
- **Asymmetric**: 40% high, 40% medium, 20% low
- **Use Case**: Boss fights, major encounters

---

## Setup Instructions

### Quick Setup (5 minutes)

#### 1. Add Components to NPC

```
1. Select your NPC GameObject in the hierarchy
2. Add Component → NPCAISystem → NPCDamageController
3. Choose your damage model and archetype
4. Adjust settings as needed
```

#### 2. Add PlayerHealth to Player

```
1. Select Player GameObject (tagged "Player")
2. Add Component → NPCAISystem → PlayerHealth
3. Configure max health and respawn settings
4. Optionally enable regeneration
```

#### 3. Test It

```
1. Enter Play Mode
2. Have NPC chase player (they will auto-attack in range)
3. Watch health bar in top-left corner
4. Observe damage numbers in Console
```

### Advanced Setup

#### Creating a Balanced Elite Enemy

```
1. Add NPCDamageController to Elite NPC
2. Set Archetype to "Elite"
3. Use Asymmetric damage model
4. Configure:
   - High Damage Probability: 0.3 (30%)
   - Medium Damage Probability: 0.5 (50%)
   - Enable Critical Hits: true
   - Critical Hit Chance: 0.15 (15%)
```

#### Creating an Adaptive Boss

```
1. Add NPCDamageController to Boss NPC
2. Set Archetype to "Boss"
3. Use Dynamic damage model
4. Enable all modifiers:
   - Health Scaling: ON (enrage at low health)
   - Aggression Scaling: ON (always aggressive)
   - Distance Scaling: ON (punish close combat)
   - Group Scaling: OFF (boss fights alone)
5. Implement health tracking:
   - Call SetHealth() to update boss health state
```

#### Tuning Difficulty

**To Make Easier:**
- Lower `Max Damage`
- Increase `Attack Cooldown`
- Reduce `Critical Hit Chance`
- For Asymmetric: Increase `Low Damage Probability`
- For Dynamic: Reduce bonus percentages

**To Make Harder:**
- Increase `Max Damage`
- Decrease `Attack Cooldown`
- Increase `Critical Hit Chance`
- For Asymmetric: Increase `High Damage Probability`
- For Dynamic: Increase bonus percentages

---

## Code Examples

### Example 1: Setting Up Symmetric Damage in Code

```csharp
using NPCAISystem;

public class NPCSetup : MonoBehaviour
{
    void Start()
    {
        NPCDamageController damageController = GetComponent<NPCDamageController>();
        
        // Configure symmetric damage
        damageController.damageModel = NPCDamageController.DamageModel.Symmetric;
        damageController.archetype = NPCDamageController.NPCArchetype.Normal;
        damageController.minDamage = 5f;
        damageController.maxDamage = 15f;
        damageController.symmetricVarianceMin = 0.7f;  // 70%-100% damage
    }
}
```

### Example 2: Creating Custom Asymmetric Distribution

```csharp
using NPCAISystem;

public class CustomBoss : MonoBehaviour
{
    void Start()
    {
        NPCDamageController damageController = GetComponent<NPCDamageController>();
        
        // Custom asymmetric damage for unpredictable boss
        damageController.damageModel = NPCDamageController.DamageModel.Asymmetric;
        damageController.minDamage = 25f;
        damageController.maxDamage = 60f;
        
        // 40% chance for devastating attacks
        damageController.highDamageProbability = 0.4f;
        damageController.highDamageMultiplier = 2.0f;  // 200% damage!
        
        // 30% chance for medium attacks
        damageController.mediumDamageProbability = 0.3f;
        damageController.mediumDamageMultiplier = 1.0f;
        
        // 30% chance for light attacks (remaining)
        damageController.lowDamageMultiplier = 0.3f;  // Only 30% damage
    }
}
```

### Example 3: Dynamic Damage with Health Tracking

```csharp
using NPCAISystem;

public class AdvancedBoss : MonoBehaviour
{
    private float bossHealth = 1000f;
    private float bossMaxHealth = 1000f;
    private NPCDamageController damageController;
    
    void Start()
    {
        damageController = GetComponent<NPCDamageController>();
        
        // Configure dynamic damage
        damageController.damageModel = NPCDamageController.DamageModel.Dynamic;
        damageController.useHealthScaling = true;
        damageController.lowHealthThreshold = 0.3f;  // Enrage below 30%
        damageController.lowHealthDamageBonus = 0.3f;  // +30% damage when enraged!
    }
    
    void Update()
    {
        // Update health state for dynamic damage calculation
        damageController.SetHealth(bossHealth, bossMaxHealth);
    }
    
    public void TakeDamage(float damage)
    {
        bossHealth -= damage;
        // Boss will automatically deal more damage when below 30% health!
    }
}
```

### Example 4: Healing Player

```csharp
using NPCAISystem;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f;
    
    void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Destroy(gameObject);  // Remove pickup
        }
    }
}
```

---

## Visual Feedback

### In-Game Health Bar
The PlayerHealth component displays a simple health bar in the top-left corner:
- **Green**: Health above 50%
- **Yellow**: Health between 25%-50%
- **Red**: Health below 25%

### Damage Flash
When `enableDamageFeedback` is true, the player flashes red when taking damage.

### Death Screen
Shows "YOU DIED" message in center of screen when player health reaches zero.

### Console Logging
Both components log damage events to the Console:
```
NPC_Combat_1 dealt 12.3 damage to player using Asymmetric model
Player took 12.3 damage. Health: 87.7/100.0
NPC_Boss_1 scored a CRITICAL HIT!
NPC_Boss_1 dealt 45.6 damage to player using Dynamic model
```

---

## Gizmo Visualization

When an NPC with NPCDamageController is selected in the Scene view:

- **Red sphere**: Attack range (when NPC can deal damage)
- **Yellow sphere**: Close combat distance (bonus damage zone)
- **Blue sphere**: Group detection radius (affects group scaling)

---

## Performance Considerations

✅ **Efficient Calculations**
- Damage calculated only when attacking (not every frame)
- Group counting uses Physics.OverlapSphere (optimized)
- No Update() overhead when not in combat

✅ **Memory Friendly**
- No allocations during damage calculations
- Minimal state tracking
- Reuses existing NPC components

---

## Troubleshooting

### NPC Not Attacking
- ✔ Check NPC has `NPCController` component
- ✔ Check NPC has `NPCDamageController` component
- ✔ Verify `Attack Range` is sufficient (default: 2m)
- ✔ Ensure NPC is in Chase state (check `currentState` in Inspector)
- ✔ Check that Player is tagged "Player"

### Damage Not Showing
- ✔ Verify Player has `PlayerHealth` component
- ✔ Check Console for damage messages
- ✔ Ensure Player's health isn't already 0

### Health Not Regenerating
- ✔ Enable `Enable Regeneration` in PlayerHealth
- ✔ Check `Regeneration Delay` (default: 5s after last damage)
- ✔ Verify `Regeneration Rate` is greater than 0

### Player Not Respawning
- ✔ Enable `Enable Respawn` in PlayerHealth
- ✔ Check `Respawn Delay` setting
- ✔ Verify respawn point is valid (if set)

---

## Integration with Existing Systems

### Works With
✅ NPCController FSM states
✅ NPCSensor detection system
✅ NPCGroup formations
✅ NavMesh navigation
✅ SoundEventManager

### Compatible With
✅ Custom player controllers
✅ Other health systems (via scripting)
✅ UI systems (access via GetHealthPercentage())
✅ Save/load systems (expose currentHealth)

---

## Best Practices

### 1. Balance Testing
- Start with Symmetric model for baseline balance
- Switch to Asymmetric for bosses and special enemies
- Use Dynamic for adaptive difficulty

### 2. NPC Variety
- Mix archetypes for diverse combat
- Use Weak Grunts in groups
- Elite and Boss as solo encounters

### 3. Player Feedback
- Keep critical hit chance reasonable (10-20%)
- Provide audio/visual cues for damage types
- Test average time-to-death (should be engaging, not frustrating)

### 4. Difficulty Curves
- Early game: Weak Grunts with Symmetric damage
- Mid game: Normal enemies with Asymmetric damage
- Late game: Elites with Dynamic damage
- Boss fights: Boss archetype with full Dynamic modifiers

---

## Future Enhancements

Possible extensions to this system:

- **Damage Types**: Physical, magical, elemental
- **Armor System**: Damage reduction and resistances
- **Status Effects**: Poison, burn, slow
- **Combo System**: Damage multiplier for consecutive hits
- **Difficulty Scaling**: Auto-adjust based on player performance
- **Animation Triggers**: Sync attacks with animation events
- **Sound Effects**: Different sounds for damage types
- **Particle Effects**: Visual feedback for hits and criticals

---

## Summary

The Probability-Controlled Damage System provides:

✅ **Three flexible damage models** (Symmetric, Asymmetric, Dynamic)
✅ **Four NPC archetypes** (Weak, Normal, Elite, Boss)
✅ **Dynamic modifiers** (health, aggression, distance, groups)
✅ **Critical hit system** for exciting combat moments
✅ **Player health management** with regeneration and respawn
✅ **Easy setup** - just add components and configure
✅ **Highly tunable** - adjust difficulty without code changes
✅ **Performance optimized** - minimal overhead

This system makes your NPCs feel intelligent, unpredictable, and fun to fight!
