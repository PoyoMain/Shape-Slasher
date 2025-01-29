using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int numberOfRooms;
    [SerializeField] private int numberOfNeighborsAllowed;
    [SerializeField] private bool canReuseRooms;

    [Header("References")]
    [SerializeField] private Room[] startRooms;
    [SerializeField] private Room[] bossRooms;
    [SerializeField] private List<Room> possibleRooms;
    [SerializeField] private Room roomPrefab;

    private Room startRoom;
    private List<Room> rooms;
    private Room bossRoom;

    private Room[,] roomGrid = new Room[11,11];
    private Vector2Int currentGridPos = new(6, 6);

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
        //spawnedRooms = new();
    }

    private void GenerateMap()
    {
        DespawnMap();

        //List<Room> startingRooms = possibleRooms.Where(x => x.RoomType == RoomType.Starting).ToList();
        //List<Room> bossRooms = possibleRooms.Where(x => x.RoomType == RoomType.Boss).ToList();
        List<Room> standardRooms = possibleRooms.Where(x => x.RoomType == RoomType.Standard).ToList();

        //Room roomToSpawn = startingRooms[UnityEngine.Random.Range(0, startingRooms.Count)];
        //Room lastRoomSpawned = Instantiate(roomToSpawn, transform.position, Quaternion.identity);
        //spawnedRooms.Add(lastRoomSpawned);

        //for (int i = 1; i < standardRooms.Count - 1; i++)
        //{
        //    List<Room> correctRooms = standardRooms.Where(x => lastRoomSpawned.Compatible(x)).ToList();

        //    if (correctRooms.Count == 0)
        //    {
        //        DespawnMap();
        //        return false;
        //    }

        //    roomToSpawn = correctRooms[UnityEngine.Random.Range(0, correctRooms.Count)];
        //    Vector2 directionToSpawnIn = lastRoomSpawned.DirectionToRoom(roomToSpawn);
        //    lastRoomSpawned = Instantiate(roomToSpawn, (Vector2)transform.localPosition + (directionToSpawnIn * new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y)), Quaternion.identity);
        //    spawnedRooms.Add(lastRoomSpawned);
        //}

        //List<Room> potentialBossRooms = standardRooms.Where(x => lastRoomSpawned.Doors.Equals(x.Doors)).ToList();

        //if (potentialBossRooms.Count == 0)
        //{
        //    DespawnMap();
        //    return false;
        //}

        //roomToSpawn = potentialBossRooms[UnityEngine.Random.Range(0, potentialBossRooms.Count)];
        //lastRoomSpawned = Instantiate(roomToSpawn, transform.position, Quaternion.identity);
        //spawnedRooms.Add(lastRoomSpawned);

        //return true;


        Room lastRoomSpawned = startRoom = Instantiate(startRooms[UnityEngine.Random.Range(0, startRooms.Length)], transform.position, Quaternion.identity);
        lastRoomSpawned.Init();
        roomGrid[currentGridPos.x, currentGridPos.y] = lastRoomSpawned;

        for (int i = 1; i < numberOfRooms; i++)
        {
            List<Room> compatibleRooms = new();
            List<Door> validPrevDoors = new();

            foreach (Door oldDoor in lastRoomSpawned.OpenDoors)
            {
                if (oldDoor.Direction == Direction.North && roomGrid[currentGridPos.x, currentGridPos.y + 1] == null) validPrevDoors.Add(oldDoor);
                else if (oldDoor.Direction == Direction.East && roomGrid[currentGridPos.x + 1, currentGridPos.y] == null) validPrevDoors.Add(oldDoor);
                else if (oldDoor.Direction == Direction.South && roomGrid[currentGridPos.x, currentGridPos.y - 1] == null) validPrevDoors.Add(oldDoor);
                else if (oldDoor.Direction == Direction.West && roomGrid[currentGridPos.x - 1, currentGridPos.y] == null) validPrevDoors.Add(oldDoor);
            }

                // Find Compatible Rooms
                foreach (Room stanRoom in standardRooms)
            {
                foreach (Door vDoor in validPrevDoors)
                {
                    foreach (Door newDoor in stanRoom.Doors)
                    {
                        if (vDoor.IsOpposite(newDoor, out Direction _))
                        {
                            compatibleRooms.Add(stanRoom);
                        }
                    }
                }
            }

            //if (compatibleRooms.Count <= 0)
            //{
            //    DespawnMap();
            //    GenerateMap();
            //    return;
            //}

            Room roomToSpawn = compatibleRooms[UnityEngine.Random.Range(0, compatibleRooms.Count)];
            Vector2Int dir = lastRoomSpawned.LinkRoom(roomToSpawn);
            lastRoomSpawned.Init();
            lastRoomSpawned = Instantiate(roomToSpawn, (Vector2)lastRoomSpawned.transform.position + (dir * new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y)), Quaternion.identity);
            lastRoomSpawned.Init();
            roomGrid[currentGridPos.x + dir.x, currentGridPos.y + dir.y] = lastRoomSpawned;

            compatibleRooms.Clear();
            validPrevDoors.Clear();
        }
    }

    [ContextMenu("Generate Map")]
    private void GenMap()
    {
        if (mapGenCoroutine != null) StopCoroutine(nameof(MapGenCoroutine));
        mapGenCoroutine = StartCoroutine(nameof(MapGenCoroutine));
        
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
            RoomAtCurrentPos.Init();

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
                    currentGridPos.Set(6,6);
                    compatibleRooms.Clear();
                    validPrevDoors.Clear();
                    break;
                }

                Room roomToSpawn = compatibleRooms[UnityEngine.Random.Range(0, compatibleRooms.Count)];

                Room spawnedRoom = Instantiate(roomToSpawn, (Vector2)RoomAtCurrentPos.transform.localPosition + new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y), Quaternion.identity);
                spawnedRoom.Init();

                Vector2Int dir = RoomAtCurrentPos.LinkRoom(spawnedRoom);
                spawnedRoom.transform.localPosition = (Vector2)RoomAtCurrentPos.transform.localPosition + (dir * new Vector2(ROOM_BOUNDS_X, ROOM_BOUNDS_Y));
                currentGridPos += dir;
                RoomAtCurrentPos = spawnedRoom;

                compatibleRooms.Clear();
                validPrevDoors.Clear();

                if (i == numberOfRooms - 1)
                {
                    roomGenerated = true;
                    currentGridPos.Set(6, 6);
                }
            }

            yield return null;
        }
    }

    [ContextMenu("Despawn Map")]
    private void DespawnMap()
    {
        foreach (Room r in roomGrid)
        {
            if (r != null) Destroy(r.gameObject);
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
}

public enum Direction
{
    North,
    East,
    South,
    West
}
