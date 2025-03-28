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
    [SerializeField] private bool performOnStart;

    [Header("References")]
    [SerializeField] private RoomSO[] startRooms;
    [SerializeField] private RoomSO[] bossRooms;
    [SerializeField] private List<RoomSO> possibleRooms;

    [Header("Broadcast Events")]
    [SerializeField] private RoomListEventSO mapLayoutMadeSO;
    [SerializeField] private VoidEventSO mapGenerationFinishedSO;

    private Room startRoom;
    private Room currentRoom;

    private Coroutine mapGenCoroutine;

    private void Start()
    {
        if (performOnStart) GenMap();
    }

    // Uncomment to allow generating the map with a button
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Return)) GenMap();
    //}

    #region Generation

    [ContextMenu("Generate Map")]
    private void GenMap()
    {
        if (mapGenCoroutine != null) StopCoroutine(nameof(MapCoroutine));
        mapGenCoroutine = StartCoroutine(nameof(MapCoroutine));
    }

    private IEnumerator MapCoroutine()
    {
        DespawnMap();
        List<RoomSO> standardRooms = possibleRooms.Where(x => x.RoomType == RoomType.Standard).ToList();

        bool mapGenerated = false;
        while (!mapGenerated)
        {
            DespawnMap();
            List<Room> spawnedRooms = new();

            // Spawn a starting room, set it to the current room
            startRoom = currentRoom = new(startRooms[UnityEngine.Random.Range(0, startRooms.Length)]);
            currentRoom.AssignPosition(new(0, 0));
            spawnedRooms.Add(currentRoom);

            // Spawn the appropriate number of rooms
            for (int i = 1; i < numberOfRooms; i++)
            {
                bool compatibleRoomsFound = false;
                List<RoomSO> compatibleRooms = new();

                while (!compatibleRoomsFound)
                {
                    // Look for a room that can be spawned in
                    foreach (RoomSO stanRoom in standardRooms)
                    {
                        if (currentRoom.IsCompatibleWith(stanRoom, out Direction dir))
                        {
                            if (CheckIfRoomPlacingPositionIsEmpty(spawnedRooms, dir)/* && CheckIfPathsFromRoomAreEmpty(spawnedRooms, stanRoom, dir)*/)
                            {
                                compatibleRooms.Add(stanRoom);
                            }
                        }
                    }

                    // If no room can be spawned, find a different room to spawn a room from
                    if (compatibleRooms.Count <= 0)
                    {
                        if (currentRoom.DirectionsWithAnUnusedDoor.Count > 0 && currentRoom != null)
                        {
                            Room replacementRoom = FindReplacementRoom(currentRoom);
                            if (replacementRoom == null) break;
                            ReplaceRoom(currentRoom, replacementRoom, spawnedRooms);
                            currentRoom = replacementRoom;
                        }

                        List<Room> roomsWithDoorsAvailable = spawnedRooms.Where(room => room.DirectionsWithAnUnusedDoor.Count > 0).ToList();

                        // If a room still cant be found, start over
                        if (roomsWithDoorsAvailable.Count <= 0) break;

                        currentRoom = roomsWithDoorsAvailable[^1];
                        currentRoom ??= roomsWithDoorsAvailable[0];
                    }
                    else compatibleRoomsFound = true;
                }

                // If a room was not found that was compatible with existing ones, start completely from scratch
                if (!compatibleRoomsFound) break;

                // Spawn Room and add it to list
                Room newRoom = new(compatibleRooms[UnityEngine.Random.Range(0, compatibleRooms.Count)]);
                spawnedRooms.Add(newRoom);

                // Determine direction and link rooms to each other
                Direction direction = currentRoom.DirectionToUnlinkedRoom(newRoom);
                currentRoom.SetAdjacentRoom(newRoom, direction);
                newRoom.SetAdjacentRoom(currentRoom, direction, true);
                
                // Set new room grid pos
                newRoom.AssignPosition(currentRoom.RoomNumber + DirectionToVectorDirection(direction));

                // Assign new current room
                currentRoom = newRoom;

                // If the right number of rooms has been spawned, close unused doors, place boss room, spawn all rooms in game, and end loop
                if (i == numberOfRooms - 1)
                {
                    if (!CloseUnusedDoors(spawnedRooms)) break;
                    if (!PlaceBossRoom(spawnedRooms)) break;
                    mapLayoutMadeSO.RaiseEvent(spawnedRooms);
                    SpawnAllRooms(spawnedRooms);
                    mapGenerated = true;
                }

                //Reset
                compatibleRooms.Clear();
            }

            yield return null;
        }

        mapGenerationFinishedSO.RaiseEvent();
        yield break;
    }

    private bool CloseUnusedDoors(List<Room> spawnedRooms)
    {
        // Find rooms with unused doors
        List<Room> deadEndRooms = spawnedRooms.Where(room => room.DirectionsWithAnUnusedDoor.Count > 0).ToList();

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
            foreach (RoomSO bossRoom in bossRooms)
            {
                // If the deadend room having a neighbor and the boss room having an unused door aren't the same sign, this isnt a fitting boss room
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.North) != null == bossRoom.HasAnUnusedDoorInThisDirection(Direction.North))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.East) != null == bossRoom.HasAnUnusedDoorInThisDirection(Direction.East))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.South) != null == bossRoom.HasAnUnusedDoorInThisDirection(Direction.South))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.West) != null == bossRoom.HasAnUnusedDoorInThisDirection(Direction.West))) continue;

                ReplaceRoom(deadEndRooms[i], new(bossRoom), spawnedRooms);
                return true;
            }
        }

        return false;
    }

    private void SpawnAllRooms(List<Room> spawnedRooms)
    {
        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            if (spawnedRooms[i].Data.Prefabs.Length <= 0) Debug.LogError(spawnedRooms[i].Data + " does not have any prefabs listed");

            GameObject roomToSpawn = spawnedRooms[i].Data.Prefabs[UnityEngine.Random.Range(0, spawnedRooms[i].Data.Prefabs.Length)];
            Instantiate(roomToSpawn, spawnedRooms[i].RoomNumber * spawnedRooms[i].Data.GetRoomBounds(roomToSpawn), Quaternion.identity, transform);
        }
    }

    #endregion

    #region Map Editing

    [ContextMenu("Despawn Map")]
    private void DespawnMap()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    private void ReplaceRoom(Room ogRoom, Room newRoom, List<Room> spawnedRooms)
    {
        Room replacedmentRoom = newRoom;
        replacedmentRoom.AssignPosition(ogRoom.RoomNumber);
        foreach (Room neighbor in ogRoom.Neighbors)
        {
            Direction dir = replacedmentRoom.DirectionToUnlinkedRoom(neighbor);
            replacedmentRoom.SetAdjacentRoom(neighbor, dir);
            neighbor.SetAdjacentRoom(replacedmentRoom, dir, true);
        }

        spawnedRooms.Remove(ogRoom);
        spawnedRooms.Add(replacedmentRoom);
        //Destroy(ogRoom.gameObject);

        return;
    }

    private Room FindReplacementRoom(Room currentRoom)
    {

        foreach (RoomSO r in possibleRooms)
        {
            if (!(currentRoom.GetAdjacentRoom(Direction.North) != null == r.HasAnUnusedDoorInThisDirection(Direction.North))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.East) != null == r.HasAnUnusedDoorInThisDirection(Direction.East))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.South) != null == r.HasAnUnusedDoorInThisDirection(Direction.South))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.West) != null == r.HasAnUnusedDoorInThisDirection(Direction.West))) continue;

            return new(r);
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
        foreach (Direction roomDirection in stanRoom.DirectionsWithAnUnusedDoor)
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

    #endregion

    #region Utility Methods

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
