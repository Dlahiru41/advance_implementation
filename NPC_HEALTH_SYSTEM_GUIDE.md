# NPC Health System Guide

## Overview

The NPC Health System adds a complete health and damage management system for NPCs, allowing player projectiles to damage and eliminate enemy NPCs. This system integrates seamlessly with the existing NPC AI, firing mechanics, and spawner systems.

## Features

### ✅ NPC Health Management
- **Health Points**: Each NPC has configurable max health and current health
- **Damage Reception**: NPCs take damage from player projectiles
- **Death Handling**: NPCs can be destroyed or respawned on death
- **Visual Feedback**: Damage flash effect when hit
- **Automatic Integration**: Works with NPCSpawner for easy setup

### ✅ Health Display System
- **NPC ID Display**: Shows NPC type and number above each NPC
- **Health Bar**: Real-time HP bar with current/max values
- **Color-Coded**: Green (>70%), Yellow (30-70%), Red (<30%)
- **World-Space Canvas**: Follows NPC and always faces camera
- **Automatic Updates**: HP updates in real-time as damage is taken

### ✅ Combat Integration
- **Player Projectiles**: Automatically damage NPCs on collision
- **Probability-Based Damage**: Uses same damage models as other systems
- **NPC Types**: Different health values for weak vs combat NPCs
- **Death Behavior**: NPCs disabled and destroyed on death

---

## Components

### 1. NPCHealth.cs

Main component that manages NPC health and damage.

**Key Settings:**
- `maxHealth`: Maximum health points (default: 50)
- `enableDamageFeedback`: Visual flash on damage (default: true)
- `damageFeedbackDuration`: Flash duration in seconds (default: 0.2)
- `damageColor`: Flash color (default: red tint)
- `destroyOnDeath`: Remove NPC on death (default: true)
- `enableRespawn`: Respawn after delay (default: false)
- `respawnDelay`: Time before respawn (default: 5s)

**Public Methods:**
```csharp
// Apply damage to NPC
void TakeDamage(float damage, Vector3 sourcePosition)

// Heal the NPC
void Heal(float amount)

// Get current health as percentage (0-1)
float GetHealthPercentage()

// Get current health value
float GetCurrentHealth()

// Check if NPC is dead
bool IsDead()

// Set health directly (for testing)
void SetHealth(float health)
```

**How It Works:**
1. Component starts with full health
2. When `TakeDamage()` is called (from projectile collision), health decreases
3. Visual feedback flash shows damage was taken
4. When health reaches 0, NPC dies
5. On death, NPCController is disabled
6. NPC is destroyed after small delay (or respawned if configured)

### 2. NPCHealthDisplay.cs

Visual display component that shows NPC ID and health bar above each NPC.

**Key Settings:**
- `enableDisplay`: Toggle display on/off (default: true)
- `heightOffset`: Height above NPC (default: 2.0)
- `canvasScale`: Size of the canvas (default: 0.01)
- `healthBarWidth`: Health bar width in pixels (default: 100)
- `healthBarHeight`: Health bar height in pixels (default: 15)
- `healthyColor`: Color when HP > 70% (default: green)
- `warnColor`: Color when HP 30-70% (default: yellow)
- `criticalColor`: Color when HP < 30% (default: red)
- `backgroundColor`: Health bar background (default: dark gray)

**Display Format:**
```
[NPC Type] NPC #[Number]
HP: [Current]/[Max]
[=========>        ] (Color-coded bar)
```

**Examples:**
- "Combat NPC #3\nHP: 75/100" with green bar
- "Weak NPC #1\nHP: 15/50" with red bar

**How It Works:**
1. Creates world-space canvas as child of NPC
2. Canvas follows NPC position with height offset
3. Canvas always faces the camera
4. Health bar updates every frame based on current HP
5. Color changes based on health percentage
6. Display hides when NPC dies

### 3. NPCSpawner Integration

The NPCSpawner has been updated to automatically attach health components to spawned NPCs.

**New Settings:**
- `enableHealthSystem`: Enable health for spawned NPCs (default: true)
- `combatNPCMaxHealth`: Health for combat NPCs (default: 100)
- `weakNPCMaxHealth`: Health for weak NPCs (default: 50)
- `enableHealthDisplay`: Show health bars (default: true)

**Automatic Configuration:**
- Combat NPCs: 100 HP, red color, aggressive behavior
- Weak NPCs: 50 HP, orange color, flee behavior
- Health components auto-attached during spawn
- Display components auto-attached if enabled

### 4. Projectile Integration

