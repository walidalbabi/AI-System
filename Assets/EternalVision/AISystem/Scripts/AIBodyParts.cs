using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBodyParts : MonoBehaviour //, I_Hitbox
{
    [SerializeField, Tooltip("The value to multiply any incoming damage by. Use to reduce damage to areas like feet, or raise it for areas like the head.")]
    private float m_Multiplier = 0.1f;

    private AIZombieManager _aiZombieManager;

    public void Hit(int damage, GameObject dealer)
    {
        SendDamageToAIBaseClass((float)damage * m_Multiplier);
    }

    private void Awake()
    {
        _aiZombieManager = GetComponentInParent<AIZombieManager>();
    }


    private void SendDamageToAIBaseClass(float dmg)
    {
        _aiZombieManager.GetAIVitalsComponent().SetDamage(-dmg);
    }

    public void SetOwnerHash(int hash)
    {
        ownerHash = hash;
    }


    public int ownerHash { get; set; }
}
