using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EternalVision.AI
{
    public class AIState_Idle : AIState
    {
        [Header("Idle Time Range")]
        [SerializeField] private float _minIdleTime;
        [SerializeField] private float _maxIdleTime;


        private bool _isIdleFinish;

      
        public override void OnEnterState()
        {
            //m_possessedAI.currentTarget.Settarget(null, Vector3.zero, 0, AITargetType.none);
            if (m_possessedAI.entityBrain.entityAnimator != null)
            {
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Vertical", 0f, .1f);
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Horziontal", 0f, .1f);
                m_possessedAI.entityBrain.entityAnimator.SetFloat("Speed", 0f, .1f);
            }

            StartCoroutine(WaitforIdleToFinish(Random.Range(_minIdleTime, _maxIdleTime)));

            m_possessedAI.SetFOVAngleToNormal();
        }
        public override AIState Tick()
        {
            m_possessedAI.FindATargerViaLineOfSight();

            if (m_possessedAI.currentTarget.targetType == AITargetType.Enemy && m_possessedAI.currentTarget.targetTransform != null)
            {
                return m_possessedAI.GetAIState(AIStateEnum.Pursuit);
            }

            if (_isIdleFinish)
            {
                _isIdleFinish = false;
                return m_possessedAI.GetAIState(AIStateEnum.Patrol);
            }

            return this;
        }


        private IEnumerator WaitforIdleToFinish(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isIdleFinish = true;
        }


    }
}

