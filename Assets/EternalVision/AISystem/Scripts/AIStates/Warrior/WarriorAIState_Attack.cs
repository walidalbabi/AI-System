using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EternalVision.AI
{

    public class WarriorAIState_Attack : AIState
    {
        [SerializeField, Tooltip("Sould be same value as for the pursuit stop distance")] private float _closeRangeAttack;
        [SerializeField] private float m_minCooldownTime;
        [SerializeField] private float m_maxCooldownTime;

        private bool m_canAttack;
        private float m_cooldown;

        private PossessedAI_Warrior m_possessedWarrior;

        public override void SetAIManagerComponent(PossessedAI component)
        {
            base.SetAIManagerComponent(component);
            m_possessedWarrior = component.GetComponent<PossessedAI_Warrior>();
        }
        public override void OnEnterState()
        {
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Vertical", 0f, .1f);
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Horziontal", 0f, .1f);
            m_possessedAI.entityBrain.entityAnimator.SetFloat("Speed", 0f, .1f);
            m_possessedAI.navMeshAgent.speed = 0f;
            m_possessedAI.navMeshAgent.updateRotation = false;
            SetCooldowm();
            m_possessedAI.SetFOVAngleToTheAlertedState();
        }
        public override AIState Tick()
        {
            CalculateAttackCooldown();
            m_possessedAI.FindATargerViaLineOfSight();

            if(m_possessedAI.entityBrain.entityAnimator.blockAttack)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_possessedAI.entityBrain.isHit || m_possessedAI.entityBrain.isDizzy)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

            if (m_possessedWarrior.currentSelectedAITargetOrganizer != null)
            {
                if (!m_possessedWarrior.currentSelectedAITargetOrganizer.IsAttacker(m_possessedAI))
                    return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }
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
                Debug.Log("AI target is out of attack range");
                return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
            }
            else if (m_cooldown > 0)
            {
                HandleAttack();
            }
            else
            {
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }

            if (m_possessedAI.currentTarget.targetTransform != null)
            {
                Vector3 dir = (m_possessedAI.currentTarget.targetTransform.position - transform.position).normalized;
                dir.y = 0f;

                m_possessedWarrior.entityBrain.RotateEntity(dir, 1.5f);
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
                if (m_possessedAI.entityBrain.UsePrimaryItemFirstAction())
                    if (m_possessedAI.currentTarget.targetTransform != null)
                        if (m_possessedAI.currentTarget.targetTransform.GetComponent<PossessedAI>())
                            m_possessedAI.currentTarget.targetTransform.GetComponent<PossessedAI>().SetAttackOnOwner(m_possessedAI);
            }
        }

        private bool CanAttack()
        {
            m_canAttack = true;

            if (m_possessedAI.entityBrain.isPerformingAttack) m_canAttack = false;
            if (m_possessedAI.entityBrain.isHit) m_canAttack = false;
            if (m_possessedAI.entityBrain.isDizzy) m_canAttack = false;
            if (m_possessedAI.entityBrain.entityAnimator.blockAttack) m_canAttack = false;

            return m_canAttack;
        }
    }

}