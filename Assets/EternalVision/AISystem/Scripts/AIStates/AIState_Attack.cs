using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class AIState_Attack : AIState
    {
        [SerializeField, Tooltip("Sould be same value as for the pursuit stop distance")] private float _closeRangeAttack;
        [SerializeField] private float m_minCooldownTime;
        [SerializeField] private float m_maxCooldownTime;

        private bool m_canAttack;
        private float m_cooldown;


        public override void OnEnterState()
        {
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Vertical", 0f, .1f);
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Horziontal", 0f, .1f);
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Speed", 0f, .1f);
            m_possessedAI.navMeshAgent.speed = 0f;
            m_possessedAI.SetFOVAngleToTheAlertedState();
        }
        public override AIState Tick()
        {
            CalculateAttackCooldown();
            //FaceTheTarget();
            m_possessedAI.FindATargerViaLineOfSight();

            if (m_possessedAI.currentTarget.targetTransform == null)
            {
                Debug.Log("AI target is lost");
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }

            //Target is Lost
            if (m_possessedAI.currentTarget.targetType != AITargetType.Enemy)
            {
                Debug.Log("AI target isn't an enemy");
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }
            //Target we have only the last scene pos of the target
            if (m_possessedAI.currentTarget.targetDistance > _closeRangeAttack)
            {
                Debug.Log("AI Lost Target Tracking");
                return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
            }
            else
            {
                HandleAttack();
            }



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
