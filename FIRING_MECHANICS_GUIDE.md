# Firing Mechanics System - Complete Guide

## Overview

This firing mechanics system provides **projectile-based combat** for both players and NPCs with **probability-controlled damage**, **line-of-sight checks**, and **predictive aiming**. The system integrates seamlessly with the existing NPC AI and damage systems.

---

## 🎯 Key Features

✅ **Player Firing Mechanics**
- Mouse-aimed shooting with left-click or spacebar
- Line-of-sight validation before firing
- Configurable fire rate and damage
- Visual muzzle flash effects
- Automatic projectile creation

✅ **NPC Firing Mechanics**
- AI-controlled shooting integrated with FSM states
- Predictive target leading for moving players
- Configurable accuracy per NPC archetype
- Range-based firing (min/max range)
- Works with existing NPCSensor vision system

✅ **Projectile System**
- Physics-based projectile movement
- Trail renderer for visual feedback
- Collision detection and damage application
- Auto-destruction after lifetime
- Owner tracking to prevent self-damage

✅ **Probability-Based Damage**
- **Symmetric**: Fair damage (same as player)
- **Asymmetric**: Weighted probability curves
- **Dynamic**: Context-aware (for projectiles, behaves like symmetric)
- Full integration with existing NPCDamageController models

---

## 📦 Core Components

### 1. Projectile.cs

The projectile GameObject that flies through the air and deals damage on impact.

**Key Properties:**
- `speed`: Projectile velocity (default: 20 m/s)
- `lifetime`: Auto-destroy timer (default: 5 seconds)
- `damageModel`: Symmetric, Asymmetric, or Dynamic
- `minDamage` / `maxDamage`: Damage range
- `enableTrail`: Visual trail renderer

**Usage:**
```csharp
// Projectiles are automatically created by weapon scripts
// You typically don't need to instantiate them manually
```

### 2. PlayerWeapon.cs

Handles player shooting input, aiming, and projectile spawning.

**Key Properties:**
- `projectilePrefab`: Projectile to spawn (auto-created if null)
- `fireRate`: Shots per second (default: 2)
- `maxRange`: Maximum firing distance (default: 50m)
- `fireButton`: Primary fire (default: Left Mouse)
- `alternativeFireButton`: Alternative fire (default: Space)
- `damageModel`: Damage calculation model
- `requireLineOfSight`: Check for obstacles before firing

**Inspector Setup:**
1. Add `PlayerWeapon` component to Player GameObject
2. Configure fire rate and damage
3. Choose damage model (Symmetric recommended for player)
4. Optionally assign custom projectile prefab
5. Set obstacle layer mask for line-of-sight

### 3. NPCWeapon.cs

Handles NPC AI shooting, integrated with NPCController FSM.

**Key Properties:**
- `enableFiring`: Toggle ranged attacks (default: true)
- `fireRate`: Shots per second (default: 1)
- `minFiringRange`: Don't fire if too close (default: 5m)
- `maxFiringRange`: Maximum shooting distance (default: 30m)
- `accuracy`: Aim accuracy 0-1 (default: 0.7)
- `enableLeadTarget`: Predictive aiming (default: true)
- `damageModel`: Damage calculation model

**Inspector Setup:**
1. Add `NPCWeapon` component to NPC GameObject
2. Configure firing range and accuracy
3. Choose damage model (Asymmetric for variety)
4. Optionally assign custom projectile prefab
5. Adjust archetype-specific settings

---

## 🚀 Quick Setup (5 Minutes)

### Step 1: Setup Player Shooting

```
1. Select your Player GameObject in Hierarchy
2. Add Component → NPCAISystem → PlayerWeapon
3. Configure settings:
   - Fire Rate: 2 (shots per second)
   - Damage Model: Symmetric
   - Min Damage: 8
   - Max Damage: 20
4. Test: Press Play and click left mouse button to shoot
```

### Step 2: Setup NPC Shooting

```
1. Select NPC GameObject in Hierarchy
2. Add Component → NPCAISystem → NPCWeapon
3. Configure settings:
   - Enable Firing: ✓
   - Fire Rate: 1 (shots per second)
   - Min Firing Range: 5
   - Max Firing Range: 30
   - Accuracy: 0.7 (70%)
   - Damage Model: Asymmetric
4. Test: Let NPC chase player - they will shoot when in range
```

### Step 3: Test Combat

```
1. Enter Play Mode
2. Move player near NPC to trigger chase
3. NPC will shoot at player from range
4. Player can shoot back with mouse/spacebar
5. Watch health bars and console logs
```

---

## 🎮 Controls

### Player Controls
- **Left Mouse Button**: Fire weapon
- **Space Bar**: Alternative fire
- **Arrow Keys / WASD**: Move player

