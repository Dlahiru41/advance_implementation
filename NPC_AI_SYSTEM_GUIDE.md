# NPC AI System Guide

## Overview

The NPC AI System implements autonomous decision-making, navigation, and sensing techniques inspired by common Game AI patterns. Each NPC reacts to the environment, avoids obstacles, forms groups when needed, and transitions between multiple behaviour states using a finite state machine (FSM).

---

## Features

### ✅ Finite State Machine (FSM)
NPCs use a robust FSM with five distinct states:
- **Idle**: Stationary with periodic detection checks
- **Patrol**: Follows waypoint paths with random wait times
- **Chase**: Pursues detected player
- **Search**: Investigates last known player position
- **Flee**: Escapes from danger (for weak NPCs)

### ✅ Advanced Sensing System
- **Vision**: Field-of-view (FOV) cone detection with line-of-sight raycasting
- **Hearing**: Sound event system for detecting player actions
- **Environmental awareness**: Obstacles block vision realistically

### ✅ Movement & Steering Behaviors
- **NavMesh Integration**: Smooth pathfinding around terrain and obstacles
- **Auto-avoidance**: Prevents NPC overlap
- **Steering behaviors**: Seek, Flee, Wander implementations

### ✅ Group Formation Behavior
- **Boids-style steering**: Separation, Alignment, Cohesion
- **Leader-Follower model**: Formation patterns (Line, V-Formation, Column, Wedge)
- **Dynamic formation**: Loosens when encountering obstacles

### ✅ Decision-Making
- **FSM-based transitions**: Clear state changes based on player detection
- **Utility-ready**: Architecture supports adding utility AI scoring

---

## Core Components

### 1. NPCController
Main component that controls NPC behavior and state management.

**Key Settings:**
- `isWeakNPC`: Determines if NPC flees or chases player
- `idleTime`: Duration before transitioning to patrol
- `patrolWaypoints`: Array of patrol route points
- `chaseSpeedMultiplier`: Speed boost when chasing (default: 1.5x)
- `searchRadius`: Area to search around last known position
- `fleeDistance`: How far to flee from danger

**States:**
- **Idle**: NPC remains stationary, checks for threats
- **Patrol**: Follows waypoints with random delays
- **Chase**: Pursues player until lost or out of range
- **Search**: Moves to last known position, performs area sweep
- **Flee**: Runs away from player (weak NPCs only)

### 2. NPCSensor
Handles vision and hearing detection.

**Vision Settings:**
- `visionAngle`: FOV cone angle (default: 110°)
- `visionRange`: Maximum sight distance (default: 15m)
- `visionObstacleMask`: Layers that block line of sight

**Hearing Settings:**
- `hearingRange`: Maximum hearing distance (default: 20m)
- Reacts to sound events from `SoundEventManager`

**Events:**
- `OnPlayerDetected`: Fired when player enters vision
- `OnPlayerLost`: Fired when player leaves vision
- `OnSoundHeard`: Fired when sound event is detected

### 3. NPCGroup
Manages group formations and Boids-style behavior.

**Boids Parameters:**
- `separationWeight`: Avoid crowding (default: 1.5)
- `alignmentWeight`: Match movement direction (default: 1.0)
- `cohesionWeight`: Move toward group center (default: 1.0)
- `separationDistance`: Minimum distance from others (default: 3m)

**Formation Types:**
- **Line**: Single file behind leader
- **V-Formation**: Classic V-shape (default)
- **Column**: Two parallel columns
- **Wedge**: Arrow/triangle formation

**Dynamic Features:**
- Formation loosens near obstacles
- Regroups in open space
- Leader issues movement goals

### 4. SoundEventManager
Broadcasts sound events that NPCs can hear.

**Usage:**
```csharp
// Broadcast a sound from code
SoundEventManager.BroadcastSound(position, radius, SoundType.Footstep);
```

**Sound Types:**
- Generic, Footstep, Gunshot, ArtifactPickup
- Explosion, Voice, DoorOpen, ObjectBreak

### 5. SoundEmitter
Component for attaching sound events to GameObjects.

**Settings:**
- `soundType`: Type of sound emitted
- `soundRadius`: Detection range
- `emitOnStart`: Auto-emit on scene start
- `emitPeriodically`: Emit at intervals

### 6. NPCSpawner
Utility for spawning multiple NPCs with configuration.

**Configuration:**
- `npcCount`: Number of NPCs to spawn
- `weakNPCRatio`: Percentage that flee (0-1)
- `spawnRadius`: Area around spawner
- `useGroups`: Organize into groups
- `groupCount`: Number of groups

---

## Quick Start

### Method 1: Using NPCSpawner (Recommended)

