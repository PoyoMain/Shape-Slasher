using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomEnterBehavior : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera cam;

    [Header("Broadcast Events")]
    [SerializeField] private Vector2EventSO playerEnterRoomSO;

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
        if (!TryGetComponent(out BoxCollider2D coll)) return Vector2.negativeInfinity;

        return transform.position / coll.size;

    }

}
