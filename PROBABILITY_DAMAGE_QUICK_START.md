# Probability-Controlled Damage System - Quick Start

## 🚀 5-Minute Setup Guide

### Step 1: Add Damage to Your NPC (30 seconds)

```
1. Select your NPC GameObject in the Hierarchy
2. Click "Add Component"
3. Type "NPCDamageController" and select it
4. Done! NPC will now attack with default settings
```

**Default Configuration:**
- Damage Model: Symmetric (fair combat)
- Archetype: Normal (balanced)
- Damage Range: 5-15
- Attack Cooldown: 2 seconds
- Attack Range: 2 meters

### Step 2: Add Health to Player (30 seconds)

```
1. Select your Player GameObject (must be tagged "Player")
2. Click "Add Component"
3. Type "PlayerHealth" and select it
4. Done! Player can now take damage
```

**Default Configuration:**
- Max Health: 100
- Respawn: Enabled
- Health Bar: Visible in top-left corner

### Step 3: Test It! (30 seconds)

```
1. Enter Play Mode
2. Make the NPC chase you (walk into vision range)
3. Let NPC get close (within 2 meters)
4. Watch the health bar decrease when NPC attacks
5. Check Console for damage numbers
```

**That's it!** You now have a working probability-controlled damage system.

---

## 🔥 Automatic Setup with NPCSpawner (NEW!)

### For Dynamically Spawned NPCs (Recommended)

If you're using **NPCSpawner** to spawn NPCs at runtime, the damage system is **automatically attached**! No manual setup needed.

```
1. Select NPCSpawner GameObject in the Hierarchy
2. In Inspector, find "Damage System Configuration"
3. Ensure "Enable Damage System" is checked (default: enabled)
4. Configure damage settings for combat and weak NPCs
5. Done! All spawned NPCs will have damage system
```

**Configuration Options:**
- **Enable Damage System**: Toggle damage system on/off for spawned NPCs
- **Combat NPC Damage Model**: Damage model for aggressive NPCs (default: Symmetric)
- **Combat NPC Archetype**: Archetype for combat NPCs (default: Normal)
- **Weak NPC Damage Model**: Damage model for fleeing NPCs (default: Symmetric)
- **Weak NPC Archetype**: Archetype for weak NPCs (default: Weak Grunt)

**Benefits:**
- ✅ Zero manual configuration required
- ✅ Consistent damage settings across all NPCs
- ✅ Different settings for weak vs combat NPCs
- ✅ Works seamlessly with existing spawner

**Example Configurations:**

*Balanced Game:*
- Combat NPCs: Symmetric model, Normal archetype
- Weak NPCs: Symmetric model, Weak Grunt archetype

*High Difficulty:*
- Combat NPCs: Asymmetric model, Elite archetype
- Weak NPCs: Symmetric model, Normal archetype

*Dynamic/Adaptive NPCs:*
- Combat NPCs: Dynamic model, Elite archetype
- Weak NPCs: Symmetric model, Weak Grunt archetype

---

## 🎯 Common Configurations

### Create a Weak Enemy (1 minute)

```
1. Add NPCDamageController to NPC
2. Set "Archetype" to "Weak Grunt"
3. Done!
```

Result: Low damage (3-8), slow attacks, easy to defeat.

### Create a Boss Enemy (1 minute)

```
1. Add NPCDamageController to NPC
2. Set "Archetype" to "Boss"
3. Set "Damage Model" to "Asymmetric"
4. Done!
```

Result: High damage (20-50), unpredictable attacks, exciting fight.

### Create an Adaptive Enemy (2 minutes)

```
1. Add NPCDamageController to NPC
2. Set "Archetype" to "Elite"
3. Set "Damage Model" to "Dynamic"
4. Check all dynamic modifiers (health, aggression, distance, group)
5. Done!
```

Result: Intelligent enemy that adapts to combat situations.

---

## 🎨 Quick Customization

### Make Enemies Harder
- Increase "Max Damage"
- Decrease "Attack Cooldown"
- Increase "Critical Hit Chance"

### Make Enemies Easier
- Decrease "Max Damage"
- Increase "Attack Cooldown"
- Decrease "Critical Hit Chance"

### Change Damage Style

**For Predictable Combat:**
- Use "Symmetric" damage model

