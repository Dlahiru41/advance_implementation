using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace NPCAISystem
{
    /// <summary>
    /// Main NPC controller implementing FSM-based behavior with autonomous decision-making,
    /// navigation, and sensing techniques inspired by common Game AI patterns.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NPCSensor))]
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Type")]
        [Tooltip("Determines if this NPC should flee from danger (weaker NPCs)")]
        public bool isWeakNPC = false;

        [Header("State Configuration")]
        [Tooltip("Current state of the NPC")]
        [SerializeField]
        private NPCState _currentState = NPCState.Idle;
        
        /// <summary>
        /// Gets the current state of the NPC (read-only for external access)
        /// </summary>
        public NPCState currentState => _currentState;

        [Header("Idle State Settings")]
        [Tooltip("Time to remain idle before transitioning to patrol")]
        public float idleTime = 3f;

        [Header("Patrol State Settings")]
        [Tooltip("Waypoints for patrol route")]
        public Transform[] patrolWaypoints;

        [Tooltip("Random wait time range at each waypoint (min, max)")]
        public Vector2 waypointWaitTime = new Vector2(2f, 5f);

        [Tooltip("Distance to waypoint before considering it reached")]
        public float waypointReachDistance = 1f;

        [Header("Chase State Settings")]
        [Tooltip("Speed multiplier when chasing")]
        public float chaseSpeedMultiplier = 1.5f;

        [Tooltip("Time before giving up chase if player is lost")]
        public float chaseLostTimeout = 5f;

        [Header("Search State Settings")]
        [Tooltip("Search radius around last known position")]
        public float searchRadius = 5f;

        [Tooltip("Time to search before returning to patrol")]
        public float searchDuration = 10f;

        [Tooltip("Number of random search points to check")]
        public int searchPointCount = 3;

        [Header("Flee State Settings")]
        [Tooltip("Distance to flee from danger")]
        public float fleeDistance = 15f;

        [Tooltip("Speed multiplier when fleeing")]
        public float fleeSpeedMultiplier = 1.8f;

        [Header("Group Behavior")]
        [Tooltip("Reference to group manager (optional)")]
        public NPCGroup groupManager;

        [Tooltip("Is this NPC a group leader?")]
        public bool isGroupLeader = false;

        [Header("Weapon System")]
        [Tooltip("Transform where weapon will be attached (auto-created if null)")]
        public Transform weaponHolder;

        // Components
        private NavMeshAgent agent;
        private NPCSensor sensor;
        private Transform player;
        private GameObject equippedWeapon;

        // State tracking
        private float stateStartTime;
        private float baseSpeed;
        private int currentWaypointIndex = 0;
        private float waypointWaitStartTime;
        private float currentWaitTime;
        private bool isWaitingAtWaypoint = false;
        private Vector3 lastKnownPlayerPosition;
        private float lastPlayerSightTime;
        private int currentSearchPointIndex = 0;
        private List<Vector3> searchPoints = new List<Vector3>();

        void Start()
        {
            // Get required components
            agent = GetComponent<NavMeshAgent>();
            sensor = GetComponent<NPCSensor>();

            // Store base speed
            baseSpeed = agent.speed;

            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            // Subscribe to sensor events
            sensor.OnPlayerDetected += OnPlayerDetected;
            sensor.OnPlayerLost += OnPlayerLost;
            sensor.OnSoundHeard += OnSoundHeard;

            // Generate patrol waypoints if none provided
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                GenerateRandomWaypoints();
            }

            // Initialize state
            TransitionToState(NPCState.Idle);
        }

        void Update()
        {
            // Update current state behavior
            switch (_currentState)
            {
                case NPCState.Idle:
                    UpdateIdleState();
                    break;
                case NPCState.Patrol:
                    UpdatePatrolState();
                    break;
                case NPCState.Chase:
                    UpdateChaseState();
                    break;
                case NPCState.Search:
                    UpdateSearchState();
                    break;
                case NPCState.Flee:
                    UpdateFleeState();
                    break;
            }
        }

        #region State Updates

        private void UpdateIdleState()
        {
            // Check if idle time has elapsed
            if (Time.time - stateStartTime >= idleTime)
            {
                TransitionToState(NPCState.Patrol);
            }
        }

        private void UpdatePatrolState()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
                return;

            // Handle waypoint waiting
            if (isWaitingAtWaypoint)
            {
                if (Time.time - waypointWaitStartTime >= currentWaitTime)
                {
                    isWaitingAtWaypoint = false;
                    MoveToNextWaypoint();
                }
                return;
            }

            // Check if reached current waypoint
            if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
            {
                // Start waiting at waypoint
                isWaitingAtWaypoint = true;
                waypointWaitStartTime = Time.time;
                currentWaitTime = Random.Range(waypointWaitTime.x, waypointWaitTime.y);
                agent.isStopped = true;
            }
        }

        private void UpdateChaseState()
        {
            if (player == null) return;

            // Check if can still see player
            if (sensor.CanSeePlayer())
            {
                lastPlayerSightTime = Time.time;
                lastKnownPlayerPosition = player.position;
                
                // Update destination to player's current position
                agent.SetDestination(player.position);
            }
            else
            {
                // Check if lost player for too long
                if (Time.time - lastPlayerSightTime >= chaseLostTimeout)
                {
                    TransitionToState(NPCState.Search);
                }
            }
        }

        private void UpdateSearchState()
        {
            // Check if search duration expired
            if (Time.time - stateStartTime >= searchDuration)
            {
                TransitionToState(NPCState.Patrol);
                return;
            }

            // Check if player is found
            if (sensor.CanSeePlayer())
            {
                TransitionToState(NPCState.Chase);
                return;
            }

            // Move between search points
            if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
            {
                currentSearchPointIndex++;
                if (currentSearchPointIndex >= searchPoints.Count)
                {
                    // Searched all points, return to patrol
                    TransitionToState(NPCState.Patrol);
                }
                else
                {
                    agent.SetDestination(searchPoints[currentSearchPointIndex]);
                }
            }
        }

        private void UpdateFleeState()
        {
            if (player == null) return;

            // Calculate flee direction (opposite to player)
            Vector3 fleeDirection = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

            // Sample position on NavMesh
            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            // Check if player is no longer visible
            if (!sensor.CanSeePlayer())
            {
                // Return to patrol after fleeing
                TransitionToState(NPCState.Patrol);
            }
        }

        #endregion

        #region State Transitions

        private void TransitionToState(NPCState newState)
        {
            // Exit current state
            ExitState(_currentState);

            // Update state
            _currentState = newState;
            stateStartTime = Time.time;

            // Enter new state
            EnterState(newState);
        }

        private void EnterState(NPCState state)
        {
            switch (state)
            {
                case NPCState.Idle:
                    agent.isStopped = true;
                    agent.speed = baseSpeed;
                    break;

                case NPCState.Patrol:
                    agent.isStopped = false;
                    agent.speed = baseSpeed;
                    if (patrolWaypoints != null && patrolWaypoints.Length > 0)
                    {
                        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
                    }
                    break;

                case NPCState.Chase:
                    agent.isStopped = false;
                    agent.speed = baseSpeed * chaseSpeedMultiplier;
                    lastPlayerSightTime = Time.time;
                    if (player != null)
                    {
                        lastKnownPlayerPosition = player.position;
                        agent.SetDestination(player.position);
                    }
                    break;

                case NPCState.Search:
                    agent.isStopped = false;
                    agent.speed = baseSpeed;
                    GenerateSearchPoints();
                    currentSearchPointIndex = 0;
                    if (searchPoints.Count > 0)
                    {
                        agent.SetDestination(searchPoints[0]);
                    }
                    break;

                case NPCState.Flee:
                    agent.isStopped = false;
                    agent.speed = baseSpeed * fleeSpeedMultiplier;
                    break;
            }
        }

        private void ExitState(NPCState state)
        {
            // Clean up state-specific data
            switch (state)
            {
                case NPCState.Patrol:
                    isWaitingAtWaypoint = false;
                    break;
            }
        }

        #endregion

        #region Sensor Event Handlers

        private void OnPlayerDetected(Transform detectedPlayer)
        {
            // Weak NPCs flee from player
            if (isWeakNPC && currentState != NPCState.Flee)
            {
                TransitionToState(NPCState.Flee);
            }
            // Combat NPCs chase player
            else if (!isWeakNPC && currentState != NPCState.Chase)
            {
                TransitionToState(NPCState.Chase);
            }
        }

        private void OnPlayerLost()
        {
            // Transition to search if we were chasing
            if (currentState == NPCState.Chase)
            {
                TransitionToState(NPCState.Search);
            }
        }

        private void OnSoundHeard(Vector3 soundPosition)
        {
            // React to sounds based on current state
            if (currentState == NPCState.Idle || currentState == NPCState.Patrol)
            {
                lastKnownPlayerPosition = soundPosition;
                TransitionToState(NPCState.Search);
            }
        }

        #endregion

        #region Helper Methods

        private void MoveToNextWaypoint()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
                return;

            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            agent.isStopped = false;
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }

        private void GenerateRandomWaypoints()
        {
            // Generate 4 random waypoints around starting position
            List<Transform> waypoints = new List<Transform>();
            float radius = 20f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
                {
                    GameObject waypointObj = new GameObject($"Waypoint_{i}");
                    waypointObj.transform.position = hit.position;
                    waypointObj.transform.parent = transform;
                    waypoints.Add(waypointObj.transform);
                }
            }

            patrolWaypoints = waypoints.ToArray();
        }

        private void GenerateSearchPoints()
        {
            searchPoints.Clear();

            // Add last known position as first search point
            searchPoints.Add(lastKnownPlayerPosition);

            // Generate random points around last known position
            for (int i = 0; i < searchPointCount - 1; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
                Vector3 searchPoint = lastKnownPlayerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

                if (NavMesh.SamplePosition(searchPoint, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                {
                    searchPoints.Add(hit.position);
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Forces NPC to investigate a specific position
        /// </summary>
        public void InvestigatePosition(Vector3 position)
        {
            lastKnownPlayerPosition = position;
            TransitionToState(NPCState.Search);
        }

        /// <summary>
        /// Gets the current state of the NPC
        /// </summary>
        public NPCState GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Equips a weapon prefab to the NPC
        /// </summary>
        /// <param name="weaponPrefab">Weapon prefab to instantiate and attach</param>
        public void EquipWeapon(GameObject weaponPrefab)
        {
            if (weaponPrefab == null)
            {
                Debug.LogWarning($"NPCController: Cannot equip null weapon prefab on {gameObject.name}");
                return;
            }

            // Create weapon holder if it doesn't exist
            if (weaponHolder == null)
            {
                GameObject holderObj = new GameObject("WeaponHolder");
                holderObj.transform.SetParent(transform);
                holderObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0f); // Side of NPC
                holderObj.transform.localRotation = Quaternion.identity;
                weaponHolder = holderObj.transform;
            }

            // Remove existing weapon if any
            if (equippedWeapon != null)
            {
                Destroy(equippedWeapon);
            }

            // Instantiate weapon
            equippedWeapon = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);

            // Initialize weapon
            Weapon weaponScript = equippedWeapon.GetComponent<Weapon>();
            if (weaponScript != null)
            {
                weaponScript.Initialize(gameObject, false); // false = not a player weapon
                Debug.Log($"NPCController: Equipped weapon on {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"NPCController: Weapon prefab on {gameObject.name} is missing Weapon component!");
            }
        }

        #endregion

        void OnDestroy()
        {
            // Unsubscribe from events
            if (sensor != null)
            {
                sensor.OnPlayerDetected -= OnPlayerDetected;
                sensor.OnPlayerLost -= OnPlayerLost;
                sensor.OnSoundHeard -= OnSoundHeard;
            }
        }

        // Visualization
        void OnDrawGizmos()
        {
            // Draw current destination
            if (agent != null && agent.hasPath)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, agent.destination);
            }

            // Draw waypoints
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < patrolWaypoints.Length; i++)
                {
                    Transform waypoint = patrolWaypoints[i];
                    if (waypoint == null) continue;
                    
                    Gizmos.DrawWireSphere(waypoint.position, 0.5f);
                    
                    // Draw line to next waypoint
                    int nextIndex = (i + 1) % patrolWaypoints.Length;
                    Transform nextWaypoint = patrolWaypoints[nextIndex];
                    if (nextWaypoint != null)
                    {
                        Gizmos.DrawLine(waypoint.position, nextWaypoint.position);
                    }
                }
            }

            // Draw search points
            if (_currentState == NPCState.Search && searchPoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (Vector3 point in searchPoints)
                {
                    Gizmos.DrawWireSphere(point, 0.3f);
                }
            }
        }
    }
}
