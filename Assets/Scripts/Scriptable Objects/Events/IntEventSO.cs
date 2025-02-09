using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Int Event", menuName = "Events/Int Event")]
public class IntEventSO : ScriptableObject
{
    public event UnityAction<int> OnEventRaised;

    [ContextMenu("Activate")]
    public void RaiseEvent(int i)
    {
        OnEventRaised?.Invoke(i);
    }
}
