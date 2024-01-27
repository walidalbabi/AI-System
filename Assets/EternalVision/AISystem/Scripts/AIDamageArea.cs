using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EternalVision.EntitySystem;

namespace EternalVision.AI
{
    public class AIDamageArea : EntityItem
    {
        //Private
        private float _dmg;
        private int _ownerHash;
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            _ownerHash = GetComponentInParent<PossessedAI>().gameObject.GetHashCode();
        }
        private void OnTriggerEnter(Collider other)
        {
            IHitbox targetHit = other.gameObject.GetComponent<IHitbox>();

            if (targetHit != null)
            {

                if (_ownerHash == targetHit.GetOwnerHash())
                {
                    Physics.IgnoreCollision(_collider, other);
                    return;
                }

                _collider.enabled = false;
                Debug.Log("<color=red> AI Damage : </color>" + _dmg + " " + other.gameObject.name);


                if (!targetHit.Equals(null))
                {
                    HitData newHitData = new HitData(m_entityBrainOwner.transform, other.ClosestPointOnBounds(targetHit.GetPosition()));
                    targetHit.DoDamage(newHitData, (int)_dmg);
                }

            }
        }

        public override void EquipItem()
        {
            return;
        }
        public override bool UseItemFirstAction()
        {
            return false;
        }

        public override bool UseItemSecondAction()
        {
            return false;
        }
    }

}

