using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Int Event", menuName = "Events/Int Event")]
public class IntEventSO : ScriptableObject
{
    public event UnityAction<int> OnEventRaised;

    public void RaiseEvent(int i)
    {
        OnEventRaised?.Invoke(i);
    }

    [ContextMenu("Activate (1)")]
    private void RaiseEventOne()
    {
        OnEventRaised?.Invoke(1);
    }

    [ContextMenu("Activate (3)")]
    private void RaiseEventThree()
    {
        OnEventRaised?.Invoke(3);
    }

    [ContextMenu("Activate (5)")]
    private void RaiseEventFive()
    {
        OnEventRaised?.Invoke(5);
    }

    [ContextMenu("Activate (10)")]
    private void RaiseEventTen()
    {
        OnEventRaised?.Invoke(10);
    }

}
