using EternalVision.EntitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace EternalVision.AI
{
    [RequireComponent(typeof(PossessedAI))]
    public class AIWarrior : EntityBrain
    {
        //Private
        private Vector2 m_velocity;

        private PossessedAI m_possessedAI;



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

        public override void SetMoveAnimationValue(Vector3 dir, float moveVelocity, bool isSprinting)
        {
            base.SetMoveAnimationValue(dir, moveVelocity, isSprinting);

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
                Vector3 targetPos = m_possessedAI.currentTarget.targetTransform.position;
                targetPos.y = transform.position.y;

                Vector3 dir = (targetPos - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }

            m_entityAnimator.SetInt(AttackClipName, attackIndex);
            m_entityAnimator.SetTrigger("Attack");
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
            return true;
        }

        #endregion Functions
    }

}
