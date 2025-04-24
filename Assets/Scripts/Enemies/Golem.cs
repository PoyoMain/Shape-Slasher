using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(DamageFlash))]
[RequireComponent(typeof(CinemachineImpulseSource), typeof(EnemyDeathEvent))]
public class Golem : MonoBehaviour, IHasEnergy
{
    [Header("Options")]
    [SerializeField] private OptionsSO options;
    [SerializeField] private bool useOptionsValues;

    [Header("Health")]
    [SerializeField] private DifficultyInt startHealth;
    [SerializeField] private int testStartHealth;
    [SerializeField] private float invincibilityTime;
    [SerializeField] private float knockbackTime;

    [Header("Energy")]
    [SerializeField] private int energyAmountOnHit;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float startPatrolTime;
    [SerializeField] private float turnTime;

    [Header("Attacking")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackDistance;
    [SerializeField] private float playerFrontDistance;
    [SerializeField] private float playerBehindDistance;
    [SerializeField] private float playerAboveDistance;
    [SerializeField] private float playerAboveTime;
    [SerializeField] private float attackMoveSpeed;
    [SerializeField] private float attackCooldownTime;
    [SerializeField, Range(0f, 1f)] private float counterAttackChance;
    [SerializeField] private float timeToLosePlayer;

    [Header("Defending")]
    [SerializeField] private float defendTime;

    [Header("Collisions")]
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private LayerMask groundLayers;

    [Header("Raycasts")]
    [SerializeField] private Transform raycastPoint;
    [SerializeField] private float groundRaycastDistance;
    [SerializeField] private float wallRaycastDistance;

    [Header("Audio")]
    [SerializeField] private SFXPlayer damageSFXPlayer;

    [Header("Currency")]
    [SerializeField] private Rigidbody2D currencyPrefab;
    [SerializeField] private int currencyDroppedOnDeath;
    [SerializeField] private float currencyShootForce;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO enemyDeathEventSO;

    // Constants
    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;

    //Properties
    private bool FacingRight => transform.localEulerAngles.y == ROTATION_FACINGRIGHT;
    private int StartingHealth => useOptionsValues ? startHealth.ReturnValue(options.Difficulty) : testStartHealth;

    // Private Variables
    private int health;
    private State state = State.Patroling;
    private Rigidbody2D rigid;
    private Animator anim;
    private DamageFlash damageFlash;
    private CinemachineImpulseSource damageImpulseSource;
    private EnemyDeathEvent deathEvent;

    private void Awake()
    {
        TryGetComponent(out rigid);
        TryGetComponent(out anim);
        TryGetComponent(out damageFlash);
        TryGetComponent(out damageImpulseSource);
        TryGetComponent(out deathEvent);

        ChangeState(State.Patroling);
    }

    private void Start()
    {
        health = StartingHealth;
    }

    private void FixedUpdate()
    {
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;

        if (IsPlayerAbove()) playerAboveTimer -= Time.fixedDeltaTime;
        else playerAboveTimer = playerAboveTime;

        if (stunned) return;

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
        else if (state == State.Defending)
        {
            DefendState();
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
                anim.SetBool("Defending", false);
                break;
            case State.Attacking:
                break;
            case State.Turning:
                StopMoving();
                turnTimer = turnTime;
                break;
            case State.Defending:
                CheckIfPlayerBehind();
                StopMoving();
                break;
        }
    }

    #region Patrolling

    private float startPatrollingTimer;
    private bool stunned;
    private int MoveDirection => FacingRight ? 1 : -1;

    private void PatrolState()
    {
        if (CheckForPlayer() || IsPlayerAbove()) 
        { 
            anim.SetTrigger("Notice");
            StopMoving(1);
            return; 
        }

        if (startPatrollingTimer > 0)
        {
            startPatrollingTimer -= Time.fixedDeltaTime;
            return;
        }
        if (CheckForTurn()) return;
        Move(patrolSpeed);
    }

    private void Move(float speed)
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = Mathf.MoveTowards(velocity.x, MoveDirection * speed, 150 * Time.fixedDeltaTime);
        rigid.velocity = velocity;
        anim.SetBool("Moving", true);
    }

    private void StopMoving(int stopTime = 0)
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = 0;
        rigid.velocity = velocity;
        anim.SetBool("Moving", false);
        stunned = true;

