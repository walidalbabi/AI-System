using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EternalVision.EntitySystem;

namespace EternalVision.AI
{
    public class AIInventory : EntityInventory
    {
        public override void InitializeInventory()
        {
            base.InitializeInventory();
            m_currentEquipedItem.OverrideForAI();
        }
    }
}
