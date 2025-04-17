using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(DamageFlash))]
[RequireComponent(typeof(CinemachineImpulseSource), typeof(EnemyDeathEvent))]
public class PlantEnemy : MonoBehaviour, IHasEnergy
{
    [Header("Health")]
    [SerializeField] private int health;
    [SerializeField] private float invincibilityTime;

    [Header("Energy")]
    [SerializeField] private int energyAmountOnHit;

    [Header("Detection")]
    [SerializeField] private float playerDetectDistance;
    [SerializeField] private float playerDetectHeight;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attacking")]
    [SerializeField] private float timeBetweenAttacks;
    [Range(-90, 90)]
    [SerializeField] private float shootAngle;
    [SerializeField] private float shootForce;
    [SerializeField] private Rigidbody2D thornBallPrefab;

    [Header("Collision")]
    [SerializeField] private CapsuleCollider2D bodyCollider;

    [Header("Audio")]
    [SerializeField] private SFXPlayer shootSFXPlayer;
    [SerializeField] private SFXPlayer damageSFXPlayer;

    [Header("Currency")]
    [SerializeField] private Rigidbody2D currencyPrefab;
    [SerializeField] private int currencyDroppedOnDeath;
    [SerializeField] private float currencyShootForce;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO enemyDeathEventSO;

    private State state = State.Idle;

    private Animator anim;
    private DamageFlash damageFlash;
    private CinemachineImpulseSource damageImpulseSource;
    private EnemyDeathEvent deathEvent;

    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out damageFlash);
        TryGetComponent(out damageImpulseSource);
        TryGetComponent(out deathEvent);
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.Idle:
                break;
            case State.Attacking:
                break;
        }
    }


    private void FixedUpdate()
    {
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Attacking:
                AttackState();
                break;
        }
    }

    #region Idle

    private void IdleState()
    {
        CheckForPlayer();
    }

    #endregion

    #region Collisions

    private float invincibleTimer;
    private bool IsInvincible => invincibleTimer > 0;

    private bool CheckForPlayer()
    {
        bool playerHit = Physics2D.CapsuleCast(bodyCollider.bounds.center, new (bodyCollider.size.x, bodyCollider.size.y * playerDetectHeight), bodyCollider.direction, 0, -transform.right, playerDetectDistance, playerLayer);

        if (playerHit) ChangeState(State.Attacking);
        else ChangeState(State.Idle);

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

    #region Attacking

    private float attackCooldownTimer;
    private bool AttackOnCooldown => attackCooldownTimer > 0;
    private void AttackState()
    {
        if (AttackOnCooldown)
        {
            attackCooldownTimer -= Time.fixedDeltaTime;

            if (!AttackOnCooldown)
            {
                CheckForPlayer();
            }
            return;
        }

        ExecuteAttack();
    }

    private void ExecuteAttack()
    {
        attackCooldownTimer = timeBetweenAttacks;
        anim.SetTrigger("Attack");
    }

    public void ShootThornBall()
    {
        Vector2 rotatedVector = Quaternion.AngleAxis(shootAngle, transform.forward) * -transform.right;
        Rigidbody2D thornBall = Instantiate(thornBallPrefab, transform.position, transform.localRotation);

        thornBall.velocity = rotatedVector * shootForce;

        shootSFXPlayer.Play();
    }

    #endregion

    #region Health & Damage

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
            Die();
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

    #region Energy

    public int EnergyAmountOnHit { get => energyAmountOnHit; private set => energyAmountOnHit = value; }

    #endregion

    private enum State { Idle, Attacking}
}
