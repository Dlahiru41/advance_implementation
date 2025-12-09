# Weapon System Guide

This guide explains how to use the Projectile and Weapon system in the NPC AI project.

## Overview

The weapon system allows NPCs and players to equip weapons that fire projectiles. The system consists of:

1. **Projectile.prefab** - A sphere projectile with physics-based movement
2. **Weapon.prefab** - A weapon that can be equipped and fires projectiles
3. **Weapon.cs** - Script that handles weapon firing logic
4. **Projectile.cs** - Script that handles projectile movement and collision

## Components

### Projectile Prefab

Located at: `Assets/Prefabs/Projectile.prefab`

**Features:**
- Sphere mesh (0.2 scale)
- SphereCollider for collision detection
- Rigidbody for physics-based movement
- Projectile script with configurable damage and visual settings

**Key Properties:**
- `speed` - Speed of the projectile (default: 20)
- `lifetime` - Auto-destruction time (default: 5 seconds)
- `damageModel` - Damage calculation model (Symmetric/Asymmetric/Dynamic)
- `minDamage` / `maxDamage` - Damage range
- `enableTrail` - Show visual trail behind projectile

### Weapon Prefab

Located at: `Assets/Prefabs/Weapon.prefab`

**Features:**
- Cube mesh (weapon body)
- Child "Muzzle" transform (projectile spawn point)
- Weapon script with configurable firing settings

**Key Properties:**
- `projectilePrefab` - Reference to Projectile.prefab (pre-assigned)
- `muzzle` - Transform where projectiles spawn (pre-assigned)
- `fireRate` - Shots per second (default: 1)
- `damageModel` - Damage calculation model
- `minDamage` / `maxDamage` - Damage range
- `projectileColor` - Color of fired projectiles

## Usage

### Equipping Weapons to NPCs

The NPCSpawner automatically equips weapons to spawned NPCs when `enableWeaponSystem` is enabled.

**In NPCSpawner Inspector:**
1. Enable "Enable Weapon System" checkbox
2. Assign weapon prefabs:
   - `Combat NPC Weapon Prefab` - Weapon for combat NPCs
   - `Weak NPC Weapon Prefab` - Optional weapon for weak NPCs

**Manual Weapon Assignment:**
```csharp
// Get NPC controller
NPCController npcController = npc.GetComponent<NPCController>();

// Equip weapon
npcController.EquipWeapon(weaponPrefab);
```

### How It Works

1. **NPCSpawner** spawns NPCs with NPCController component
2. If weapon system is enabled, NPCSpawner calls `EquipWeapon()` on each NPC
3. NPCController creates a `weaponHolder` transform (if not present)
4. Weapon prefab is instantiated as a child of weaponHolder
5. Weapon script is initialized with the NPC as owner
6. NPCs can now fire projectiles using their equipped weapon

### Weapon Holder

Each NPC can have a `weaponHolder` Transform that determines where the weapon is attached:

