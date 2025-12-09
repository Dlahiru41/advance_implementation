using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace NPCAISystem
{
    /// <summary>
    /// Spawns NPCs in the scene with proper configuration and group assignments
    /// </summary>
    public class NPCSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [Tooltip("Number of NPCs to spawn")]
        [Range(1, 50)]
        public int npcCount = 5;

        [Tooltip("Percentage of NPCs that should be weak (flee behavior)")]
        [Range(0f, 1f)]
        public float weakNPCRatio = 0.3f;

        [Tooltip("Spawn radius around spawner")]
        public float spawnRadius = 30f;

        [Tooltip("Minimum distance between spawned NPCs")]
        public float minSpacingBetweenNPCs = 5f;

        [Header("NPC Appearance")]
        [Tooltip("Color for combat NPCs")]
        public Color combatNPCColor = Color.red;

        [Tooltip("Color for weak NPCs")]
        public Color weakNPCColor = new Color(1f, 0.5f, 0f); // Orange

        [Tooltip("NPC scale relative to player")]
        public float npcScale = 1f;

        [Header("Group Configuration")]
        [Tooltip("Organize NPCs into groups")]
        public bool useGroups = true;

        [Tooltip("Number of groups to create")]
        [Range(1, 10)]
        public int groupCount = 2;

        [Tooltip("Formation type for groups")]
        public NPCGroup.FormationType formationType = NPCGroup.FormationType.VFormation;

        [Header("Detection Settings")]
        [Tooltip("Vision range for NPCs")]
        public float visionRange = 15f;

        [Tooltip("Vision angle for NPCs")]
        [Range(0f, 180f)]
        public float visionAngle = 110f;

        [Tooltip("Hearing range for NPCs")]
        public float hearingRange = 20f;

        [Header("Damage System Configuration")]
        [Tooltip("Enable damage system for spawned NPCs")]
        public bool enableDamageSystem = true;

        [Tooltip("Damage model for combat NPCs")]
        public NPCDamageController.DamageModel combatNPCDamageModel = NPCDamageController.DamageModel.Symmetric;

        [Tooltip("Archetype for combat NPCs")]
        public NPCDamageController.NPCArchetype combatNPCArchetype = NPCDamageController.NPCArchetype.Normal;

        [Tooltip("Damage model for weak NPCs")]
        public NPCDamageController.DamageModel weakNPCDamageModel = NPCDamageController.DamageModel.Symmetric;

        [Tooltip("Archetype for weak NPCs")]
        public NPCDamageController.NPCArchetype weakNPCArchetype = NPCDamageController.NPCArchetype.WeakGrunt;

        [Header("Health System Configuration")]
        [Tooltip("Enable health system for spawned NPCs")]
        public bool enableHealthSystem = true;

        [Tooltip("Max health for combat NPCs")]
        public float combatNPCMaxHealth = 100f;

        [Tooltip("Max health for weak NPCs")]
        public float weakNPCMaxHealth = 50f;

        [Tooltip("Enable health display above NPCs")]
        public bool enableHealthDisplay = true;

        [Header("Weapon System Configuration")]
        [Tooltip("Enable weapon system for spawned NPCs")]
        public bool enableWeaponSystem = true;

        [Tooltip("Weapon prefab for combat NPCs")]
        public GameObject combatNPCWeaponPrefab;

        [Tooltip("Weapon prefab for weak NPCs (optional)")]
        public GameObject weakNPCWeaponPrefab;

        [Header("References")]
        [Tooltip("Terrain reference for height sampling")]
        public Terrain terrain;

        // Spawned NPCs
        private List<GameObject> spawnedNPCs = new List<GameObject>();
        private List<NPCGroup> createdGroups = new List<NPCGroup>();
        
        // Shared materials for NPCs to reduce memory usage
        private Material combatNPCMaterial;
        private Material weakNPCMaterial;

        void Start()
        {
            // Auto-find terrain if not set
            if (terrain == null)
            {
                terrain = FindObjectOfType<Terrain>();
            }

            // Create shared materials
            CreateSharedMaterials();

            // Spawn NPCs if in play mode
            if (Application.isPlaying)
            {
                SpawnNPCs();
            }
        }

        /// <summary>
        /// Creates shared materials for NPCs to reduce memory usage
        /// </summary>
        private void CreateSharedMaterials()
        {
            if (combatNPCMaterial == null)
            {
                combatNPCMaterial = new Material(Shader.Find("Standard"));
                combatNPCMaterial.color = combatNPCColor;
            }

            if (weakNPCMaterial == null)
            {
                weakNPCMaterial = new Material(Shader.Find("Standard"));
                weakNPCMaterial.color = weakNPCColor;
            }
        }

        /// <summary>
        /// Spawns all NPCs
        /// </summary>
        [ContextMenu("Spawn NPCs Now")]
        public void SpawnNPCs()
        {
            // Clear existing NPCs
            ClearSpawnedNPCs();

            // Calculate number of weak NPCs
            int weakCount = Mathf.RoundToInt(npcCount * weakNPCRatio);
            int combatCount = npcCount - weakCount;

            List<Vector3> spawnPositions = GenerateSpawnPositions(npcCount);

            // Create groups if enabled
            if (useGroups && groupCount > 0)
            {
                CreateGroups();
            }

            // Spawn combat NPCs
            for (int i = 0; i < combatCount; i++)
            {
                if (i < spawnPositions.Count)
                {
                    SpawnNPC(spawnPositions[i], false, i);
                }
            }

            // Spawn weak NPCs
            for (int i = 0; i < weakCount; i++)
            {
                int posIndex = combatCount + i;
                if (posIndex < spawnPositions.Count)
                {
                    SpawnNPC(spawnPositions[posIndex], true, posIndex);
                }
            }

            Debug.Log($"NPCSpawner: Spawned {npcCount} NPCs ({combatCount} combat, {weakCount} weak) across {createdGroups.Count} groups");
        }

        private void SpawnNPC(Vector3 position, bool isWeak, int index)
        {
            // Create NPC GameObject
            GameObject npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObj.name = isWeak ? $"WeakNPC_{index}" : $"CombatNPC_{index}";
            npcObj.transform.position = position;
            npcObj.transform.localScale = Vector3.one * npcScale;

            // Set color using shared material
            Renderer renderer = npcObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = isWeak ? weakNPCMaterial : combatNPCMaterial;
            }

            // Add NavMeshAgent
            NavMeshAgent agent = npcObj.AddComponent<NavMeshAgent>();
            agent.radius = 0.5f * npcScale;
            agent.height = 2f * npcScale;
            agent.speed = isWeak ? 4f : 3.5f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;

            // Add NPCSensor
            NPCSensor sensor = npcObj.AddComponent<NPCSensor>();
            sensor.visionRange = visionRange;
            sensor.visionAngle = visionAngle;
            sensor.hearingRange = hearingRange;

            // Add NPCController
            NPCController controller = npcObj.AddComponent<NPCController>();
            controller.isWeakNPC = isWeak;

            // Add NPCDamageController if enabled
            if (enableDamageSystem)
            {
                NPCDamageController damageController = npcObj.AddComponent<NPCDamageController>();
                
                // Configure damage controller based on NPC type
                if (isWeak)
                {
                    damageController.damageModel = weakNPCDamageModel;
                    damageController.archetype = weakNPCArchetype;
                }
                else
                {
                    damageController.damageModel = combatNPCDamageModel;
                    damageController.archetype = combatNPCArchetype;
                }
            }

            // Add NPCHealth if enabled
            if (enableHealthSystem)
            {
                NPCHealth health = npcObj.AddComponent<NPCHealth>();
                
                // Configure health based on NPC type
                if (isWeak)
                {
                    health.maxHealth = weakNPCMaxHealth;
                }
                else
                {
                    health.maxHealth = combatNPCMaxHealth;
                }
                
                // Enable destroy on death (NPCs don't respawn by default)
                health.destroyOnDeath = true;
                health.enableRespawn = false;
            }

            // Add NPCHealthDisplay if enabled
            if (enableHealthSystem && enableHealthDisplay)
            {
                NPCHealthDisplay healthDisplay = npcObj.AddComponent<NPCHealthDisplay>();
                healthDisplay.enableDisplay = true;
            }

            // Equip weapon if enabled
            if (enableWeaponSystem)
            {
                GameObject weaponPrefab = null;
                
                // Choose weapon based on NPC type
                if (isWeak && weakNPCWeaponPrefab != null)
                {
                    weaponPrefab = weakNPCWeaponPrefab;
                }
                else if (!isWeak && combatNPCWeaponPrefab != null)
                {
                    weaponPrefab = combatNPCWeaponPrefab;
                }

                // Equip weapon if we have a prefab
                if (weaponPrefab != null)
                {
                    controller.EquipWeapon(weaponPrefab);
                }
            }

            // Assign to group if groups are enabled
            if (useGroups && createdGroups.Count > 0)
            {
                int groupIndex = index % createdGroups.Count;
                createdGroups[groupIndex].AddMember(controller);
            }

            // Track spawned NPC
            spawnedNPCs.Add(npcObj);
        }

        private List<Vector3> GenerateSpawnPositions(int count)
        {
            List<Vector3> positions = new List<Vector3>();
            int maxAttempts = count * 10;
            int attempts = 0;

            while (positions.Count < count && attempts < maxAttempts)
            {
                attempts++;

                // Generate random position in radius
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 candidatePos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // Sample position on NavMesh
                if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
                {
                    // Check spacing from other NPCs
                    bool validSpacing = true;
                    foreach (Vector3 existingPos in positions)
                    {
                        if (Vector3.Distance(hit.position, existingPos) < minSpacingBetweenNPCs)
                        {
                            validSpacing = false;
                            break;
                        }
                    }

                    if (validSpacing)
                    {
                        // Adjust Y to terrain height if terrain exists
                        if (terrain != null)
                        {
                            float terrainHeight = terrain.SampleHeight(hit.position);
                            hit.position = new Vector3(hit.position.x, terrainHeight + 1f, hit.position.z);
                        }

                        positions.Add(hit.position);
                    }
                }
            }

            if (positions.Count < count)
            {
                Debug.LogWarning($"NPCSpawner: Could only generate {positions.Count} valid positions out of {count} requested");
            }

            return positions;
        }

        private void CreateGroups()
        {
            createdGroups.Clear();

            for (int i = 0; i < groupCount; i++)
            {
                GameObject groupObj = new GameObject($"NPCGroup_{i}");
                groupObj.transform.parent = transform;
                
                NPCGroup group = groupObj.AddComponent<NPCGroup>();
                group.formationType = formationType;
                group.useLeaderFollower = true;

                createdGroups.Add(group);
            }
        }

        /// <summary>
        /// Clears all spawned NPCs
        /// </summary>
        [ContextMenu("Clear Spawned NPCs")]
        public void ClearSpawnedNPCs()
        {
            foreach (GameObject npc in spawnedNPCs)
            {
                if (npc != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(npc);
                    }
                    else
                    {
                        DestroyImmediate(npc);
                    }
                }
            }
            spawnedNPCs.Clear();

            // Clear groups
            foreach (NPCGroup group in createdGroups)
            {
                if (group != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(group.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(group.gameObject);
                    }
                }
            }
            createdGroups.Clear();
        }

        void OnDestroy()
        {
            ClearSpawnedNPCs();
        }

        // Visualization
        void OnDrawGizmosSelected()
        {
            // Draw spawn radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            // Draw spawned NPC positions
            Gizmos.color = Color.green;
            foreach (GameObject npc in spawnedNPCs)
            {
                if (npc != null)
                {
                    Gizmos.DrawWireSphere(npc.transform.position, minSpacingBetweenNPCs / 2f);
                }
            }
        }
    }
}
