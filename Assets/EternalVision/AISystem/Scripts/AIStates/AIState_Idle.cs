using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState_Idle : AIState
{
    [Header("Idle Time Range")]
    [SerializeField] private float _minIdleTime;
    [SerializeField] private float _maxIdleTime;


    private bool _isIdleFinish;

    public override AIState Tick()
    {
        _aiZombieManager.FindATargerViaLineOfSight();

        if (_aiZombieManager.currentTarget.targetType == AITargetType.Enemy && _aiZombieManager.currentTarget.targetTransform != null)
        {
            return _aiZombieManager.GetAIState(AIStateEnum.Pursuit);
        }

        if (_isIdleFinish)
        {
            _isIdleFinish = false;
            return _aiZombieManager.GetAIState(AIStateEnum.Patrol);
        }

        return this;
    }

    public override void OnEnterState()
    {
        //_aiZombieManager.currentTarget.Settarget(null, Vector3.zero, 0, AITargetType.none);
        if(_aiZombieManager.anim != null)
        {
            _aiZombieManager.anim.SetFloat("Vertical", 0f);
            _aiZombieManager.anim.SetFloat("Horziontal", 0f);
            _aiZombieManager.anim.SetFloat("Speed", 0f);
        }

        StartCoroutine(WaitforIdleToFinish(Random.Range(_minIdleTime, _maxIdleTime)));

        _aiZombieManager.SetFOVAngleToNormal();
    }

    private IEnumerator WaitforIdleToFinish(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isIdleFinish = true;
    }


    public override void SetAIManagerComponent(AIZombieManager component)
    {
        _aiZombieManager = component;
    }
}
