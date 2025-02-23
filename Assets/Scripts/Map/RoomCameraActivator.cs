using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCameraActivator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CinemachineVirtualCamera cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            cam.enabled = true;
            cam.Follow = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.root.CompareTag("Player"))
        {
            cam.enabled = false;
        }
    }

}
