# RTS Camera System - Quick Start Guide

## Overview

The RTS Camera Controller provides a Clash of Clans-style free-moving camera that lets you view the entire play area from above. The camera stays positioned above the terrain and never snaps to the player, giving you full control over your view.

## Features

✅ **Top-down/Isometric View**: 45° angle by default for strategic overview
✅ **Free Movement**: Pan anywhere across the terrain
✅ **Zoom Control**: Get closer or pull back for wider view
✅ **Rotation**: Rotate camera to view from different angles
✅ **Boundary Constraints**: Camera stays within terrain boundaries
✅ **Edge Panning**: Move mouse to screen edges to pan (optional)
✅ **Smooth Movement**: Interpolated camera movement for polish

## Controls

### Panning (Moving the Camera)
- **WASD Keys** or **Arrow Keys**: Move camera in corresponding directions
- **Mouse Edge Panning**: Move mouse to screen edges (can be disabled)

### Zooming
- **Mouse Wheel Up**: Zoom in (lower camera)
- **Mouse Wheel Down**: Zoom out (raise camera)

### Rotation
- **Q Key**: Rotate camera counter-clockwise
- **E Key**: Rotate camera clockwise
- **Middle Mouse Button**: Hold and drag to rotate

### Help
- **F1 Key**: Show/hide camera controls overlay in-game

## Configuration

The camera can be customized through the Inspector:

### Camera Settings
- **Initial Height**: Starting height above terrain (default: 80)
- **Initial Angle**: Viewing angle in degrees (default: 45° for isometric)
- **Initial Rotation Y**: Starting rotation around Y axis

### Pan Settings
- **Pan Speed**: How fast the camera moves (default: 30)
- **Enable Edge Pan**: Toggle mouse edge panning
- **Edge Pan Border**: Distance from edge to trigger panning in pixels

### Zoom Settings
- **Zoom Speed**: How fast zoom changes (default: 20)
- **Min Height**: Closest zoom level (default: 20)
- **Max Height**: Farthest zoom level (default: 150)

### Rotation Settings
- **Rotation Speed**: How fast camera rotates in degrees/second (default: 100)
- **Enable Rotation**: Toggle rotation controls

### Boundaries
- **Terrain**: Reference to terrain for boundary calculations
- **Boundary Padding**: Distance from terrain edge (default: 20)

### Smoothing
- **Smooth Time**: Camera movement interpolation time (default: 0.1)

## Camera Behavior

### On Game Start
1. Camera automatically finds terrain in scene
2. Calculates terrain center point
3. Positions itself above center at configured height
4. Sets isometric angle for strategic view
5. All NPCs and player are visible from this position

### During Gameplay
- Camera smoothly follows input commands
- Stays within terrain boundaries with padding
- Maintains configured height constraints
- Never attaches to player (free-moving)

## Integration with Existing Systems

The camera works seamlessly with:
- **Terrain System**: Auto-detects terrain and calculates boundaries
- **Player System**: Player visible but camera doesn't follow
- **NPC System**: All NPCs visible in the play area
- **Artifact System**: Can zoom to see artifacts clearly

## Troubleshooting

**Camera starts in wrong position:**
- Check that terrain reference is set in Inspector
- Use "Reset Camera" context menu option on component

**Camera movement is jerky:**
- Increase Smooth Time for more interpolation
- Check that frame rate is stable

**Can't rotate camera:**
- Ensure "Enable Rotation" is checked
- Verify Q/E keys aren't bound elsewhere

**Edge panning too sensitive:**
- Increase "Edge Pan Border" value
- Or disable "Enable Edge Pan"

**Camera goes outside play area:**
- Verify terrain reference is set correctly
- Increase "Boundary Padding" value

## Advanced Usage

### Focus on Specific Location
```csharp
RTSCameraController camera = Camera.main.GetComponent<RTSCameraController>();
camera.FocusOn(new Vector3(x, y, z));
```

### Reset Camera Position
```csharp
RTSCameraController camera = Camera.main.GetComponent<RTSCameraController>();
camera.ResetCamera();
```

### Programmatic Control
You can access all public properties to change camera behavior at runtime:
- `panSpeed`, `zoomSpeed`, `rotationSpeed`
- `enableEdgePan`, `enableRotation`
- `minHeight`, `maxHeight`

## Comparison with Clash of Clans

This camera implements the core features of Clash of Clans camera:
- ✅ Free-moving top-down view
- ✅ Entire play area visible
- ✅ No player attachment
- ✅ Zoom in/out capability
- ✅ Smooth panning across terrain
- ✅ Strategic overview perspective

## Tips for Best Experience

1. **Start Wide**: Keep camera at medium-high altitude initially
2. **Use Edge Pan**: Efficient for large terrain traversal
3. **Rotate for Detail**: Different angles reveal different information
4. **Zoom for Combat**: Get closer when action is happening
5. **Stay Centered**: Return to terrain center for best overview

## Performance Notes

- Camera uses SmoothDamp for efficient interpolation
- No performance impact on gameplay systems
- Boundary checks are fast AABB tests
- Works smoothly even with many NPCs

---

**Created**: December 2025
**Compatible with**: Unity 2019.4+
**Part of**: Terrain Tutorial - Advanced Implementation
