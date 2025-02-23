using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCameraActivator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CinemachineVirtualCamera cam;

    private bool isColliding;
    private Coroutine resetCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (isColliding) return;

            isColliding = true;
            Invoke(nameof(ResetStuff), Time.deltaTime);
            //if (resetCoroutine != null) return;
            //else resetCoroutine = StartCoroutine(nameof(ResetStuff));

            cam.enabled = true;
            cam.Follow = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (isColliding) return;

            isColliding = true;
            Invoke(nameof(ResetStuff), Time.deltaTime);
            //isColliding = false;
            cam.enabled = false;
        }
    }

    private void ResetStuff()
    { 
        isColliding = false;
        return;
    }

}