- Auto-created if not present (positioned at NPC's right side)
- Can be manually assigned in Inspector for custom positioning
- Position: (0.5, 0.5, 0) relative to NPC (default)

## Integration with Existing Systems

### NPCWeapon Script

The existing `NPCWeapon.cs` script works independently of this weapon system. The new `Weapon.cs` script provides a generic weapon that can be used by both NPCs and players, while `NPCWeapon.cs` is specifically designed for NPC combat behavior.

**Differences:**
- `NPCWeapon.cs` - Integrated with NPC AI states, has accuracy, predictive aiming
- `Weapon.cs` - Generic weapon component, simpler, can be equipped dynamically

### Player Integration

Players can also use the Weapon system:

```csharp
// In player script
public GameObject weaponPrefab;
private Weapon equippedWeapon;

void Start()
{
    // Instantiate weapon
    GameObject weaponObj = Instantiate(weaponPrefab, weaponHolder);
    equippedWeapon = weaponObj.GetComponent<Weapon>();
    equippedWeapon.Initialize(gameObject, true); // true = player weapon
}

void Update()
{
    if (Input.GetMouseButton(0) && equippedWeapon.CanFire())
    {
        Vector3 fireDirection = GetAimDirection();
        equippedWeapon.Fire(fireDirection);
    }
}
```

## Customization

### Creating Custom Weapons

1. Duplicate `Weapon.prefab`
2. Modify visual appearance (mesh, materials)
3. Adjust weapon properties in Inspector:
   - Fire rate
   - Damage range
   - Projectile color
4. Optionally assign different projectile prefab

### Creating Custom Projectiles

1. Duplicate `Projectile.prefab`
2. Modify mesh/materials for different appearance
3. Adjust Projectile script properties:
   - Speed
   - Damage
   - Trail settings
   - Damage model
4. Assign to weapon's `projectilePrefab` field

## Best Practices

1. **Performance**: Projectiles auto-destroy after lifetime to prevent accumulation
2. **Damage Models**: 
   - Use `Symmetric` for fair, predictable damage
   - Use `Asymmetric` for varied, unpredictable damage
   - Use `Dynamic` for context-aware damage calculation
3. **Visual Feedback**: Enable trails on projectiles for better visibility
4. **Weapon Positioning**: Adjust weaponHolder position for proper weapon placement

## Troubleshooting

**NPCs not firing projectiles:**
- Check if weapon prefab is assigned in NPCSpawner
- Verify enableWeaponSystem is checked
- Ensure Projectile.prefab is assigned in Weapon prefab

**Projectiles not dealing damage:**
- Verify target has appropriate health component (PlayerHealth/NPCHealth)
- Check if projectile has correct owner and isPlayerProjectile flag
- Ensure collision layers are set up correctly

**Weapon not visible:**
- Check weaponHolder position in Scene view
- Verify weapon prefab has visible mesh
- Adjust weapon scale if too small

## File Structure

```
Assets/
├── Prefabs/
│   ├── Projectile.prefab       # Projectile prefab
│   ├── Projectile.prefab.meta
│   ├── Weapon.prefab           # Weapon prefab
│   └── Weapon.prefab.meta
└── Scripts/
    ├── Weapon.cs               # Weapon script
    ├── Weapon.cs.meta
    ├── Projectile.cs           # Projectile script (existing)
    ├── NPCController.cs        # Modified with EquipWeapon()
    └── NPCSpawner.cs           # Modified with weapon assignment
```

## API Reference

### Weapon.cs

**Public Methods:**
- `Initialize(GameObject owner, bool forPlayer)` - Initialize weapon with owner
- `Fire(Vector3 direction)` - Fire projectile in direction (returns bool)
- `CanFire()` - Check if weapon is ready to fire
- `GetCooldownTime()` - Get remaining cooldown time

**Public Fields:**
- `projectilePrefab` - Projectile to spawn
- `muzzle` - Spawn point transform
- `fireRate` - Shots per second
- `damageModel` - Damage calculation model
- `minDamage` / `maxDamage` - Damage range
- `projectileColor` - Projectile color

### NPCController.cs

**New Public Methods:**
- `EquipWeapon(GameObject weaponPrefab)` - Equip weapon to NPC

**New Public Fields:**
- `weaponHolder` - Transform for weapon attachment

### NPCSpawner.cs

**New Public Fields:**
- `enableWeaponSystem` - Enable/disable weapon spawning
- `combatNPCWeaponPrefab` - Weapon for combat NPCs
- `weakNPCWeaponPrefab` - Weapon for weak NPCs

## See Also

- `FIRING_MECHANICS_GUIDE.md` - Existing firing system documentation
- `NPC_AI_SYSTEM_GUIDE.md` - NPC AI system documentation
- `PROBABILITY_DAMAGE_GUIDE.md` - Damage system documentation
