using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EternalVision.EntitySystem;

namespace EternalVision.AI
{
    public class UI_FormationGroup : MonoBehaviour
    {
        [SerializeField] private Slider m_slider;
        [SerializeField] private TextMeshProUGUI m_amountText;

        private UI_PlayerFormationManager m_manager;
        private AIFormationManager m_aiFormation;


        private void Awake()
        {
            m_manager = GetComponentInParent<UI_PlayerFormationManager>();

            m_amountText.text = 0 +"/" + m_manager.availbleEntities.entities.Count.ToString();
            m_slider.onValueChanged.AddListener(AddOrRemoveEntity);

        }
        private void Start()
        {
            m_slider.maxValue = m_manager.availbleEntities.entities.Count;
        }
        private void OnDestroy()
        {
            m_slider.onValueChanged.RemoveListener(AddOrRemoveEntity);
        }


        public void SetAIFormatiomComponent(AIFormationManager component)
        {
            m_aiFormation = component;
        }
        public void RemoveFormation()
        {
            m_manager.RemoveFormation(this);
            Destroy(m_aiFormation.gameObject);
            Destroy(gameObject);
        }

        private void AddOrRemoveEntity(float amount)
        {
            if (amount > m_aiFormation.entitiesInFormation.Count)
            {
                m_aiFormation.AddEntityToFormtaion(m_manager.RequestEntityForFormation());
            }
            else if (amount < m_aiFormation.entitiesInFormation.Count)
            {
                m_aiFormation.RemoveEntityToFormtaion();
            }

            m_amountText.text = m_aiFormation.entitiesInFormation.Count + "/" + m_manager.availbleEntities.entities.Count.ToString();
        }
    }
}

