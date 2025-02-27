using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Golem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health;
    [SerializeField] private float invincibilityTime;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float startPatrolTime;
    [SerializeField] private float turnTime;

    [Header("Attacking")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackDistance;
    [SerializeField] private float playerBehindDistance;
    [SerializeField] private float attackCooldownTime;

    [Header("Collisions")]
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private LayerMask groundLayers;

    [Header("Raycasts")]
    [SerializeField] private Transform raycastPoint;
    [SerializeField] private float groundRaycastDistance;
    [SerializeField] private float wallRaycastDistance;

    // Constants
    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;

    // Private Variables
    private State state = State.Patroling;
    private Rigidbody2D rigid;
    private Animator anim;

    private void Awake()
    {
        TryGetComponent(out rigid);
        TryGetComponent(out anim);

        ChangeState(State.Patroling);
    }

    private void FixedUpdate()
    {
        if (state == State.Patroling)
        {
            PatrolState();
        }
        else if (state == State.Attacking)
        {
            AttackState();
        }
        else if (state == State.Turning)
        {
            TurnState();
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.Patroling:
                StopMoving();
                startPatrollingTimer = startPatrolTime;
                break;
            case State.Attacking:
                StopMoving();
                break;
            case State.Turning:
                StopMoving();
                turnTimer = turnTime;
                break;
        }
    }

    #region Patrolling

    private float startPatrollingTimer;
    private bool facingRight = true;

    private int MoveDirection => facingRight ? 1 : -1;

    private void PatrolState()
    {
        if (CheckForPlayer()) return;
        if (startPatrollingTimer > 0)
        {
            startPatrollingTimer -= Time.fixedDeltaTime;
            return;
        }
        if (CheckForTurn()) return;
        Move();
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

    #region Attacking

    private float attackCooldownTimer;

    private bool AttackOnCooldown => attackCooldownTimer > 0;

    private void AttackState()
    {
        if (AttackOnCooldown)
        {
            attackCooldownTimer -= Time.fixedDeltaTime;
            return;
        }
        if (!CheckForPlayer()) return;

        ExecuteAttack();
    }

    private void ExecuteAttack()
    {
        anim.SetTrigger("Attack");
        attackCooldownTimer = attackCooldownTime;
    }

    #endregion

    #region Collisions

    private float invincibleTimer;
    private bool IsInvincible => invincibleTimer > 0;
    private bool CheckForTurn()
    {
        // Check for ground
        bool groundHit = Physics2D.Raycast(raycastPoint.position, Vector2.down, bodyCollider.size.y / 2 + groundRaycastDistance, groundLayers);
        bool wallHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, MoveDirection * Vector2.right, wallRaycastDistance, groundLayers);

        if (!groundHit || wallHit)
        {
            ChangeState(State.Turning);
        }
        return !groundHit || wallHit;
    }

    private bool CheckForPlayer()
    {
        bool playerHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, MoveDirection * Vector2.right, attackDistance, playerLayer);
        bool playerHitBehind = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, -MoveDirection * Vector2.right, playerBehindDistance, playerLayer);

        if (playerHit && state != State.Attacking) ChangeState(State.Attacking);
        else if (!playerHit && state != State.Patroling) ChangeState(State.Patroling);
        else if (playerHitBehind && state != State.Turning) TurnAround();

        return playerHit;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out DamageComponent damageComponent))
        {
            if (IsInvincible) return;

            TakeDamage(damageComponent.Damage);
        }
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
            ChangeState(State.Patroling);
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

    #region Health & Damage

    private void TakeDamage(int damage)
    {
        health -= damage;

        if (health < 0) Destroy(gameObject);
        else invincibleTimer = invincibilityTime;
    }

    #endregion

    private enum State { Patroling, Attacking, Turning }
}
