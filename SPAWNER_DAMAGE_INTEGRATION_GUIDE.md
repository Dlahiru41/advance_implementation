# NPCSpawner Damage System Integration Guide

## Overview

The NPCSpawner now automatically attaches and configures the NPCDamageController component to all spawned NPCs. This eliminates the need for manual component attachment and ensures consistent damage behavior across dynamically spawned NPCs.

## What Changed

### Before
- NPCDamageController had to be manually added to each NPC GameObject
- Difficult to use with dynamically spawned NPCs
- Required manual configuration in the Unity Editor

### After
- NPCDamageController is automatically added when NPCs are spawned
- Configurable through NPCSpawner Inspector settings
- Different configurations for combat vs weak NPCs
- Can be toggled on/off globally

## Features

### 1. Automatic Component Attachment
When `NPCSpawner.enableDamageSystem` is true (default), each spawned NPC automatically receives:
- NPCDamageController component
- Configured damage model
- Configured archetype
- Ready to attack the player

### 2. Separate Configurations
Combat NPCs and weak NPCs can have different damage characteristics:

**Combat NPCs** (chase the player):
- `combatNPCDamageModel`: Damage calculation model (Symmetric/Asymmetric/Dynamic)
- `combatNPCArchetype`: Archetype preset (WeakGrunt/Normal/Elite/Boss)

**Weak NPCs** (flee from the player):
- `weakNPCDamageModel`: Damage calculation model
- `weakNPCArchetype`: Archetype preset (typically WeakGrunt)

### 3. Easy Toggle
The entire damage system can be enabled/disabled via the `enableDamageSystem` checkbox.

## Configuration Guide

### Step 1: Basic Setup

1. Open your Unity scene
2. Select the NPCSpawner GameObject in the Hierarchy
3. Find "Damage System Configuration" in the Inspector

### Step 2: Configure Combat NPCs

**For Balanced Gameplay:**
- Damage Model: Symmetric
- Archetype: Normal

**For Challenging Gameplay:**
- Damage Model: Asymmetric
- Archetype: Elite

**For Adaptive NPCs:**
- Damage Model: Dynamic
- Archetype: Elite

### Step 3: Configure Weak NPCs

**For Easy Enemies:**
- Damage Model: Symmetric
- Archetype: WeakGrunt

**For Dangerous Cowards:**
- Damage Model: Asymmetric
- Archetype: Normal

### Step 4: Add Player Health

Don't forget to add PlayerHealth component to your player:
1. Select Player GameObject
2. Add Component → PlayerHealth
3. Ensure Player is tagged as "Player"

## Testing Instructions

### Manual Testing in Unity Editor

1. **Setup Test Scene:**
   - Ensure NavMesh is baked
   - Player has "Player" tag
   - Player has PlayerHealth component
   - NPCSpawner exists with default settings

2. **Enter Play Mode:**
   - Observe NPCs spawning
   - Check Console for spawn messages

3. **Test Combat NPC Damage:**
   - Move player near a red NPC (combat)
   - Let NPC chase player
   - Stay within 2 meters (default attack range)
   - Watch health bar decrease
   - Check Console for damage messages

4. **Test Weak NPC Damage:**
   - Move player near an orange NPC (weak)
   - NPC should flee
   - If you can catch it, verify it can still deal damage

5. **Verify Configuration:**
   - Check spawned NPCs in Hierarchy during play mode
   - Verify NPCDamageController component is attached
   - Verify damage settings match configured values

### Expected Results

✅ NPCs spawn with NPCDamageController attached  
✅ Combat NPCs use configured combat settings  
✅ Weak NPCs use configured weak settings  
✅ Damage appears in Console log  
✅ Player health bar decreases when hit  
✅ No errors in Console  

## Configuration Examples

### Example 1: Standard Action Game

```
Combat NPCs:
- Damage Model: Symmetric
- Archetype: Normal
- Result: Fair, balanced combat (5-15 damage, 10% crit)

Weak NPCs:
- Damage Model: Symmetric
- Archetype: WeakGrunt
- Result: Low threat enemies (3-8 damage, 5% crit)
```

### Example 2: High Difficulty Game

```
Combat NPCs:
- Damage Model: Asymmetric
- Archetype: Elite
- Result: Unpredictable, dangerous (10-25 damage, 15% crit)

Weak NPCs:
- Damage Model: Symmetric
- Archetype: Normal
- Result: Even fleeing enemies are threats (5-15 damage)
```

### Example 3: Dynamic Adaptive AI

```
Combat NPCs:
- Damage Model: Dynamic
- Archetype: Elite
- Result: Intelligent, context-aware (10-25 damage, adapts)

Weak NPCs:
- Damage Model: Symmetric
- Archetype: WeakGrunt
- Result: Simple but consistent (3-8 damage)
```

## Troubleshooting

### NPCs Not Dealing Damage