### Visual Feedback
- **Yellow Trail**: Player projectiles
- **Red Trail**: NPC projectiles  
- **Flash Effect**: Muzzle flash when firing
- **Console Logs**: Damage and hit notifications

---

## ⚙️ Configuration Guide

### Player Weapon Configuration

#### Basic Settings
```csharp
PlayerWeapon weapon = player.GetComponent<PlayerWeapon>();

// Fire rate
weapon.fireRate = 3f;  // 3 shots per second

// Range
weapon.maxRange = 50f;  // 50 meter max range

// Damage
weapon.minDamage = 10f;
weapon.maxDamage = 25f;
```

#### Symmetric Damage (Balanced)
```csharp
weapon.damageModel = NPCDamageController.DamageModel.Symmetric;
weapon.symmetricVarianceMin = 0.8f;  // 80-100% of base damage
```

#### Asymmetric Damage (Varied)
```csharp
weapon.damageModel = NPCDamageController.DamageModel.Asymmetric;
weapon.highDamageProbability = 0.25f;   // 25% high damage
weapon.mediumDamageProbability = 0.5f;  // 50% medium damage
// Remaining 25% = low damage
weapon.highDamageMultiplier = 1.4f;
weapon.mediumDamageMultiplier = 1.0f;
weapon.lowDamageMultiplier = 0.6f;
```

### NPC Weapon Configuration

#### Archetype Presets

Call `ConfigureArchetype()` to apply preset values:

```csharp
NPCWeapon weapon = npc.GetComponent<NPCWeapon>();

// Weak Grunt
weapon.ConfigureArchetype(NPCDamageController.NPCArchetype.WeakGrunt);
// Result: 3-8 damage, 0.5 fire rate, 50% accuracy

// Normal
weapon.ConfigureArchetype(NPCDamageController.NPCArchetype.Normal);
// Result: 5-15 damage, 1.0 fire rate, 70% accuracy

// Elite
weapon.ConfigureArchetype(NPCDamageController.NPCArchetype.Elite);
// Result: 10-25 damage, 1.5 fire rate, 85% accuracy

// Boss
weapon.ConfigureArchetype(NPCDamageController.NPCArchetype.Boss);
// Result: 20-50 damage, 2.0 fire rate, 90% accuracy
```

#### Custom Configuration
```csharp
NPCWeapon weapon = npc.GetComponent<NPCWeapon>();

// Firing behavior
weapon.enableFiring = true;
weapon.fireRate = 1.5f;
weapon.minFiringRange = 8f;
weapon.maxFiringRange = 25f;

// Accuracy
weapon.accuracy = 0.75f;  // 75% accurate
weapon.enableLeadTarget = true;  // Predictive aiming

// Damage
weapon.damageModel = NPCDamageController.DamageModel.Asymmetric;
weapon.minDamage = 8f;
weapon.maxDamage = 20f;
```

---

## 🎯 Damage Models Explained

### Symmetric Design (Fair & Balanced)

**When to use**: Player weapons, balanced NPCs

**How it works**:
- Random damage within min-max range
- Uses variance factor (e.g., 80-100% of base)
- Predictable and fair

**Example**:
```
Base Damage: 15
Variance: 0.8 (80%)
Result: 12-15 damage per shot
```

### Asymmetric Design (Unpredictable)

**When to use**: Enemy weapons, boss fights

**How it works**:
- Three damage tiers with probabilities
- High (150%), Medium (100%), Low (50%)
- Creates tension through variance

**Example**:
```
Base Damage: 15
Probabilities: 20% high, 50% medium, 30% low
Results: 
  - 20% chance: 22.5 damage
  - 50% chance: 15 damage
  - 30% chance: 7.5 damage
```

---

## 🔧 Advanced Features

### Line-of-Sight Checking

Both player and NPC weapons check for obstacles before firing:

```csharp
// PlayerWeapon
weapon.requireLineOfSight = true;  // Enable LOS check
weapon.obstacleLayerMask = LayerMask.GetMask("Default", "Terrain");
```

The system raycasts before firing to ensure there are no walls or obstacles blocking the shot.

### Predictive Aiming (NPC Only)

NPCs can lead moving targets:

```csharp
weapon.enableLeadTarget = true;  // Predict player movement
```

This calculates player velocity and projectile travel time to aim ahead of the target.

### Accuracy System (NPC Only)

Control how well NPCs aim:

```csharp
weapon.accuracy = 0.5f;   // 50% accurate - lots of spread
weapon.accuracy = 0.85f;  // 85% accurate - tight grouping
weapon.accuracy = 1.0f;   // Perfect accuracy - no spread
```

Lower accuracy adds random deviation to the firing angle.

### Visual Customization