The Projectile script has been updated to damage NPCs on collision.

**Changes Made:**
- Removed TODO comment about NPC health system
- Added NPCHealth component detection on collision
- Calls `TakeDamage()` with calculated damage
- Logs damage amount and model used
- Shows warning if NPC missing health component

**How It Works:**
1. Player fires projectile
2. Projectile hits NPC
3. Projectile detects NPCHealth component
4. Damage calculated based on damage model
5. Damage applied to NPC via `TakeDamage()`
6. Projectile destroyed on impact

---

## Quick Start

### Method 1: Using NPCSpawner (Recommended)

The easiest way to use the NPC health system is through the NPCSpawner, which automatically sets everything up.

**Steps:**
1. Ensure you have an NPCSpawner in your scene
2. Configure health settings in Inspector:
   - `enableHealthSystem`: ✓ (checked)
   - `combatNPCMaxHealth`: 100 (or your preferred value)
   - `weakNPCMaxHealth`: 50 (or your preferred value)
   - `enableHealthDisplay`: ✓ (checked)
3. Spawn NPCs (automatically or via context menu)
4. NPCs will automatically have health and display components

**That's it!** NPCs will now:
- Have health bars above them
- Take damage from player projectiles
- Show visual feedback when hit
- Die and be removed when health reaches 0

### Method 2: Manual Setup

For existing NPCs or manual configuration:

**Steps:**
1. Select your NPC GameObject in Hierarchy
2. Add Component → NPCHealth
3. Configure health settings:
   - Set `maxHealth` (50-100 recommended)
   - Enable `destroyOnDeath` for combat
4. Add Component → NPCHealthDisplay
5. Configure display settings:
   - Set `heightOffset` (2.0 default works well)
   - Adjust colors if desired
6. Test in Play mode

**Player Weapon Setup:**
- Ensure Player has PlayerWeapon component
- Ensure projectile prefab has Projectile script
- Player projectiles will automatically damage NPCs

---

## Configuration Guide

### NPC Type Presets

**Combat NPC (Aggressive):**
```csharp
NPCHealth:
  maxHealth: 100
  destroyOnDeath: true
  enableRespawn: false
NPCController:
  isWeakNPC: false
```

**Weak NPC (Flee Behavior):**
```csharp
NPCHealth:
  maxHealth: 50
  destroyOnDeath: true
  enableRespawn: false
NPCController:
  isWeakNPC: true
```

**Boss NPC (High Health):**
```csharp
NPCHealth:
  maxHealth: 300
  destroyOnDeath: true
  enableRespawn: false
  damageFeedbackDuration: 0.3
```

**Civilian NPC (Respawning):**
```csharp
NPCHealth:
  maxHealth: 30
  destroyOnDeath: false
  enableRespawn: true
  respawnDelay: 10
```

### Display Customization

**Minimal Display (ID Only):**
```csharp
NPCHealthDisplay:
  healthBarHeight: 10
  canvasScale: 0.008
```

**Prominent Display (Large Bar):**
```csharp
NPCHealthDisplay:
  healthBarWidth: 150
  healthBarHeight: 20
  canvasScale: 0.012
  heightOffset: 2.5
```

**Custom Colors:**
```csharp
NPCHealthDisplay:
  healthyColor: RGB(0, 255, 0)    // Bright green
  warnColor: RGB(255, 165, 0)      // Orange
  criticalColor: RGB(255, 0, 0)    // Red
  backgroundColor: RGB(0, 0, 0, 0.9) // Black
```

---

## Integration Examples

### Example 1: Arena Combat Setup

```csharp
// Configure NPCSpawner for arena combat
NPCSpawner spawner = GetComponent<NPCSpawner>();
spawner.npcCount = 10;
spawner.weakNPCRatio = 0.3f; // 30% weak, 70% combat
spawner.enableHealthSystem = true;
spawner.combatNPCMaxHealth = 100f;
spawner.weakNPCMaxHealth = 50f;
spawner.enableHealthDisplay = true;
spawner.useGroups = false; // Free-for-all
```

### Example 2: Boss Fight Setup

```csharp
// Manually create boss NPC with high health
GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
boss.name = "Boss_NPC";

// Add components
NPCController controller = boss.AddComponent<NPCController>();
controller.isWeakNPC = false;
controller.chaseSpeedMultiplier = 2.0f;

NPCHealth health = boss.AddComponent<NPCHealth>();
health.maxHealth = 500f;
health.damageFeedbackDuration = 0.4f;

NPCHealthDisplay display = boss.AddComponent<NPCHealthDisplay>();
display.heightOffset = 3f;
display.healthBarWidth = 200f;
display.canvasScale = 0.015f;
```