1. **Add NPCSpawner to scene:**
   - Create empty GameObject
   - Add `NPCSpawner` component
   - Configure spawn settings

2. **Configure spawner:**
   - Set `npcCount` (e.g., 5-10 NPCs)
   - Set `weakNPCRatio` (e.g., 0.3 for 30% weak NPCs)
   - Enable `useGroups` for formation behavior
   - Set `groupCount` (e.g., 2-3 groups)

3. **Spawn NPCs:**
   - Right-click NPCSpawner → "Spawn NPCs Now"
   - Or press Play (spawns automatically)

### Method 2: Manual Setup

1. **Create NPC GameObject:**
   - Create Capsule primitive
   - Add `NavMeshAgent` component
   - Add `NPCSensor` component
   - Add `NPCController` component

2. **Configure NPCController:**
   - Set NPC type (weak or combat)
   - Create patrol waypoints
   - Adjust detection ranges

3. **Tag Player:**
   - Ensure player GameObject has "Player" tag

4. **Bake NavMesh:**
   - Window → AI → Navigation
   - Select terrain/walkable surfaces
   - Click "Bake"

---

## Behavior State Details

### Idle State
**Trigger:** Default starting state or after search timeout
**Behavior:**
- NPC remains stationary
- Periodically checks for player (every 0.2s)
- Transitions to Patrol after `idleTime` seconds

**Use Case:** Starting state, rest between patrol routes

### Patrol State
**Trigger:** From Idle after timeout
**Behavior:**
- Follows waypoint path in sequence
- Random wait time at each waypoint (2-5s default)
- Continuously checks for player detection
- Checks for group leader commands

**Transitions:**
- → Chase: When player detected (combat NPCs)
- → Flee: When player detected (weak NPCs)
- → Search: When sound heard

### Chase State
**Trigger:** Player enters detection radius or vision cone
**Behavior:**
- Moves toward player using NavMesh pathfinding
- Speed increased by `chaseSpeedMultiplier`
- Updates destination to player's current position
- Tracks last known player position

**Transitions:**
- → Search: After losing sight for `chaseLostTimeout` seconds
- Continues: While player is visible

### Search State
**Trigger:** Lost sight of player during Chase, or heard sound
**Behavior:**
- Moves to last known player position
- Generates random search points in `searchRadius`
- Checks each point sequentially
- Performs area sweep

**Transitions:**
- → Chase: If player re-detected
- → Patrol: After `searchDuration` seconds
- → Patrol: After checking all search points

### Flee State
**Trigger:** Weak NPC detects player
**Behavior:**
- Calculates direction away from player
- Moves to position `fleeDistance` away
- Speed increased by `fleeSpeedMultiplier`
- Uses vector steering opposite to threat

**Transitions:**
- → Patrol: When player no longer visible
- Continues: While player is detected

---

## Group Formation System

### Boids-Style Behavior

**Separation:**
- NPCs maintain minimum distance from each other
- Stronger effect when closer to neighbors
- Prevents crowding and collisions

**Alignment:**
- NPCs match movement direction of nearby group members
- Creates coordinated group movement
- Smooth transitions between directions

**Cohesion:**
- NPCs move toward center of nearby group
- Keeps group together
- Balances with separation to maintain spacing

### Leader-Follower Formation

**Leader:**
- One NPC designated as group leader
- Leader follows normal behavior (patrol/chase/search)
- Other NPCs maintain formation relative to leader

**Followers:**
- Calculate target position based on formation type
- Maintain offset from leader's position and rotation
- Apply separation to avoid collisions

**Formation Types:**

1. **Line Formation:**
   - Single file behind leader
   - Offset: (0, 0, -spacing * index)

2. **V-Formation:**
   - Classic V-shape (bird flock)
   - Alternating left/right behind leader
   - Offset: (±row*spacing, 0, -row*spacing)

3. **Column Formation:**
   - Two parallel columns
   - Left/right alternating
   - Offset: (±spacing/2, 0, -row*spacing)

4. **Wedge Formation:**
   - Arrow/triangle shape
   - Wider at back
   - Dynamic positioning based on row

### Dynamic Formation Switching

**Obstacle Detection:**
- Leader checks for obstacles ahead
- Formation loosens when obstacles detected
- Followers have more freedom to navigate

**Regrouping:**
- In open space, formation tightens
- Followers return to precise positions
- Smooth transitions using lerp

---

## Sound System

### Broadcasting Sounds

**From Code:**
```csharp
using NPCAISystem;

// Simple sound
SoundEventManager.BroadcastSound(transform.position, 10f);

// Typed sound
SoundEventManager.BroadcastSound(transform.position, 15f, SoundType.Gunshot);
```

