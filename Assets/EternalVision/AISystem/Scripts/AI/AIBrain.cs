using EternalVision.EntitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace EternalVision.AI
{
    public class AIBrain : EntityBrain
    {

        //Private
        private Vector2 m_velocity;

        private PossessedAI m_possessedAI;

        //Public
        public Action<float> OnAttackPlayer; // Attack Cooldown 


        #region Unity

        protected override void Awake()
        {
            base.Awake();

            m_possessedAI = GetComponent<PossessedAI>();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void Update()
        {
            base.Update();
        }

        #endregion Unity

        #region Functions

        public override void SetMoveAnimationValue(Vector3 dir, float moveVelocity, bool isSprinting , bool canMove)
        {
            base.SetMoveAnimationValue(dir, moveVelocity, isSprinting, canMove);

            dir.y = 0;
            float dx = Vector3.Dot(transform.right, dir);
            float dy = Vector3.Dot(transform.forward, dir);

            Vector2 deltaPosition = new Vector2(dx, dy);

            m_velocity = deltaPosition / Time.deltaTime;

            if (m_possessedAI.navMeshAgent.remainingDistance <= m_possessedAI.navMeshAgent.stoppingDistance)
                m_velocity = Vector2.Lerp(Vector2.zero, m_velocity, m_possessedAI.navMeshAgent.remainingDistance / m_possessedAI.navMeshAgent.stoppingDistance);

            if(m_velocity.magnitude >= 0.01f && m_possessedAI.navMeshAgent.remainingDistance > m_possessedAI.navMeshAgent.radius)
            {
                m_entityAnimator.SetFloat("Horizontal", m_velocity.x, .08f);
                m_entityAnimator.SetFloat("Vertical", m_velocity.y, .08f);
            }
            else
            {
                m_entityAnimator.SetFloat("Horizontal", 0f, .2f);
                m_entityAnimator.SetFloat("Vertical", 0f, .2f);
            }
        }

        public override void PerformAttack(string AttackClipName, int attackIndex, float cooldown)
        {
            base.PerformAttack(AttackClipName, attackIndex, cooldown);

            if (!CanPerformAttack()) return;

            if (m_possessedAI.currentTarget.targetTransform == null)
            {
                return;
            }
            else
            {
                //Vector3 targetPos = m_possessedAI.currentTarget.targetTransform.position;
                //targetPos.y = transform.position.y;

                //Vector3 dir = (targetPos - transform.position).normalized;
                //RotateEntity(dir, 1f);
                if (m_possessedAI.currentTarget.targetTransform.GetComponent<ThirdPersonPlayerBrain>())
                    OnAttackPlayer?.Invoke(cooldown);
            }

            m_entityAnimator.SetInt(AttackClipName, attackIndex);
            m_entityAnimator.SetTrigger("Attack");
        }
        public override void ToggelBlock()
        {
            base.ToggelBlock();

            if (!CanBlockAttack())
            {
                m_isBlockingAttack = false;
                m_entityAnimator.SetBool("Blocking", false);
                return;
            }
            m_isBlockingAttack = !m_isBlockingAttack;
            m_entityAnimator.SetBool("Blocking", m_isBlockingAttack);
        }

        public override bool UsePrimaryItemFirstAction()
        {
            if (m_entityInventory.currentEquipedItem.UseItemFirstAction())
            {
                //Play Animatiom
                Debug.Log("Attack");
                return true;
            }

            //Failed To Use Item
            return true;
        }
        public override bool UsePrimaryItemSecondAction()
        {
            if (m_entityInventory.currentEquipedItem.UseItemSecondAction())
            {
                //Play Animatiom
                Debug.Log("Block");
                return true;
            }

            //Failed To Use Item
            return true;
        }

        #endregion Functions
    }

}