        if (stopTime > 0) Invoke(nameof(ResumeMoving), stopTime);
        else stunned = false;
    }

    private void ResumeMoving()
    {
        stunned = false;
    }

    #endregion

    #region Attacking

    private float attackCooldownTimer;
    private float playerAboveTimer;
    private float losePlayerTimer;
    private bool AttackOnCooldown => attackCooldownTimer > 0;

    private void AttackState()
    {
        if (AttackOnCooldown)
        {
            attackCooldownTimer -= Time.fixedDeltaTime;
            return;
        }

        if (playerAboveTimer <= 0)
        {
            ExecuteSpikeAttack();
            return;
        }

        if (!CheckForPlayer()) return;

        if (IsPlayerInAttackDistance()) ExecuteAttack();
        else Move(attackMoveSpeed);
    }

    private void ExecuteAttack()
    {
        StopMoving();
        anim.SetTrigger("Attack");
        attackCooldownTimer = attackCooldownTime;
    }

    private void ExecuteSpikeAttack()
    {
        StopMoving();
        anim.SetTrigger("Spike");
        playerAboveTimer = playerAboveTime;
        attackCooldownTimer = attackCooldownTime;
    }

    private void CounterAttackChance()
    {
        bool counter = Random.Range(0f, 1f) <= counterAttackChance;

        if (counter && CheckForPlayer()) ExecuteAttack();
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
        bool playerHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, MoveDirection * Vector2.right, playerFrontDistance, playerLayer);
        bool playerHitBehind = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, -MoveDirection * Vector2.right, playerBehindDistance, playerLayer);

        if (!playerHit && !playerHitBehind && losePlayerTimer > 0 && state != State.Patroling && state != State.Defending)
        {
            losePlayerTimer -= Time.fixedDeltaTime;

            if (losePlayerTimer <= 0)
            {
                ChangeState(State.Patroling);
            }
        }
        else losePlayerTimer = timeToLosePlayer;

        if (playerHit && state != State.Attacking) ChangeState(State.Attacking);
        else if (playerHitBehind && state != State.Turning) TurnAround();
        //else if (!playerHit && state != State.Patroling) ChangeState(State.Patroling); 

        return playerHit;
    }

    private bool IsPlayerInAttackDistance()
    {
        return Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, MoveDirection * Vector2.right, attackDistance, playerLayer);
    }

    private void CheckIfPlayerBehind()
    {
        bool playerHitBehind = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, -MoveDirection * Vector2.right, playerBehindDistance, playerLayer);
        if (playerHitBehind) TurnAround();
    }

    private bool IsPlayerAbove()
    {
        bool returnValue = Physics2D.CapsuleCast(bodyCollider.bounds.center, bodyCollider.size, bodyCollider.direction, 0, Vector2.up, playerAboveDistance, playerLayer);
        if (returnValue) ChangeState(State.Attacking);
        return returnValue;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out DamageComponent damageComponent))
        {
            if (IsInvincible) return;

            Vector2 directionToHitbox = Vector2.zero;
            if (collision.TryGetComponent(out PlayerHitbox playerHitbox))
            {
                if (playerHitbox.HitboxAxis == PlayerHitbox.Axis.Down) directionToHitbox = Vector2.up;
                else if (playerHitbox.HitboxAxis == PlayerHitbox.Axis.Up) directionToHitbox = Vector2.down;
                else if (playerHitbox.HitboxAxis == PlayerHitbox.Axis.Horizontal)
                {
                    directionToHitbox = (collision.transform.position - transform.position).normalized;
                    if (directionToHitbox.x > 0.7f) directionToHitbox = Vector2.right;
                    else if (directionToHitbox.x < -0.7f) directionToHitbox = Vector2.left;
                }
            }
            else
            {
                directionToHitbox = (collision.transform.position - transform.position).normalized;

                if (directionToHitbox.x > 0.7f) directionToHitbox = Vector2.right;
                else if (directionToHitbox.x < -0.7f) directionToHitbox = Vector2.left;
                else if (directionToHitbox.y > 0.7f) directionToHitbox = Vector2.up;
                else if (directionToHitbox.y < 0.7f) directionToHitbox = Vector2.down;
            }
            //print((collision.transform.position - transform.position).normalized);

            

            if (Defending)
            {
                if (collision.TryGetComponent(out PlayerEnergyBlast _))
                {
                    StopDefense();
                    ChangeState(State.Attacking);
                    return;
                }

                if ((FacingRight && directionToHitbox == Vector2.right) || (!FacingRight && directionToHitbox == Vector2.left))
                {
                    StopDefense();
                    ExecuteAttack();
                    ChangeState(State.Attacking);
                    return;
                }
                else if ((FacingRight && directionToHitbox == Vector2.left) || (!FacingRight && directionToHitbox == Vector2.right) || directionToHitbox == Vector2.up)
                {
                    StopDefense();
                    TakeDamage(damageComponent.Damage);
                    ChangeState(State.Attacking);
                }
            }
            else TakeDamage(damageComponent.Damage);

            if (directionToHitbox != Vector2.up)
            {
                timesAttacked++;
            }

            Vector2 forceDirection;
            if (collision.transform.root.position.x > transform.position.x) forceDirection = Vector2.left;
            else forceDirection = Vector2.right;
            Vector2 force = new((forceDirection * damageComponent.Knockback).x, rigid.velocity.y);
            Knockback(force);
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
        if (state == State.Defending) return; 

        Vector3 euler = transform.localEulerAngles;
        if (euler.y == ROTATION_FACINGLEFT) euler.y = ROTATION_FACINGRIGHT;
        else euler.y = ROTATION_FACINGLEFT;
        transform.localEulerAngles = euler;
    }

    #endregion

    #region Health & Damage

    private int timesAttacked;
    private void TakeDamage(int damage)
    {
        health -= damage;

        damageFlash.CallDamageFlash();
        damageImpulseSource.GenerateImpulse();

        if (health <= 0)
        {
            enemyDeathEventSO.RaiseEvent();
            deathEvent.OnDeath?.Invoke();
            damageSFXPlayer.PlayClipAtPoint();
            Invoke(nameof(Die), 0.1f);
        }
        else
        {
            damageSFXPlayer.Play();
            invincibleTimer = invincibilityTime;
        }
    }

    private const float CURRENCYSHOOT_HORIZONTALDIRECTION_MIN = -0.5f;
    private const float CURRENCYSHOOT_HORIZONTALDIRECTION_MAX = 0.5f;

    private void Die()
    {
        for (int i = 0; i < currencyDroppedOnDeath; i++)
        {
            Vector2 currencyShootDirection = new Vector2(Random.Range(CURRENCYSHOOT_HORIZONTALDIRECTION_MIN, CURRENCYSHOOT_HORIZONTALDIRECTION_MAX), 1).normalized;

            Rigidbody2D currency = Instantiate(currencyPrefab, transform.position, Quaternion.identity);
            currency.AddForce(currencyShootDirection * currencyShootForce);
        }

        Destroy(gameObject);
    }

    #endregion

    #region Knockback

    private State cachedStateBeforeKnockback;
    private void Knockback(Vector2 force)
    {
        Vector2 velocity = rigid.velocity;
        velocity.x = force.x;
        rigid.velocity = velocity;
        if (state != State.Knockback) cachedStateBeforeKnockback = state;
        state = State.Knockback;
        Invoke(nameof(EndKnockback), knockbackTime);
    }

    private void EndKnockback()
    {
        if (timesAttacked % 3 == 0)
        {
            ChangeState(State.Defending);
            timesAttacked = 0;
        }
        else state = cachedStateBeforeKnockback;
    }

    #endregion

    #region Defending

    private float defendTimer;

    private bool Defending => defendTimer > 0;

    private void DefendState()
    {
        if (Defending)
        {
            defendTimer -= Time.fixedDeltaTime;

            if (!Defending)
            {
                anim.SetBool("Defending", Defending);
                CheckForPlayer();
            }
            return;
        }

        ExecuteDefense();
    }

    private void ExecuteDefense()
    {
        defendTimer = defendTime;
        anim.SetBool("Defending", Defending);
    }

    private void StopDefense()
    {
        defendTimer = 0;
        anim.SetBool("Defending", Defending);
    }

    #endregion

    #region Energy

    public int EnergyAmountOnHit { get => energyAmountOnHit; private set => energyAmountOnHit = value; }

    #endregion

    private enum State { Patroling, Attacking, Turning, Knockback, Defending }
}
