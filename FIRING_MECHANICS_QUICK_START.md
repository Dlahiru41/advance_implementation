# Firing Mechanics - Quick Start Guide

## ⚡ 5-Minute Setup

### Step 1: Enable Player Shooting

1. **Select Player GameObject** in Hierarchy
2. **Add Component** → Scripts → NPCAISystem → **PlayerWeapon**
3. **Done!** Player can now shoot with Left Mouse or Space

### Step 2: Enable NPC Shooting

1. **Select NPC GameObject** in Hierarchy
2. **Add Component** → Scripts → NPCAISystem → **NPCWeapon**
3. **Done!** NPC will shoot at player during chase

### Step 3: Test It

1. **Press Play**
2. **Move player** near NPC (triggers chase)
3. **Click mouse** to shoot back
4. **Watch combat** unfold!

---

## 🎮 Default Controls

| Action | Control |
|--------|---------|
| Move Player | Arrow Keys / WASD |
| Fire Weapon | Left Mouse Button |
| Fire (Alt) | Space Bar |

---

## 🎯 What You Get Out of the Box

### Player Weapon
- ✅ 2 shots per second
- ✅ 8-20 damage per shot
- ✅ Symmetric (balanced) damage
- ✅ 50m range
- ✅ Mouse-aimed shooting
- ✅ Line-of-sight checking
- ✅ Yellow projectile trails
- ✅ Muzzle flash effects

### NPC Weapon
- ✅ 1 shot per second
- ✅ 5-15 damage per shot
- ✅ Asymmetric (varied) damage
- ✅ 5-30m firing range
- ✅ 70% accuracy
- ✅ AI-controlled aiming
- ✅ Predictive targeting
- ✅ Red projectile trails
- ✅ Only fires during Chase state

---

## ⚙️ Quick Tweaks

### Make Player More Powerful

```
Select Player → Inspector → PlayerWeapon component:
- Fire Rate: 3 (instead of 2)
- Max Damage: 30 (instead of 20)
```

### Make NPCs Easier

```
Select NPC → Inspector → NPCWeapon component:
- Accuracy: 0.5 (instead of 0.7)
- Fire Rate: 0.5 (instead of 1)
```

### Make NPCs Harder

```
Select NPC → Inspector → NPCWeapon component:
- Accuracy: 0.9 (instead of 0.7)
- Fire Rate: 2 (instead of 1)
- Enable Lead Target: ✓ (if not already checked)
```

---

## 🎨 Customize Visuals

### Change Projectile Colors

**Player Projectiles:**
```
PlayerWeapon → Visual Settings → Trail Color
Default: Yellow
Try: Cyan, White, Blue
```

**NPC Projectiles:**
```
NPCWeapon → Visual Settings → Projectile Color  
Default: Red
Try: Orange, Purple, Green
```

---

## 🔧 Common Configurations

### Sniper Player (Slow, High Damage)
```
Fire Rate: 0.5
Min Damage: 30
Max Damage: 60
Max Range: 80
```

### Machine Gun Player (Fast, Low Damage)
```
Fire Rate: 5
Min Damage: 3
Max Damage: 8
Max Range: 30
```

### Elite NPC (Accurate, Dangerous)
```
Fire Rate: 1.5
Min Damage: 15
Max Damage: 30
Accuracy: 0.9
Min Firing Range: 10
Max Firing Range: 40
```

### Weak NPC (Inaccurate, Harmless)
```
Fire Rate: 0.5
Min Damage: 2
Max Damage: 5
Accuracy: 0.4
```

---

## 🐛 Quick Troubleshooting

### Player Not Shooting
- Check: PlayerWeapon component added?
- Check: Player alive? (health > 0)
- Check: Clicking left mouse button?

### NPC Not Shooting
- Check: NPCWeapon component added?
- Check: NPC in Chase state? (needs to see player)
- Check: Player within 5-30m range?
- Check: Enable Firing checked?
- Check: Not a weak NPC? (weak NPCs flee, don't shoot)

### Projectiles Disappear
- Normal! Projectiles auto-destroy after 5 seconds
- Or when hitting walls/terrain/targets

### No Damage Applied
- Check: Player has PlayerHealth component?
- Check: Player tagged as "Player"?
- Check: Projectile hitting target? (watch console logs)

---

## 📊 Damage Models Simplified

### Symmetric (Balanced)
- **Same rules for everyone**
- Fair and predictable
- Good for: Player weapons

### Asymmetric (Varied)
- **Random damage spikes**
- 20% high, 50% medium, 30% low
- Good for: Enemy variety

### Dynamic (Smart)
- **Adapts to situation**
- For projectiles, same as Symmetric
- Good for: Melee attacks

---

## 🎯 Next Steps

### Add More NPCs
1. Duplicate existing NPC with NPCWeapon
2. Change accuracy/fire rate for variety
3. Test different configurations

### Create Custom Projectiles
1. Create a sphere GameObject
2. Add Rigidbody (no gravity)
3. Add Projectile script
4. Assign to weapon's Projectile Prefab field

### Fine-Tune Balance
1. Playtest with different ranges
2. Adjust damage values
3. Modify fire rates
4. Test accuracy settings

---

## 📚 More Information

- **Full Documentation**: See FIRING_MECHANICS_GUIDE.md
- **Damage System**: See PROBABILITY_DAMAGE_GUIDE.md
- **NPC AI System**: See NPC_AI_SYSTEM_GUIDE.md

---

## ✅ Checklist

- [ ] PlayerWeapon added to Player
- [ ] NPCWeapon added to NPC(s)
- [ ] Tested player shooting
- [ ] Tested NPC shooting
- [ ] Adjusted damage for balance
- [ ] Customized visual effects
- [ ] Ready to play!

---

**That's it! You now have a fully functional firing system with probability-based damage control!** 🎉
