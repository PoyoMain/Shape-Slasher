using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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

    private List<Door> openDoors;
    public List<Door> OpenDoors => openDoors;

    private GameObject player;

    private void Awake()
    {
        openDoors = doors.Where(x => !x.Linked).ToList();
    }

    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam.Follow = player.transform;
    }

    public void Init()
    {
        openDoors = doors.Where(x => !x.Linked).ToList();
    }

    public void LinkDoor(Door d)
    {
        OpenDoors.Remove(d);
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