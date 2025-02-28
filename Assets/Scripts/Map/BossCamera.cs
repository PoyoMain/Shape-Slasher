using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCamera : MonoBehaviour
{
    [SerializeField] private bool lockYPosition;

    private float yPosition;

    private void Awake()
    {
        if (lockYPosition) yPosition = transform.position.y;
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
