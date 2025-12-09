using UnityEngine;
using System.Collections.Generic;

namespace NPCAISystem
{
    /// <summary>
    /// Manages sound events in the game world that NPCs can hear.
    /// Notifies nearby NPCs when sounds occur (footsteps, gunshots, artifacts, etc.)
    /// </summary>
    public class SoundEventManager : MonoBehaviour
    {
        private static SoundEventManager instance;

        [Header("Debug Settings")]
        [Tooltip("Show debug spheres for sound events")]
        public bool showDebugSounds = true;

        [Tooltip("Duration to show debug visualization (seconds)")]
        public float debugVisualizationDuration = 2f;

        // Active debug sounds for visualization
        private List<DebugSound> activeSounds = new List<DebugSound>();

        private class DebugSound
        {
            public Vector3 position;
            public float radius;
            public float endTime;
            public Color color;
        }

        void Awake()
        {
            // Singleton pattern
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            // Clean up expired debug sounds
            activeSounds.RemoveAll(s => Time.time > s.endTime);
        }

        /// <summary>
        /// Broadcasts a sound event to all nearby NPCs
        /// </summary>
        /// <param name="position">World position where sound originated</param>
        /// <param name="radius">How far the sound can be heard</param>
        /// <param name="soundType">Type of sound for classification</param>
        public static void BroadcastSound(Vector3 position, float radius, SoundType soundType = SoundType.Generic)
        {
            if (instance == null)
            {
                Debug.LogWarning("SoundEventManager: No instance found. Creating one.");
                GameObject managerObj = new GameObject("SoundEventManager");
                instance = managerObj.AddComponent<SoundEventManager>();
            }

            // Find all NPCs in range
            NPCSensor[] sensors = FindObjectsOfType<NPCSensor>();
            foreach (NPCSensor sensor in sensors)
            {
                if (sensor != null)
                {
                    sensor.HearSound(position, radius);
                }
            }

            // Add debug visualization
            if (instance.showDebugSounds)
            {
                Color debugColor = GetSoundTypeColor(soundType);
                instance.activeSounds.Add(new DebugSound
                {
                    position = position,
                    radius = radius,
                    endTime = Time.time + instance.debugVisualizationDuration,
                    color = debugColor
                });
            }

            #if UNITY_EDITOR
            Debug.Log($"Sound broadcast at {position} with radius {radius} ({soundType})");
            #endif
        }

        private static Color GetSoundTypeColor(SoundType type)
        {
            switch (type)
            {
                case SoundType.Footstep:
                    return Color.green;
                case SoundType.Gunshot:
                    return Color.red;
                case SoundType.ArtifactPickup:
                    return Color.yellow;
                case SoundType.Explosion:
                    return new Color(1f, 0.5f, 0f); // Orange
                case SoundType.Voice:
                    return Color.cyan;
                default:
                    return Color.white;
            }
        }

        // Visualization
        void OnDrawGizmos()
        {
            if (!showDebugSounds) return;

            foreach (DebugSound sound in activeSounds)
            {
                Gizmos.color = sound.color;
                Gizmos.DrawWireSphere(sound.position, sound.radius);
                
                // Fade alpha based on time remaining
                float remainingTime = sound.endTime - Time.time;
                float alpha = Mathf.Clamp01(remainingTime / debugVisualizationDuration);
                Color fadedColor = sound.color;
                fadedColor.a = alpha * 0.3f;
                Gizmos.color = fadedColor;
                Gizmos.DrawSphere(sound.position, sound.radius);
            }
        }
    }

    /// <summary>
    /// Types of sounds that can be detected by NPCs
    /// </summary>
    public enum SoundType
    {
        Generic,
        Footstep,
        Gunshot,
        ArtifactPickup,
        Explosion,
        Voice,
        DoorOpen,
        ObjectBreak
    }

    /// <summary>
    /// Helper component to emit sound events from game objects
    /// </summary>
    public class SoundEmitter : MonoBehaviour
    {
        [Header("Sound Settings")]
        [Tooltip("Type of sound this emitter produces")]
        public SoundType soundType = SoundType.Generic;

        [Tooltip("Radius in which NPCs can hear this sound")]
        public float soundRadius = 10f;

        [Tooltip("Emit sound automatically on Start")]
        public bool emitOnStart = false;

        [Tooltip("Emit sound at regular intervals")]
        public bool emitPeriodically = false;

        [Tooltip("Interval between sound emissions (if periodic)")]
        public float emissionInterval = 2f;

        private float lastEmissionTime;

        void Start()
        {
            if (emitOnStart)
            {
                EmitSound();
            }
        }

        void Update()
        {
            if (emitPeriodically)
            {
                if (Time.time - lastEmissionTime >= emissionInterval)
                {
                    EmitSound();
                    lastEmissionTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Emits a sound at this object's position
        /// </summary>
        public void EmitSound()
        {
            SoundEventManager.BroadcastSound(transform.position, soundRadius, soundType);
        }

        /// <summary>
        /// Emits a sound at a specific position
        /// </summary>
        public void EmitSoundAt(Vector3 position)
        {
            SoundEventManager.BroadcastSound(position, soundRadius, soundType);
        }

        /// <summary>
        /// Emits a sound with a custom radius
        /// </summary>
        public void EmitSoundWithRadius(float radius)
        {
            SoundEventManager.BroadcastSound(transform.position, radius, soundType);
        }
    }
}
