using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Buyable Event", menuName = "Events/Buyable Event")]
public class BuyableEventSO : ScriptableObject
{
    public UnityAction<BuyableEventSO> OnEventRaised;

    public void RaiseEvent(BuyableEventSO buyable)
    {
        OnEventRaised?.Invoke(buyable);
    }
}
