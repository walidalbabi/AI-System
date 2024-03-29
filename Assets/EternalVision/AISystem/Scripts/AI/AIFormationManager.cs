using EternalVision.AI;
using EternalVision.EntitySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EternalVision.AI
{
    public class AIFormationManager : MonoBehaviour
    {
        [SerializeField] private FormationType m_formationType;
        [SerializeField] private Vector2 m_offset;

        private Dictionary<PossessedAI, Transform> m_formationSlots = new Dictionary<PossessedAI, Transform>();

        private List<GameObject> m_objectsInUse = new List<GameObject>();
        private List<GameObject> m_objectsReadyToBeUsed = new List<GameObject>();

        private List<PossessedAI> m_entitiesInFormation = new List<PossessedAI>();

        public Dictionary<PossessedAI, Transform> formationSlots => m_formationSlots;
        public  List<PossessedAI> entitiesInFormation => m_entitiesInFormation;

        #region Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);

            if (m_formationSlots.Count > 0)
                foreach (var slot in m_formationSlots)
                {
                    Gizmos.DrawSphere(slot.Value.transform.position, .5f);
                }
        }
        private void OnEnable()
        {
            SquareFormation();
        }
        private void Awake()
        {
            EntityHealth.OnEntityDie += CheckAndRemoveDiedEntity;
        }
        private void OnDestroy()
        {
            EntityHealth.OnEntityDie -= CheckAndRemoveDiedEntity;
        }
        #endregion

        #region Functions
        private void RecalculateFormation()
        {
            switch (m_formationType)
            {
                case FormationType.Square:
                    SquareFormation();
                    break;
            }
        }
        private void SquareFormation()
        {
            ResetFormation();

            Vector3 targetPos = new Vector3(0f, 0f, -m_entitiesInFormation.Count / 2);

            int counter = -1;
            int xOffset = -1;

            float sqrt = Mathf.Sqrt(m_entitiesInFormation.Count);
            float startX = targetPos.x;

            for (int i = 0; i < m_entitiesInFormation.Count; i++)
            {
                counter++;
                xOffset++;

                if (xOffset > 1)
                {
                    xOffset = 1;
                }

                targetPos = new Vector3(targetPos.x + xOffset * m_offset.x, 0f, targetPos.z);

                if (counter == Mathf.Floor(sqrt))
                {
                    counter = 0;
                    targetPos.x = startX;
                    targetPos.z += 1 + m_offset.y;
                }

                GameObject slotObj = GetASlotObject();
                m_objectsInUse.Add(slotObj);
                slotObj.transform.localPosition = targetPos;
                m_formationSlots.Add(m_entitiesInFormation[i], slotObj.transform);
            }
        }

        private void ResetFormation()
        {
            if (m_objectsInUse.Count > 0)
            {
                foreach (var obj in m_objectsInUse)
                {
                    m_objectsReadyToBeUsed.Add(obj);
                }
                m_objectsInUse.Clear();
            }
            m_formationSlots.Clear();
        }

        public void AddEntityToFormtaion(PossessedAI entity)
        {
            if (!m_entitiesInFormation.Contains(entity))
            {
                m_entitiesInFormation.Add(entity);
                entity.AddAIToFormation(this);
                RecalculateFormation();
            }
        }
        public void RemoveEntityToFormtaion(PossessedAI entity)
        {
            if (m_entitiesInFormation.Contains(entity))
            {
                m_entitiesInFormation.Remove(entity);
                entity.AddAIToFormation(null);
                RecalculateFormation();
            }
        }
        public void RemoveEntityToFormtaion()
        {
            PossessedAI entity = m_entitiesInFormation.ElementAt(m_entitiesInFormation.Count - 1);
            if (entity)
            {
                m_entitiesInFormation.Remove(entity);
                entity.AddAIToFormation(null);
                RecalculateFormation();
            }
        }
        #endregion

        #region Callbacks
        private void CheckAndRemoveDiedEntity(EntityBrain brain)
        {
            if (brain.possessedEntity.GetType() == typeof(PossessedAI))
                RemoveEntityToFormtaion((PossessedAI)brain.possessedEntity);
        }
        #endregion

        #region Getters
        private GameObject GetASlotObject()
        {
            if (m_objectsReadyToBeUsed.Count > 0)
            {
                GameObject newObjectFromList = m_objectsReadyToBeUsed[0];
                m_objectsReadyToBeUsed.RemoveAt(0);

                return newObjectFromList;
            }

            GameObject newObject = new GameObject();
            newObject.transform.parent = transform;
            return newObject;
        }
        public Transform GetMySlot(PossessedAI entityRequester)
        {
            if(m_formationSlots.TryGetValue(entityRequester, out Transform slot))
            {
                return slot;
            }
            return null;
        }
        #endregion

    }

    public enum FormationType
    {
        Square,
    }
}
