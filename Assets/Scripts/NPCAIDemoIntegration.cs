using UnityEngine;
using NPCAISystem;

/// <summary>
/// Demo script showing how to integrate the NPC AI System with the existing game.
/// This script demonstrates player interaction with NPCs, sound emission, and artifact integration.
/// </summary>
public class NPCAIDemoIntegration : MonoBehaviour
{
    [Header("Player Configuration")]
    [Tooltip("Player GameObject (auto-detected if null)")]
    public GameObject player;

    [Header("Sound Settings")]
    [Tooltip("Emit footstep sounds while moving")]
    public bool emitFootstepSounds = true;

    [Tooltip("Footstep sound radius")]
    public float footstepRadius = 8f;

    [Tooltip("Time between footstep sounds")]
    public float footstepInterval = 0.5f;

    [Header("Input Settings")]
    [Tooltip("Key to emit a loud noise (alert NPCs)")]
    public KeyCode alertKey = KeyCode.N;

    [Tooltip("Loud noise radius")]
    public float alertRadius = 30f;

    // Internal state
    private float lastFootstepTime;
    private Vector3 lastPosition;
    private bool isMoving = false;

    void Start()
    {
        // Auto-detect player
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("NPCAIDemoIntegration: No player found! Please tag your player GameObject.");
            }
        }

        // Ensure SoundEventManager exists
        if (FindObjectOfType<SoundEventManager>() == null)
        {
            GameObject soundManager = new GameObject("SoundEventManager");
            soundManager.AddComponent<SoundEventManager>();
            Debug.Log("NPCAIDemoIntegration: Created SoundEventManager");
        }

        lastPosition = player != null ? player.transform.position : transform.position;
    }

    void Update()
    {
        if (player == null) return;

        // Check if player is moving
        Vector3 currentPosition = player.transform.position;
        isMoving = Vector3.Distance(currentPosition, lastPosition) > 0.01f;
        lastPosition = currentPosition;

        // Emit footstep sounds
        if (emitFootstepSounds && isMoving)
        {
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                EmitFootstep();
                lastFootstepTime = Time.time;
            }
        }

        // Manual alert (press N key)
        if (Input.GetKeyDown(alertKey))
        {
            EmitAlert();
        }

        // Show instructions
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowHelp();
        }
    }

    private void EmitFootstep()
    {
        if (player != null)
        {
            SoundEventManager.BroadcastSound(player.transform.position, footstepRadius, SoundType.Footstep);
        }
    }

    private void EmitAlert()
    {
        if (player != null)
        {
            SoundEventManager.BroadcastSound(player.transform.position, alertRadius, SoundType.Voice);
            Debug.Log("Alert sound emitted! NPCs will investigate.");
        }
    }

    private void ShowHelp()
    {
        Debug.Log("=== NPC AI Demo Controls ===\n" +
                  $"Press '{alertKey}' - Emit loud sound to alert NPCs\n" +
                  "Press 'H' - Show this help\n" +
                  "Move around - Emit footstep sounds (if enabled)\n" +
                  "\nNPC Behaviors:\n" +
                  "- Red NPCs: Combat (will chase player)\n" +
                  "- Orange NPCs: Weak (will flee from player)\n" +
                  "- NPCs patrol waypoints when not alerted\n" +
                  "- NPCs search last known position when player escapes\n" +
                  "- NPCs form groups with coordinated movement");
    }

    void OnGUI()
    {
        // Display help text
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;
        style.padding = new RectOffset(10, 10, 10, 10);

        string helpText = $"NPC AI Demo\nPress 'H' for help\nPress '{alertKey}' to alert NPCs";
        
        if (isMoving && emitFootstepSounds)
        {
            helpText += "\n🔊 Footsteps active";
        }

        GUI.Box(new Rect(10, 10, 250, 100), helpText, style);

        // Display NPC count and states
        NPCController[] npcs = FindObjectsOfType<NPCController>();
        if (npcs.Length > 0)
        {
            int idleCount = 0, patrolCount = 0, chaseCount = 0, searchCount = 0, fleeCount = 0;

            foreach (NPCController npc in npcs)
            {
                switch (npc.currentState)
                {
                    case NPCState.Idle: idleCount++; break;
                    case NPCState.Patrol: patrolCount++; break;
                    case NPCState.Chase: chaseCount++; break;
                    case NPCState.Search: searchCount++; break;
                    case NPCState.Flee: fleeCount++; break;
                }
            }

            string stateText = $"NPCs: {npcs.Length}\n" +
                             $"Idle: {idleCount}\n" +
                             $"Patrol: {patrolCount}\n" +
                             $"Chase: {chaseCount}\n" +
                             $"Search: {searchCount}\n" +
                             $"Flee: {fleeCount}";

            GUI.Box(new Rect(10, 120, 200, 140), stateText, style);
        }
    }

    /// <summary>
    /// Call this method when an artifact is picked up to alert nearby NPCs
    /// </summary>
    public static void OnArtifactPickup(Vector3 position)
    {
        SoundEventManager.BroadcastSound(position, 15f, SoundType.ArtifactPickup);
        Debug.Log("Artifact pickup sound emitted!");
    }

    /// <summary>
    /// Call this method for loud events like explosions
    /// </summary>
    public static void OnExplosion(Vector3 position, float radius = 40f)
    {
        SoundEventManager.BroadcastSound(position, radius, SoundType.Explosion);
    }

    /// <summary>
    /// Call this method when a door opens
    /// </summary>
    public static void OnDoorOpen(Vector3 position)
    {
        SoundEventManager.BroadcastSound(position, 12f, SoundType.DoorOpen);
    }
}
