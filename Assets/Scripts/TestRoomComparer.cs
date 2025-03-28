using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRoomComparer : MonoBehaviour
{
    [SerializeField] private bool compareOnStart;
    [SerializeField] private RoomSO room1;
    [SerializeField] private RoomSO room2;

    private void OnEnable()
    {
        if (compareOnStart) CompareRooms();
    }

    [ContextMenu("Compare")]
    private void CompareRooms()
    {
        Room r1 = new(room1);
        string truth = r1.IsCompatibleWith(room2, out Direction _) ? "are" : "are not";
        print("Rooms " + truth + " Compatible");
    }
}
