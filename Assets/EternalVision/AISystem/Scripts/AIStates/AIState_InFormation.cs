using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class AIState_InFormation : AIState
    {
        [SerializeField, Tooltip("Sould be same value as for the pursuit stop distance")] private float _closeRangeAttack;
        [SerializeField] private float m_minCooldownTime;
        [SerializeField] private float m_maxCooldownTime;

        [Range(0f, 5f)][SerializeField] private float m_minDistanceToStartWalking;
        [Range(1f, 3f)][SerializeField] private float m_moveSpeed;
        [Range(1f, 3f)][SerializeField] private float m_defaultWalkSpeed;
        [Range(10f, 360f)][SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_stoppingDistance;

        private Transform m_formationSlotTransform;
        private bool m_canAttack;
        private float m_cooldown;

        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = m_stoppingDistance;
            m_possessedAI.navMeshAgent.speed = m_moveSpeed;
            m_possessedAI.navMeshAgent.angularSpeed = m_rotateSpeed;
            m_possessedAI.SetFOVAngleToTheAlertedState();
            m_formationSlotTransform = null;
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();

            if (m_possessedAI.aiFormation == null) return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_formationSlotTransform == null)
                m_formationSlotTransform = m_possessedAI.aiFormation.GetMySlot(m_possessedAI);

            m_possessedAI.HandleMoveToTarget();

            if (m_possessedAI.currentTarget.targetDistance > m_possessedAI.navMeshAgent.stoppingDistance)
            {
                //Must Investigate
                SetAIDestination(m_formationSlotTransform.position);
            }

            //Target we have only the last scene pos of the target
            if (m_possessedAI.currentTarget.targetDistance <= _closeRangeAttack)
            {
                CalculateAttackCooldown();
                HandleAttack();
            }

            if (m_possessedAI.entityBrain.entityAnimator.blockAttack)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_possessedAI.entityBrain.isHit || m_possessedAI.entityBrain.isDizzy)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_possessedAI.navMeshAgent.remainingDistance <= m_minDistanceToStartWalking) m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;
            else m_possessedAI.navMeshAgent.speed = m_moveSpeed;


            return this;
        }

        private void CalculateAttackCooldown()
        {
            if (m_cooldown > 0)
            {
                m_cooldown -= Time.deltaTime;
            }
        }
        private void SetCooldowm()
        {
            m_cooldown = Random.Range(m_minCooldownTime, m_maxCooldownTime);
        }
        private void HandleAttack()
        {
            if (m_possessedAI.currentTarget.targetDistance <= _closeRangeAttack && CanAttack())
            {
                //Perform a close Range Attack
                SetCooldowm();
                m_possessedAI.entityBrain.UsePrimaryItemFirstAction();
            }
        }

        private bool CanAttack()
        {
            m_canAttack = true;

            if (m_possessedAI.entityBrain.isPerformingAttack) m_canAttack = false;
            if (m_possessedAI.entityBrain.entityAnimator.blockAttack) m_canAttack = false;
            if (m_cooldown > 0) m_canAttack = false;

            return m_canAttack;
        }
    }
}


