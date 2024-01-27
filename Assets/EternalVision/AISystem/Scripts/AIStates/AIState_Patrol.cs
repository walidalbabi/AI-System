using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



namespace EternalVision.AI
{
    public class AIState_Patrol : AIState
    {

        [Range(1f, 3f)][SerializeField] private float _moveSpeed;
        [Range(10f, 360f)][SerializeField] private float _rotateSpeed;
        [SerializeField] private float _stoppingDistance;

        private float _distanceToTarget;


        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = _stoppingDistance;
            //Set Speed
            m_possessedAI.navMeshAgent.speed = _moveSpeed;
            m_possessedAI.navMeshAgent.angularSpeed = _rotateSpeed;
            //Get A random Point
            m_possessedAI.SetRandomPointForPatrol();

            m_possessedAI.SetFOVAngleToNormal();
        }
        public override AIState Tick()
        {
            _distanceToTarget = Vector3.Distance(transform.position, m_possessedAI.currentTarget.targetPos);

            SetAIDestination();
            m_possessedAI.FindATargerViaLineOfSight();
            m_possessedAI.HandleMoveToTarget();

            if (m_possessedAI.currentTarget.targetType == AITargetType.Enemy)
            {
                return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
            }

            if (_distanceToTarget <= m_possessedAI.navMeshAgent.stoppingDistance || m_possessedAI.navMeshAgent.isPathStale || m_possessedAI.navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }
            return this;
        }

        private void SetAIDestination()
        {
            m_possessedAI.navMeshAgent.SetDestination(m_possessedAI.currentTarget.targetPos);
        }

    }
}

