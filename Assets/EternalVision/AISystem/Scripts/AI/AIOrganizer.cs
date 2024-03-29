using EternalVision.EntitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.EventSystems.EventTrigger;

namespace EternalVision.AI
{
    public class AIOrganizer : MonoBehaviour
    {
        //Inspector
        [SerializeField] private int m_attackersNumber = 3;
        [SerializeField] private float m_reRollmaxTime = 7f;

        //Private
        private float m_timer = 0;
        private AITarget m_currentTarget;
        private PossesedEntity m_owner;

        private List<PossessedAI> m_listedEntities = new List<PossessedAI>();
        private List<PossessedAI> m_listedAttackers = new List<PossessedAI>();
        private List<PossessedAI> m_previouseListedAttackers = new List<PossessedAI>();

        //Properties
        public AITarget currentTarget => m_currentTarget;
        public List<PossessedAI> listedAttackers => m_listedAttackers;

        private void Awake()
        {
            WarriorHealth.OnEntityDie += OnEntityDie;
        }
        private void Update()
        {
            if (m_currentTarget.targetTransform == null) return;

            if (m_timer > 0)
                m_timer -= Time.deltaTime;
            else
            {
                m_timer = m_reRollmaxTime;
                RollForAttackers();
            }
        }
        private void OnDestroy()
        {
            EntityManager.instance.RemoveAIOrganizer(m_owner);

            WarriorHealth.OnEntityDie -= OnEntityDie;
        }


        public void SetOwner(PossesedEntity owner)
        {
            m_owner = owner;
        }
        public void SetTarget(AITarget target)
        {
            m_currentTarget = target;
        }

        public void ListEntity(PossessedAI entity)
        {
            if(!m_listedEntities.Contains(entity))
                m_listedEntities.Add(entity);
        }
        public void UnlistEntity(PossessedAI entity)
        {
            if (m_listedEntities.Contains(entity))
                m_listedEntities.Remove(entity);

            UnlistAttacker(entity);

            if (m_listedEntities.Count <= 0 || m_currentTarget.targetTransform == null)
                Destroy(gameObject);
        }
        public void ListAttacker(PossessedAI entity)
        {
            if (!m_listedAttackers.Contains(entity))
                m_listedAttackers.Add(entity);
        }
        public void UnlistAttacker(PossessedAI entity)
        {
            if (m_listedAttackers.Contains(entity))
                m_listedAttackers.Remove(entity);
        }

        public void RollForAttackers()
        {
            if (m_owner == null || m_owner.entityBrain.entityHealth.isDead)
                Destroy(gameObject);

            EternalVision.GlobalHelper.GlobalSystemHelper.ShuffleList(m_listedEntities);

            m_previouseListedAttackers.Clear();

            foreach (var attacker in m_listedAttackers)
            {
                m_previouseListedAttackers.Add(attacker);
            }
           
            m_listedAttackers.Clear();

            int i = 0;
            while (i < m_listedEntities.Count && m_listedAttackers.Count < m_attackersNumber)
            {
                PossessedAI element = m_listedEntities[i];
                if (!m_listedAttackers.Contains(element) && !element.entityBrain.entityHealth.isDead)
                {
                    if (!m_previouseListedAttackers.Contains(m_listedEntities[i]))
                    {
                        m_listedAttackers.Add(element);
                    }

                }
                i++;
            }
        }

        private void OnEntityDie(EntityBrain brain)
        {
            if (brain.GetType() != typeof(PossessedAI)) return;

            UnlistEntity((PossessedAI)brain.possessedEntity);
            if (m_owner == brain.possessedEntity)
                Destroy(gameObject);
        }

        public bool IsAttacker(PossessedAI entity)
        {
            return m_listedAttackers.Contains(entity);
        }

    }
}

