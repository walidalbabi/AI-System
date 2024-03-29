using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;
using EternalVision.EntitySystem;
using System;

public class UI_EntityWorldCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup m_visibleUIWhenLockedOnTargetCanvas;

    private PossesedEntity m_entityOwner;
    private UI_targetLockedWorldCanvas m_targetLockedWorldCanvas;

    private void Awake()
    {
        m_targetLockedWorldCanvas = GetComponent<UI_targetLockedWorldCanvas>();
        m_entityOwner = GetComponentInParent<PossesedEntity>();

        EntityHealth.OnEntityDie += DisableUI;
    }
    private void OnDestroy()
    {
        EntityHealth.OnEntityDie -= DisableUI;
    }


    public void EnableTargetUI()
    {
        m_targetLockedWorldCanvas.SetTargetToFollow(m_entityOwner.transform);
   //     m_visibleUIWhenLockedOnTargetCanvas.DOFade(1f, 0.2f);
    }
    public void DisableTargetUI() 
    {
    //    m_visibleUIWhenLockedOnTargetCanvas.DOFade(0f, 0.2f);
        m_targetLockedWorldCanvas.SetTargetToFollow(null);
    }

    private void DisableUI(EntityBrain brain)
    {
        if(brain == m_entityOwner.entityBrain) {
        gameObject.SetActive(false);
        }
    }
}
