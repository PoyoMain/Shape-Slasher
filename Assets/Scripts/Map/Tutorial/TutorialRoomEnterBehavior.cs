using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialRoomEnterBehavior : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera cam;

    [Header("Room Num")]
    [SerializeField] private Vector2 roomNum;

    [Header("Broadcast Events")]
    [SerializeField] private Vector2EventSO playerEnterRoomSO;

    [SerializeField] private UnityEvent onRoomEnter;

    private bool isColliding;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Transform playerCamFocus = collision.transform;
            for (int i = 0; i < collision.transform.childCount; i++)
            {
                if (collision.transform.GetChild(i).CompareTag("PlayerCamFocus"))
                {
                    playerCamFocus = collision.transform.GetChild(i);
                    break;
                }

                if (i == collision.transform.childCount - 1) return;
            }

            if (isColliding) return;

            isColliding = true;
            Invoke(nameof(ResetStuff), Time.deltaTime);

            cam.enabled = true;
            cam.Follow = playerCamFocus;
            onRoomEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            for (int i = 0; i < collision.transform.childCount; i++)
            {
                if (collision.transform.GetChild(i).CompareTag("PlayerCamFocus")) break;

                if (i == collision.transform.childCount - 1) return;
            }

            if (isColliding) return;

            isColliding = true;
            Invoke(nameof(ResetStuff), Time.deltaTime);
            cam.enabled = false;
        }
    }

    private void ResetStuff()
    {
        isColliding = false;
        return;
    }

    public void ActivateRoom()
    {
        playerEnterRoomSO.RaiseEvent(GetRoomNumber());
    }

    private Vector2 GetRoomNumber()
    {
        return roomNum;
    }

}