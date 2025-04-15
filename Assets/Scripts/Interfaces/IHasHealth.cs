using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasHealth
{
    public int Health { get; protected set; }
    public float InvincibleTimer { get; protected set; }

    //private void TakeDamage(int damage)
    //{

    //}

    //private void Die()
    //{

    //}
}
