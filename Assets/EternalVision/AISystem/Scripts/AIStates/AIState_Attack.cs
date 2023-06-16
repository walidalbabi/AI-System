using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState_Attack : AIState
{
    [SerializeField, Tooltip("Sould be same value as for the pursuit stop distance")] private float _closeRangeAttack;
    [SerializeField] private float _minCooldownTime;
    [SerializeField] private float _maxCooldownTime;

    [SerializeField] private bool _performingAttack;
    [SerializeField] private bool _canAttack;
    [SerializeField] private float _cooldown;


    public override AIState Tick()
    {
        CalculateAttackCooldown();
        FaceTheTarget();
        _aiZombieManager.FindATargerViaLineOfSight();

        if (_aiZombieManager.currentTarget.targetTransform == null)
        {
            Debug.Log("AI target is lost");
            return _aiZombieManager.GetAIState(AIStateEnum.Idle);
        }

        //Target is Lost
        if (_aiZombieManager.currentTarget.targetType != AITargetType.Enemy)
        {
            Debug.Log("AI target isn't an enemy");
            return _aiZombieManager.GetAIState(AIStateEnum.Idle);
        }
        //Target we have only the last scene pos of the target
        if (_aiZombieManager.currentTarget.targetDistance > _closeRangeAttack)
        {
            Debug.Log("AI Lost Target Tracking");
            return _aiZombieManager.GetAIState(AIStateEnum.Pursuit);
        }
        else
        {
            HandleAttack();
        }

  

        return this;
    }


    private void FaceTheTarget()
    {
        Vector3 direction = (_aiZombieManager.currentTarget.targetPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation = new Quaternion(0f, lookRotation.y, 0f, lookRotation.w);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void CalculateAttackCooldown()
    {
        if(_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
        }
        else
        {
            _performingAttack = false;
        }
    }

    private void SetCooldowm()
    {
        _cooldown = Random.Range(_minCooldownTime, _maxCooldownTime);
    }


    private void HandleAttack()
    {

        if (_aiZombieManager.currentTarget.targetDistance <= _closeRangeAttack && !_performingAttack && _canAttack)
        {
            //Perform a close Range Attack
            SetCooldowm();
            _performingAttack = true;
            _aiZombieManager.HandleAttack(Random.Range(0, 100), true);
        }
    }

    public override void OnEnterState()
    {
        _aiZombieManager.anim.SetFloat("Vertical", 0f);
        _aiZombieManager.anim.SetFloat("Horziontal", 0f);
        _aiZombieManager.anim.SetFloat("Speed", 0f);
        _aiZombieManager.navMeshAgent.speed = 0f;
        _aiZombieManager.SetFOVAngleToTheAlertedState();
    }


    public override void SetAIManagerComponent(AIZombieManager component)
    {
        _aiZombieManager = component;
    }

}
