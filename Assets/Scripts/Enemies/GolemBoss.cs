using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("Waiting")]
    [SerializeField] private float waitTime;

    [Header("Collison")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float playerDistanceAllowedAway;
    [SerializeField] private BoxCollider2D playerDetectBox;
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private float wallDistance;
    [SerializeField] private LayerMask wallLayer;

    // Constants
    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;

    private State state;
    private Vector2 lastCheckedPlayerPosition;
    private bool facingRight;

    private Coroutine jumpCoroutine;

    private void Awake()
    {
        ChangeState(State.Jumping);
    }

    // Method to check for player pos
    // Calculate a parabola with boss pos and player pos
    // Function to get parabola position

    private void FixedUpdate()
    {
        if (state == State.Jumping)
        {
            JumpState();
        }
        else if (state == State.Waiting)
        {
            WaitState();
        }
    }

    [ContextMenu("Jump")]
    private void BeginJumping()
    {
        ChangeState(State.Jumping);
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
                break;
            case State.Jumping:
                interpolationValue = 0;
                lastCheckedPlayerPosition = Vector2.zero;
                jumpStartPosition = transform.position;
                lastCheckedPlayerPosition = GetPlayerPosition();
                FacePlayer(lastCheckedPlayerPosition);

                if (lastCheckedPlayerPosition == Vector2.zero)
                {
                    ChangeState(State.Waiting);
                    return;
                }

                break;
        }
    }

    #region Collisions

    private bool IsCollidingWithWall()
    {
        bool wallHitLeft = Physics2D.Raycast(bodyCollider.bounds.center, Vector2.right, wallDistance, wallLayer);
        bool wallHitRight = Physics2D.Raycast(bodyCollider.bounds.center, Vector2.right, wallDistance, wallLayer);

        return wallHitLeft || wallHitRight;
    }

    #endregion

    #region Jumping

    private float interpolationValue;
    private float percentage;
    private bool isJumping;
    private Vector2 jumpStartPosition;

    private void JumpState()
    {
        //if (isJumping) return;
        //isJumping = true;

        //if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
        //jumpCoroutine = StartCoroutine(JumpToPlayerCoroutine());

        Jump();
    }

    private void Jump()
    {
        interpolationValue += Time.deltaTime;
        percentage = Mathf.Clamp01(interpolationValue / jumpTime);

        Vector2 targetPositon = new(lastCheckedPlayerPosition.x, jumpStartPosition.y);

        Vector2 dest = GetPositionOnParabola(jumpStartPosition, targetPositon, jumpHeight, percentage);
        if (IsCollidingWithWall()) transform.position = new(transform.position.x, dest.y);
        else transform.position = dest;

        print("Percentage: " + percentage * 100 + "%" + "\nPosition = " + transform.position);

        if (percentage == 1f)
        {
            isJumping = false;
            ChangeState(State.Waiting);
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

            if (waitTimer <= 0) ChangeState(State.Jumping);
        }
    }

    #endregion

    #region Player

    private Vector2 GetPlayerPosition()
    {
        Collider2D[] playerResults = new Collider2D[1];
        Physics2D.OverlapBoxNonAlloc(playerDetectBox.bounds.center, playerDetectBox.size, 0, playerResults, playerLayer);

        if (playerResults[0] == null) return Vector2.zero;

        Vector2 playerPos = playerResults[0].transform.position;

        return playerPos;
    }

    private void FacePlayer(Vector2 playerPos)
    {
        Vector3 euler = transform.localEulerAngles;
        if (euler.y == ROTATION_FACINGLEFT && playerPos.x > transform.position.x) euler.y = ROTATION_FACINGRIGHT;
        else if (euler.y == ROTATION_FACINGRIGHT && playerPos.x < transform.position.x) euler.y = ROTATION_FACINGLEFT;
        transform.localEulerAngles = euler;

        facingRight = !facingRight;
    }

    #endregion

    private enum State { Inactive, Starting, Waiting, Jumping }
}
