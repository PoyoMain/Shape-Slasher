using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private OptionsSO options;
    [SerializeField] private bool usesOptionsValues;

    [Header("Stats")]
    [SerializeField] private DifficultyInt damage;
    [SerializeField] private int testDamage;
    [SerializeField] private int knockback;
    [SerializeField] private Transform knockbackOrigin;

    public int Damage => Mathf.CeilToInt(multiplier * (usesOptionsValues ? damage.ReturnValue(options.Difficulty) : testDamage));
    public int Knockback => knockback;
    public Transform KnockbackOrigin => knockbackOrigin != null ? knockbackOrigin : transform;

    private float multiplier = 1;

    public void IncreaseDamageByMultiplier(float mult)
    {
        multiplier = mult;
    }
}
