using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Vector2 Event", menuName = "Events/Vector2 Event")]
public class Vector2EventSO : ScriptableObject
{
    public UnityAction<Vector2> OnEventRaised;

    public void RaiseEvent(Vector2 v)
    {
        OnEventRaised?.Invoke(v);
    }
}
