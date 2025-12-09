using UnityEngine;

namespace NPCAISystem
{
    /// <summary>
    /// Enum representing the various states an NPC can be in
    /// </summary>
    public enum NPCState
    {
        Idle,       // NPC is stationary or performing idle animations
        Patrol,     // NPC follows waypoint paths
        Chase,      // NPC pursues the player
        Search,     // NPC searches for player at last known position
        Flee        // NPC runs away from danger
    }
}
