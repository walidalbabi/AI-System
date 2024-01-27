using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalVision.AI
{
    public class AIAnimationsEvents : MonoBehaviour
    {

        private PossessedAI _aiZombieManager;

        private void Awake()
        {
            _aiZombieManager = GetComponent<PossessedAI>();
        }


        public void OnLeftAttack()
        {
            //m_possessedAI.leftDamageArea.enabled = true;
        }

        public void OnRightAttack()
        {
            //m_possessedAI.rightDamageArea.enabled = true;
        }

        public void OnFinishAttack()
        {
            //m_possessedAI.leftDamageArea.enabled = false;
            // m_possessedAI.rightDamageArea.enabled = false;
        }


        public void PlayAttackSound()
        {

        }
    }

}

