using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornBall : MonoBehaviour
{
    [SerializeField] private CircleCollider2D coll;

    private void Update()
    {
        if (CheckForWall()) Destroy(gameObject);
    }

    private bool CheckForWall()
    {
        return Physics2D.Raycast(coll.bounds.center, Vector2.left, coll.radius + 0.01f);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(coll.bounds.center, Vector2.left);
    }
}
