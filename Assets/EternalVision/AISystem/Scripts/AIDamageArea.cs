using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageArea : MonoBehaviour
{
    private float _dmg;
    private Collider _collider;
    private int _ownerHash;

    private void Awake()
    {
        _dmg = GetComponentInParent<AIZombieManager>().attackDamage;
        _collider = GetComponent<Collider>();

        _ownerHash = GetComponentInParent<AIZombieManager>().gameObject.GetHashCode();
    }

    private void OnTriggerEnter(Collider other)
    {
     //   if (!GameManager.instance.networkContext.Ownership.isServer) return;

        I_Hitbox targetHit = other.gameObject.GetComponent<I_Hitbox>();

        if (targetHit != null)
        {

            if (_ownerHash == targetHit.ownerHash)
            {
                Physics.IgnoreCollision(_collider, other);
                return;
            }

            _collider.enabled = false;
            Debug.Log("<color=red> AI Damage : </color>" + _dmg + " " + other.gameObject.name);
            other.gameObject.GetComponent<I_Hitbox>().Hit((int)_dmg, transform.parent.gameObject);
        }
    }
}
