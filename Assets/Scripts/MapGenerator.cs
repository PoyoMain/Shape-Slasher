using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int numberOfRooms;
    //[SerializeField] private int numberOfNeighborsAllowed;
    //[SerializeField] private bool canReuseRooms;
    [SerializeField] private bool performOnAwake;

    [Header("References")]
    [SerializeField] private Room[] startRooms;
    [SerializeField] private Room[] bossRooms;
    [SerializeField] private List<Room> possibleRooms;

    private Room startRoom;
    private List<Room> rooms;


    private Room[,] roomGrid = new Room[50,50];
    private Vector2Int currentGridPos = new(25, 25);
    private Room currentRoom;

    private Room RoomAtCurrentPos
    {
        get => roomGrid[currentGridPos.x, currentGridPos.y];
        set => roomGrid[currentGridPos.x, currentGridPos.y] = value;
    }

    private const float ROOM_BOUNDS_X = 40;
    private const float ROOM_BOUNDS_Y = 40;

    private Coroutine mapGenCoroutine;

    private void Awake()
    {
        rooms = new();
        roomGrid = new Room[50, 50];

        if (performOnAwake) GenMap();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) GenMap();
    }

    [ContextMenu("Generate Map")]
    private void GenMap()
    {
        if (mapGenCoroutine != null) StopCoroutine(nameof(MapCoroutine));
        mapGenCoroutine = StartCoroutine(nameof(MapCoroutine));
    }

    private IEnumerator MapCoroutine()
    {
        DespawnMap();
        List<Room> standardRooms = possibleRooms.Where(x => x.RoomType == RoomType.Standard).ToList();

        bool mapGenerated = false;
        while (!mapGenerated)
        {
            DespawnMap();
            List<Room> spawnedRooms = new();

            // Spawn a starting room, set it to the current room
            startRoom = currentRoom = Instantiate(startRooms[UnityEngine.Random.Range(0, startRooms.Length)], transform.position, Quaternion.identity, transform);
            currentRoom.AssignPosition(new(0, 0));
            spawnedRooms.Add(currentRoom);

            // Spawn the appropriate number of rooms
            for (int i = 1; i < numberOfRooms; i++)
            {
                bool compatibleRoomsFound = false;
                List<Room> compatibleRooms = new();

                while (!compatibleRoomsFound)
                {
                    // Look for a room that can be spawned in
                    foreach (Room stanRoom in standardRooms)
                    {
                        if (currentRoom.IsCompatibleWith(stanRoom, out Direction dir))
                        {
                            if (CheckIfRoomPlacingPositionIsEmpty(spawnedRooms, dir) && CheckIfPathsFromRoomAreEmpty(spawnedRooms, stanRoom, dir))
                            {
                                compatibleRooms.Add(stanRoom);
                            }
                        }
                    }

                    // If no room can be spawned, find a different room to spawn a room from
                    if (compatibleRooms.Count <= 0)
                    {
                        if (currentRoom.HasSpotsOpen)
                        {
                            Room replacementRoom = FindReplacementRoom(currentRoom);
                            ReplaceRoom(currentRoom, replacementRoom, spawnedRooms);
                            currentRoom = replacementRoom;
                        }

                        List<Room> roomsWithDoorsAvailable = spawnedRooms.Where(room => room.HasSpotsOpen).ToList();

                        // If a room still cant be found, start over
                        if (roomsWithDoorsAvailable.Count <= 0) break;

                        currentRoom = roomsWithDoorsAvailable[^1];
                    }
                    else compatibleRoomsFound = true;
                }

                // If a room was not found, start completely from scratch
                if (!compatibleRoomsFound) break;

                // Spawn Room and add it to list
                Room newRoom = Instantiate(compatibleRooms[UnityEngine.Random.Range(0, compatibleRooms.Count)], transform);
                spawnedRooms.Add(newRoom);

                // Determine direction and link rooms to each other
                Direction direction = currentRoom.DirectionToUnlinkedRoom(newRoom);
                currentRoom.SetAdjacentRoom(newRoom, direction);
                newRoom.SetAdjacentRoom(currentRoom, direction, true);
                
                // Set new room position
                newRoom.transform.localPosition = (Vector2)currentRoom.transform.localPosition + (DirectionToVectorDirection(direction) * new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y));

                // Set new room grid pos
                newRoom.AssignPosition(currentRoom.RoomNumber + DirectionToVectorDirection(direction));

                // Assign new current room
                currentRoom = newRoom;

                // If the right number of rooms has been spawned, close unused doors, place boss room, and end loop
                if (i == numberOfRooms - 1)
                {
                    if (!CloseUnusedDoors(spawnedRooms)) break;
                    if (!PlaceBossRoom(spawnedRooms)) break;
                    mapGenerated = true;
                }

                //Reset
                compatibleRooms.Clear();
            }

            yield return null;
        }


    }

    private bool CloseUnusedDoors(List<Room> spawnedRooms)
    {
        // Find rooms with unused doors
        List<Room> deadEndRooms = spawnedRooms.Where(room => room.DirectionsFacing.Count > 0).ToList();

        // Replace them with rooms with no unused doors
        foreach (Room deadEndRoom in deadEndRooms)
        {
            Room replacementRoom = FindReplacementRoom(deadEndRoom);

            if (replacementRoom == null) return false;

            ReplaceRoom(deadEndRoom, replacementRoom, spawnedRooms);
        }

        return true;
    }

    private bool PlaceBossRoom(List<Room> spawnedRooms)
    {
        List<Room> deadEndRooms = new();

        // Get rooms with only one neighbor, excluding the start room
        deadEndRooms = spawnedRooms.Where(room => room.Neighbors.Count == 1).ToList();
        if (deadEndRooms.Contains(startRoom)) deadEndRooms.Remove(startRoom);
        deadEndRooms = deadEndRooms.OrderByDescending(room => FindDistanceBetweenTwoRooms(room, startRoom)).ToList();

        // Find one of these rooms to replace with a boss room
        for (int i = 0; i < deadEndRooms.Count; i++)
        {
            foreach (Room bossRoom in bossRooms)
            {
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.North) != null == bossRoom.IsFacingDirection(Direction.North))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.East) != null == bossRoom.IsFacingDirection(Direction.East))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.South) != null == bossRoom.IsFacingDirection(Direction.South))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.West) != null == bossRoom.IsFacingDirection(Direction.West))) continue;

                ReplaceRoom(deadEndRooms[i], bossRoom, spawnedRooms);
                return true;
            }
        }

        return false;
    }

    private void ReplaceRoom(Room ogRoom, Room newRoom, List<Room> spawnedRooms)
    {
        Room replacedmentRoom = Instantiate(newRoom, ogRoom.transform.position, Quaternion.identity, transform);
        replacedmentRoom.AssignPosition(ogRoom.RoomNumber);
        foreach (Room neighbor in ogRoom.Neighbors)
        {
            Direction dir = replacedmentRoom.DirectionToUnlinkedRoom(neighbor);
            replacedmentRoom.SetAdjacentRoom(neighbor, dir);
            neighbor.SetAdjacentRoom(replacedmentRoom, dir, true);
        }

        spawnedRooms.Remove(ogRoom);
        spawnedRooms.Add(replacedmentRoom);
        Destroy(ogRoom.gameObject);

        return;
    }

    private Room FindReplacementRoom(Room currentRoom)
    {

        foreach (Room r in possibleRooms)
        {
            if (!(currentRoom.GetAdjacentRoom(Direction.North) != null == r.IsFacingDirection(Direction.North))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.East) != null == r.IsFacingDirection(Direction.East))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.South) != null == r.IsFacingDirection(Direction.South))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.West) != null == r.IsFacingDirection(Direction.West))) continue;

            return r;
        }

        return null;
    }

    private bool CheckIfRoomPlacingPositionIsEmpty(List<Room> spawnedRooms, Direction dir)
    {
        foreach (Room room in spawnedRooms)
        {
            if (room.RoomNumber == currentRoom.RoomNumber + DirectionToVectorDirection(dir))
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckIfPathsFromRoomAreEmpty(List<Room> spawnedRooms, Room stanRoom, Direction dir)
    {
        foreach (Direction roomDirection in stanRoom.DirectionsFacing)
        {
            if (-DirectionToVectorDirection(roomDirection) != DirectionToVectorDirection(dir))
            {
                foreach (Room room in spawnedRooms)
                {
                    if (room.RoomNumber == (currentRoom.RoomNumber + DirectionToVectorDirection(dir)) + DirectionToVectorDirection(roomDirection))
                    {

                        return false;
                    }
                }
            }
        }

        return true;
    }

    [ContextMenu("Despawn Map")]
    private void DespawnMap()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    private Vector2 GetRoomPosInGrid(Room roomToCheck)
    {
        for (int i = 0; i < roomGrid.GetLength(0); i++)
        {
            for (int j = 0;  j < roomGrid.GetLength(1); j++)
            {
                if (roomGrid[i,j] == roomToCheck)  return new(i,j);
            }
        }

        return Vector2.negativeInfinity;
    }

    #region Utility Classes

    private Direction VectorDirectionToDirection(Vector2Int directionVector)
        => directionVector switch
        {
            var _ when directionVector == Vector2Int.up => Direction.North,
            var _ when directionVector == Vector2Int.right => Direction.East,
            var _ when directionVector == Vector2Int.down => Direction.South,
            var _ when directionVector == Vector2Int.left => Direction.West,
            _ => throw new System.NotImplementedException(),
        };

    private Direction VectorDirectionToDirection(Vector2 directionVector)
        => directionVector switch
        {
            var _ when directionVector == Vector2Int.up => Direction.North,
            var _ when directionVector == Vector2Int.right => Direction.East,
            var _ when directionVector == Vector2Int.down => Direction.South,
            var _ when directionVector == Vector2Int.left => Direction.West,
            _ => throw new System.NotImplementedException(),
        };

    private Vector2Int DirectionToVectorDirection(Direction dir)
        => dir switch
        {
            Direction.North => Vector2Int.up,
            Direction.East => Vector2Int.right,
            Direction.South => Vector2Int.down,
            Direction.West => Vector2Int.left,
            _ => throw new System.NotImplementedException(),
        };

    private float FindDistanceBetweenTwoRooms(Room room1, Room room2)
    {
        return Mathf.Sqrt(Mathf.Pow(room2.RoomNumber.x - room1.RoomNumber.x, 2) + Mathf.Pow(room2.RoomNumber.y - room1.RoomNumber.y, 2));
        //return Vector2Int.Distance(room1.RoomNumber, room2.RoomNumber);
    }

    #endregion
}

public enum Direction
{
    North,
    East,
    South,
    West
}
