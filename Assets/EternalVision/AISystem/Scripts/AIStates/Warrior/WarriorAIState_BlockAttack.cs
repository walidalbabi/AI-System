using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EternalVision.AI
{
    public class WarriorAIState_BlockAttack : AIState
    {
        [SerializeField] [Range(0f, 100f)] private float m_blockingPercentage;

        private PossessedAI_Warrior m_possessedWarrior;

        private float m_timer;
        private float m_generatedBlockingPercentage;
        private bool m_isBlocking;

        public override void SetAIManagerComponent(PossessedAI component)
        {
            base.SetAIManagerComponent(component);
            m_possessedWarrior = component.GetComponent<PossessedAI_Warrior>();
        }
        public override void OnEnterState()
        {
            if (m_possessedAI.entityBrain.entityAnimator != null)
            {
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Vertical", 0f, .1f);
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Horziontal", 0f, .1f);
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Speed", 0f, .1f);
            }
            m_possessedAI.navMeshAgent.speed = 0f;
            m_timer = 0f;
            m_generatedBlockingPercentage = Random.Range(0, 101f);
            m_isBlocking = false;
            m_possessedAI.navMeshAgent.updateRotation = false;
        }
        public override void OnExitState()
        {
            if (m_isBlocking)
                m_isBlocking = !m_possessedAI.entityBrain.UsePrimaryItemSecondAction();
        }

        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();

            if (m_possessedAI.entityBrain.isHit || m_possessedAI.entityBrain.isDizzy)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_generatedBlockingPercentage <= m_blockingPercentage)
            {
                //Block Attack
                PerformBlocking();

                if (m_possessedAI.attackerOnThisOwner == null)
                {
                    if (m_timer >= 1f)
                        return m_possessedAI.GetAIState(AIStateEnum.Idle);
                }
                else
                {
                    if (m_timer >= 2f)
                    {
                        m_possessedAI.SetAttackOnOwner(null);
                        return m_possessedAI.GetAIState(AIStateEnum.Idle);
                    }

                }

                m_timer += Time.deltaTime;

            }else
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            return this;
        }


        private void PerformBlocking()
        {
            if (!m_isBlocking)
            {
                m_isBlocking = m_possessedAI.entityBrain.UsePrimaryItemSecondAction();
            }
        }

    }

}