**For Exciting Unpredictability:**
- Use "Asymmetric" damage model
- Adjust probability sliders

**For Intelligent Adaptation:**
- Use "Dynamic" damage model
- Enable desired modifiers

---

## 🔍 Damage Models at a Glance

### Symmetric (Fair)
✓ Same rules as player  
✓ Predictable damage range  
✓ Good for balanced gameplay  
**Use for:** Normal enemies, RPGs

### Asymmetric (Unpredictable)
✓ Weighted damage tiers  
✓ Creates tension with spikes  
✓ Highly tunable  
**Use for:** Bosses, special enemies

### Dynamic (Adaptive)
✓ Context-aware damage  
✓ Reacts to health, distance, groups  
✓ Intelligent behavior  
**Use for:** Elite enemies, advanced AI

---

## 📊 NPC Archetypes

| Archetype | Damage Range | Crit Chance | Best For |
|-----------|--------------|-------------|----------|
| Weak Grunt | 3-8 | 5% | Cannon fodder, early game |
| Normal | 5-15 | 10% | Standard enemies |
| Elite | 10-25 | 15% | Tough enemies, mini-bosses |
| Boss | 20-50 | 20% | Boss fights |

---

## ❓ Quick Troubleshooting

**NPC not attacking?**
- ✔ Check NPC has NPCController component
- ✔ Check NPC is in Chase state
- ✔ Verify Attack Range is large enough

**No damage showing?**
- ✔ Check Player has PlayerHealth component
- ✔ Check Player is tagged "Player"
- ✔ Look at Console for damage messages

**Health not regenerating?**
- ✔ Enable "Enable Regeneration" on PlayerHealth
- ✔ Wait for Regeneration Delay (default: 5 seconds)

---

## 📖 Next Steps

### Want More Details?
Read the full **PROBABILITY_DAMAGE_GUIDE.md** for:
- In-depth explanations of each damage model
- Code examples and integration patterns
- Advanced configuration options
- Performance considerations

### Want Examples?
Check **DamageSystemExample.cs** for:
- Pre-configured example setups
- Code you can copy-paste
- Best practices

### Ready to Customize?
Use the Inspector to:
- Fine-tune damage values
- Adjust probabilities
- Enable/disable modifiers
- Configure critical hits

---

## 💡 Pro Tips

1. **Start Simple**: Use Symmetric model for your first implementation
2. **Test Balance**: Playtest to find the right damage values
3. **Mix Archetypes**: Create variety with different enemy types
4. **Watch the Console**: Damage logs help with balancing
5. **Use Gizmos**: Select NPC to see attack ranges in Scene view

---

## 🎮 Example Scenarios

### Scenario 1: Standard Action Game
- Normal enemies: Symmetric, Normal archetype
- Tough enemies: Symmetric, Elite archetype
- Bosses: Asymmetric, Boss archetype

### Scenario 2: Difficulty Progression
- Early game: Weak Grunt, Symmetric
- Mid game: Normal, Asymmetric
- Late game: Elite, Dynamic
- End boss: Boss, Dynamic with all modifiers

### Scenario 3: Stealth Game
- Alerted guards: Normal, Symmetric
- Elite guards: Elite, Dynamic with distance scaling
- Boss: Boss, Asymmetric with high burst damage

---

## ✅ Checklist

### Manual Setup:
- [ ] NPCDamageController added to NPC
- [ ] PlayerHealth added to Player
- [ ] Player tagged as "Player"
- [ ] NPCController component exists on NPC
- [ ] Tested in Play Mode
- [ ] Damage numbers appear in Console
- [ ] Health bar visible in game
- [ ] NPC attacks when in range

### NPCSpawner Setup (Recommended):
- [ ] PlayerHealth added to Player
- [ ] Player tagged as "Player"
- [ ] NPCSpawner exists in scene
- [ ] "Enable Damage System" is checked on NPCSpawner
- [ ] Damage models configured for combat/weak NPCs
- [ ] Tested in Play Mode
- [ ] Spawned NPCs have damage system
- [ ] Damage numbers appear in Console
- [ ] Health bar visible in game

---

**Need Help?** See PROBABILITY_DAMAGE_GUIDE.md for comprehensive documentation!
