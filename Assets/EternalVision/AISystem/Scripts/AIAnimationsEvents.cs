using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimationsEvents : MonoBehaviour
{

    private AIZombieManager _aiZombieManager;

    private void Awake()
    {
        _aiZombieManager = GetComponent<AIZombieManager>();
    }


    public void OnLeftAttack()
    {
        _aiZombieManager.leftDamageArea.enabled = true;
    }

    public void OnRightAttack()
    {
        _aiZombieManager.rightDamageArea.enabled = true;
    }  

    public void OnFinishAttack()
    {
        _aiZombieManager.leftDamageArea.enabled = false;
        _aiZombieManager.rightDamageArea.enabled = false;
    }
    
    
    public void PlayAttackSound()
    {
     
    }
}