### Example 3: Healing System

```csharp
// Create healing pickup that restores NPC health
void OnTriggerEnter(Collider other)
{
    NPCHealth npcHealth = other.GetComponent<NPCHealth>();
    if (npcHealth != null && !npcHealth.IsDead())
    {
        npcHealth.Heal(25f);
        Destroy(gameObject); // Destroy healing pickup
    }
}
```

### Example 4: Custom Death Effects

```csharp
// Extend NPCHealth for custom death behavior
public class CustomNPCHealth : NPCHealth
{
    public GameObject deathEffect;
    
    private void Die()
    {
        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Drop loot
        SpawnLoot();
        
        // Award points to player
        GameManager.Instance.AddScore(100);
        
        // Call base death
        base.Die();
    }
    
    private void SpawnLoot()
    {
        // Custom loot spawning logic
    }
}
```

---

## Troubleshooting

### Health bar not showing

**Solutions:**
1. Check `enableDisplay` is true in NPCHealthDisplay
2. Verify NPCHealthDisplay component is attached
3. Check `heightOffset` isn't too high or low
4. Ensure Camera.main exists in scene
5. Check canvas isn't hidden by terrain/obstacles

### NPCs not taking damage

**Solutions:**
1. Verify NPCHealth component is attached to NPC
2. Check projectile has Projectile script
3. Ensure player projectiles have `isPlayerProjectile = true`
4. Check NPCs have colliders
5. Verify projectile speed isn't too high (causing collision miss)
6. Check layers and collision matrix

### NPCs not dying

**Solutions:**
1. Check if `maxHealth` is too high
2. Verify damage values in PlayerWeapon
3. Check damage model is configured correctly
4. Enable debug logs to see damage amounts
5. Test with `SetHealth(0)` to force death

### Display not facing camera

**Solutions:**
1. Ensure Camera.main exists and is tagged "MainCamera"
2. Check canvas is set to WorldSpace mode
3. Verify Update() is being called
4. Check canvas isn't disabled

### Performance issues with many NPCs

**Solutions:**
1. Reduce canvas update frequency
2. Use lower resolution health bars
3. Disable health bars for distant NPCs
4. Use object pooling for NPCs
5. Limit total NPC count

---

## Best Practices

### Performance
- **Limit NPC count**: 10-20 NPCs with displays works well
- **Disable far displays**: Hide health bars for NPCs far from player
- **Use shared materials**: NPCSpawner already does this
- **Pool NPCs**: Reuse NPC GameObjects instead of destroy/instantiate

### Design
- **Clear differentiation**: Use different health values for NPC types
- **Visual feedback**: Keep damage flash duration short (0.1-0.3s)
- **Fair gameplay**: Balance NPC health with player damage
- **Progressive difficulty**: Increase health for later waves/levels

### Debugging
- **Enable logs**: Check console for damage and death messages
- **Use Gizmos**: Visualize health values in Scene view
- **Test incrementally**: Test with 1-2 NPCs before spawning many
- **Monitor performance**: Use Profiler to check canvas overhead

---

## API Reference

### NPCHealth

```csharp
// Properties
float maxHealth              // Maximum health points
float currentHealth          // Current health (read-only via GetCurrentHealth)
bool enableDamageFeedback    // Enable visual feedback
float damageFeedbackDuration // Flash duration
Color damageColor            // Flash color
bool destroyOnDeath          // Destroy NPC on death
bool enableRespawn           // Enable respawning
float respawnDelay           // Respawn delay time

// Methods
void TakeDamage(float damage, Vector3 sourcePosition)
void Heal(float amount)
float GetHealthPercentage()
float GetCurrentHealth()
bool IsDead()
void SetHealth(float health)
```

### NPCHealthDisplay

```csharp
// Properties
bool enableDisplay           // Toggle display
float heightOffset           // Height above NPC
float canvasScale            // Canvas scale
float healthBarWidth         // Bar width
float healthBarHeight        // Bar height
Color healthyColor           // High HP color
Color warnColor              // Medium HP color
Color criticalColor          // Low HP color
Color backgroundColor        // Bar background color

// Methods
void SetDisplayEnabled(bool enabled)
```

---

## Credits

Developed as part of the Advanced Game AI Implementation project, integrating with the existing NPC AI system, firing mechanics, and damage probability systems.

---

## License

This NPC Health system is provided as educational material for learning Unity game development techniques.