**Using SoundEmitter:**
```csharp
// Get component
SoundEmitter emitter = GetComponent<SoundEmitter>();

// Emit sound
emitter.EmitSound();

// Emit at position
emitter.EmitSoundAt(somePosition);

// Emit with custom radius
emitter.EmitSoundWithRadius(20f);
```

### Sound Types and Ranges

Recommended ranges by sound type:
- **Footstep**: 5-10m (quiet)
- **Voice**: 10-15m (medium)
- **DoorOpen**: 10-15m (medium)
- **ArtifactPickup**: 12-18m (noticeable)
- **ObjectBreak**: 15-20m (loud)
- **Gunshot**: 25-40m (very loud)
- **Explosion**: 40-60m (extremely loud)

### NPC Hearing Response

When NPC hears sound:
1. Receives sound position and radius
2. Checks if within `hearingRange`
3. Stores as last known player position
4. Transitions to Search state
5. Investigates sound origin

---

## Integration with Existing Systems

### Player Integration

**Requirements:**
- Player GameObject must have "Player" tag
- Player should be on NavMesh or have valid position
- Optional: Add SoundEmitter for footsteps

**Example Player Sound:**
```csharp
public class PlayerController : MonoBehaviour
{
    public float footstepSoundRadius = 8f;
    private float lastFootstepTime;
    
    void Update()
    {
        if (IsMoving() && Time.time - lastFootstepTime > 0.5f)
        {
            SoundEventManager.BroadcastSound(
                transform.position, 
                footstepSoundRadius, 
                SoundType.Footstep
            );
            lastFootstepTime = Time.time;
        }
    }
}
```

### Terrain Integration

**NavMesh Setup:**
1. Select terrain in Hierarchy
2. Window → AI → Navigation
3. Mark as "Navigation Static"
4. Adjust agent settings (radius, height, slope)
5. Click "Bake"

**With TerrainScaleManager:**
- NPCSpawner auto-detects terrain
- Spawn positions snap to terrain height
- Compatible with procedural generation

### Artifact Integration

**Add sound to artifact pickup:**
```csharp
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        // Emit sound before destroying
        SoundEventManager.BroadcastSound(
            transform.position, 
            15f, 
            SoundType.ArtifactPickup
        );
        
        Destroy(gameObject);
    }
}
```

---

## Configuration Guide

### NPC Type Presets

**Aggressive Combat NPC:**
- `isWeakNPC`: false
- `chaseSpeedMultiplier`: 1.5
- `visionRange`: 15m
- `visionAngle`: 110°
- `chaseLostTimeout`: 5s

**Alert Guard NPC:**
- `isWeakNPC`: false
- `chaseSpeedMultiplier`: 1.3
- `visionRange`: 18m
- `visionAngle`: 90°
- `chaseLostTimeout`: 8s

**Weak Civilian NPC:**
- `isWeakNPC`: true
- `fleeSpeedMultiplier`: 1.8
- `visionRange`: 12m
- `visionAngle`: 120°
- `fleeDistance`: 20m

**Scout NPC:**
- `isWeakNPC`: false
- `chaseSpeedMultiplier`: 1.7
- `visionRange`: 20m
- `visionAngle`: 140°
- `hearingRange`: 25m

### Performance Optimization

**For large NPC counts (20+):**
- Increase `detectionInterval` to 0.3-0.5s
- Reduce `searchPointCount` to 2-3
- Use fewer groups with more members
- Disable dynamic formation for some groups

**For low-end devices:**
- Limit to 10-15 NPCs
- Increase detection intervals
- Disable debug visualizations
- Use simpler formation types (Line, Column)

---

## Troubleshooting

### NPCs not detecting player

**Solutions:**
1. Verify player has "Player" tag
2. Check vision range and angle settings
3. Ensure no obstacles blocking line of sight
4. Check `visionObstacleMask` layer settings
5. Verify player is on correct layer

### NPCs not moving

**Solutions:**
1. Bake NavMesh (Window → AI → Navigation → Bake)
2. Check NavMeshAgent is enabled
3. Verify spawn position is on NavMesh
4. Check patrol waypoints are set
5. Ensure terrain is marked "Navigation Static"

### Formation not working

**Solutions:**
1. Verify `useLeaderFollower` is enabled
2. Check that NPCs have `groupManager` assigned
3. Ensure leader is set
4. Verify NPCs are in patrol/idle state
5. Check formation spacing isn't too large

### Sounds not detected

**Solutions:**
1. Verify SoundEventManager exists in scene
2. Check `hearingRange` on NPCSensor
3. Ensure sound radius overlaps NPC position
4. Verify NPCs are in Idle or Patrol state
5. Check debug visualization is showing sounds

