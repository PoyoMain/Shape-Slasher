using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Room", menuName = "Room")]
public class RoomSO : ScriptableObject
{
    [Header("Type")]
    [SerializeField] private RoomType roomType;

    [Header("Doors")]
    [SerializeField] private List<Door> doors;

    [Header("Components")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Collider2D boundsCollider;

    public RoomType RoomType => roomType;
    public List<Door> Doors => doors;
    public GameObject[] Prefabs => prefabs;
    public Vector2 Bounds
    {
        get => boundsCollider.bounds.size;
    }

    private bool DoorsOnNorthSide
    {
        get => doors.Any(door => door.Direction == Direction.North);
    }
    private bool DoorsOnEastSide
    {
        get => Doors.Any(door => door.Direction == Direction.East);
    }
    private bool DoorsOnSouthSide
    {
        get => Doors.Any(door => door.Direction == Direction.South);
    }
    private bool DoorsOnWestSide
    {
        get => Doors.Any(door => door.Direction == Direction.West);
    }

    public bool HasAnUnusedDoorInThisDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                if (DoorsOnNorthSide) return true;
                break;
            case Direction.East:
                if (DoorsOnEastSide) return true;
                break;
            case Direction.South:
                if (DoorsOnSouthSide) return true;
                break;
            case Direction.West:
                if (DoorsOnWestSide) return true;
                break;
            default:
                return false;
        }

        return false;
    }

    public Vector2 GetRoomBounds(GameObject roomPrefab)
    {
        roomPrefab.TryGetComponent(out BoxCollider2D coll);
        if (coll != null) return coll.size;
        else return Vector2.negativeInfinity;
    }
}
