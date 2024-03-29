using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;
using EternalVision.EntitySystem;
using System;
using EternalVision.AI;

public class UI_AttackIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float m_scaleSpeed = 1f;
    [SerializeField] private Vector3 m_increasedScale;


    [SerializeField] private Color m_startColor;
    [SerializeField] private Color m_targetColor;

    private Vector3 m_targetScale;

    private Image m_icon;
    private AIBrain m_aiOwner;

    private void Awake()
    {
        m_icon = GetComponent<Image>();
        m_aiOwner = GetComponentInParent<AIBrain>();

       // m_aiOwner.OnAttackPlayer += SetDuration;

        m_targetScale = transform.localScale + m_increasedScale;
    }
    private void OnDestroy()
    {
      //  m_aiOwner.OnAttackPlayer -= SetDuration;
    }



    public void SetDuration(float duration)
    {
        //m_icon.color = m_startColor;
        //transform.DOScale(m_targetScale, m_scaleSpeed).SetEase(Ease.Linear).SetLoops(int.MaxValue, LoopType.Yoyo);

        //m_icon.DOColor(m_targetColor, duration - 0.1f).OnComplete(() =>
        //{
        //    m_icon.DOFade(0f, 0.1f).OnComplete(() => transform.DOPause());
        //});
    }

}
