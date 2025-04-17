using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int numberOfRooms;
    //[SerializeField] private int numberOfNeighborsAllowed;
    [SerializeField] private bool canReuseRooms;
    [SerializeField] private bool canHaveDuplicatesInARow;
    [SerializeField] private bool performOnStart;

    [Header("Room Specific Settings")]
    [SerializeField] private bool spawnBoss;
    [SerializeField] private bool spawnShop;

    [Header("References")]
    [SerializeField] private RoomSO[] startRooms;
    [SerializeField] private RoomSO[] bossRooms;
    [SerializeField] private RoomSO[] shopRooms;
    [SerializeField] private List<RoomSO> possibleRooms;

    [Header("Broadcast Events")]
    [SerializeField] private RoomListEventSO mapLayoutMadeSO;
    [SerializeField] private VoidEventSO mapGenerationFinishedSO;
    [SerializeField] private VoidEventSO mapDespawedEventSO;

    private Room startRoom;
    private Room bossRoom;
    private Room shopRoom;
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
        List<RoomSO> roomsToPickFrom = possibleRooms.Where(x => (x.RoomType == RoomType.Standard || x.RoomType == RoomType.Challenge) && x.Prefabs.Length > 0).ToList();

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
                    foreach (RoomSO stanRoom in roomsToPickFrom)
                    {
                        if (!canHaveDuplicatesInARow && currentRoom.Data == stanRoom)
                        {
                            continue;
                        }

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
                    if (spawnBoss) if (!PlaceBossRoom(spawnedRooms)) break;
                    if (spawnShop) if (!PlaceShopRoom(spawnedRooms)) break;
                    if (!SpawnAllRooms(spawnedRooms)) break;
                    print("Distance to Boss: " + FindDistanceFromRoom(startRoom, bossRoom, 0));
                    mapLayoutMadeSO.RaiseEvent(spawnedRooms);
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
        deadEndRooms = deadEndRooms.OrderByDescending(room => FindDistanceFromRoom(startRoom, room, 0) + FindDistanceFromStartRoom(room)).ToList();

        // Find one of these rooms to replace with a boss room
        for (int i = 0; i < deadEndRooms.Count; i++)
        {
            foreach (RoomSO bossRoomSO in bossRooms)
            {
                // If the deadend room having a neighbor and the boss room having an unused door aren't the same sign, this isnt a fitting boss room
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.North) != null == bossRoomSO.HasAnUnusedDoorInThisDirection(Direction.North))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.East) != null == bossRoomSO.HasAnUnusedDoorInThisDirection(Direction.East))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.South) != null == bossRoomSO.HasAnUnusedDoorInThisDirection(Direction.South))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.West) != null == bossRoomSO.HasAnUnusedDoorInThisDirection(Direction.West))) continue;

                if (!deadEndRooms[i].CanBeReplacedWith(bossRoomSO)) continue;

                bossRoom = ReplaceRoom(deadEndRooms[i], new(bossRoomSO), spawnedRooms);
                return true;
            }
        }

        return false;
    }

    private bool PlaceShopRoom(List<Room> spawnedRooms)
    {
        List<Room> deadEndRooms = new();

        // Get rooms with only one neighbor, excluding the start room
        deadEndRooms = spawnedRooms.Where(room => room.Neighbors.Count == 1 && room.Data.RoomType != RoomType.Boss).ToList();
        if (deadEndRooms.Contains(startRoom)) deadEndRooms.Remove(startRoom);
        deadEndRooms = deadEndRooms.OrderByDescending(room => FindDistanceFromRoom(startRoom, room, 0) + FindDistanceFromStartRoom(room)).ToList();

        // Find one of these rooms to replace with a boss room
        for (int i = 0; i < deadEndRooms.Count; i++)
        {
            foreach (RoomSO shopRoomSO in shopRooms)
            {
                // If the deadend room having a neighbor and the boss room having an unused door aren't the same sign, this isnt a fitting boss room
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.North) != null == shopRoomSO.HasAnUnusedDoorInThisDirection(Direction.North))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.East) != null == shopRoomSO.HasAnUnusedDoorInThisDirection(Direction.East))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.South) != null == shopRoomSO.HasAnUnusedDoorInThisDirection(Direction.South))) continue;
                if (!(deadEndRooms[i].GetAdjacentRoom(Direction.West) != null == shopRoomSO.HasAnUnusedDoorInThisDirection(Direction.West))) continue;

                if (!deadEndRooms[i].CanBeReplacedWith(shopRoomSO)) continue;

                shopRoom = ReplaceRoom(deadEndRooms[i], new(shopRoomSO), spawnedRooms);
                return true;
            }
        }

        return false;
    }

    private bool SpawnAllRooms(List<Room> spawnedRooms)
    {
        List<string> instantiatedRooms = new();

        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            if (spawnedRooms[i].Data.Prefabs.Length <= 0) Debug.LogError(spawnedRooms[i].Data + " does not have any prefabs listed");

            List<GameObject> prefabs = spawnedRooms[i].Data.Prefabs.ToList();
            if (!canReuseRooms) prefabs.RemoveAll(x => instantiatedRooms.Contains(x.name));

            if (prefabs.Count == 0) return false;

            GameObject roomToSpawn = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
            Instantiate(roomToSpawn, spawnedRooms[i].RoomNumber * spawnedRooms[i].Data.GetRoomBounds(roomToSpawn), Quaternion.identity, transform);
            instantiatedRooms.Add(roomToSpawn.name);
        }

        return true;
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

        mapDespawedEventSO.RaiseEvent();
    }

    private Room ReplaceRoom(Room ogRoom, Room newRoom, List<Room> spawnedRooms)
    {
        Room replacementRoom = newRoom;
        replacementRoom.AssignPosition(ogRoom.RoomNumber);
        foreach (Room neighbor in ogRoom.Neighbors)
        {
            Direction dir = replacementRoom.DirectionToUnlinkedRoom(neighbor);
            replacementRoom.SetAdjacentRoom(neighbor, dir);
            neighbor.SetAdjacentRoom(replacementRoom, dir, true);
        }

        spawnedRooms.Remove(ogRoom);
        spawnedRooms.Add(replacementRoom);
        //Destroy(ogRoom.gameObject);

        return replacementRoom;
    }

    private Room FindReplacementRoom(Room currentRoom)
    {

        foreach (RoomSO r in possibleRooms)
        {
            if (!(currentRoom.GetAdjacentRoom(Direction.North) != null == r.HasAnUnusedDoorInThisDirection(Direction.North))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.East) != null == r.HasAnUnusedDoorInThisDirection(Direction.East))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.South) != null == r.HasAnUnusedDoorInThisDirection(Direction.South))) continue;
            if (!(currentRoom.GetAdjacentRoom(Direction.West) != null == r.HasAnUnusedDoorInThisDirection(Direction.West))) continue;

            if (!currentRoom.CanBeReplacedWith(r)) continue;

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

    private float FindDistanceFromStartRoom(Room room)
    {
        return Mathf.Sqrt(Mathf.Pow(startRoom.RoomNumber.x - room.RoomNumber.x, 2) + Mathf.Pow(startRoom.RoomNumber.y - room.RoomNumber.y, 2));
    }

    private int FindDistanceFromRoom(Room root, Room target, int level, Room prevRoom = null) 
    {
        if (root == null) return -1;
        if (root == target) return level;

        List<Room> neighborsToCheck = root.Neighbors;
        if (prevRoom != null) neighborsToCheck.Remove(prevRoom);

        for (int i = 0; i < neighborsToCheck.Count; i++)
        {
            int result = FindDistanceFromRoom(neighborsToCheck[i], target, level + 1, root);
            if (result != -1) return result;
        }

        return -1;
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