**Check:**
- [ ] Player has PlayerHealth component
- [ ] Player is tagged "Player"
- [ ] enableDamageSystem is checked on NPCSpawner
- [ ] NPCs are entering Chase state (combat) or getting close enough (weak)
- [ ] Attack range is appropriate (default: 2 meters)

**Debug Steps:**
1. Select spawned NPC during play mode
2. Verify NPCDamageController component exists
3. Check damage model and archetype settings
4. Watch Console for damage messages
5. Ensure player is within attack range

### Wrong Damage Values

**Check:**
- [ ] Correct archetype selected
- [ ] Damage model matches intent
- [ ] NPCSpawner configuration is correct
- [ ] No override scripts modifying damage

**Solution:**
1. Verify NPCSpawner settings in Inspector
2. Check spawned NPC's NPCDamageController settings
3. Compare with archetype defaults (see NPCDamageController.cs ConfigureArchetype)

### System Disabled Unexpectedly

**Check:**
- [ ] enableDamageSystem checkbox on NPCSpawner
- [ ] No errors in Console preventing component addition
- [ ] NPCSpawner is active and enabled

## Code Reference

### NPCSpawner.cs Changes

```csharp
[Header("Damage System Configuration")]
public bool enableDamageSystem = true;
public NPCDamageController.DamageModel combatNPCDamageModel = NPCDamageController.DamageModel.Symmetric;
public NPCDamageController.NPCArchetype combatNPCArchetype = NPCDamageController.NPCArchetype.Normal;
public NPCDamageController.DamageModel weakNPCDamageModel = NPCDamageController.DamageModel.Symmetric;
public NPCDamageController.NPCArchetype weakNPCArchetype = NPCDamageController.NPCArchetype.WeakGrunt;
```

### Spawning Logic

```csharp
// Add NPCDamageController if enabled
if (enableDamageSystem)
{
    NPCDamageController damageController = npcObj.AddComponent<NPCDamageController>();
    
    // Configure damage controller based on NPC type
    if (isWeak)
    {
        damageController.damageModel = weakNPCDamageModel;
        damageController.archetype = weakNPCArchetype;
    }
    else
    {
        damageController.damageModel = combatNPCDamageModel;
        damageController.archetype = combatNPCArchetype;
    }
}
```

## API Documentation

### NPCSpawner Public Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `enableDamageSystem` | bool | true | Enable/disable automatic damage system |
| `combatNPCDamageModel` | DamageModel | Symmetric | Damage model for combat NPCs |
| `combatNPCArchetype` | NPCArchetype | Normal | Archetype for combat NPCs |
| `weakNPCDamageModel` | DamageModel | Symmetric | Damage model for weak NPCs |
| `weakNPCArchetype` | NPCArchetype | WeakGrunt | Archetype for weak NPCs |

### Related Components

- **NPCDamageController**: Handles damage calculation and delivery
- **NPCController**: Controls NPC state machine and behavior
- **NPCSensor**: Handles detection and line of sight
- **PlayerHealth**: Receives and tracks damage to player

## Integration with Existing Systems

### Works With:
- ✅ NPC AI System (NPCController)
- ✅ Group formations (NPCGroup)
- ✅ Detection system (NPCSensor)
- ✅ Sound system (SoundEventManager)
- ✅ All damage models (Symmetric/Asymmetric/Dynamic)

### Requirements:
- Player GameObject must be tagged "Player"
- Player must have PlayerHealth component
- NavMesh must be baked for NPC spawning to work

## Performance Considerations

- **Component Addition**: Adding NPCDamageController at spawn time has negligible performance impact
- **Memory**: No additional memory overhead compared to manual attachment
- **Runtime**: Damage calculations occur only during attacks (respects attack cooldown)

## Best Practices

1. **Start Simple**: Use Symmetric model with Normal archetype for initial testing
2. **Test Incrementally**: Enable damage system and test before adjusting settings
3. **Balance Carefully**: Playtest different configurations to find the right difficulty
4. **Use Archetypes**: Leverage built-in archetypes before custom tuning
5. **Monitor Console**: Watch damage logs during testing for balance insights
6. **Separate Configurations**: Use different settings for combat vs weak NPCs for variety

## Related Documentation

- **PROBABILITY_DAMAGE_QUICK_START.md**: Quick start guide for damage system
- **PROBABILITY_DAMAGE_GUIDE.md**: Comprehensive damage system documentation
- **NPC_QUICK_START.md**: Quick start guide for NPC AI system
- **NPC_AI_SYSTEM_GUIDE.md**: Comprehensive NPC AI documentation

## Summary

The automatic damage system integration makes it easy to add combat functionality to dynamically spawned NPCs. With just a few Inspector settings, you can configure different damage behaviors for different NPC types, creating varied and engaging gameplay without manual component management.

**Key Benefits:**
- 🎮 Zero manual component attachment
- ⚙️ Configurable through Inspector
- 🎯 Different settings per NPC type
- 🔄 Works seamlessly with existing systems
- 📊 Consistent damage behavior
- 🚀 Ready to use out of the box
