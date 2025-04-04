using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHandler : MonoBehaviour
{
    [Header("Room Icon Stuff")]
    [SerializeField] private MinimapIcon roomIconPrefab;
    [SerializeField] private RectTransform minimapParent;

    [Header("Room Mini Icons")]
    [SerializeField] private Sprite bossRoomMiniIcon;

    [Header("Listen Events")]
    [SerializeField] private RoomListEventSO mapLayoutMadeSO;
    [SerializeField] private VoidEventSO mapDespawnedSO;
    [SerializeField] private Vector2EventSO playerEnteredRoomSO;
    [SerializeField] private Vector2EventSO playerExitedRoomSO;

    private List<MinimapIcon> minimapIcons;
    private Vector2 previousRoomNum;

    private void OnEnable()
    {
        mapLayoutMadeSO.OnEventRaised += GenerateMinimap;
        mapDespawnedSO.OnEventRaised += DespawnMinimap;
        playerEnteredRoomSO.OnEventRaised += ActivateRoomIcon;
        playerExitedRoomSO.OnEventRaised += DeactivateRoomIcon;
    }

    private void OnDisable()
    {
        mapLayoutMadeSO.OnEventRaised -= GenerateMinimap;
        mapDespawnedSO.OnEventRaised -= DespawnMinimap;
        playerEnteredRoomSO.OnEventRaised -= ActivateRoomIcon;
        playerExitedRoomSO.OnEventRaised -= DeactivateRoomIcon;
    }

    private void Awake()
    {
        minimapIcons = new();
    }

    private void GenerateMinimap(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            MinimapIcon roomIcon = Instantiate(roomIconPrefab, minimapParent);
            if (rooms[i].Data.RoomType == RoomType.Boss) roomIcon.Init(rooms[i], bossRoomMiniIcon);
            else roomIcon.Init(rooms[i]);

            minimapIcons.Add(roomIcon);
        }
    }

    private void DespawnMinimap()
    {
        foreach(Transform t in minimapParent)
        {
            Destroy(t.gameObject);
        }

        minimapIcons.Clear();
    }


    private void ActivateRoomIcon(Vector2 numToCheck)
    {
        for(int i = 0; i < minimapIcons.Count; i++)
        {
            minimapIcons[i].Deactivate();
        }

        MinimapIcon iconToActivate = minimapIcons.Find(x => x.RoomNum == numToCheck);
        iconToActivate.Activate();

        minimapParent.localPosition = -(numToCheck * iconToActivate.Size);
    }

    private void DeactivateRoomIcon(Vector2 numToCheck)
    {
        MinimapIcon iconToDeactivate = minimapIcons.Find(x => x.RoomNum == numToCheck);
        iconToDeactivate.Deactivate();

        MinimapIcon iconToActivate = minimapIcons.Find(x => x.RoomNum == previousRoomNum);
        iconToActivate.Activate();

        minimapParent.localPosition = -(numToCheck * iconToActivate.Size);
        previousRoomNum = iconToActivate.RoomNum;
    }
}
