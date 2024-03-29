using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace EternalVision.AI
{

    public class WarriorAIState_StayInRange : AIState
    {
        [Range(0f, 5f)][SerializeField] private float m_minDistanceToStartWalking;
        [Range(1f, 10f)][SerializeField] private float m_moveSpeed;
        [Range(1f, 3f)][SerializeField] private float m_defaultWalkSpeed;
        [Range(10f, 360f)][SerializeField] private float m_rotateSpeed;
        [SerializeField] private float m_minDistance = 0f;
        [SerializeField] private float m_maxDistance = 0f;


        private float m_timer;
        private float m_maxTimeBeforeSwitchingToAnotherState;
        private int m_randomStrafe = 0;
        private bool m_canStrafe;

        private PossessedAI_Warrior m_possessedWarrior;
        private PossessedAI_Warrior m_targetPossessedWarrior;

        public override void SetAIManagerComponent(PossessedAI component)
        {
            base.SetAIManagerComponent(component);
            m_possessedWarrior = component.GetComponent<PossessedAI_Warrior>();
        }
        public override void OnEnterState()
        {
            m_possessedAI.navMeshAgent.stoppingDistance = 0;
            m_possessedAI.navMeshAgent.speed = 0;
            m_possessedAI.navMeshAgent.angularSpeed = m_rotateSpeed;
            m_timer = 0f;
            m_canStrafe = false;
            m_maxTimeBeforeSwitchingToAnotherState = Random.Range(0f, 6f);
            m_randomStrafe = Random.Range(-1, 2);
            m_possessedAI.SetFOVAngleToTheAlertedState();
            m_possessedAI.navMeshAgent.updateRotation = false;
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();
            m_possessedAI.HandleMoveToTarget();

            if(m_possessedAI.currentTarget.targetTransform != null)
            {
                Vector3 dir = (m_possessedAI.currentTarget.targetTransform.position - transform.position).normalized;
                dir.y = 0f;

                m_possessedWarrior.entityBrain.RotateEntity(dir, 5f);
            }

            if (m_possessedAI.entityBrain.isDizzy)
                return m_possessedAI.GetAIState(AIStateEnum.Idle);

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
            else
            {
                return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }


            if(EntitySystem.EntityManager.instance.FindOrganizer(m_possessedAI) != null)
            {
                if (EntitySystem.EntityManager.instance.FindOrganizer(m_possessedAI).listedAttackers.Count > 0)
                    return m_possessedAI.GetAIState(AIStateEnum.Idle);
            }

            if (m_possessedAI.currentTarget.targetDistance >= m_maxDistance)
            {
                FollowTarget();
            }
            else if (m_possessedAI.currentTarget.targetDistance <= m_minDistance)
            {
                GoBackwardFacingTarget();
            }
            else
            {
                if (m_canStrafe)
                {
                    if (m_randomStrafe == 0)
                    {
                        m_possessedAI.navMeshAgent.speed = 0f;
                    }
                    else if (m_randomStrafe == 1)
                    {
                        StrafeRight();
                    }
                    else if (m_randomStrafe == -1)
                    {
                        StrafeLeft();
                    }

                }
            }

            m_timer += Time.deltaTime;

            if (m_timer >= m_maxTimeBeforeSwitchingToAnotherState / 3f) m_canStrafe = true;

            if (m_timer > m_maxTimeBeforeSwitchingToAnotherState) return m_possessedAI.GetAIState(AIStateEnum.Idle);

            return this;
        }

        private void FollowTarget()
        {
            SetAIDestination(m_possessedAI.currentTarget.targetPos);

            if (m_possessedAI.currentTarget.targetDistance <= m_minDistanceToStartWalking)
                m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;
            else
                m_possessedAI.navMeshAgent.speed = m_moveSpeed;

            m_timer = 0f;
        }
        private void GoBackwardFacingTarget()
        {
            // Calculate the direction from the AI to the target
            Vector3 directionToTarget = m_possessedAI.currentTarget.targetPos - transform.position;

            // Move backward facing the target
            SetAIDestination(transform.position - directionToTarget.normalized);

            // Adjust the AI's speed based on the distance to the target
            if (m_possessedAI.currentTarget.targetDistance <= m_minDistanceToStartWalking)
                m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;
            else
                m_possessedAI.navMeshAgent.speed = m_moveSpeed;

            m_timer = 0f;
        }
        private void StrafeRight()
        {
            // Move backward facing the target
            SetAIDestination(transform.position + transform.right + (transform.forward * .1f));
            m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;

            if (m_possessedAI.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial ||
                m_possessedAI.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                m_possessedAI.navMeshAgent.speed = 0;
        }
        private void StrafeLeft()
        {
            // Move backward facing the target
            SetAIDestination(transform.position + (transform.right * -1f) + (transform.forward * .1f));
            // Adjust the AI's speed based on the distance to the target
            m_possessedAI.navMeshAgent.speed = m_defaultWalkSpeed;

            if (m_possessedAI.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial ||
                m_possessedAI.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
                m_possessedAI.navMeshAgent.speed = 0;
        }

    }

}

