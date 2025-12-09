// csharp
using UnityEngine;
using UnityEditor;
using NPCAISystem;

namespace NPCAISystemEditor
{
    /// <summary>
    /// Editor window for easy NPC AI System setup
    /// </summary>
    public class NPCSetupWindow : EditorWindow
    {
        private int npcCount = 5;
        private float spawnRadius = 30f;
        private float weakNPCRatio = 0.3f;
        private bool useGroups = true;
        private int groupCount = 2;
        private NPCGroup.FormationType formationType = NPCGroup.FormationType.VFormation;

        private GameObject terrain;
        private bool autoSpawn = true;

        [MenuItem("Tools/Setup NPC AI System")]
        public static void ShowWindow()
        {
            NPCSetupWindow window = GetWindow<NPCSetupWindow>("NPC AI Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("NPC AI System Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps you quickly set up the NPC AI System in your scene. " +
                "It will create an NPCSpawner and optionally spawn NPCs automatically.",
                MessageType.Info
            );

            GUILayout.Space(10);

            // Spawn Configuration
            GUILayout.Label("Spawn Configuration", EditorStyles.boldLabel);
            npcCount = EditorGUILayout.IntSlider("NPC Count", npcCount, 1, 50);
            spawnRadius = EditorGUILayout.Slider("Spawn Radius", spawnRadius, 10f, 100f);
            weakNPCRatio = EditorGUILayout.Slider("Weak NPC Ratio", weakNPCRatio, 0f, 1f);

            GUILayout.Space(10);

            // Group Configuration
            GUILayout.Label("Group Configuration", EditorStyles.boldLabel);
            useGroups = EditorGUILayout.Toggle("Use Groups", useGroups);

            if (useGroups)
            {
                groupCount = EditorGUILayout.IntSlider("Group Count", groupCount, 1, 10);
                formationType = (NPCGroup.FormationType)EditorGUILayout.EnumPopup("Formation Type", formationType);
            }

            GUILayout.Space(10);

            // Scene References
            GUILayout.Label("Scene References", EditorStyles.boldLabel);
            terrain = (GameObject)EditorGUILayout.ObjectField("Terrain (Optional)", terrain, typeof(GameObject), true);

            GUILayout.Space(10);

            // Auto Spawn
            autoSpawn = EditorGUILayout.Toggle("Auto-Spawn on Setup", autoSpawn);

            GUILayout.Space(20);

            // Setup Buttons
            if (GUILayout.Button("Setup NPCSpawner", GUILayout.Height(40)))
            {
                SetupNPCSpawner();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Setup SoundEventManager", GUILayout.Height(30)))
            {
                SetupSoundManager();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Setup Everything", GUILayout.Height(40)))
            {
                SetupEverything();
            }

            GUILayout.Space(20);

            // Help Section
            if (GUILayout.Button("Open Documentation", GUILayout.Height(30)))
            {
                string docPath = "NPC_AI_SYSTEM_GUIDE.md";
                System.Diagnostics.Process.Start(docPath);
            }
        }

        private void SetupNPCSpawner()
        {
            // Check if spawner already exists
            NPCSpawner existingSpawner = FindObjectOfType<NPCSpawner>();
            if (existingSpawner != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "NPCSpawner Exists",
                    "An NPCSpawner already exists in the scene. Do you want to replace it?",
                    "Replace",
                    "Cancel"))
                {
                    return;
                }

                DestroyImmediate(existingSpawner.gameObject);
            }

            // Create spawner
            GameObject spawnerObj = new GameObject("NPCSpawner");
            NPCSpawner spawner = spawnerObj.AddComponent<NPCSpawner>();

            // Configure spawner
            spawner.npcCount = npcCount;
            spawner.spawnRadius = spawnRadius;
            spawner.weakNPCRatio = weakNPCRatio;
            spawner.useGroups = useGroups;
            spawner.groupCount = groupCount;
            spawner.formationType = formationType;

            // Set terrain reference
            if (terrain != null)
            {
                spawner.terrain = terrain.GetComponent<Terrain>();
            }
            else
            {
                // Try to auto-find terrain
                Terrain foundTerrain = FindObjectOfType<Terrain>();
                if (foundTerrain != null)
                {
                    spawner.terrain = foundTerrain;
                }
            }

            // Position at scene center or player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                spawnerObj.transform.position = player.transform.position;
            }

