using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private Transform miniIconParent;
    [SerializeField] private Image miniIconPrefab;
    [SerializeField] private GameObject almaIcon;

    public Vector2Int RoomNum => associatedRoom.RoomNumber;
    public Vector2 Size => rectTransform.sizeDelta;
    public bool IconActive => image.enabled;

    private Image image;
    private RectTransform rectTransform;
    private Room associatedRoom;

    private void Awake()
    {
        TryGetComponent(out image);
        TryGetComponent(out rectTransform);
    }

    public void Init(Room room, Sprite miniIcon = null)
    {
        associatedRoom = room;
        image.sprite = room.Data.Icon;
        if (miniIcon != null)
        {
            Image mIcon = Instantiate(miniIconPrefab, miniIconParent);
            mIcon.sprite = miniIcon;
        }

        rectTransform.localPosition = RoomNum * rectTransform.sizeDelta;
    }

    public void Activate()
    {
        if (!image.enabled) image.enabled = true;

        almaIcon.SetActive(true);
    }

    public void Deactivate()
    {
        almaIcon.SetActive(false);
    }
}
