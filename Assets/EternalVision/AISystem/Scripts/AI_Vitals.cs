using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Vitals : NetworkBehaviour
{

    //Inspector Assigne

    [Header("Stats")]
    [SerializeField] private float _health;

    [Header("Components")]
    [SerializeField] private List<Rigidbody> _rbBodyPartsList = new List<Rigidbody>();

    //Var's

    private bool _isDead;

    private AIZombieManager _aiZombieManager;


    //Properties

    public bool isDead => _isDead;

    private void Awake()
    {
        _aiZombieManager = GetComponentInParent<AIZombieManager>();

        foreach (var rb in _rbBodyPartsList)
        {
        //    rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
            rb.isKinematic = true;

            var joint = rb.GetComponent<CharacterJoint>();
            if(joint != null)
            {
                joint.enableCollision = false;
                joint.enablePreprocessing = false;
            }

            rb.GetComponent<Collider>().enabled = false;
            rb.GetComponent<AIBodyParts>().SetOwnerHash(gameObject.GetHashCode());
        }
    }

    private void Start()
    { 
        _health = _aiZombieManager.maxHealth;
    }


    public void SetDamage(float damage)
    {
        Debug.Log(damage + " Dealt");
        _health += damage;

        if (_health <= 0) HandleDeath();
    }

    private void HandleDeath()
    {
        if (base.IsServer)
        {
            DeathTriggerAction();
            ObserversSetDeath();
        }
    }

    [ObserversRpc]
    private void ObserversSetDeath()
    {
        if (base.IsServer) return;
        DeathTriggerAction();
    }

    private void DeathTriggerAction()
    {
        _isDead = true;
        _aiZombieManager.navMeshAgent.enabled = false;
        _aiZombieManager.anim.enabled = false;
        _aiZombieManager.enabled = false;
        _aiZombieManager.GetComponent<Collider>().enabled = false;
        _aiZombieManager.GetComponent<Rigidbody>().isKinematic = true;
    }

}
