# Quick Start: Fix Camera to Follow Player

## Problem
When I press Play, the camera focuses on a random area instead of the player.

## Solution (5 Minutes)

### Step 1: Open Unity Scene
Open your scene in Unity Editor

### Step 2: Select Main Camera
In **Hierarchy** window → Click **Main Camera**

### Step 3: Disable Old Camera Script (if present)
In **Inspector**:
- Find `RTSCameraController` component
- Uncheck the checkbox to disable it

### Step 4: Add Player Camera Follow
In **Inspector**:
- Click **Add Component**
- Type "PlayerCameraFollow"
- Select it from the list

### Step 5: Configure (Use Recommended Settings)
Set these values in the **PlayerCameraFollow** component:

```
Target: [leave empty - auto-finds Player]
Offset: X=0, Y=15, Z=-10
View Angle: 50
Follow Smoothness: 0.125
```

### Step 6: Press Play
✅ Camera should now follow your player!

## Want Player Movement Too?

If your player doesn't move and you want to test camera follow:

1. Select **Player** GameObject in Hierarchy
2. **Add Component** → Type "SimplePlayerMovement"
3. Press Play
4. Use **Arrow Keys** to move player and watch camera follow

## Full Documentation

See `CAMERA_FOLLOW_GUIDE.md` for:
- Detailed explanations
- Different camera angle presets
- Troubleshooting
- Advanced configuration

## Comparison

| PlayerCameraFollow | RTSCameraController |
|-------------------|---------------------|
| Follows player automatically | Free-moving, manual control |
| Best for action games | Best for strategy games |
| Player always visible | Player may go off-screen |

## Result

✅ Camera positioned above player  
✅ Camera looks at player with nice angle  
✅ Player always visible  
✅ Smooth following movement  
✅ Works anywhere player spawns
