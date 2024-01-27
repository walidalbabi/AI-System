using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace EternalVision.AI
{
    public class AIState_Pursuit : AIState
    {
        [Range(0f, 5f)][SerializeField] private float _minDistanceToStartWalking;
        [Range(1f, 3f)][SerializeField] private float _moveSpeed;
        [Range(1f, 3f)][SerializeField] private float _defaultWalkSpeed;
        [Range(10f, 360f)][SerializeField] private float _rotateSpeed;
        [SerializeField] private float _stoppingDistance;


        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = _stoppingDistance;
            m_possessedAI.navMeshAgent.speed = _moveSpeed;
            m_possessedAI.navMeshAgent.angularSpeed = _rotateSpeed;
            m_possessedAI.SetFOVAngleToTheAlertedState();
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();
            SetAIDestination();

            m_possessedAI.HandleMoveToTarget();

            if (m_possessedAI.currentTarget.targetDistance <= m_possessedAI.navMeshAgent.stoppingDistance && m_possessedAI.currentTarget.targetType != AITargetType.Enemy)
            {
                //Must Investigate
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            }
            else
            {
                if (m_possessedAI.currentTarget.targetDistance <= m_possessedAI.navMeshAgent.stoppingDistance && m_possessedAI.currentTarget.targetType == AITargetType.Enemy)
                {
                    return m_possessedAI.GetAIState(AIStateEnum.Attack);
                }
            }


            if (m_possessedAI.currentTarget.targetDistance <= _minDistanceToStartWalking) m_possessedAI.navMeshAgent.speed = _defaultWalkSpeed;
            else m_possessedAI.navMeshAgent.speed = _moveSpeed;


            return this;
        }


        private void SetAIDestination()
        {
            m_possessedAI.navMeshAgent.SetDestination(m_possessedAI.currentTarget.targetPos);
        }
    }
}

