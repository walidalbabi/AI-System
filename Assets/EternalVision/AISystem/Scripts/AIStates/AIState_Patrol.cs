using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



namespace EternalVision.AI
{
    public class AIState_Patrol : AIState
    {

        [Range(1f, 3f)][SerializeField] private float m_moveSpeed;
        [Range(10f, 360f)][SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_stoppingDistance;

        private float m_distanceToTarget;


        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = m_stoppingDistance;
            //Set Speed
            m_possessedAI.navMeshAgent.speed = m_moveSpeed;
            m_possessedAI.navMeshAgent.angularSpeed = m_rotateSpeed;
            m_possessedAI.navMeshAgent.updateRotation = true;
            //Get A random Point
            m_possessedAI.SetRandomPointForPatrol();

            m_possessedAI.SetFOVAngleToNormal();
        }
        public override AIState Tick()
        {
            m_distanceToTarget = Vector3.Distance(transform.position, m_possessedAI.currentTarget.targetPos);

            SetAIDestination();
            m_possessedAI.FindATargerViaLineOfSight();
            m_possessedAI.HandleMoveToTarget();

            if (m_possessedAI.aiFormation)
            {
                return m_possessedAI.GetAIState(AIStateEnum.InFormation);
            }

            if (m_possessedAI.currentTarget.targetType == AITargetType.Enemy)
            {
                return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
            }

            if (m_distanceToTarget <= m_possessedAI.navMeshAgent.stoppingDistance || m_possessedAI.navMeshAgent.isPathStale || m_possessedAI.navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }
            return this;
        }
    }
}

