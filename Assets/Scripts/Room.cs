using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private RoomType roomType;

    [Header("Doors")]
    [SerializeField] private List<Door> doors;

    [Header("Components")]
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private Collider2D boundsCollider;

    // Properties
    public RoomType RoomType => roomType;
    public Vector2Int RoomNumber => roomNumber;
    public List<Door> Doors => doors;
    public List<Room> Neighbors
    {
        get
        {
            List<Room> n = new();
            if (northNeighborRoom != null) n.Add(northNeighborRoom);
            if (eastNeighborRoom != null) n.Add(eastNeighborRoom);
            if (southNeighborRoom != null) n.Add(southNeighborRoom);
            if (westNeighborRoom != null) n.Add(westNeighborRoom);
            return n;
        }
    }
    public List<Direction> DirectionsWithAnUnusedDoor
    {
        get
        {
            List<Direction> dList = new();
            if (northNeighborRoom == null && DoorsOnNorthSide) dList.Add(Direction.North);
            if (eastNeighborRoom == null && DoorsOnEastSide) dList.Add(Direction.East);
            if (southNeighborRoom == null && DoorsOnSouthSide) dList.Add(Direction.South);
            if (westNeighborRoom == null && DoorsOnWestSide) dList.Add(Direction.West);
            return dList;
        }
    }
    public Vector2 Bounds
    {
        get => boundsCollider.bounds.size;
    }
    private bool DoorsOnNorthSide
    {
        get => Doors.Any(door => door.Direction == Direction.North);
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

    // Private Variables
    private Vector2Int roomNumber;
    private Room northNeighborRoom;
    private Room eastNeighborRoom;
    private Room southNeighborRoom;
    private Room westNeighborRoom;

    public void AssignPosition(Vector2Int pos)
    {
        roomNumber = pos;
    }

    public void SetAdjacentRoom(Room adjacentRoom, Direction dir, bool opposite = false)
    {
        if (!opposite)
        {
            if (dir == Direction.North) northNeighborRoom = adjacentRoom;
            else if (dir == Direction.East) eastNeighborRoom = adjacentRoom;
            else if (dir == Direction.South) southNeighborRoom = adjacentRoom;
            else if (dir == Direction.West) westNeighborRoom = adjacentRoom;
        }
        else
        {
            if (dir == Direction.North) southNeighborRoom = adjacentRoom;
            else if (dir == Direction.East) westNeighborRoom = adjacentRoom;
            else if (dir == Direction.South) northNeighborRoom = adjacentRoom;
            else if (dir == Direction.West) eastNeighborRoom = adjacentRoom;
        }
        
    }

    public Room GetAdjacentRoom(Direction dir)
        => dir switch
        {
            Direction.North => northNeighborRoom,
            Direction.East => eastNeighborRoom,
            Direction.South => southNeighborRoom,
            Direction.West => westNeighborRoom,
            _ => null
        };

    public bool IsCompatibleWith(Room otherRoom, out Direction direction)
    {
        if (northNeighborRoom == null && DoorsOnNorthSide)
        {
            if ((Doors.Any(door => door.Type == DoorType.North1) == otherRoom.Doors.Any(door => door.Type == DoorType.South1))
            && (Doors.Any(door => door.Type == DoorType.North2) == otherRoom.Doors.Any(door => door.Type == DoorType.South2))
            && (Doors.Any(door => door.Type == DoorType.North3) == otherRoom.Doors.Any(door => door.Type == DoorType.South3)))
            {
                direction = Direction.North;
                return true;
            }
        }
        if (eastNeighborRoom == null && DoorsOnEastSide)
        {
            if ((Doors.Any(door => door.Type == DoorType.East1) == otherRoom.Doors.Any(door => door.Type == DoorType.West1))
            && (Doors.Any(door => door.Type == DoorType.East2) == otherRoom.Doors.Any(door => door.Type == DoorType.West2))
            && (Doors.Any(door => door.Type == DoorType.East3) == otherRoom.Doors.Any(door => door.Type == DoorType.West3)))
            {
                direction = Direction.East;
                return true;
            }
        }
        if (southNeighborRoom == null && DoorsOnSouthSide)
        {
            if ((Doors.Any(door => door.Type == DoorType.South1) == otherRoom.Doors.Any(door => door.Type == DoorType.North1))
                        && (Doors.Any(door => door.Type == DoorType.South2) == otherRoom.Doors.Any(door => door.Type == DoorType.North2))
                        && (Doors.Any(door => door.Type == DoorType.South3) == otherRoom.Doors.Any(door => door.Type == DoorType.North3)))
            {
                direction = Direction.South;
                return true;
            }
        }
        if (westNeighborRoom == null && DoorsOnWestSide)
        {
            if ((Doors.Any(door => door.Type == DoorType.West1) == otherRoom.Doors.Any(door => door.Type == DoorType.East1))
            && (Doors.Any(door => door.Type == DoorType.West2) == otherRoom.Doors.Any(door => door.Type == DoorType.East2))
            && (Doors.Any(door => door.Type == DoorType.West3) == otherRoom.Doors.Any(door => door.Type == DoorType.East3)))
            {
                direction = Direction.West;
                return true;
            }
        }


        direction = Direction.North;
        return false;
    }

    public Direction DirectionToUnlinkedRoom(Room otherRoom)
    {
        if (DoorsOnNorthSide && northNeighborRoom == null)
        {
            if ((Doors.Any(door => door.Type == DoorType.North1) == otherRoom.Doors.Any(door => door.Type == DoorType.South1))
            && (Doors.Any(door => door.Type == DoorType.North2) == otherRoom.Doors.Any(door => door.Type == DoorType.South2))
            && (Doors.Any(door => door.Type == DoorType.North3) == otherRoom.Doors.Any(door => door.Type == DoorType.South3)))
            {
                return Direction.North;
            }
        }
        
        if (DoorsOnEastSide && eastNeighborRoom == null)
        {
            if ((Doors.Any(door => door.Type == DoorType.East1) == otherRoom.Doors.Any(door => door.Type == DoorType.West1))
            && (Doors.Any(door => door.Type == DoorType.East2) == otherRoom.Doors.Any(door => door.Type == DoorType.West2))
            && (Doors.Any(door => door.Type == DoorType.East3) == otherRoom.Doors.Any(door => door.Type == DoorType.West3)))
            {
                return Direction.East;
            }
        }

        if (DoorsOnSouthSide && southNeighborRoom == null)
        {
            if ((Doors.Any(door => door.Type == DoorType.South1) == otherRoom.Doors.Any(door => door.Type == DoorType.North1))
            && (Doors.Any(door => door.Type == DoorType.South2) == otherRoom.Doors.Any(door => door.Type == DoorType.North2))
            && (Doors.Any(door => door.Type == DoorType.South3) == otherRoom.Doors.Any(door => door.Type == DoorType.North3)))
            {
                return Direction.South;
            }
        }

        if (DoorsOnWestSide && westNeighborRoom == null)
        {
            if ((Doors.Any(door => door.Type == DoorType.West1) == otherRoom.Doors.Any(door => door.Type == DoorType.East1))
            && (Doors.Any(door => door.Type == DoorType.West2) == otherRoom.Doors.Any(door => door.Type == DoorType.East2))
            && (Doors.Any(door => door.Type == DoorType.West3) == otherRoom.Doors.Any(door => door.Type == DoorType.East3)))
            {
                return Direction.West;
            }
        }

        return Direction.North;
    }

    public bool HasAnUnusedDoorInThisDirection(Direction dir)
    {
        if (DirectionsWithAnUnusedDoor.Contains(dir)) return true;

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cam.enabled = true;
            cam.Follow = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cam.enabled = false;
        }
    }
}

public enum RoomType
{
    Standard,
    Starting,
    Boss
}

public enum DoorType
{
    North1,
    North2,
    North3,
    East1,
    East2,
    East3,
    South1,
    South2,
    South3,
    West1,
    West2,
    West3,
}

[Serializable]
public class Door
{
    [SerializeField] private DoorType type;

    public DoorType Type => type;
    public Direction Direction => type switch
    {
        DoorType.North1 => Direction.North,
        DoorType.North2 => Direction.North,
        DoorType.North3 => Direction.North,
        DoorType.East1 => Direction.East,
        DoorType.East2 => Direction.East,
        DoorType.East3 => Direction.East,
        DoorType.South1 => Direction.South,
        DoorType.South2 => Direction.South,
        DoorType.South3 => Direction.South,
        DoorType.West1 => Direction.West,
        DoorType.West2 => Direction.West,
        DoorType.West3 => Direction.West,
        _ => throw new System.NotImplementedException(),
    };
}