using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private Axis axis;
    private enum Axis { Horizontal, Vertical } 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (axis == Axis.Horizontal)
        {
            SendMessageUpwards("WallKnockback", collision.contacts[0].point);
        }
    }
}
