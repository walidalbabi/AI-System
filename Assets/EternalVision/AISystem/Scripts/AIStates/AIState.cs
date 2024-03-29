using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public abstract class AIState : MonoBehaviour
    {
        [SerializeField] protected AIStateEnum m_currentState;

        protected PossessedAI m_possessedAI;

        public AIStateEnum currentState => m_currentState;



        public virtual void SetAIManagerComponent(PossessedAI component) { m_possessedAI = component; }
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { }
        public virtual AIState Tick()
        {
            return this;
        }
        protected void SetAIDestination()
        {
            if (m_possessedAI.navMeshAgent.enabled)
                m_possessedAI.navMeshAgent.SetDestination(m_possessedAI.currentTarget.targetPos);
        }
        protected void SetAIDestination(Vector3 target)
        {
            if (m_possessedAI.navMeshAgent.enabled)
                m_possessedAI.navMeshAgent.SetDestination(target);
        }
    }
}

