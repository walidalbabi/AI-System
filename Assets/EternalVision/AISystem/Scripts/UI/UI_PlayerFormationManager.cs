using EternalVision.AI;
using EternalVision.EntitySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerFormationManager : MonoBehaviour
{
    [SerializeField] private UI_FormationGroup m_formationGroupPrefab;
    [SerializeField] private AIFormationManager m_formationManager;
    [SerializeField] private Transform m_content;


    private List<UI_FormationGroup> m_formationsList = new List<UI_FormationGroup>();
    private PossesedEntityGroup m_availbleEntities = new PossesedEntityGroup();

    public PossesedEntityGroup availbleEntities => m_availbleEntities;

    void Start()
    {

    }

    public void AddNewFormationGroup()
    {
        m_availbleEntities = EntityManager.instance.GetEntityGroupBelongToFaction(EntityManager.instance.localPlayer.entityBrain.faction);
        if (m_availbleEntities == null) return;

        var newUIFormation = Instantiate(m_formationGroupPrefab, m_content);
        var newFormation = Instantiate(m_formationManager, Vector3.zero, Quaternion.identity);
        newUIFormation.SetAIFormatiomComponent(newFormation);
        m_formationsList.Add(newUIFormation);
    }
    public void RemoveFormation(UI_FormationGroup formtation)
    {
        if(m_formationsList.Contains(formtation))
            m_formationsList.Remove(formtation);
    }
    public PossessedAI RequestEntityForFormation()
    {
        foreach (var entity in m_availbleEntities.entities)
        {
            PossessedAI ai = entity.GetComponent<PossessedAI>();
            if (ai && ai.aiFormation == null)
            {
                return ai;
            }
        }
        return null;
    }
}
