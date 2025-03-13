using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class GolemBoss : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health;
    [SerializeField] private float invincibilityTime;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpTime;
    [SerializeField] private float timeBetweenJumps;
    [SerializeField] private Transform middleOfRoomTransform;

    [Header("Waiting")]
    [SerializeField] private float waitTime;

    [Header("Shockwaves")]
    [SerializeField] private ConstantProjectile shockwavePrefab;
    [SerializeField] private Transform shockwaveSpawnTransformFront;
    [SerializeField] private Transform shockwaveSpawnTransformBack;

    [Header("Collison")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private BoxCollider2D playerDetectBox;
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private float wallDistance;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float groundDistance;

    [Header("Audio")]
    [SerializeField] private SFXPlayer jumpSFXPlayer;
    [SerializeField] private SFXPlayer fallSFXPlayer;
    [SerializeField] private SFXPlayer slamSFXPlayer;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO bossDefeatedEventSO;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO bossFightStartEventSO;

    // Constants
    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;

    private State state;
    private Vector2 lastCheckedPlayerPosition;
    private bool facingRight = true;

    private Animator anim;

    private void Awake()
    {
        TryGetComponent(out anim);

        ChangeState(State.Inactive);
    }

    private void OnEnable()
    {
        bossFightStartEventSO.OnEventRaised += Activate;
    }

    private void OnDisable()
    {
        bossFightStartEventSO.OnEventRaised -= Activate;
    }

    private void Activate()
    {
        ChangeState(State.Waiting);
    }

    private void FixedUpdate()
    {
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;

        if (state == State.JumpingToPlayer)
        {
            JumpToPlayerState();
        }
        else if (state == State.JumpingToMiddleOfRoom)
        {
            JumpToMiddleState();
        }
        else if (state == State.Waiting)
        {
            WaitState();
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.Inactive:
                break;
            case State.Starting:
                break;
            case State.Waiting:
                waitTimer = waitTime;
                if (GetPlayerPosition(out Vector2 playerPos))
                {
                    FacePlayer(playerPos);
                }
                break;
            case State.JumpingToPlayer:
                {
                    interpolationValue = 0;
                    jumpStartPosition = transform.position;
                    timesJumpedInARow++;
                    jumpSFXPlayer.Play();
                }
                break;

            case State.JumpingToMiddleOfRoom:
                {
                    timesJumpedInARow = 0;
                    interpolationValue = 0;
                    jumpStartPosition = transform.position;
                }
                break;
            case State.Shockwave:
                anim.SetTrigger("Shockwave");
                break;
        }
    }

    #region Collisions

    private float invincibleTimer;
    private bool IsInvincible => invincibleTimer > 0;

    private bool IsCollidingWithWall(Direction dir)
    {
        bool wallHit;
        if (dir == Direction.Left)
        {
            wallHit = Physics2D.Raycast(bodyCollider.bounds.center, Vector2.left, wallDistance, wallLayer);
        }
        else
        {
            wallHit = Physics2D.Raycast(bodyCollider.bounds.center, Vector2.right, wallDistance, wallLayer);
        }

        return wallHit;
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

    #region Jumping

    private float interpolationValue;
    private float percentage;
    private Vector2 jumpStartPosition;
    private int timesJumpedInARow = 0;

    private void JumpToPlayerState()
    {
        interpolationValue += Time.deltaTime;
        percentage = Mathf.Clamp01(interpolationValue / jumpTime);

        Vector2 targetPositon = new(lastCheckedPlayerPosition.x, jumpStartPosition.y);
        Direction targetDirection = Direction.Right;
        if (jumpStartPosition.x > targetPositon.x) targetDirection = Direction.Left;
        else if (jumpStartPosition.x < targetPositon.x) targetDirection = Direction.Right;

        Vector2 dest = GetPositionOnParabola(jumpStartPosition, targetPositon, jumpHeight, percentage);
        if (IsCollidingWithWall(targetDirection)) transform.position = new(transform.position.x, dest.y);
        else transform.position = dest;

        //print("Percentage: " + percentage * 100 + "%" + "\nPosition = " + transform.position);

        if (percentage == 1f)
        {
            fallSFXPlayer.Play();
            ChangeState(State.Waiting);
        }
    }

    private void JumpToMiddleState()
    {
        interpolationValue += Time.deltaTime;
        percentage = Mathf.Clamp01(interpolationValue / jumpTime);

        Vector2 targetPositon = new(middleOfRoomTransform.position.x, jumpStartPosition.y);
        Direction targetDirection = Direction.Right;
        if (jumpStartPosition.x > targetPositon.x) targetDirection = Direction.Left;
        else if (jumpStartPosition.x < targetPositon.x) targetDirection = Direction.Right;

        Vector2 dest = GetPositionOnParabola(jumpStartPosition, targetPositon, jumpHeight, percentage);
        if (IsCollidingWithWall(targetDirection)) transform.position = new(transform.position.x, dest.y);
        else transform.position = dest;

        //print("Percentage: " + percentage * 100 + "%" + "\nPosition = " + transform.position);

        if (percentage == 1f)
        {
            ChangeState(State.Shockwave);
        }
    }

    private Vector2 GetPositionOnParabola(Vector2 startPos, Vector2 endPos, float height, float t)
    {
        float fractionOfParabola = -4 * Mathf.Pow(t, 2) * height + 4 * t * height;

        float parabolaPosX = Vector2.Lerp(startPos, endPos, t).x;
        float parabolaPosY = fractionOfParabola + Mathf.Lerp(startPos.y, endPos.y, t);

        return new(parabolaPosX, parabolaPosY);
    }

    #endregion

    #region Waiting

    private float waitTimer;

    private void WaitState()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                if (GetPlayerPosition(out Vector2 _) && timesJumpedInARow < 3)
                {
                    ChangeState(State.JumpingToPlayer);
                }
                else
                {
                    if (transform.position.x == middleOfRoomTransform.position.x) ChangeState(State.Shockwave);
                    else ChangeState(State.JumpingToMiddleOfRoom);
                }
            }
        }
    }

    #endregion

    #region Attacks

