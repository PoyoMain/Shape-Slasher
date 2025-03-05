using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Player : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReaderSO inputReader;

    [Space(20)]
    [SerializeField] private Stats stats;

    [Header("Collision")]
    [SerializeField] private CapsuleCollider2D hurtboxCollider;

    [Header("Camera")]
    [SerializeField] private Transform camFocusTransform;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO playerDamagedEventSO;
    [SerializeField] private IntEventSO playerHealthLossEventSO;
    [SerializeField] private IntEventSO playerHealthUpdatedEventSO;

    // Properties
    private PlayerControls.GameplayControlsActions Controls => inputReader.Controls;

    // Constants
    private const int ROTATION_FACING_RIGHT = 0;
    private const int ROTATION_FACING_LEFT = 180;

    // Public variables
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    // Private Variables
    private int health;
    private Vector2 moveInput;
    private Vector2 velocity;
    private bool jumpDown;
    private bool jumpHeld;
    private bool attackDown;
    private bool cachedQueryStartInColliders;
    private float time;
    private Rigidbody2D rb;
    private Animator anim;


    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out anim);

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

        if (!attackDown)
        {
            attackDown = Controls.Attack.WasPressedThisFrame();
        }

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

        if (moveInput.y != previousVertDirectionValue)
        {
            lookTimer = stats.TimeToLook;
        }

        previousVertDirectionValue = moveInput.y;
    }

    private void FixedUpdate()
    {
        HandleInvincibility();
        CheckCollisions();

        HandleAttack();

        HandleJump();
        HandleDirection();
        HandleGravity();

        HandleLooking();

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

    #region Attack

    private float attackTimer;
    private bool CanAttack => attackTimer <= 0;
    private void HandleAttack()
    {
        if (!CanAttack) attackTimer -= Time.fixedDeltaTime;

        if (!attackDown) return;

        if (attackDown && CanAttack) ExecuteAttack();

        attackDown = false;
    }

    private void ExecuteAttack()
    {
        anim.SetTrigger("Attack");
        attackTimer = stats.TimeBetweenAttacks;
    }

    #endregion

    #region Jump & Crouch

    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool endedJumpEarly;
    private bool coyoteUsable;
    private float jumpTimer;
    private float timeJumpWasPressed;

    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGround + stats.CoyoteTime;

    private bool CrouchDown => moveInput.y < 0;

    private void HandleJump()
    {
        if (!endedJumpEarly && !grounded && !jumpHeld && rb.velocity.y >= 0)
        {
            endedJumpEarly = true;
        }

        if ((jumpHeld || endedJumpEarly)) HandleJumpMovement();

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
        //velocity.y = stats.JumpPower;
        jumpTimer = stats.JumpTime;
        velocity.y = stats.GroundingForce;
        Jumped?.Invoke();
    }

    private void HandleJumpMovement()
    {
        if (endedJumpEarly && jumpTimer < stats.JumpTime - stats.MinJumpTime)
        {
            return;
        }

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.fixedDeltaTime;
            velocity.y = Mathf.MoveTowards(velocity.y, stats.JumpPower, stats.JumpAcceleration * Time.fixedDeltaTime);
        }
        
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
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * maxSpeed, stats.HorizontalAcceleration * Time.fixedDeltaTime);

            // Rotate GameObject based on movement input
            if (moveInput.x > 0 && transform.localEulerAngles.y != ROTATION_FACING_RIGHT)
            {
                Vector3 newRot = new(0, ROTATION_FACING_RIGHT, 0);
                transform.localEulerAngles = newRot;
            }
            else if (moveInput.x < 0 && transform.localEulerAngles.y != ROTATION_FACING_LEFT)
            {
                Vector3 newRot = new(0, ROTATION_FACING_LEFT, 0);
                transform.localEulerAngles = newRot;
            }
        }
    }

    #endregion

    #region Knockback

    private void Knockback(Vector2 force) => velocity += force;

    [ContextMenu("Ignore")]
    private void WallKnockback(Vector2 collPos)
    {
        Vector2 forceDirection;
        if (collPos.x > transform.position.x) forceDirection = Vector2.left;
        else forceDirection = Vector2.right;
        
        velocity = new ((forceDirection * stats.SurfaceKnockback).x, velocity.y); 
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
            if (endedJumpEarly && velocity.y > 0 && jumpTimer < stats.JumpTime - stats.MinJumpTime)
            {
                inAirGravity *= stats.JumpEndEarlyGravityModifier;
            }
            velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Looking

    private float lookTimer;
    private float previousVertDirectionValue;

    private void HandleLooking()
    {
        Vector2 lookPos = camFocusTransform.localPosition;

        if (moveInput.x == 0 && moveInput.y != 0)
        {
            lookTimer -= Time.fixedDeltaTime;

            if (lookTimer <= 0)
            {
                if (moveInput.y > 0)
                {
                    if (lookPos.y != stats.LookDistance) lookPos.y = stats.LookDistance;
                }
                else
                {
                    if (lookPos.y != -stats.LookDistance) lookPos.y = -stats.LookDistance;
                }
            }  
        }
        else
        {
            if (lookPos.y != 0)
            {
                lookPos.y = 0;
                lookTimer = stats.TimeToLook;
            }
        }

        camFocusTransform.localPosition = lookPos;
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
    public float HorizontalAcceleration;
    public float GroundDeceleration;
    public float AirDeceleration;
    public float GroundingForce;
    public float GrounderDistance;
    public float PlatformDistance;

    [Header("Jump")]
    public float JumpPower;
    public float JumpTime;
    public float MinJumpTime;
    public float MaxFallSpeed;
    public float JumpAcceleration;
    public float FallAcceleration;
    public float JumpEndEarlyGravityModifier;
    public float CoyoteTime;
    public float JumpBuffer;

    [Header("Attacking")]
    public float TimeBetweenAttacks;
    public float SurfaceKnockback;

    [Header("Looking")]
    public float LookDistance;
    public float TimeToLook;

    [Header("Health")]
    public int MaxHealth;
    public float InvincibleTime;
}
