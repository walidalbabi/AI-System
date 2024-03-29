using EternalVision.EntitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EternalVision.AI
{
    public enum AIStateEnum { Idle, Patrol, Pursuit, Attack, BlockAttack, Investigate, StayInRange, InFormation }
    public enum AITargetType { none, Enemy, Noise, Point }

    /// <summary>
    /// Struct Represent The target That the AI is seeing
    /// </summary>
    [System.Serializable]
    public struct AITarget
    {
        public Transform targetTransform { get; private set; }
        public Vector3 targetPos { get; private set; }
        public float targetDistance { get; private set; }
        public AITargetType targetType { get; private set; }


        //Public Setters
        public void Settarget(Transform targetTr, Vector3 pos, float distance, AITargetType type)
        {
            targetTransform = targetTr;
            targetPos = pos;
            targetDistance = distance;
            targetType = type;
        }

        public void SetTargetTransform(Transform targetTr)
        {
            targetTransform = targetTr;
        }

        public void SetTargetPos(Vector3 pos)
        {
            targetPos = pos;
            Debug.Log("Target Pos is updated " + targetPos);
        }

        public void SetTargetDistance(float distance)
        {
            targetDistance = distance;
        }

        public void SetTargetPos(AITargetType type)
        {
            targetType = type;
        }

        public void Cleartarget()
        {
            targetTransform = null;
            //    Debug.Log("AI: Target Cleared");
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class PossessedAI : PossesedEntity
    {

        #region Variables

        //Inspector
        [Header("AI Settings")]
        [SerializeField] protected AIState m_startingState;
        [SerializeField] protected bool m_rootMotion;
        [Header("Detection")]
        [SerializeField] protected LayerMask m_detectionLayer;
        [SerializeField] protected LayerMask m_ignoredLineOfSightLayer;
        [SerializeField] protected float m_detectionRadius;
        [SerializeField][Range(-360, 0)] protected float m_minFieldOfViewAngle;
        [SerializeField][Range(0, 360)] protected float m_maxFieldOfViewAngle;
        [SerializeField][Range(-360, 0)] protected float m_minFieldOfViewAngleOnHavingTarget;
        [SerializeField][Range(0, 360)] protected float m_maxFieldOfViewAngleOnHavingTarget;
        [SerializeField] protected float m_looseSightTime = 3f;

        [Header("References")]
        [SerializeField] protected Transform m_playerHeadTransform;

        [Header("Others")]
        [SerializeField] protected bool m_inverseForward;

        //Private
        private AIState m_currentState;
        private AIState m_nextState;

        private float m_minFOVAngle;
        private float m_maxFOVAngle;

        private Vector2 m_smoothDeltaPos;
        private Vector3 m_velocity;

        private IEnumerator m_clearTargetCoroutine;

        private PossesedEntity m_attackerOnThisOwner;

        //Protected
        protected AITarget m_currentTarget;

        protected List<Transform> m_targetsToFilter = new List<Transform>();
        protected List<AIState> m_allAIStates = new List<AIState>();

        protected AIFormationManager m_aiFormation;

        protected NavMeshAgent m_navMeshAgent;


        //Properties
        public AITarget currentTarget => m_currentTarget;
        public NavMeshAgent navMeshAgent => m_navMeshAgent;
        public AIFormationManager aiFormation => m_aiFormation;
        public PossesedEntity attackerOnThisOwner => m_attackerOnThisOwner;

        #endregion Variables

        #region Unity

        protected override void Awake()
        {
            base.Awake();

            //Set Starter State
            m_currentState = m_startingState;
            m_navMeshAgent = GetComponent<NavMeshAgent>();

            ToggleRootMotion(m_rootMotion);
        }
        protected override void Start()
        {
            base.Start();

            //Get All AI States on this Gameobject And Store Them in a List
            var states = GetComponents<AIState>();
            foreach (var state in states)
            {
                if (!m_allAIStates.Contains(state))
                    m_allAIStates.Add(state);
            }

            //Set AI Manager In All AIStates Scripts
            foreach (var state in m_allAIStates)
            {
                state.SetAIManagerComponent(this);
            }


            m_entityBrain.entityHealth.OnEntityGotHit += ResetAttackerOnOwner;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_entityBrain.entityHealth.OnEntityGotHit -= ResetAttackerOnOwner;
        }
        protected override void Update()
        {
            base.Update();

            HandleState();

            Vector3 direction = m_navMeshAgent.nextPosition - transform.position;
            if (m_navMeshAgent.enabled)
                m_entityBrain.SetMoveAnimationValue(direction, m_navMeshAgent.speed, m_navMeshAgent.speed >= 3, true);
        }
        protected void LateUpdate()
        {
            if (m_currentTarget.targetPos != Vector3.zero)
                m_currentTarget.SetTargetDistance(Vector3.Distance(transform.position, m_currentTarget.targetPos));
        }
        protected void OnAnimatorMove()
        {
          //  if (m_tickPassedForCheckingTargets % EntityManager.instance.tickrate != 0) return;

            if (m_rootMotion || m_entityBrain.isPerformingAttack || m_entityBrain.isHit || entityBrain.entityAnimator.rootMotion)
            {
                Vector3 rootPosition = m_entityBrain.entityAnimator.animator.rootPosition;
                rootPosition.y = m_navMeshAgent.nextPosition.y;
                MoveEntity(rootPosition);
            }
            else
                MoveEntity(m_navMeshAgent.nextPosition); // Important for 2d locomotion
        }
        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_detectionRadius);

            Gizmos.color = Color.red;
            float rayRange = 10.0f;
            float halfFOV = m_maxFieldOfViewAngle / 2.0f;
            float coneDirection = 90;

            Quaternion upRayRotation = Quaternion.AngleAxis(-halfFOV + coneDirection, m_inverseForward == false ? Vector3.up : Vector3.down);
            Quaternion downRayRotation = Quaternion.AngleAxis(halfFOV + coneDirection, m_inverseForward == false ? Vector3.up : Vector3.down);

            Vector3 upRayDirection = upRayRotation * transform.right * rayRange;
            Vector3 downRayDirection = downRayRotation * transform.right * rayRange;

            Gizmos.DrawRay(transform.position + Vector3.up, upRayDirection + Vector3.up);
            Gizmos.DrawRay(transform.position + Vector3.up, downRayDirection + Vector3.up);
            // Gizmos.DrawLine(transform.position + Vector3.up + downRayDirection, transform.position + Vector3.up + upRayDirection);

            Gizmos.color = Color.yellow;
            //  if (m_currentTarget.targetTransform != null)
            Gizmos.DrawSphere(currentTarget.targetPos, .5f);
        }

        #endregion Unity

        #region Functions

        public void ToggleRootMotion(bool state)
        {
            m_rootMotion = state;

            if (m_rootMotion)
            {
                if (m_entityBrain.entityAnimator != null)
                    m_entityBrain.entityAnimator.animator.applyRootMotion = true;

                m_navMeshAgent.updatePosition = false;
                m_navMeshAgent.updateRotation = true;
            }
            else
            {
                if (m_entityBrain.entityAnimator != null)
                    m_entityBrain.entityAnimator.animator.applyRootMotion = false;

                m_navMeshAgent.updatePosition = true;
                m_navMeshAgent.updateRotation = true;
            }

            m_navMeshAgent.updatePosition = false; // Important for 2d locomotion
        }

        private void HandleState()
        {
            if (m_currentState != null)
            {
                if (m_nextState != m_currentState)
                {
                    m_currentState.OnEnterState();
                    if (m_nextState != null) m_nextState.OnExitState();
                    m_nextState = m_currentState;
                   // Debug.Log(m_currentState.currentState);
                }

                //Run State Logic
                m_currentState = m_currentState.Tick();
            }
            else
            {
                m_currentState = GetAIState(AIStateEnum.Idle);
            }
        }

        public void HandleMoveToTarget()
        {
            if (m_navMeshAgent.enabled && m_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || m_navMeshAgent.isPathStale)
            {
                if (entityBrain.entityAnimator.animator != null)
                {
                    m_entityBrain.SetMoveAnimationValue(Vector3.zero, 0, false, true);
                }
                return;
            }

            if (m_rootMotion)
            {
                //Vector3 worldDeltaPosition = (m_navMeshAgent.nextPosition.normalized) - transform.position;
                //worldDeltaPosition.y = 0;

                //float dx = Vector3.Dot(transform.right, worldDeltaPosition);
                //float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
                //Vector2 deltaPosition = new Vector2(dx, dy);

                //float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
                //m_smoothDeltaPos = Vector2.Lerp(m_smoothDeltaPos, deltaPosition, smooth);
                //m_velocity = m_smoothDeltaPos / Time.deltaTime;
                //if (m_navMeshAgent.remainingDistance <= m_navMeshAgent.stoppingDistance)
                //{
                //    m_velocity = Vector2.Lerp(Vector2.zero, m_velocity,
                //    m_navMeshAgent.remainingDistance / m_navMeshAgent.stoppingDistance);
                //}

                //if (entityBrain.entityAnimator.animator != null)
                //{
                //    entityBrain.entityAnimator.animator.SetFloat("Vertical", m_velocity.magnitude);
                //    //   m_possessedAI.anim.SetFloat("Horizontal",  velocity.y);
                //    entityBrain.entityAnimator.animator.SetFloat("Speed", m_navMeshAgent.speed);
                //}

                return;
            }
        }

        public virtual void FindATargerViaLineOfSight()
        {
            //Detect All Enteties in the radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_detectionRadius, m_detectionLayer);
            m_targetsToFilter.Clear();

            for (int i = 0; i < colliders.Length; i++)
            {
                //Check if we have a valid target
                if (colliders[i].gameObject != null)
                {
                    EntityBrain entityBrainTarget = colliders[i].gameObject.GetComponent<EntityBrain>();
                    if (entityBrainTarget == null) continue;
                    if (entityBrainTarget.entityHealth.isDead) continue;
                    //Check for factions
                    if (!IsTargetAnEnemy(entityBrainTarget)) continue;


                    //Check if he is in our field of view
                    Vector3 targetDir = transform.position - colliders[i].transform.position;
                    float viewableAngele = Vector3.Angle(targetDir, m_inverseForward == false ? transform.forward : transform.forward * -1f);

                    if (viewableAngele > m_minFOVAngle && viewableAngele < m_maxFOVAngle)
                    {
                        //Check if the target is Blocked by a wall ...
                        RaycastHit hit;
                        Vector3 startPoint = m_playerHeadTransform.position;
                        Vector3 endPoint = new Vector3(colliders[i].transform.position.x, colliders[i].transform.position.y + 1f, colliders[i].transform.position.z);

                        if (!Physics.Linecast(startPoint, endPoint, out hit, m_ignoredLineOfSightLayer))
                        {
                            m_targetsToFilter.Add(colliders[i].transform);
                        }

                    }
                }
            }

            //Filter Closest Target
            if(m_targetsToFilter.Count > 0)
            {
                Transform target = EternalVision.GlobalHelper.GlobalSystemHelper.GetClosestTransform(transform, m_targetsToFilter.ToArray());
                float distance = Vector3.Distance(transform.position, target.position);
                m_currentTarget.Settarget(target, target.position, distance, AITargetType.Enemy);
                m_currentSelectedTarget = target;
                SetClearTargetAfterDelay();
            }
            else
            {
                m_currentTarget.Cleartarget();
                m_currentSelectedTarget = null;
            }

        }
        private void SetClearTargetAfterDelay()
        {
            if (m_clearTargetCoroutine != null)
            {
                StopCoroutine(m_clearTargetCoroutine);
                m_clearTargetCoroutine = null;
            }
            m_clearTargetCoroutine = ClearTargetAfterDelay();
            StartCoroutine(m_clearTargetCoroutine);
        }
        private IEnumerator ClearTargetAfterDelay()
        {
            yield return new WaitForSeconds(m_looseSightTime);
            m_currentTarget.Cleartarget();
        }

        public void SetRandomPointForPatrol()
        {
            Vector3 target = GetRandomPointOnNavMesh();
            float dist = Vector3.Distance(transform.position, target);
            m_currentTarget.Settarget(null, target, dist, AITargetType.Point);
        }

        public void SetFOVAngleToNormal()
        {
            m_minFOVAngle = m_minFieldOfViewAngle;
            m_maxFOVAngle = m_maxFieldOfViewAngle;
        }
        public void SetFOVAngleToTheAlertedState()
        {
            m_minFOVAngle = m_minFieldOfViewAngleOnHavingTarget;
            m_maxFOVAngle = m_maxFieldOfViewAngleOnHavingTarget;
        }


        public override void MoveEntity(Vector3 dir)
        {
            transform.position = dir;
            m_navMeshAgent.nextPosition = dir;
        }
        protected override IEnumerator AddForceCoroutine(Vector3 dir)
        {
            m_navMeshAgent.enabled = false;
            yield return base.AddForceCoroutine(dir);
            m_navMeshAgent.enabled = true;
        }
        public void SetAttackOnOwner(PossesedEntity attacker)
        {
            m_attackerOnThisOwner = attacker;
        }

        public void AddAIToFormation(AIFormationManager targetAIFormation)
        {
            m_aiFormation = targetAIFormation;
        }
        #endregion Functions

        #region Callback

        private void ResetAttackerOnOwner(Transform transform)
        {
            m_attackerOnThisOwner = null;
        }

        #endregion Callback

        #region Getters

        protected Vector3 GetRandomPointOnNavMesh()
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, 1);
            Vector3 finalPosition = hit.position;
            return finalPosition;
        }
        public AIState GetAIState(AIStateEnum stateEnum)
        {
            foreach (var state in m_allAIStates)
            {
                if (state.currentState == stateEnum) return state;
            }

            return null;
        }

        public override bool IsMoving()
        {
            return m_navMeshAgent.velocity.sqrMagnitude > .1f;
        }

        #endregion Getters


    }

}