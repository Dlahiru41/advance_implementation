# Weapon System Quick Start

This guide provides a quick reference for using the newly implemented weapon system.

## What Was Implemented

✅ **Projectile.prefab** - Physics-based projectile with damage system
✅ **Weapon.prefab** - Generic weapon that fires projectiles
✅ **Weapon.cs** - Weapon component with Fire() and EquipWeapon() methods
✅ **NPCController.EquipWeapon()** - Method to attach weapons to NPCs
✅ **NPCSpawner weapon integration** - Automatic weapon assignment during spawn

## Quick Usage

### 1. Enable Weapons in NPCSpawner (Easiest Method)

1. Select NPCSpawner in Hierarchy
2. In Inspector, check "Enable Weapon System"
3. Drag `Assets/Prefabs/Weapon.prefab` to "Combat NPC Weapon Prefab"
4. (Optional) Drag to "Weak NPC Weapon Prefab" if weak NPCs should have weapons
5. Press Play - NPCs will spawn with weapons!

### 2. Manual Weapon Assignment (Code)

```csharp
using NPCAISystem;

// Get reference to weapon prefab
public GameObject weaponPrefab;

// Equip weapon to NPC
NPCController npc = GetComponent<NPCController>();
npc.EquipWeapon(weaponPrefab);
```

### 3. Create Custom Weapon

1. Duplicate `Assets/Prefabs/Weapon.prefab`
2. Rename to your weapon name (e.g., "Rifle.prefab")
3. Select prefab and modify in Inspector:
   - Fire Rate: Shots per second
   - Min/Max Damage: Damage range
   - Projectile Color: Visual appearance
   - Damage Model: Symmetric/Asymmetric/Dynamic
4. Use in NPCSpawner or via EquipWeapon()

## File Locations

```
Assets/
├── Prefabs/
│   ├── Projectile.prefab    ← Projectile prefab
│   └── Weapon.prefab        ← Weapon prefab
└── Scripts/
    ├── Weapon.cs            ← Generic weapon script
    ├── NPCController.cs     ← Has EquipWeapon() method
    └── NPCSpawner.cs        ← Auto-assigns weapons
```

## Key Features

✨ **Dynamic Equipment**: Weapons can be equipped/unequipped at runtime
✨ **Auto-Positioning**: Weapon holder auto-created at configurable position
✨ **Damage Models**: Support for Symmetric, Asymmetric, and Dynamic damage
✨ **Visual Customization**: Colors, trails, and effects configurable
✨ **Performance**: Projectiles auto-destroy after lifetime

## Configuration Options

### NPCController
- `weaponHolder` - Transform where weapon attaches (auto-created)
- `weaponHolderOffset` - Position offset for weapon holder (default: 0.5, 0.5, 0)

### Weapon Prefab
- `projectilePrefab` - Projectile to fire (pre-assigned to Projectile.prefab)
- `muzzle` - Spawn point for projectiles (pre-configured)
- `fireRate` - Shots per second (default: 1)
- `damageModel` - Damage calculation type
- `minDamage` / `maxDamage` - Damage range
- `projectileColor` - Visual color

### Projectile Prefab
- `speed` - Movement speed (default: 20)
- `lifetime` - Auto-destroy time (default: 5 seconds)
- `damageModel` - Damage calculation type
- `minDamage` / `maxDamage` - Damage range
- `enableTrail` - Visual trail effect

## Troubleshooting

❌ **Problem**: NPCs spawn but no weapons visible
✅ **Solution**: 
- Check "Enable Weapon System" is checked in NPCSpawner
- Verify weapon prefab is assigned to Combat/Weak NPC Weapon Prefab fields
- Check Console for warnings

❌ **Problem**: Weapon positioned incorrectly
✅ **Solution**:
- Adjust `weaponHolderOffset` in NPCController Inspector
- Or manually create and position weaponHolder Transform

❌ **Problem**: Projectiles don't deal damage
✅ **Solution**:
- Verify NPCs have NPCHealth component
- Check Player has PlayerHealth component
- Ensure collision detection is enabled

## Integration with Existing Systems

This weapon system works alongside existing systems:
- ✅ Compatible with NPCWeapon.cs (AI-specific weapon)
- ✅ Uses existing Projectile.cs damage system
- ✅ Works with NPCHealth and PlayerHealth
- ✅ Integrates with NPCSpawner workflow

**Note**: NPCWeapon.cs has AI-specific features (predictive aiming, accuracy, etc). 
The new Weapon.cs is a simpler, generic component for dynamic equipping.

## Next Steps

1. ✅ Test weapon spawning by playing the scene
2. 🎨 Customize weapon appearance and stats
3. 🔫 Create multiple weapon types (rifle, pistol, etc.)
4. 🎯 Adjust damage models for gameplay balance

## Full Documentation

See `WEAPON_SYSTEM_GUIDE.md` for complete documentation including:
- Detailed API reference
- Advanced customization
- Player weapon integration
- Custom projectile creation
- Best practices

## Support

If you encounter issues:
1. Check Console for warnings/errors
2. Verify all prefab references are assigned
3. Ensure NPCs have required components (NPCController, etc.)
4. Review WEAPON_SYSTEM_GUIDE.md for troubleshooting

---

**Created**: December 2024
**Version**: 1.0
**Status**: ✅ Production Ready
