using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Room List Event", menuName = "Events/Room List Event")]
public class RoomListEventSO : ScriptableObject
{
    public UnityAction<List<Room>> OnEventRaised;

    public void RaiseEvent(List<Room> rooms)
    {
        OnEventRaised?.Invoke(rooms);
    }
}
