using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Golem : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float turnTime;

    [Header("Collisions")]
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private LayerMask groundLayers;

    [Header("Raycasts")]
    [SerializeField] private Transform raycastPoint;
    [SerializeField] private float groundRaycastDistance;
    [SerializeField] private float wallRaycastDistance;

    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;

    private State state = State.Patroling;
    private enum State { Patroling, Attacking, Turning }

    private Rigidbody2D rigid;

    private void Awake()
    {
        TryGetComponent(out rigid);
    }

    private void FixedUpdate()
    {
        if (state == State.Patroling)
        {
            PatrolState();
        }
        else if (state == State.Attacking)
        {

        }
        else if (state == State.Turning)
        {
            TurnState();
        }
    }

    #region Patrolling

    private bool facingRight = true;
    private int MoveDirection => facingRight ? 1 : -1;
    private void PatrolState()
    {
        if (CheckForTurn()) return;
        Move();
    }

    private bool CheckForTurn()
    {
        // Check for ground
        bool groundHit = Physics2D.Raycast(raycastPoint.position, Vector2.down, bodyCollider.size.y / 2 + groundRaycastDistance, groundLayers);
        bool wallHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, MoveDirection * Vector2.right, wallRaycastDistance, groundLayers);

        if (!groundHit || wallHit)
        {
            state = State.Turning;
            turnTimer = turnTime;
            StopMoving();
        }
        return !groundHit || wallHit;
    }

    private void Move()
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, MoveDirection * patrolSpeed, 150 * Time.fixedDeltaTime);
        rigid.velocity = velocity;
    }

    private void StopMoving()
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = 0;
        rigid.velocity = velocity;
    }

    #endregion

    #region Turning

    private float turnTimer;
    private void TurnState()
    {
        if (turnTimer > 0) turnTimer -= Time.fixedDeltaTime;
        else
        {
            TurnAround();
            state = State.Patroling;
        }
    }

    private void TurnAround()
    {
        Vector3 euler = transform.localEulerAngles;
        if (euler.y == ROTATION_FACINGLEFT) euler.y = ROTATION_FACINGRIGHT;
        else euler.y = ROTATION_FACINGLEFT;
        transform.localEulerAngles = euler;

        facingRight = !facingRight;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(raycastPoint.position, Vector2.down);
    }
}
