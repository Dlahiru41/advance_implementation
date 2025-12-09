using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace NPCAISystem
{
    /// <summary>
    /// Implements Boids-style group behavior with separation, alignment, and cohesion.
    /// Also supports Leader-Follower formation patterns.
    /// </summary>
    public class NPCGroup : MonoBehaviour
    {
        [Header("Group Configuration")]
        [Tooltip("NPCs in this group")]
        public List<NPCController> groupMembers = new List<NPCController>();

        [Tooltip("Leader NPC (if using leader-follower model)")]
        public NPCController leader;

        [Tooltip("Use leader-follower formation instead of pure Boids")]
        public bool useLeaderFollower = true;

        [Header("Boids Parameters")]
        [Tooltip("Weight for separation behavior (avoid crowding)")]
        [Range(0f, 5f)]
        public float separationWeight = 1.5f;

        [Tooltip("Weight for alignment behavior (match direction)")]
        [Range(0f, 5f)]
        public float alignmentWeight = 1f;

        [Tooltip("Weight for cohesion behavior (move toward center)")]
        [Range(0f, 5f)]
        public float cohesionWeight = 1f;

        [Tooltip("Minimum distance to maintain from other NPCs")]
        public float separationDistance = 3f;

        [Tooltip("Maximum distance to consider for group behaviors")]
        public float neighborDistance = 10f;

        [Header("Formation Settings")]
        [Tooltip("Formation pattern for followers")]
        public FormationType formationType = FormationType.VFormation;

        [Tooltip("Spacing between formation positions")]
        public float formationSpacing = 2f;

        [Tooltip("Looseness of formation (higher = more flexible)")]
        [Range(0f, 1f)]
        public float formationLooseness = 0.3f;

        [Header("Dynamic Formation")]
        [Tooltip("Loosen formation when encountering obstacles")]
        public bool dynamicFormation = true;

        [Tooltip("Distance to check for obstacles ahead")]
        public float obstacleCheckDistance = 5f;

        // Formation types
        public enum FormationType
        {
            Line,           // Single line behind leader
            VFormation,     // V-shaped formation
            Column,         // Two columns
            Wedge           // Wedge/arrow formation
        }

        // Internal state
        private Dictionary<NPCController, Vector3> formationOffsets = new Dictionary<NPCController, Vector3>();
        private bool formationLoose = false;

        void Start()
        {
            // Auto-detect group members if not set
            if (groupMembers.Count == 0)
            {
                NPCController[] npcs = FindObjectsOfType<NPCController>();
                foreach (var npc in npcs)
                {
                    if (npc.groupManager == this)
                    {
                        groupMembers.Add(npc);
                    }
                }
            }

            // Auto-select leader if not set
            if (leader == null && groupMembers.Count > 0)
            {
                leader = groupMembers[0];
                leader.isGroupLeader = true;
            }

            // Calculate formation offsets
            CalculateFormationOffsets();
        }

        void Update()
        {
            if (groupMembers.Count == 0) return;

            // Check for dynamic formation switching
            if (dynamicFormation)
            {
                CheckForObstacles();
            }

            // Apply group behaviors
            if (useLeaderFollower && leader != null)
            {
                ApplyLeaderFollowerBehavior();
            }
            else
            {
                ApplyBoidsBehavior();
            }
        }

        #region Boids Behaviors

        private void ApplyBoidsBehavior()
        {
            foreach (NPCController npc in groupMembers)
            {
                if (npc == null) continue;

                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
                if (agent == null) continue;

                // Calculate steering forces
                Vector3 separation = CalculateSeparation(npc) * separationWeight;
                Vector3 alignment = CalculateAlignment(npc) * alignmentWeight;
                Vector3 cohesion = CalculateCohesion(npc) * cohesionWeight;

                // Combine forces
                Vector3 steeringForce = separation + alignment + cohesion;

                // Apply to movement (modify destination slightly)
                if (steeringForce.magnitude > 0.1f)
                {
                    Vector3 targetPosition = npc.transform.position + steeringForce.normalized * 2f;
                    
                    if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        // Only adjust if NPC is in patrol or idle state
                        if (npc.GetCurrentState() == NPCState.Patrol || npc.GetCurrentState() == NPCState.Idle)
                        {
                            agent.SetDestination(hit.position);
                        }
                    }
                }
            }
        }

        private Vector3 CalculateSeparation(NPCController npc)
        {
            Vector3 separationForce = Vector3.zero;
            int count = 0;

            foreach (NPCController other in groupMembers)
            {
                if (other == null || other == npc) continue;

                float distance = Vector3.Distance(npc.transform.position, other.transform.position);
                
                if (distance < separationDistance && distance > 0)
                {
                    Vector3 awayVector = (npc.transform.position - other.transform.position).normalized;
                    awayVector /= distance; // Weight by distance (closer = stronger)
                    separationForce += awayVector;
                    count++;
                }
            }

            if (count > 0)
            {
                separationForce /= count;
            }

            return separationForce;
        }

        private Vector3 CalculateAlignment(NPCController npc)
        {
            Vector3 averageDirection = Vector3.zero;
            int count = 0;

            foreach (NPCController other in groupMembers)
            {
                if (other == null || other == npc) continue;

                float distance = Vector3.Distance(npc.transform.position, other.transform.position);
                
                if (distance < neighborDistance)
                {
                    NavMeshAgent otherAgent = other.GetComponent<NavMeshAgent>();
                    if (otherAgent != null)
                    {
                        averageDirection += otherAgent.velocity.normalized;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                averageDirection /= count;
                return averageDirection - npc.GetComponent<NavMeshAgent>().velocity.normalized;
            }

            return Vector3.zero;
        }

        private Vector3 CalculateCohesion(NPCController npc)
        {
            Vector3 centerOfMass = Vector3.zero;
            int count = 0;

            foreach (NPCController other in groupMembers)
            {
                if (other == null || other == npc) continue;

                float distance = Vector3.Distance(npc.transform.position, other.transform.position);
                
                if (distance < neighborDistance)
                {
                    centerOfMass += other.transform.position;
                    count++;
                }
            }

            if (count > 0)
            {
                centerOfMass /= count;
                return (centerOfMass - npc.transform.position).normalized;
            }

            return Vector3.zero;
        }

        #endregion

        #region Leader-Follower Formation

        private void ApplyLeaderFollowerBehavior()
        {
            if (leader == null) return;

            foreach (NPCController follower in groupMembers)
            {
                if (follower == null || follower == leader) continue;

                // Skip if follower is not in patrol state
                if (follower.GetCurrentState() != NPCState.Patrol && follower.GetCurrentState() != NPCState.Idle)
                    continue;

                NavMeshAgent agent = follower.GetComponent<NavMeshAgent>();
                if (agent == null) continue;

                // Get formation offset for this follower
                if (!formationOffsets.ContainsKey(follower))
                    continue;

                // Calculate target formation position
                Vector3 formationOffset = formationOffsets[follower];
                Vector3 targetPosition = leader.transform.position + leader.transform.TransformDirection(formationOffset);

                // Apply formation looseness
                if (formationLoose || formationLooseness > 0)
                {
                    float loosenessFactor = formationLoose ? 1f : formationLooseness;
                    targetPosition = Vector3.Lerp(follower.transform.position, targetPosition, 1f - loosenessFactor);
                }

                // Sample on NavMesh
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                // Also apply separation to avoid collisions
                Vector3 separation = CalculateSeparation(follower) * separationWeight;
                if (separation.magnitude > 0.1f)
                {
                    Vector3 adjustedPosition = hit.position + separation.normalized * 1f;
                    if (NavMesh.SamplePosition(adjustedPosition, out NavMeshHit adjustedHit, 5f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(adjustedHit.position);
                    }
                }
            }
        }

        private void CalculateFormationOffsets()
        {
            formationOffsets.Clear();

            if (groupMembers.Count <= 1) return;

            int followerCount = groupMembers.Count - 1; // Exclude leader
            int followerIndex = 0;

            foreach (NPCController npc in groupMembers)
            {
                if (npc == leader) continue;

                Vector3 offset = CalculateFormationPosition(followerIndex, followerCount);
                formationOffsets[npc] = offset;
                followerIndex++;
            }
        }

        private Vector3 CalculateFormationPosition(int index, int totalFollowers)
        {
            Vector3 offset = Vector3.zero;

            switch (formationType)
            {
                case FormationType.Line:
                    // Single line behind leader
                    offset = new Vector3(0, 0, -(index + 1) * formationSpacing);
                    break;

                case FormationType.VFormation:
                    // V-formation (alternating left and right)
                    int side = (index % 2 == 0) ? 1 : -1;
                    int row = index / 2 + 1;
                    offset = new Vector3(side * row * formationSpacing, 0, -row * formationSpacing);
                    break;

                case FormationType.Column:
                    // Two columns
                    int columnSide = (index % 2 == 0) ? 1 : -1;
                    int columnRow = index / 2;
                    offset = new Vector3(columnSide * formationSpacing * 0.5f, 0, -columnRow * formationSpacing);
                    break;

                case FormationType.Wedge:
                    // Wedge/arrow formation
                    int wedgeRow = Mathf.FloorToInt(Mathf.Sqrt(index + 1));
                    int positionInRow = index - (wedgeRow * wedgeRow - 1);
                    int rowWidth = wedgeRow * 2 + 1;
                    float xPos = (positionInRow - wedgeRow) * formationSpacing;
                    offset = new Vector3(xPos, 0, -wedgeRow * formationSpacing);
                    break;
            }

            return offset;
        }

        #endregion

        #region Dynamic Formation

        private void CheckForObstacles()
        {
            if (leader == null) return;

            // Check if leader is encountering obstacles
            NavMeshAgent leaderAgent = leader.GetComponent<NavMeshAgent>();
            if (leaderAgent == null) return;

            Vector3 checkDirection = leaderAgent.velocity.normalized;
            if (checkDirection.magnitude < 0.1f)
            {
                checkDirection = leader.transform.forward;
            }

            // Raycast for obstacles
            if (Physics.Raycast(leader.transform.position, checkDirection, out RaycastHit hit, obstacleCheckDistance))
            {
                // Obstacle detected - loosen formation
                formationLoose = true;
            }
            else
            {
                // No obstacles - tighten formation
                formationLoose = false;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds an NPC to the group
        /// </summary>
        public void AddMember(NPCController npc)
        {
            if (!groupMembers.Contains(npc))
            {
                groupMembers.Add(npc);
                npc.groupManager = this;
                CalculateFormationOffsets();
            }
        }

        /// <summary>
        /// Removes an NPC from the group
        /// </summary>
        public void RemoveMember(NPCController npc)
        {
            if (groupMembers.Contains(npc))
            {
                groupMembers.Remove(npc);
                formationOffsets.Remove(npc);
                npc.groupManager = null;
                
                // Select new leader if current leader was removed
                if (npc == leader && groupMembers.Count > 0)
                {
                    leader = groupMembers[0];
                    leader.isGroupLeader = true;
                }
                
                CalculateFormationOffsets();
            }
        }

        /// <summary>
        /// Sets a new leader for the group
        /// </summary>
        public void SetLeader(NPCController newLeader)
        {
            if (groupMembers.Contains(newLeader))
            {
                if (leader != null)
                {
                    leader.isGroupLeader = false;
                }
                
                leader = newLeader;
                leader.isGroupLeader = true;
                CalculateFormationOffsets();
            }
        }

        /// <summary>
        /// Issues a movement command to the entire group
        /// </summary>
        public void IssueGroupMovement(Vector3 targetPosition)
        {
            if (leader != null)
            {
                leader.InvestigatePosition(targetPosition);
            }
        }

        #endregion

        // Visualization
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || leader == null) return;

            // Draw formation positions
            Gizmos.color = Color.cyan;
            foreach (var kvp in formationOffsets)
            {
                if (kvp.Key == null) continue;
                
                Vector3 formationPos = leader.transform.position + leader.transform.TransformDirection(kvp.Value);
                Gizmos.DrawWireSphere(formationPos, 0.3f);
                Gizmos.DrawLine(kvp.Key.transform.position, formationPos);
            }

            // Draw leader
            if (leader != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(leader.transform.position + Vector3.up * 2f, 0.5f);
            }
        }
    }
}
