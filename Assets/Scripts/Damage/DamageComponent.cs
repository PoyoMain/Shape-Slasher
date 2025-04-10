using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private int knockback;
    [SerializeField] private Transform knockbackOrigin;

    public int Damage => damage;
    public int Knockback => knockback;
    public Transform KnockbackOrigin => knockbackOrigin != null ? knockbackOrigin : transform;

    public void IncreaseDamageByMultiplier(float multiplier)
    {
        damage = Mathf.CeilToInt(damage * multiplier);
    }
}