### NPCs stuck or spinning

**Solutions:**
1. Increase NavMeshAgent `stoppingDistance`
2. Check for invalid patrol waypoints
3. Increase `waypointReachDistance`
4. Verify NavMesh is properly baked
5. Check for obstacles blocking paths

---

## API Reference

### NPCController

```csharp
// Get current state
NPCState state = npcController.GetCurrentState();

// Force investigation of position
npcController.InvestigatePosition(Vector3 position);
```

### NPCSensor

```csharp
// Check if player is visible
bool canSee = sensor.CanSeePlayer();

// Get last known position
if (sensor.TryGetLastKnownPosition(out Vector3 pos))
{
    // Use last known position
}

// Clear last known position
sensor.ClearLastKnownPosition();

// Manually trigger sound
sensor.HearSound(Vector3 position, float radius);
```

### NPCGroup

```csharp
// Add member to group
group.AddMember(NPCController npc);

// Remove member from group
group.RemoveMember(NPCController npc);

// Change leader
group.SetLeader(NPCController newLeader);

// Issue group movement command
group.IssueGroupMovement(Vector3 targetPosition);
```

### SoundEventManager

```csharp
// Broadcast sound (static method)
SoundEventManager.BroadcastSound(Vector3 position, float radius);
SoundEventManager.BroadcastSound(Vector3 position, float radius, SoundType type);
```

### SoundEmitter

```csharp
// Emit sound
emitter.EmitSound();
emitter.EmitSoundAt(Vector3 position);
emitter.EmitSoundWithRadius(float radius);
```

---

## Advanced Usage

### Custom State Behavior

Extend NPCController to add custom states:

```csharp
public class CustomNPCController : NPCController
{
    protected override void UpdateCustomState()
    {
        // Add your custom state logic
    }
}
```

### Utility AI Integration

Add utility scoring to decision-making:

```csharp
public class NPCUtilityAI : MonoBehaviour
{
    private NPCController controller;
    
    float CalculateChaseUtility()
    {
        float distanceScore = 1f - (distance / maxRange);
        float healthScore = currentHealth / maxHealth;
        return distanceScore * 0.7f + healthScore * 0.3f;
    }
}
```

### Custom Formation Patterns

Create custom formations in NPCGroup:

```csharp
private Vector3 CalculateCustomFormation(int index)
{
    // Return custom offset for follower
    return new Vector3(
        Mathf.Sin(index * 0.5f) * spacing,
        0,
        -index * spacing
    );
}
```

---

## Best Practices

### Performance
- Batch NPC spawning during scene load
- Use object pooling for NPCs that respawn
- Limit active NPCs based on distance to player
- Adjust detection intervals based on state

### Design
- Mix weak and combat NPCs for variety
- Use groups of 3-5 NPCs for best formations
- Vary vision/hearing ranges by NPC type
- Place waypoints strategically for patrol routes

### Debugging
- Enable Gizmos visualization in Scene view
- Use debug sound visualization
- Monitor state transitions in Inspector
- Test with single NPC before spawning many

---

## Examples

### Example 1: Guard Patrol

```csharp
// Setup guard NPC
NPCController guard = npcObj.AddComponent<NPCController>();
guard.isWeakNPC = false;
guard.chaseSpeedMultiplier = 1.3f;

NPCSensor sensor = npcObj.AddComponent<NPCSensor>();
sensor.visionRange = 15f;
sensor.visionAngle = 90f;

// Create patrol route around entrance
GameObject[] waypoints = CreateWaypointsAroundPoint(entrancePosition, 10f, 4);
guard.patrolWaypoints = waypoints;
```

### Example 2: Fleeing Civilians

```csharp
// Spawn group of civilians
NPCSpawner spawner = gameObject.AddComponent<NPCSpawner>();
spawner.npcCount = 6;
spawner.weakNPCRatio = 1.0f; // All weak
spawner.useGroups = false;
spawner.SpawnNPCs();
```

### Example 3: Military Squad

```csharp
// Create tactical squad
NPCGroup squad = squadObj.AddComponent<NPCGroup>();
squad.formationType = NPCGroup.FormationType.Wedge;
squad.useLeaderFollower = true;

// Add 5 squad members
for (int i = 0; i < 5; i++)
{
    NPCController soldier = SpawnSoldier();
    squad.AddMember(soldier);
}
```

---

## Credits

Developed as part of the Advanced Game AI Implementation project.
Implements FSM, sensing, pathfinding, and group formation patterns from game AI literature.

---

## License

This NPC AI system is provided as educational material for learning Unity game AI techniques.
