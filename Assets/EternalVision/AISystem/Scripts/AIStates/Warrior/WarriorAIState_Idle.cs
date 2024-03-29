using EternalVision.EntitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class WarriorAIState_Idle : AIState
{
        [Header("Idle Time Range")]
        [SerializeField] private float m_minIdleTime;
        [SerializeField] private float m_maxIdleTime;
        [SerializeField] private float m_minDistance;
        [SerializeField] private float m_maxDistance;

        private bool m_isIdleFinish;
        private float m_idleTime;
        private float m_timer;

        private PossessedAI_Warrior m_possessedWarrior;

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
            m_idleTime = Random.Range(m_minIdleTime, m_maxIdleTime);
            m_timer = 0f;
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();

            if (m_possessedAI.attackerOnThisOwner != null && !m_possessedAI.entityBrain.isHit)
            {
                return m_possessedAI.GetAIState(AIStateEnum.BlockAttack);
            }
            else
                m_possessedAI.SetAttackOnOwner(null);

            if (m_possessedAI.currentTarget.targetType == AITargetType.Enemy && m_possessedAI.currentTarget.targetTransform != null)
            {
                Vector3 dir = (m_possessedAI.currentTarget.targetTransform.position - transform.position).normalized;
                dir.y = 0f;

                m_possessedWarrior.entityBrain.RotateEntity(dir, 5f);

                if (m_possessedWarrior.currentSelectedAITargetOrganizer != null && !m_possessedWarrior.currentSelectedAITargetOrganizer.IsAttacker(m_possessedAI))
                {
                    if (m_possessedAI.currentTarget.targetDistance <= m_minDistance ||
                        m_possessedAI.currentTarget.targetDistance >= m_maxDistance)
                    {
                        return m_possessedAI.GetAIState(AIStateEnum.StayInRange);
                    }
                }
            }

            if (m_isIdleFinish)
            {
                m_isIdleFinish = false;
                if (m_possessedAI.currentTarget.targetTransform == null)
                    return m_possessedAI.GetAIState(AIStateEnum.Patrol);

                if (m_possessedAI.currentTarget.targetType == AITargetType.Enemy
                && m_possessedAI.currentTarget.targetTransform != null)
                {
                    if (m_possessedWarrior.currentSelectedAITargetOrganizer != null)
                    {
                        if (m_possessedWarrior.currentSelectedAITargetOrganizer.IsAttacker(m_possessedAI))
                        {
                            return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
                        }
                    }
                }

                return m_possessedAI.GetAIState(AIStateEnum.StayInRange);
            }

            if(m_timer < m_idleTime)
            {
                m_timer += Time.deltaTime;
            }
            else
            {
                m_isIdleFinish = true;
            }

            return this;
        }

    }
}

