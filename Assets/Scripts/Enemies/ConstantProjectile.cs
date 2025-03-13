using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ConstantProjectile : MonoBehaviour
{
    [SerializeField] private float disappearTime;
    [SerializeField] private Vector2 targetVelocity;
    [SerializeField] private float acceleration;

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
        if (velocity != targetVelocity)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity.x, acceleration * Time.deltaTime);
            velocity.y = Mathf.MoveTowards(velocity.y, targetVelocity.y, acceleration * Time.deltaTime);
            rb.velocity = velocity * transform.right;
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
