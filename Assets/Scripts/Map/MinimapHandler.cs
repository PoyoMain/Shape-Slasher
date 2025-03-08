using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHandler : MonoBehaviour
{
    [Header("Inspector Objects")]
    [SerializeField] private Image roomIconPrefab;
    [SerializeField] private RectTransform minimapParent;

    [Header("Listen Events")]
    [SerializeField] private RoomListEventSO mapLayoutMadeSO;
    [SerializeField] private Vector2EventSO playerEnteredRoomSO;

    private List<MinimapIcon> minimapIcons;

    private void OnEnable()
    {
        minimapIcons = new();

        mapLayoutMadeSO.OnEventRaised += GenerateMinimap;
        playerEnteredRoomSO.OnEventRaised += ActivateRoomIcon;
    }

    private void OnDisable()
    {
        mapLayoutMadeSO.OnEventRaised -= GenerateMinimap;
        playerEnteredRoomSO.OnEventRaised -= ActivateRoomIcon;
    }

    private void GenerateMinimap(List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Image roomIcon = Instantiate(roomIconPrefab, minimapParent);
            roomIcon.rectTransform.localPosition = rooms[i].RoomNumber * (roomIconPrefab.rectTransform.sizeDelta /*+ new Vector2(spaceBetweenMinimapIcons, spaceBetweenMinimapIcons)*/);
            roomIcon.sprite = rooms[i].Data.Icon;

            MinimapIcon icon = new()
            {
                image = roomIcon,
                associatedRoom = rooms[i]
            };
            minimapIcons.Add(icon);
        }
    }

    private void ActivateRoomIcon(Vector2 numToCheck)
    {
        MinimapIcon icon = minimapIcons.Find(x => x.RoomNum == numToCheck);

        if (!icon.image.enabled) icon.image.enabled = true;
    }

    private struct MinimapIcon
    {
        public readonly Vector2Int RoomNum => associatedRoom.RoomNumber;

        public Image image;
        public Room associatedRoom;
    }
}
