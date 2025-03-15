using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private Axis axis;

    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;
    private enum Axis { Horizontal, Vertical } 

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (axis == Axis.Horizontal)
        {
            if (transform.eulerAngles.y == ROTATION_FACINGLEFT) SendMessageUpwards("HitboxKnockback", Vector2.right);
            else if (transform.eulerAngles.y == ROTATION_FACINGRIGHT) SendMessageUpwards("HitboxKnockback", Vector2.left);
        }
    }
}
