# Quick Testing Guide - Weapon System Fix

## How to Test the Fix

### Prerequisites
1. Open the project in Unity Editor
2. Load `Assets/Scenes/SampleScene.unity`
3. Ensure you have a NavMesh baked (if not, use Window → AI → Navigation → Bake)

### Testing Steps

#### 1. Start Play Mode
- Press the **Play** button in Unity Editor
- You should see:
  - Player spawned at position (152, 8, 37)
  - 5 NPCs spawning around position (100, 10, 100)
  - NPCs will be red (combat) or orange (weak)

#### 2. Test Player Firing
- **Controls**: 
  - **Left Mouse Button** or **Spacebar** to fire
  - **WASD** to move
  - **Mouse** to aim
- **Expected behavior**:
  - Yellow projectiles should spawn from player position
  - Projectiles fly toward mouse cursor direction
  - Projectiles have a yellow trail effect
  - Fire rate: 2 shots per second

#### 3. Test Player Damaging NPCs
- Move close to an NPC
- Fire projectiles at the NPC
- **Expected behavior**:
  - Console shows: "Player projectile hit NPC for X damage"
  - NPC health bar (if visible) should decrease
  - After multiple hits, NPC should be destroyed
  - NPC color may change as health decreases

#### 4. Test NPC Firing
- Wait for NPCs to detect the player
- NPCs should transition to Chase state
- **Expected behavior**:
  - NPCs fire red projectiles toward player
  - Console shows: "[NPCName] fired projectile using [DamageModel] model"
  - Red muzzle flash effect at NPC position
  - Projectiles have red trail effect

#### 5. Test NPCs Damaging Player
- Let NPC projectiles hit the player
- **Expected behavior**:
  - Console shows: "Projectile hit player for X damage"
  - Player health decreases
  - If health reaches 0, player respawns at (100, 10, 100)

### Debugging Tips

#### If Player Can't Fire:
1. Check Console for warnings about missing projectile prefab
2. Verify PlayerWeapon component exists on Player GameObject
3. Check projectilePrefab field is assigned in Inspector
4. Try clicking directly on an NPC (not just empty space)

#### If NPCs Don't Spawn:
1. Check Console for NavMesh errors
2. Ensure NPCSpawner GameObject exists in scene hierarchy
3. Check enableWeaponSystem is true in NPCSpawner
4. Verify terrain exists and has NavMesh baked

#### If NPCs Don't Fire:
1. Get close to an NPC (within 30 units)
2. Ensure NPC is in Chase state (check Console logs)
3. Check line-of-sight isn't blocked by terrain
4. Verify NPCWeapon component added to NPC (check in Inspector during Play mode)

#### If Damage Doesn't Work:
1. Check that NPCHealth component exists on NPCs
2. Check that PlayerHealth component exists on Player
3. Verify projectiles have Collider enabled
4. Check Console for collision detection logs

### Expected Console Output

```
NPCSpawner: Spawned 5 NPCs (3 combat, 2 weak) across 2 groups
CombatNPC_0 fired projectile using Symmetric model
Player fired projectile using Symmetric model
Player projectile hit NPC for 12.5 damage using Symmetric model
Projectile hit player for 8.3 damage using Asymmetric model
NPC_WeakNPC_1 health: 37.5 / 50
Player health: 91.7 / 100
```

### Performance Metrics

- **Target FPS**: 60 FPS on modern hardware
- **NPC Count**: 5 (can be increased to 20+ for stress testing)
- **Projectile Lifetime**: 5 seconds (auto-destroyed)
- **Memory Usage**: Should be stable (no leaks from projectiles)

### Success Criteria

✅ Player can fire projectiles
✅ NPCs spawn with weapons
✅ NPCs can fire at player
✅ Player projectiles reduce NPC health
✅ NPC projectiles reduce player health
✅ NPCs are destroyed when health reaches 0
✅ Player respawns when health reaches 0
✅ No errors or warnings in Console
✅ Stable frame rate during combat

### Known Limitations

1. **No Weapon Prefabs Assigned**: The system uses NPCWeapon component fallback
2. **Default Projectiles**: If Projectile.prefab fails to load, runtime defaults are created
3. **Meta Files**: Prefab .meta files are gitignored, Unity regenerates them
4. **Line-of-Sight**: NPCs need clear view to fire (terrain can block shots)

### Next Steps

If all tests pass:
1. Consider adding more NPCs
2. Test different damage models (Asymmetric, Dynamic)
3. Experiment with fire rates and ranges
4. Add visual effects (explosions, hit markers)

If issues found:
1. Check WEAPON_FIX_SUMMARY.md for implementation details
2. Review Console errors carefully
3. Verify all component references in Inspector
4. Check that NavMesh is properly baked
