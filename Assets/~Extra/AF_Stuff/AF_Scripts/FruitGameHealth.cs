using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FruitGameHealth : MonoBehaviour
{
    [SerializeField] private VoidEventSO bombHitEventSO;
    [SerializeField] private VoidEventSO fruitFellEventSO;
    [SerializeField] private VoidEventSO gameEndEventSO;
    [SerializeField] private Image xMarkPrefab;
    [SerializeField] private Transform xMarkParent;

    private int numberOfHits;

    private void OnEnable()
    {
        bombHitEventSO.OnEventRaised += DealDamage;
        fruitFellEventSO.OnEventRaised += DealDamage;
    }

    private void OnDisable()
    {
        bombHitEventSO.OnEventRaised -= DealDamage;
        fruitFellEventSO.OnEventRaised -= DealDamage;
    }

    private void DealDamage()
    {
        Instantiate(xMarkPrefab, xMarkParent);
        numberOfHits++;

        if (numberOfHits >= 3)
        {
            gameEndEventSO.RaiseEvent();
        }
    }
}