            // Mark dirty for saving
            EditorUtility.SetDirty(spawnerObj);

            // Select spawner
            Selection.activeGameObject = spawnerObj;

            // Auto-spawn if enabled and in play mode
            if (autoSpawn && Application.isPlaying)
            {
                spawner.SpawnNPCs();
            }

            Debug.Log("NPCSpawner setup complete!");
        }

        private void SetupSoundManager()
        {
            // Check if manager already exists
            SoundEventManager existingManager = FindObjectOfType<SoundEventManager>();
            if (existingManager != null)
            {
                Debug.Log("SoundEventManager already exists in scene.");
                Selection.activeGameObject = existingManager.gameObject;
                return;
            }

            // Create manager
            GameObject managerObj = new GameObject("SoundEventManager");
            SoundEventManager manager = managerObj.AddComponent<SoundEventManager>();
            manager.showDebugSounds = true;

            // Mark dirty for saving
            EditorUtility.SetDirty(managerObj);

            // Select manager
            Selection.activeGameObject = managerObj;

            Debug.Log("SoundEventManager setup complete!");
        }

        private void SetupEverything()
        {
            SetupSoundManager();
            SetupNPCSpawner();

            EditorUtility.DisplayDialog(
                "Setup Complete",
                "NPC AI System has been set up in your scene!\n\n" +
                "Next steps:\n" +
                "1. Ensure your terrain is marked as 'Navigation Static'\n" +
                "2. Bake NavMesh (Window → AI → Navigation → Bake)\n" +
                "3. Tag your player GameObject as 'Player'\n" +
                "4. Press Play to see NPCs in action!",
                "OK"
            );
        }
    }

    /// <summary>
    /// Custom inspector for NPCController
    /// </summary>
    [CustomEditor(typeof(NPCController))]
    public class NPCControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            NPCController controller = (NPCController)base.target;

            GUILayout.Space(10);

            // Show current state
            EditorGUILayout.HelpBox($"Current State: {controller.currentState}", MessageType.Info);

            GUILayout.Space(5);

            // Quick actions
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Force Chase Player"))
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        // Simulate player detection
                        NPCSensor sensor = controller.GetComponent<NPCSensor>();
                        if (sensor != null)
                        {
                            sensor.HearSound(player.transform.position, 5f);
                        }
                    }
                }

                if (GUILayout.Button("Return to Patrol"))
                {
                    // Would need to expose TransitionToState as public or use reflection
                    Debug.Log("To return to patrol, move NPC away from player.");
                }
            }
        }
    }

    /// <summary>
    /// Custom inspector for NPCSpawner
    /// </summary>
    [CustomEditor(typeof(NPCSpawner))]
    public class NPCSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            NPCSpawner spawner = (NPCSpawner)base.target;

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Use the buttons below to spawn or clear NPCs. " +
                "Make sure NavMesh is baked before spawning.",
                MessageType.Info
            );

            GUILayout.Space(5);

            if (GUILayout.Button("Spawn NPCs Now", GUILayout.Height(40)))
            {
                if (Application.isPlaying)
                {
                    spawner.SpawnNPCs();
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Not in Play Mode",
                        "NPC spawning requires Play Mode. Enter Play Mode and try again.",
                        "OK"
                    );
                }
            }

            if (GUILayout.Button("Clear Spawned NPCs", GUILayout.Height(30)))
            {
                spawner.ClearSpawnedNPCs();
            }
        }
    }
}