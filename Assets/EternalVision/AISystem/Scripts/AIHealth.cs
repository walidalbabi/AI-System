using EternalVision.EntitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class AIHealth : WarriorHealth
    {

        //Private
        private PossessedAI m_possessedAI;


        #region Unity

        protected override void Awake()
        {
            base.Awake();

            m_possessedAI = GetComponentInParent<PossessedAI>();
        }

        #endregion Unity

        #region Functions

        private void DeathTriggerAction()
        {
            m_possessedAI.navMeshAgent.enabled = false;
            m_possessedAI.enabled = false;
            m_possessedAI.GetComponent<Collider>().enabled = false;
            if (m_possessedAI.GetComponent<Rigidbody>())
                m_possessedAI.GetComponent<Rigidbody>().isKinematic = true;
        }

        #endregion Functions

        #region Callbacks

        protected override void OnDead(EntityBrain brain)
        {
            if (brain != m_entityBrain) return;

            base.OnDead(brain);

            DeathTriggerAction();
        }

        #endregion Callbacks

    }

}

