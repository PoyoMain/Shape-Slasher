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

    [Header("References")]
    [SerializeField] private Room[] startRooms;
    [SerializeField] private Room[] bossRooms;
    [SerializeField] private List<Room> possibleRooms;

    private Room startRoom;
    private List<Room> rooms;
    private Room bossRoom;

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

    }

    [ContextMenu("Generate Map")]
    private void GenMap()
    {
        if (mapGenCoroutine != null) StopCoroutine(nameof(MapCoroutine));
        mapGenCoroutine = StartCoroutine(nameof(MapCoroutine));
        
    }
    private IEnumerator MapGenCoroutine()
    {
        DespawnMap();
        List<Room> standardRooms = possibleRooms.Where(x => x.RoomType == RoomType.Standard).ToList();

        bool roomGenerated = false;
        while (!roomGenerated)
        {
            DespawnMap();
            RoomAtCurrentPos = Instantiate(startRooms[UnityEngine.Random.Range(0, startRooms.Length)], transform.position, Quaternion.identity);
            RoomAtCurrentPos.SetUpRoom(currentGridPos);

            for (int i = 1; i < numberOfRooms; i++)
            {
                List<Room> compatibleRooms = new();
                List<Door> validPrevDoors = new();

                foreach (Door oldDoor in RoomAtCurrentPos.OpenDoors)
                {
                    if (oldDoor.Direction == Direction.North && roomGrid[currentGridPos.x, currentGridPos.y + 1] == null) validPrevDoors.Add(oldDoor);
                    else if (oldDoor.Direction == Direction.East && roomGrid[currentGridPos.x + 1, currentGridPos.y] == null) validPrevDoors.Add(oldDoor);
                    else if (oldDoor.Direction == Direction.South && roomGrid[currentGridPos.x, currentGridPos.y - 1] == null) validPrevDoors.Add(oldDoor);
                    else if (oldDoor.Direction == Direction.West && roomGrid[currentGridPos.x - 1, currentGridPos.y] == null) validPrevDoors.Add(oldDoor);
                }

                foreach (Room stanRoom in standardRooms)
                {
                    foreach (Door vDoor in validPrevDoors)
                    {
                        foreach (Door newDoor in stanRoom.Doors)
                        {
                            if (vDoor.IsOpposite(newDoor, out Direction _)) compatibleRooms.Add(stanRoom);
                        }
                    }
                }

                if (compatibleRooms.Count <= 0)
                {
                    currentGridPos.Set(25,25);
                    compatibleRooms.Clear();
                    validPrevDoors.Clear();
                    break;
                }

                Room roomToSpawn = compatibleRooms[UnityEngine.Random.Range(0, compatibleRooms.Count)];

                Room spawnedRoom = Instantiate(roomToSpawn, (Vector2)RoomAtCurrentPos.transform.localPosition + new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y), Quaternion.identity);
                Vector2Int dir = RoomAtCurrentPos.LinkRoom(spawnedRoom);
                
                spawnedRoom.transform.localPosition = (Vector2)RoomAtCurrentPos.transform.localPosition + (dir * new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y));
                currentGridPos += dir;
                RoomAtCurrentPos = spawnedRoom;
                spawnedRoom.SetUpRoom(currentGridPos);

                compatibleRooms.Clear();
                validPrevDoors.Clear();

                if (i == numberOfRooms - 1)
                {
                    roomGenerated = true;
                    currentGridPos.Set(25, 25);
                }
            }

            yield return null;
        }
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

            currentRoom = Instantiate(startRooms[UnityEngine.Random.Range(0, startRooms.Length)], transform.position, Quaternion.identity, transform);
            currentRoom.AssignPosition(new(0, 0));
            spawnedRooms.Add(currentRoom);

            for (int i = 1; i < numberOfRooms; i++)
            {
                bool compatibleRoomsFound = false;
                List<Room> compatibleRooms = new();

                while (!compatibleRoomsFound)
                {
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


                    if (compatibleRooms.Count <= 0)
                    {
                        if (currentRoom.HasSpotsOpen)
                        {
                            Room replacementRoom = Instantiate(FindReplacementRoom(currentRoom), currentRoom.transform.position, Quaternion.identity, transform);
                            replacementRoom.AssignPosition(currentRoom.RoomNumber);
                            foreach (Room neighbor in currentRoom.Neighbors)
                            {
                                Direction dir = replacementRoom.DirectionToUnlinkedRoom(neighbor);
                                replacementRoom.SetAdjacentRoom(neighbor, dir);
                                neighbor.SetAdjacentRoom(replacementRoom, dir, true);
                            }

                            spawnedRooms.Remove(currentRoom);
                            spawnedRooms.Add(replacementRoom);
                            Destroy(currentRoom.gameObject);
                            currentRoom = replacementRoom;
                        }

                        List<Room> roomsWithDoorsAvailable = spawnedRooms.Where(room => room.HasSpotsOpen).ToList();

                        if (roomsWithDoorsAvailable.Count <= 0) break;

                        currentRoom = roomsWithDoorsAvailable[^1];
                    }
                    else compatibleRoomsFound = true;
                }

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

                // If the right number of rooms has been spawned, end loop
                if (i == numberOfRooms - 1)
                {
                    mapGenerated = true;
                }

                //Reset
                compatibleRooms.Clear();
            }

            yield return null;
        }
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

    #endregion
}

public enum Direction
{
    North,
    East,
    South,
    West
}
