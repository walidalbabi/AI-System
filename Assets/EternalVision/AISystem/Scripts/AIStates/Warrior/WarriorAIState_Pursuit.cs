using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class WarriorAIState_Pursuit : AIState
    {
        [Range(0f, 5f)][SerializeField] private float m_minDistanceToStartWalking;
        [Range(1f, 10f)][SerializeField] private float m_moveSpeed;
        [Range(1f, 3f)][SerializeField] private float m_defaultWalkSpeed;
        [Range(10f, 360f)][SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_stoppingDistance;


        private PossessedAI_Warrior m_possessedWarrior;


        public override void SetAIManagerComponent(PossessedAI component)
        {
            base.SetAIManagerComponent(component);
            m_possessedWarrior = component.GetComponent<PossessedAI_Warrior>();
        }
        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = m_stoppingDistance;
            m_possessedAI.navMeshAgent.speed = m_moveSpeed;
            m_possessedAI.navMeshAgent.angularSpeed = m_rotateSpeed;
            m_possessedAI.navMeshAgent.updateRotation = true;
            m_possessedAI.SetFOVAngleToTheAlertedState();
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();
            m_possessedAI.HandleMoveToTarget();

            SetAIDestination();

            if (m_possessedAI.currentTarget.targetTransform == null)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if(m_possessedWarrior.currentSelectedAITargetOrganizer != null)
            {
                if (!m_possessedWarrior.currentSelectedAITargetOrganizer.IsAttacker(m_possessedAI))
                    return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }

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


            if (m_possessedAI.currentTarget.targetDistance <= m_minDistanceToStartWalking) m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;
            else m_possessedAI.navMeshAgent.speed = m_moveSpeed;


            return this;
        }


    }

}