```csharp
// Projectile visuals
Projectile proj = projectilePrefab.GetComponent<Projectile>();
proj.trailColor = Color.cyan;
proj.trailWidth = 0.2f;
proj.trailTime = 0.8f;

// Muzzle flash
weapon.enableMuzzleFlash = true;
weapon.muzzleFlashDuration = 0.15f;
```

---

## 🎨 Integration with Existing Systems

### Works With

✅ **NPCController FSM**
- NPCs only fire during Chase state
- Respects weak NPC behavior (no firing)
- Integrates with state transitions

✅ **NPCSensor**
- Uses `CanSeePlayer()` for line-of-sight
- Respects vision range and FOV
- No firing without visual contact

✅ **NPCDamageController**
- Shares damage model enums
- Compatible probability systems
- Consistent archetype definitions

✅ **PlayerHealth**
- Projectiles call `TakeDamage()` on hit
- Damage feedback and health bar
- Death and respawn handling

### Layer Setup

For proper collision and line-of-sight:

```
1. Create layers (Edit → Project Settings → Tags and Layers):
   - Player (Layer 8)
   - NPC (Layer 9)
   - Projectile (Layer 10)

2. Configure Physics collision matrix:
   - Player projectiles collide with: NPCs, Terrain, Default
   - NPC projectiles collide with: Player, Terrain, Default
   - Projectiles don't collide with themselves
```

---

## 🐛 Troubleshooting

### Player Can't Shoot

**Check:**
- ✔ PlayerWeapon component added to Player
- ✔ Player is not dead (check PlayerHealth)
- ✔ Fire rate allows shooting (check cooldown)
- ✔ Mouse is over valid target (check raycast)

**Debug:**
```csharp
Debug.Log($"Can fire: {Time.time - lastFireTime >= 1f / fireRate}");
Debug.Log($"Has LOS: {HasLineOfSight(direction)}");
```

### NPCs Not Shooting