#pragma warning disable IDE0051
    private void SpawnDualShockwaves()
    {
        Instantiate(shockwavePrefab, shockwaveSpawnTransformFront.position, transform.rotation);
        ConstantProjectile backWave = Instantiate(shockwavePrefab, shockwaveSpawnTransformBack.position, transform.localRotation);

        Vector3 reverseEuler = transform.localEulerAngles;
        reverseEuler.y = facingRight ? ROTATION_FACINGLEFT : ROTATION_FACINGRIGHT;
        backWave.transform.eulerAngles = reverseEuler;

        slamSFXPlayer.Play();
    }
#pragma warning restore IDE0051

    public void EndAttack()
    {
        ChangeState(State.Waiting);
    }

    #endregion

    #region Player

    private bool GetPlayerPosition(out Vector2 playerPos)
    {
        Collider2D[] playerResults = new Collider2D[1];
        Physics2D.OverlapBoxNonAlloc(playerDetectBox.bounds.center, playerDetectBox.size, 0, playerResults, playerLayer);

        playerPos = Vector2.zero;

        if (playerResults[0] == null) return false;

        playerPos = playerResults[0].transform.position;
        lastCheckedPlayerPosition = playerPos;

        return true;
    }

    private void FacePlayer(Vector2 playerPos)
    {
        Vector3 euler = transform.localEulerAngles;
        if (euler.y == ROTATION_FACINGLEFT && playerPos.x > transform.position.x)
        {
            euler.y = ROTATION_FACINGRIGHT;
            facingRight = true;
        }
        else if (euler.y == ROTATION_FACINGRIGHT && playerPos.x < transform.position.x)
        {
            euler.y = ROTATION_FACINGLEFT;
            facingRight = false;
        }
        transform.localEulerAngles = euler;
    }

    #endregion

    #region Health & Damage

    private void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
            bossDefeatedEventSO.RaiseEvent();
        }
        else invincibleTimer = invincibilityTime;
    }

    #endregion

    private enum State { Inactive, Starting, Waiting, JumpingToPlayer, JumpingToMiddleOfRoom, Shockwave }
    private enum Direction { Left, Right }
}
