using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMap : MonoBehaviour
{
    [SerializeField] private RoomSO[] rooms;
    [SerializeField] private Vector2Int[] roomPositions;
    [SerializeField] private RoomListEventSO mapMadeEventSO;

    private void Start()
    {
        if (rooms.Length != roomPositions.Length) { Debug.LogError("Different amounts of rooms and room positions"); return; }

        List<Room> createdRooms = new();

        for (int i = 0; i < rooms.Length; i++)
        {
            Room r = new(rooms[i]);
            createdRooms.Add(r);

            r.AssignPosition(roomPositions[i]);
        }

        mapMadeEventSO.RaiseEvent(createdRooms);
    }
}
