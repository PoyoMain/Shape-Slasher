using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class BossCamera : MonoBehaviour
{
    [SerializeField] private bool lockYPosition;

    private float yPosition;
    private CinemachineVirtualCamera cam;

    private void OnEnable()
    {
        if (lockYPosition) yPosition = transform.position.y;

        TryGetComponent(out cam);
        cam.Follow = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (lockYPosition == !lockYPosition) yPosition = GetCameraYPositon();

        if (lockYPosition)
        {
            Vector2 newPos = transform.position;
            newPos.y = yPosition;
            transform.position = newPos;
        }
    }

    private float GetCameraYPositon()
    {
        return transform.position.y;
    }
}
