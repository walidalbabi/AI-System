using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EternalVision.EntitySystem;
using System;

namespace EternalVision.AI
{
    public class PossessedAI_Warrior : PossessedAI
    {
        private AIOrganizer m_currentSelectedAITargetOrganizer;


        public AIOrganizer currentSelectedAITargetOrganizer => m_currentSelectedAITargetOrganizer;


        #region Unity

        protected override void Awake()
        {
            base.Awake();

            WarriorHealth.OnEntityDie += OnDeath;

        }
        protected override void Start()
        {
            base.Start();

            m_entityBrain.entityHealth.OnEntityGotHit += SetTargetAgro;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            WarriorHealth.OnEntityDie -= OnDeath;
            m_entityBrain.entityHealth.OnEntityGotHit -= SetTargetAgro;
        }

        #endregion Unity

        #region Functions

        public override void FindATargerViaLineOfSight()
        {
            if (m_currentSelectedAITargetOrganizer == null)
                base.FindATargerViaLineOfSight();
            else
            {
                Transform target = m_currentSelectedAITargetOrganizer.currentTarget.targetTransform;
                float distance = Vector3.Distance(transform.position, target.position);

                if (distance < m_detectionRadius / 2)
                    m_currentTarget.Settarget(target, target.position, distance, AITargetType.Enemy);
                else base.FindATargerViaLineOfSight();
            }

            if (m_currentTarget.targetTransform != null)
            {
                if (m_currentTarget.targetTransform.GetComponent<EntityBrain>().entityHealth.isDead)
                {
                    m_currentTarget.Cleartarget();
                    return;
                }

                if (m_currentSelectedAITargetOrganizer == null
                    || EntityManager.instance.RequestAIOrganizer(m_currentTarget.targetTransform.GetComponent<PossesedEntity>(), this) != m_currentSelectedAITargetOrganizer)
                {
                    if (m_currentSelectedAITargetOrganizer != null) m_currentSelectedAITargetOrganizer.UnlistEntity(this);

                    m_currentSelectedAITargetOrganizer = EntityManager.instance.RequestAIOrganizer(m_currentTarget.targetTransform.GetComponent<PossesedEntity>(), this);

                    m_currentSelectedAITargetOrganizer.ListEntity(this);
                }
            }
            else
            {
                m_currentSelectedAITargetOrganizer = null;
            }
        }


        #endregion Functions

        #region Callbacks

        private void SetTargetAgro(Transform hitCauser)
        {
            if (!m_entityBrain.entityHealth.isDead)
            {
                if (hitCauser.GetComponent<EntityBrain>())
                    if (!IsTargetAnEnemy(hitCauser.GetComponent<EntityBrain>()))
                        return;

                m_currentTarget.Settarget(hitCauser, hitCauser.position, Vector3.Distance(transform.position, hitCauser.position), AITargetType.Enemy);
                if (m_currentSelectedAITargetOrganizer != null) m_currentSelectedAITargetOrganizer.UnlistEntity(this);
                m_currentSelectedAITargetOrganizer = EntityManager.instance.RequestAIOrganizer(m_currentTarget.targetTransform.GetComponent<PossesedEntity>(), this);
                m_currentSelectedAITargetOrganizer.ListEntity(this);
            }
        }
        private void OnDeath(EntityBrain entityOwner)
        {
            if (entityOwner != m_entityBrain) return;
            m_currentSelectedAITargetOrganizer = null;
        }

        #endregion Callbacks

        #region Getters

        public bool IsTarget(Transform entityTranform)
        {
            if(currentTarget.targetTransform != null)
            {
                if (entityBrain == currentTarget.targetTransform)
                    return true;
            }

            return false;
        }

        #endregion Getters
    }
}

