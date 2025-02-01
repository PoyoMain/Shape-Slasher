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

    // Properties
    public RoomType RoomType => roomType;
    public List<Door> Doors => doors;
    public List<Door> OpenDoors => openDoors;
    public Vector2Int RoomNumber => roomNumber;

    // Private Variables
    private GameObject player;

    private List<Door> openDoors;
    private Vector2Int roomNumber;
    private Room northNeighborRoom;
    private Room eastNeighborRoom;
    private Room southNeighborRoom;
    private Room westNeighborRoom;

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

    public bool HasSpotsOpen
    {
        get
        {
            if (DoorsOnNorthSide && northNeighborRoom == null) return true;
            else if (DoorsOnEastSide && eastNeighborRoom == null) return true;
            else if (DoorsOnSouthSide && southNeighborRoom == null) return true;
            else if (DoorsOnWestSide && westNeighborRoom == null) return true;
            return false;

        }
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

    public List<Direction> DirectionsFacing
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

    private void Awake()
    {
        openDoors = doors.Where(x => !x.Linked).ToList();
    }

    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam.Follow = player.transform;
    }

    public void SetUpRoom(Vector2Int roomNum)
    {
        openDoors = doors.Where(x => !x.Linked).ToList();
        roomNumber = roomNum;
    }

    public void LinkDoor(Door d)
    {
        OpenDoors.Remove(d);
    }

    public void AssignPosition(Vector2Int pos)
    {
        roomNumber = pos;
    }

    public Vector2Int LinkRoom(Room otherRoom)
    {
        foreach (Door oldDoor in OpenDoors)
        {
            foreach (Door newDoor in otherRoom.Doors)
            {
                if (oldDoor.IsOpposite(newDoor, out Direction dir))
                {
                    oldDoor.Link();
                    LinkDoor(oldDoor);
                    newDoor.Link();
                    otherRoom.LinkDoor(newDoor);
                    return dir switch
                    {
                        Direction.North => Vector2Int.up,
                        Direction.East => Vector2Int.right,
                        Direction.South => Vector2Int.down,
                        Direction.West => Vector2Int.left,
                        _ => throw new NotImplementedException(),
                    };
                }
            }
        }

        return Vector2Int.zero;
    }

    public Door[] GetOpenDoors()
    {
        List<Door> dList = new();

        if (northNeighborRoom == null)
        {
            dList.AddRange(Doors.Where(door => door.Direction == Direction.North).ToList());
        }
        if (eastNeighborRoom == null)
        {
            dList.AddRange(Doors.Where(door => door.Direction == Direction.East).ToList());
        }
        if (southNeighborRoom == null)
        {
            dList.AddRange(Doors.Where(door => door.Direction == Direction.South).ToList());
        }
        if (westNeighborRoom == null)
        {
            dList.AddRange(Doors.Where(door => door.Direction == Direction.West).ToList());
        }

        return dList.ToArray();
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

    public Direction DirectionToNeighbor(Room otherRoom)
    {
        Vector2Int dirVector = otherRoom.roomNumber - roomNumber;

        return dirVector switch
        {
            var _ when dirVector == Vector2Int.up => Direction.North,
            var _ when dirVector == Vector2Int.left => Direction.East,
            var _ when dirVector == Vector2Int.down => Direction.South,
            var _ when dirVector == Vector2Int.right => Direction.West,
            _ => throw new System.NotImplementedException(),
        };
    }

    public bool IsFacingDirection(Direction dir)
    {
        if (DirectionsFacing.Contains(dir)) return true;

        return false;
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
    private bool linked;

    public DoorType Type => type;
    public bool Linked => linked;
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


    public void Link() => linked = true;
    public void Unlink() => linked = false;

    public bool IsOpposite(Door otherDoor, out Direction directionGoing)
    {
        if (otherDoor.Type == DoorType.North1 && Type == DoorType.South1) { directionGoing = Direction.South; return true; }
        else if (otherDoor.Type == DoorType.North2 && Type == DoorType.South2) { directionGoing = Direction.South; return true; }
        else if (otherDoor.Type == DoorType.North3 && Type == DoorType.South3) { directionGoing = Direction.South; return true; }
        else if (otherDoor.Type == DoorType.East1 && Type == DoorType.West1) { directionGoing = Direction.West; return true; }
        else if (otherDoor.Type == DoorType.East2 && Type == DoorType.West2) { directionGoing = Direction.West; return true; }
        else if (otherDoor.Type == DoorType.East3 && Type == DoorType.West3) { directionGoing = Direction.West; return true; }
        else if (otherDoor.Type == DoorType.South1 && Type == DoorType.North1) { directionGoing = Direction.North; return true; }
        else if (otherDoor.Type == DoorType.South2 && Type == DoorType.North2) { directionGoing = Direction.North; return true; }
        else if (otherDoor.Type == DoorType.South3 && Type == DoorType.North3) { directionGoing = Direction.North; return true; }
        else if (otherDoor.Type == DoorType.West1 && Type == DoorType.East1) { directionGoing = Direction.East; return true; }
        else if (otherDoor.Type == DoorType.West2 && Type == DoorType.East2) { directionGoing = Direction.East; return true; }
        else if (otherDoor.Type == DoorType.West3 && Type == DoorType.East3) { directionGoing = Direction.East; return true; }
        else { directionGoing = Direction.North; return false; }
    }
}

//public class Side
//{
//    private Door door1;
//    private Door door2;
//    private Door door3;

//    public Side(Door d1, Door d2, Door d3)
//    {
//        door1 = d1;
//        door2 = d2;
//        door3 = d3;
//    }

//    public bool Equals(Side otherSide)
//    {
//        if (otherSide.door1 != null)
//    }
//}