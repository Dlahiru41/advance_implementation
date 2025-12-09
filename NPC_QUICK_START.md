# NPC AI System - Quick Start Implementation Guide

## Prerequisites

1. ✅ Unity 2019.4 or later
2. ✅ NavMesh baked on terrain/walkable surfaces
3. ✅ Player GameObject tagged as "Player"

## Method 1: Automatic Setup (Recommended - 2 minutes)

### Step 1: Open Setup Window
1. In Unity Editor, go to menu: **Tools → Setup NPC AI System**

### Step 2: Configure Settings
- **NPC Count**: 5-10 (start small for testing)
- **Spawn Radius**: 30m (adjust based on terrain size)
- **Weak NPC Ratio**: 0.3 (30% will flee, 70% will chase)
- **Use Groups**: ✅ Enabled
- **Group Count**: 2 (creates 2 groups of NPCs)
- **Formation Type**: V-Formation (classic formation)

### Step 3: Setup
1. Click **"Setup Everything"** button
2. Follow the dialog instructions
3. Press Play to see NPCs in action!

### Step 4: Test
- Move player near NPCs to trigger detection
- Press **N** to emit alert sound
- Press **H** to show help overlay

---

## Method 2: Manual Setup (5 minutes)

### Step 1: Bake NavMesh
1. Select terrain GameObject
2. Mark as "Navigation Static" in Inspector
3. Window → AI → Navigation → Bake
4. Verify NavMesh visualizes properly (blue overlay)

### Step 2: Tag Player
1. Select your player GameObject
2. In Inspector, set Tag to "Player"

### Step 3: Add NPCSpawner
1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to "NPCSpawner"
3. Add Component → NPCSpawner
4. Configure in Inspector:
   - NPC Count: 5
   - Weak NPC Ratio: 0.3
   - Use Groups: ✅
   - Group Count: 2

### Step 4: Add SoundEventManager
1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to "SoundEventManager"
3. Add Component → SoundEventManager

### Step 5: Add Demo Integration (Optional)
1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to "NPCAIDemoIntegration"
3. Add Component → NPCAIDemoIntegration
4. This enables footstep sounds and UI overlay

### Step 6: Press Play
1. Enter Play Mode
2. NPCs spawn automatically
3. Move player to test detection

---

## Method 3: Manual NPC Creation (Advanced)

### For each NPC:

1. **Create GameObject**
   ```
   GameObject → 3D Object → Capsule
   ```

2. **Add Required Components**
   - Add Component → NavMeshAgent
   - Add Component → NPCSensor
   - Add Component → NPCController

3. **Configure NPCController**
   - Set `isWeakNPC` (true = flees, false = chases)
   - Create patrol waypoints (see below)
   - Adjust speeds and ranges

4. **Create Patrol Waypoints** (Optional)
   - Create 3-4 empty GameObjects around NPC
   - Position them in a patrol route
   - Assign to NPCController.patrolWaypoints array

5. **Configure NPCSensor**
   - Vision Range: 15m
   - Vision Angle: 110°
   - Hearing Range: 20m

6. **Assign to Group** (Optional)
   - Create GameObject with NPCGroup component
   - Add NPCs to group.groupMembers list
   - Set formation type

---

## Verification Checklist

After setup, verify:

- ✅ NavMesh is baked (blue overlay visible in Scene view)
- ✅ Player has "Player" tag
- ✅ NPCSpawner exists in scene
- ✅ SoundEventManager exists in scene
- ✅ NPCs spawn when entering Play Mode
- ✅ NPCs are on NavMesh (not floating/clipping)
- ✅ NPCs patrol when player is far away
- ✅ NPCs react when player gets close

---

## Testing Behaviors

### Test Idle → Patrol
1. Press Play
2. Observe NPCs start in Idle state
3. After ~3 seconds, they transition to Patrol

### Test Patrol → Chase
1. Move player near a combat NPC (red)
2. NPC should detect player and chase
3. NPC speed increases during chase

