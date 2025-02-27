using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReaderSO inputReader;

    [Header("Stats")]
    [SerializeField] private Stats stats;

    [Header("Collision")]
    [SerializeField] private CapsuleCollider2D hurtboxCollider;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO playerDamagedEventSO;
    [SerializeField] private IntEventSO playerHealthLossEventSO;
    [SerializeField] private IntEventSO playerHealthUpdatedEventSO;

    [Header("Attack")]
    [SerializeField] private GameObject hitbox;
    [SerializeField] private Animator hitboxAnimator;

    // Properties
    private PlayerControls.GameplayControlsActions Controls => inputReader.Controls;

    // Public variables
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    // Private Variables
    private int health;
    private Vector2 moveInput;
    private Vector2 velocity;
    private bool jumpDown;
    private bool jumpHeld;
    private bool cachedQueryStartInColliders;
    private float time;
    private Rigidbody2D rb;
    private bool facingRight = true;


    private void Awake()
    {
        TryGetComponent(out rb);

        health = stats.MaxHealth;
        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        time += Time.deltaTime;
        GetInput();
    }

    private void GetInput()
    {
        jumpDown = Controls.Jump.WasPressedThisFrame();
        jumpHeld = Controls.Jump.IsPressed();
        moveInput = Controls.Move.ReadValue<Vector2>();

        if (stats.SnapInput)
        {
            moveInput.x = Mathf.Abs(moveInput.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(moveInput.x);
            moveInput.y = Mathf.Abs(moveInput.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(moveInput.y);
        }

        if (jumpDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = time;
        }

        if (Controls.Attack.WasPressedThisFrame())
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        HandleInvincibility();
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    #region Invincibility

    private float invincibilityTimer;
    private bool IsInvincible => invincibilityTimer > 0;
    private void HandleInvincibility()
    {
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            if (invincibilityTimer <= 0)
            {

            }
        }
    }

    #endregion

    #region Collision

    private float frameLeftGround = float.MinValue;
    private bool grounded;
    private Collider2D colliderToTurnOnOnceGrounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground, Bouncepad, and Ceiling Check
        bool groundHit = Physics2D.CapsuleCast(hurtboxCollider.bounds.center, hurtboxCollider.size, hurtboxCollider.direction, 0, Vector2.down, stats.GrounderDistance, stats.GroundLayers);
        RaycastHit2D bounceHit = Physics2D.CapsuleCast(hurtboxCollider.bounds.center, hurtboxCollider.size, hurtboxCollider.direction, 0, Vector2.down, stats.GrounderDistance, stats.BounceLayer);
        bool ceilingHit = Physics2D.CapsuleCast(hurtboxCollider.bounds.center, hurtboxCollider.size, hurtboxCollider.direction, 0, Vector2.up, stats.GrounderDistance, stats.CeilingLayers);

        // Hit a Ceiling
        if (ceilingHit) velocity.y = Mathf.Min(0, velocity.y);

        // Landed on a bounce pad
        if (bounceHit && bounceHit.collider.TryGetComponent(out BouncePad bouncePad))
        {
            KnockbackOnlyVertical(Vector2.up * bouncePad.BounceAmount);
        }

        // Landed on Ground
        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            if (colliderToTurnOnOnceGrounded != null)
            {
                Physics2D.IgnoreCollision(hurtboxCollider, colliderToTurnOnOnceGrounded, false);
                colliderToTurnOnOnceGrounded = null;
            }
            GroundedChanged?.Invoke(true, Mathf.Abs(velocity.y));
        }

        //Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGround = time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Hit something that damages the player
        if (collider.TryGetComponent(out DamageComponent dmgComponent))
        {
            if (IsInvincible) return;

            Damage(dmgComponent.Damage);

            if (health <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                invincibilityTimer = stats.InvincibleTime; 

                Vector2 directionToHitbox = (collider.transform.position - transform.position).normalized;
                Vector2 force = -directionToHitbox * dmgComponent.Knockback;
                Knockback(force);

            }
        }
        else if (collider.TryGetComponent(out UpwardForceApplier upForceApplier))
        {
            if (velocity.y > 0) Knockback(Vector2.up * upForceApplier.ForceStrength);
        }
    }

    #endregion

    #region Jump & Crouch

    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool endedJumpEarly;
    private bool coyoteUsable;
    private float timeJumpWasPressed;

    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGround + stats.CoyoteTime;

    private bool CrouchDown => moveInput.y < 0;

    private void HandleJump()
    {
        if (!endedJumpEarly && !grounded && !jumpHeld && rb.velocity.y >= 0) endedJumpEarly = true;

        if (!jumpToConsume && !HasBufferedJump) return;

        if (CrouchDown && CheckCrouch()) return; 

        if (grounded || CanUseCoyote) ExecuteJump();

        jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        velocity.y = stats.JumpPower;
        Jumped?.Invoke();
    }

    private bool CheckCrouch()
    {
        bool groundHit = Physics2D.CapsuleCast(hurtboxCollider.bounds.center, hurtboxCollider.size, hurtboxCollider.direction, 0, Vector2.down, stats.PlatformDistance, stats.SolidSurfaceLayer);
        RaycastHit2D platformHit = Physics2D.CapsuleCast(hurtboxCollider.bounds.center, hurtboxCollider.size, hurtboxCollider.direction, 0, Vector2.down, stats.PlatformDistance, stats.OneWayPlatformLayer);

        if (!groundHit && platformHit)
        {
            colliderToTurnOnOnceGrounded = platformHit.collider;
            Physics2D.IgnoreCollision(hurtboxCollider, colliderToTurnOnOnceGrounded, true);
            return true;
        }
        else return false;
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (moveInput.x == 0)
        {
            var deceleration = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            var maxSpeed = grounded ? stats.MaxGroundSpeed : stats.MaxAirSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * maxSpeed, stats.Acceleration * Time.fixedDeltaTime);

            if ((moveInput.x > 0 && !facingRight) || (moveInput.x < 0 && facingRight))
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }


    #endregion

    #region Knockback

    private void Knockback(Vector2 force)
    {
        velocity += force;
    }

    private void KnockbackOnlyVertical(Vector2 force)
    {
        velocity.y = force.y;
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (grounded && velocity.y <= 0f)
        {
            velocity.y = stats.GroundingForce;
        }
        else
        {
            float inAirGravity = stats.FallAcceleration;
            if (endedJumpEarly && velocity.y > 0) inAirGravity *= stats.JumpEndEarlyGravityModifier;
            velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    private void ApplyMovement() => rb.velocity = velocity;

    #region Health & Damage

    public void Damage(int dmgAmount)
    {
        playerHealthLossEventSO.RaiseEvent(dmgAmount);
        playerDamagedEventSO.RaiseEvent();


        health -= dmgAmount;
        playerHealthUpdatedEventSO.RaiseEvent(health);
    }

    #endregion

    #region Attacking

    private void Attack()
    {
        if (hitbox == null || hitboxAnimator == null) return;

        Vector3 scale = hitbox.transform.localScale;
        scale.x = facingRight ? 1 : -1;
        hitbox.transform.localScale = scale;

        hitboxAnimator.SetTrigger("Attack");

        hitbox.SetActive(true);
        StartCoroutine(DisableHitboxAfterDelay(0.3f));
    }

    private IEnumerator DisableHitboxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hitbox.SetActive(false);
    }

    #endregion
}

[Serializable]
public struct Stats
{
    [Header("Layers")]
    public LayerMask PlayerLayer;
    public LayerMask CeilingLayers;
    public LayerMask GroundLayers;
    public LayerMask BounceLayer;
    public LayerMask OneWayPlatformLayer;
    public LayerMask SolidSurfaceLayer;

    [Header("Input")]
    public bool SnapInput;
    public float VerticalDeadZoneThreshold;
    public float HorizontalDeadZoneThreshold;

    [Header("Movement")]
    public float MaxGroundSpeed;
    public float MaxAirSpeed;
    public float Acceleration;
    public float GroundDeceleration;
    public float AirDeceleration;
    public float GroundingForce;
    public float GrounderDistance;
    public float PlatformDistance;

    [Header("Jump")]
    public float JumpPower;
    public float MaxFallSpeed;
    public float FallAcceleration;
    public float JumpEndEarlyGravityModifier;
    public float CoyoteTime;
    public float JumpBuffer;

    [Header("Attacking")]
    public float TimeBetweenAttacks;
    public float SurfaceKnockback;

    [Header("Health")]
    public int MaxHealth;
    public float InvincibleTime;
}
