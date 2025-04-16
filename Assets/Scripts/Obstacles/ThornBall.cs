using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornBall : MonoBehaviour
{
    [SerializeField] private CircleCollider2D coll;
    [SerializeField] private LayerMask wallLayer;

    private void Update()
    {
        if (CheckForWall()) Destroy(gameObject);
    }

    private bool CheckForWall()
    {
        return Physics2D.Raycast(coll.bounds.center, Vector2.left, coll.radius + 0.03f, wallLayer);
    }
}
