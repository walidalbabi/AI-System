using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Hitbox 
{

    public abstract int ownerHash { get; set; }

    public abstract void  Hit(int damage, GameObject dealer);

}