**Check:**
- ✔ NPCWeapon component added
- ✔ `enableFiring` is true
- ✔ NPC is in Chase state (check NPCController)
- ✔ Player in firing range (5-30m default)
- ✔ NPC can see player (check NPCSensor)
- ✔ Not a weak NPC (weak NPCs don't shoot)

**Debug:**
```csharp
Debug.Log($"State: {npcController.currentState}");
Debug.Log($"Distance: {Vector3.Distance(transform.position, player.position)}");
Debug.Log($"Can see: {npcSensor.CanSeePlayer()}");
```

### Projectiles Not Hitting

**Check:**
- ✔ Projectile has Rigidbody and Collider
- ✔ Physics collision matrix configured
- ✔ Target has collider
- ✔ Projectile speed appropriate (not too fast)

### Damage Not Applied

**Check:**
- ✔ Player has PlayerHealth component
- ✔ Player tagged as "Player"
- ✔ Projectile.Initialize() called correctly
- ✔ Damage values > 0

---

## 📊 Performance Considerations

✅ **Efficient Design**
- Projectiles auto-destroy after lifetime
- No Update() overhead when not firing
- Physics-based collision (hardware accelerated)
- Line-of-sight checks cached

✅ **Memory Friendly**
- Projectile pooling can be added if needed
- Trail renderers cleaned up automatically
- No memory leaks or allocations in hot path

✅ **Scalability**
- Tested with 10+ NPCs firing simultaneously
- Frame rate remains stable
- Collision detection optimized

---

## 🔮 Future Enhancements

Possible extensions:

- **Projectile Pooling**: Reuse projectile objects
- **Hit Effects**: Particle systems on impact
- **Sound Effects**: Gunshot and impact sounds
- **Weapon Types**: Different projectile behaviors
- **Ammo System**: Limited ammunition
- **Reload Mechanic**: Time between magazine changes
- **Spread Patterns**: Shotgun-style multi-projectile
- **Ballistics**: Gravity-affected projectiles
- **Penetration**: Projectiles pierce multiple targets

---

## 📝 Code Examples

### Example 1: Setup Player with Custom Weapon

```csharp
using UnityEngine;
using NPCAISystem;

public class PlayerSetup : MonoBehaviour
{
    void Start()
    {
        // Add weapon component
        PlayerWeapon weapon = gameObject.AddComponent<PlayerWeapon>();
        
        // Configure symmetric damage
        weapon.damageModel = NPCDamageController.DamageModel.Symmetric;
        weapon.minDamage = 12f;
        weapon.maxDamage = 25f;
        weapon.symmetricVarianceMin = 0.85f;
        
        // High fire rate
        weapon.fireRate = 3f;
        
        // Long range
        weapon.maxRange = 60f;
        
        Debug.Log("Player weapon configured");
    }
}
```

### Example 2: Setup Elite Sniper NPC

```csharp
using UnityEngine;
using NPCAISystem;

public class SniperNPCSetup : MonoBehaviour
{
    void Start()
    {
        NPCWeapon weapon = GetComponent<NPCWeapon>();
        
        // Sniper configuration
        weapon.fireRate = 0.5f;  // Slow, powerful shots
        weapon.minFiringRange = 20f;  // Long range combat
        weapon.maxFiringRange = 50f;
        weapon.accuracy = 0.95f;  // Very accurate
        weapon.enableLeadTarget = true;  // Predictive aiming
        
        // High damage
        weapon.damageModel = NPCDamageController.DamageModel.Asymmetric;
        weapon.minDamage = 25f;
        weapon.maxDamage = 60f;
        weapon.highDamageProbability = 0.3f;
        weapon.highDamageMultiplier = 2.0f;
        
        Debug.Log("Sniper NPC configured");
    }
}
```

### Example 3: Dynamic Weapon Switching

```csharp
using UnityEngine;
using NPCAISystem;

public class DynamicWeaponSystem : MonoBehaviour
{
    private PlayerWeapon weapon;
    
    void Start()
    {
        weapon = GetComponent<PlayerWeapon>();
    }
    
    void Update()
    {
        // Press 1 for symmetric (balanced)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weapon.damageModel = NPCDamageController.DamageModel.Symmetric;
            Debug.Log("Switched to Symmetric damage");
        }
        
        // Press 2 for asymmetric (high variance)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weapon.damageModel = NPCDamageController.DamageModel.Asymmetric;
            Debug.Log("Switched to Asymmetric damage");
        }
        
        // Press + to increase fire rate
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            weapon.fireRate = Mathf.Min(weapon.fireRate + 0.5f, 10f);
            Debug.Log($"Fire rate: {weapon.fireRate}");
        }
        
        // Press - to decrease fire rate
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            weapon.fireRate = Mathf.Max(weapon.fireRate - 0.5f, 0.5f);
            Debug.Log($"Fire rate: {weapon.fireRate}");
        }
    }
}
```

### Example 4: Boss with Phase-Based Firing

```csharp
using UnityEngine;
using NPCAISystem;

public class BossFiringController : MonoBehaviour
{
    private NPCWeapon weapon;
    private float bossHealth = 1000f;
    private float bossMaxHealth = 1000f;
    
    void Start()
    {
        weapon = GetComponent<NPCWeapon>();
        
        // Initial phase
        weapon.fireRate = 1f;
        weapon.accuracy = 0.8f;
    }
    
    void Update()
    {
        // Calculate health percentage
        float healthPercent = bossHealth / bossMaxHealth;
        
        // Phase 1: Above 66% health
        if (healthPercent > 0.66f)
        {
            weapon.fireRate = 1f;
            weapon.accuracy = 0.8f;
        }
        // Phase 2: 33-66% health (enraged)
        else if (healthPercent > 0.33f)
        {
            weapon.fireRate = 2f;
            weapon.accuracy = 0.85f;
            weapon.highDamageProbability = 0.35f;
        }
        // Phase 3: Below 33% health (desperate)
        else
        {
            weapon.fireRate = 3f;
            weapon.accuracy = 0.7f;  // Less accurate but rapid fire
            weapon.highDamageProbability = 0.5f;
        }
    }
    
    public void TakeDamage(float damage)
    {
        bossHealth -= damage;
    }
}
```

---

## 🎓 Best Practices

### 1. Balance Testing
- Start with low fire rates (1-2 shots/sec)
- Test damage at different ranges
- Ensure players can dodge some shots
- Monitor time-to-kill

### 2. Visual Feedback
- Always enable muzzle flash
- Use different trail colors for teams
- Add impact effects (future enhancement)
- Provide audio cues (future enhancement)

### 3. Performance
- Limit max simultaneous projectiles (use fire rate)
- Keep projectile lifetime reasonable (3-5 seconds)
- Use layer masks efficiently
- Consider object pooling for many NPCs

### 4. Gameplay
- Give player ranged advantage (higher fire rate)
- NPCs should miss occasionally (accuracy < 1.0)
- Predictive aiming makes NPCs challenging
- Balance melee vs ranged combat

---

## 📖 Summary

The Firing Mechanics System provides:

✅ **Complete projectile-based combat** for player and NPCs  
✅ **Probability-controlled damage** (Symmetric/Asymmetric/Dynamic)  
✅ **Line-of-sight validation** prevents shooting through walls  
✅ **AI integration** works seamlessly with NPC FSM  
✅ **Predictive aiming** makes NPCs intelligent opponents  
✅ **Easy setup** - add components and configure  
✅ **Highly configurable** - tune balance without code  
✅ **Performance optimized** - scales to many NPCs  

Now your game has a complete combat system with intelligent, unpredictable, and fun-to-fight NPCs!
