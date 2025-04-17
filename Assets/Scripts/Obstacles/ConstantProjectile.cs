using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ConstantProjectile : MonoBehaviour
{
    [SerializeField] private float disappearTime;
    [SerializeField] private Vector2 targetVelocity;
    [SerializeField] private float acceleration;

    [Space(15)]
    [SerializeField] private bool excludeXVelocity;
    [SerializeField] private bool excludeYVelocity;

    private float disappearTimer;
    private Vector2 velocity;

    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);

        disappearTimer = disappearTime;
    }

    private void Update()
    {
        if (!excludeXVelocity && !excludeYVelocity)
        {
            if (velocity != targetVelocity)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity.x, acceleration * Time.deltaTime);
                velocity.y = Mathf.MoveTowards(velocity.y, targetVelocity.y, acceleration * Time.deltaTime);
                rb.velocity = velocity * transform.right;
            }
        }
        else if (!excludeXVelocity)
        {
            if (velocity.x != targetVelocity.x)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity.x, acceleration * Time.deltaTime);
                velocity.y = rb.velocity.y;
                rb.velocity = new((velocity * transform.right).x, velocity.y);
            }
        }
        else if (!excludeYVelocity)
        {
            if (velocity.x != targetVelocity.x)
            {
                velocity.y = Mathf.MoveTowards(velocity.y, targetVelocity.y, acceleration * Time.deltaTime);
                velocity.x = rb.velocity.x;
                rb.velocity = new(velocity.x, (velocity * transform.right).y);
            }
        }


        if (disappearTimer > 0)
        {
            disappearTimer -= Time.deltaTime;

            if (disappearTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
