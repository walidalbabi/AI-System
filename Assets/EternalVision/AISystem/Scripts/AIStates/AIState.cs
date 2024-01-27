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
        public virtual AIState Tick()
        {
            return this;
        }

    }
}

