using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Void Event", menuName = "Events/Void Event")]
public class VoidEventSO : ScriptableObject
{
    public event UnityAction OnEventRaised;

    [ContextMenu("Activate")]
    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}