### Test Patrol → Flee
1. Move player near a weak NPC (orange)
2. NPC should detect player and flee away
3. NPC runs in opposite direction

### Test Chase → Search
1. Trigger chase behavior
2. Hide player behind obstacle
3. NPC loses sight and transitions to Search
4. NPC moves to last known position

### Test Search → Patrol
1. Trigger search behavior
2. Stay hidden for ~10 seconds
3. NPC gives up and returns to Patrol

### Test Hearing
1. Press **N** key (emit alert sound)
2. Nearby NPCs should investigate
3. NPCs transition to Search state
4. NPCs move toward sound origin

### Test Group Formation
1. Spawn NPCs with groups enabled
2. Observe NPCs maintain formation
3. Leader moves, followers maintain offsets
4. Formation adjusts around obstacles

---

## Common Issues & Solutions

### NPCs not spawning
**Problem**: NPCs don't appear in scene
**Solutions**:
- Ensure NavMesh is baked
- Check spawn radius isn't too small
- Verify terrain reference is set
- Check Console for error messages

### NPCs not detecting player
**Problem**: NPCs ignore player presence
**Solutions**:
- Verify player has "Player" tag
- Check vision range/angle in NPCSensor
- Ensure line of sight isn't blocked
- Verify player is within detection range

### NPCs not moving
**Problem**: NPCs stand still or jitter
**Solutions**:
- Bake NavMesh (Window → AI → Navigation)
- Check NavMeshAgent is enabled
- Verify patrol waypoints are set
- Ensure spawn position is on NavMesh

### Formation not working
**Problem**: NPCs don't maintain formation
**Solutions**:
- Enable "Use Leader Follower" in NPCGroup
- Verify all NPCs have groupManager assigned
- Check that leader is set
- Ensure NPCs are in patrol state

### Sounds not detected
**Problem**: NPCs don't react to sounds
**Solutions**:
- Verify SoundEventManager exists
- Check hearing range in NPCSensor
- Ensure sound radius is large enough
- Verify NPCs are in Idle/Patrol state

---

## Next Steps

### Enhance Your Implementation

1. **Custom NPC Types**
   - Create different NPC variants
   - Adjust detection ranges
   - Vary speeds and behaviors

2. **Integration with Artifacts**
   - Call `NPCAIDemoIntegration.OnArtifactPickup()` on pickup
   - NPCs investigate artifact sounds
   - Add strategic artifact placement

3. **Advanced Formations**
   - Experiment with formation types
   - Adjust formation spacing
   - Enable dynamic formation switching

4. **Custom Sounds**
   - Add SoundEmitter to objects
   - Create environmental sounds
   - Trigger sounds on game events

5. **Utility AI (Optional)**
   - Add scoring system to NPCController
   - Weight actions by distance/health/threat
   - Create more intelligent decision-making

---

## Performance Tips

### For 20+ NPCs:
- Increase detection interval (0.3-0.5s)
- Reduce search point count (2-3)
- Use fewer groups with more members
- Disable debug visualizations

### For Low-End Devices:
- Limit to 10-15 NPCs
- Increase detection intervals
- Use simpler formations
- Disable dynamic formation

---

## Resources

- **Full Documentation**: `NPC_AI_SYSTEM_GUIDE.md`
- **API Reference**: See guide for complete API
- **Examples**: Included in NPCAIDemoIntegration.cs
- **Unity Forums**: Search for "NavMesh" and "FSM"

---

## Support

If you encounter issues:
1. Check Console for errors
2. Review troubleshooting section above
3. Verify setup checklist
4. Enable Gizmos in Scene view for visualization
5. Review NPC_AI_SYSTEM_GUIDE.md

---

## Summary

You should now have a working NPC AI system with:
- ✅ Multiple NPCs patrolling the scene
- ✅ Detection and reaction to player
- ✅ Group formations and coordination
- ✅ Sound-based alert system
- ✅ FSM-based behavior states

**Congratulations!** Your game now has intelligent, autonomous NPCs! 🎉
