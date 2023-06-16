using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIState_Patrol : AIState
{
    
    [Range(1f, 3f)] [SerializeField] private float _moveSpeed;
    [Range(10f, 360f)] [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _stoppingDistance;

    private float _distanceToTarget;

    public override AIState Tick()
    {
        _distanceToTarget = Vector3.Distance(transform.position, _aiZombieManager.currentTarget.targetPos);

        SetAIDestination();
        _aiZombieManager.FindATargerViaLineOfSight();
        _aiZombieManager.HandleMoveToTarget();

        if (_aiZombieManager.currentTarget.targetType == AITargetType.Enemy)
        {
            return _aiZombieManager.GetAIState(AIStateEnum.Pursuit);
        }

        if (_distanceToTarget <= _aiZombieManager.navMeshAgent.stoppingDistance || _aiZombieManager.navMeshAgent.isPathStale || _aiZombieManager.navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            return _aiZombieManager.GetAIState(AIStateEnum.Idle);
        }
        return this;
    }

    private void SetAIDestination()
    {
        _aiZombieManager.navMeshAgent.SetDestination(_aiZombieManager.currentTarget.targetPos);
    }


    public override void OnEnterState()
    {
        _aiZombieManager.navMeshAgent.stoppingDistance = _stoppingDistance;
        //Set Speed
        _aiZombieManager.navMeshAgent.speed = _moveSpeed;
        _aiZombieManager.navMeshAgent.angularSpeed = _rotateSpeed;
        //Get A random Point
        _aiZombieManager.SetRandomPointForPatrol();

        _aiZombieManager.SetFOVAngleToNormal();
    }


    public override void SetAIManagerComponent(AIZombieManager component)
    {
        _aiZombieManager = component;
    }
}
