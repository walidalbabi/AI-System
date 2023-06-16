using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState_Pursuit : AIState
{
    [Range(0f, 5f)] [SerializeField] private float _minDistanceToStartWalking;
    [Range(1f, 3f)] [SerializeField] private float _moveSpeed;
    [Range(1f, 3f)] [SerializeField] private float _defaultWalkSpeed;
    [Range(10f, 360f)] [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _stoppingDistance;

    public override AIState Tick()
    {
        _aiZombieManager.FindATargerViaLineOfSight();
        SetAIDestination();

        _aiZombieManager.HandleMoveToTarget();

        if (_aiZombieManager.currentTarget.targetDistance <= _aiZombieManager.navMeshAgent.stoppingDistance && _aiZombieManager.currentTarget.targetType != AITargetType.Enemy)
        {
            //Must Investigate
            return _aiZombieManager.GetAIState(AIStateEnum.Idle);

        }
        else
        {
            if (_aiZombieManager.currentTarget.targetDistance <= _aiZombieManager.navMeshAgent.stoppingDistance && _aiZombieManager.currentTarget.targetType == AITargetType.Enemy)
            {
                return _aiZombieManager.GetAIState(AIStateEnum.Attack);
            }
        }


        if (_aiZombieManager.currentTarget.targetDistance <= _minDistanceToStartWalking) _aiZombieManager.navMeshAgent.speed = _defaultWalkSpeed;
        else _aiZombieManager.navMeshAgent.speed = _moveSpeed;


        return this;
    }

    public override void OnEnterState()
    {
        _aiZombieManager.navMeshAgent.stoppingDistance = _stoppingDistance;
        _aiZombieManager.navMeshAgent.speed = _moveSpeed;
        _aiZombieManager.navMeshAgent.angularSpeed = _rotateSpeed;
      //  _aiZombieManager.currentTarget.Settarget(null, Vector3.zero, 0f, AITargetType.none);
        _aiZombieManager.SetFOVAngleToTheAlertedState();
    }


    private void SetAIDestination()
    {
        _aiZombieManager.navMeshAgent.SetDestination(_aiZombieManager.currentTarget.targetPos);
    }

    public override void SetAIManagerComponent(AIZombieManager component)
    {
        _aiZombieManager = component;
    }
}
