using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    [SerializeField] protected AIStateEnum _currentState;


    protected AIZombieManager _aiZombieManager;

    public AIStateEnum currentState => _currentState;

    //Base Class For all AI States
    public virtual AIState Tick()
    {
        return this;
    }

    public virtual void OnEnterState() { }


    public abstract void SetAIManagerComponent(AIZombieManager component);
}
