using Cinemachine;
using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(DamageFlash))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class Player : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReaderSO inputReader;

    [Space(20)]
    [SerializeField] private Stats stats;

    [Header("Components")]
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private BoxCollider2D hurtboxCollider;
    [SerializeField] private Transform camFocusTransform;

    [Header("Audio")]
    [SerializeField] private SFXPlayer jumpSFXPlayer;
    [SerializeField] private SFXPlayer damageSFXPlayer;
    [SerializeField] private SFXPlayer deathSFXPlayer;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO playerDamagedEventSO;
    [SerializeField] private VoidEventSO playerHealedEventSO;
    [SerializeField] private VoidEventSO playerDeathEventSO;
    [SerializeField] private IntEventSO playerHealthLossEventSO;
    [SerializeField] private IntEventSO playerHealthGainedEventSO;
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
    private bool dashDown;
    private bool interactDown;
    private bool cachedQueryStartInColliders;
    private float time;
    private Rigidbody2D rb;
    private Animator anim;
    private DamageFlash damageFlash;
    private CinemachineImpulseSource damageImpulseSource;


    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out anim);
        TryGetComponent(out damageFlash);
        TryGetComponent(out damageImpulseSource);

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
        
        if (!dashDown && hasDashAbility)
        {
            dashDown = Controls.Dash.WasPressedThisFrame();
        }

        if (!interactDown)
        {
            interactDown = Controls.Interact.WasPressedThisFrame();
        }

        if (stats.SnapInput)
        {
            moveInput.x = Mathf.Abs(moveInput.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(moveInput.x);
            moveInput.y = Mathf.Abs(moveInput.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(moveInput.y);
        }

        if (moveInput.y < 0) anim.SetInteger("VerticalInput", -1);
        else if (moveInput.y > 0) anim.SetInteger("VerticalInput", 1);
        else anim.SetInteger("VerticalInput", 0);

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

        HandleInteract();
        HandleDash();
        HandleAttack();

        HandleJump();
        HandleDirection();
        HandleKnockback();
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
                hurtboxCollider.enabled = true;
                damageFlash.SetAlphaToOne();
                anim.SetBool("IsInvincible", false);
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

        bool groundHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.down, stats.GrounderDistance, stats.GroundLayers);
        RaycastHit2D bounceHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.down, stats.GrounderDistance, stats.BounceLayer);
        bool ceilingHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.up, stats.GrounderDistance, stats.CeilingLayers);

        // Hit a Ceiling
        if (ceilingHit) velocity.y = Mathf.Min(0, velocity.y);

        // Landed on a bounce pad
        if (bounceHit && velocity.y <= 0 && bounceHit.collider.TryGetComponent(out BouncePad bouncePad))
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
            hasDashInAir = true;
            if (colliderToTurnOnOnceGrounded != null)
            {
                Physics2D.IgnoreCollision(bodyCollider, colliderToTurnOnOnceGrounded, false);
                colliderToTurnOnOnceGrounded = null;
            }
            GroundedChanged?.Invoke(true, Mathf.Abs(velocity.y));
            anim.SetBool("IsGrounded", true);
        }

        //Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGround = time;
            GroundedChanged?.Invoke(false, 0);
            anim.SetBool("IsGrounded", false);
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

            if (health > 0)
            {
                invincibilityTimer = stats.InvincibleTime;
                anim.SetBool("IsInvincible", true);
                hurtboxCollider.enabled = false;

                Vector2 directionToHitbox = (dmgComponent.KnockbackOrigin.position - transform.position).normalized;
                directionToHitbox.y = Mathf.Max(0, directionToHitbox.y);
                Vector2 force = -directionToHitbox * dmgComponent.Knockback;
                Knockback(force);

            }
        }
        else if (collider.TryGetComponent(out UpwardForceApplier upForceApplier))
        {
            if (velocity.y > 0) Knockback(Vector2.up * upForceApplier.ForceStrength);
        }
        else if (collider.TryGetComponent(out Pickup pickup))
        {
            switch (pickup.Type)
            {
                case Pickup.PickupType.Health:
                    if (health < stats.MaxHealth)
                    {
                        Heal(1);
                        Destroy(pickup.gameObject);
                    }
                    break;
                case Pickup.PickupType.Currency:
                    if (currency < 999)
                    {
                        GainedCurrency(1);
                        Destroy(pickup.gameObject);
                    }
                    break;
            }

            
        }
    }

    #endregion

    #region Interact

    private void HandleInteract()
    {
        if (!interactDown) return;

        if (!grounded && !IsDashing && !IsAttacking && !IsInKnockback) return;

        ExecuteInteract();
        interactDown = false;
    }

    private void ExecuteInteract()
    {
        RaycastHit2D interactHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.down, 0, stats.InteractLayer);

        if (interactHit && interactHit.collider.TryGetComponent(out BuyableItem buyable))
        {
            if (currency < buyable.Cost) return;
            
            SpentCurrency(buyable.Cost);
            buyable.Buy();

            switch (buyable.Type)
            {
                case BuyableItem.Buyable.Dash:
                    hasDashAbility = true;
                    break;
                case BuyableItem.Buyable.AttackUp:
                    IncreaseAttack(1.5f);
                    break;
                case BuyableItem.Buyable.FullRecovery:
                    Heal(stats.MaxHealth);
                    break;
            }
        }
    }

    #endregion

    #region Dash

    private float dashTimer;
    private readonly float dashDecelerationPercentage = 0.2f;
    private bool hasDashAbility;
    private bool hasDashInAir;
    private bool IsDashing => dashTimer > 0;

    private void HandleDash()
    {
        if (IsDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            
            if (dashTimer <= stats.DashTime * dashDecelerationPercentage) velocity.x = Mathf.MoveTowards(velocity.x, 0, stats.GroundDeceleration * Time.fixedDeltaTime);

            if (dashTimer <= 0) anim.SetBool("IsDashing", false);
        }

        if (!dashDown) return;

        if (!grounded && !hasDashInAir) return;

        if (dashDown && !IsDashing) ExecuteDash();

        dashDown = false;
    }

    private void ExecuteDash()
    {
        velocity = new(stats.DashPower * NumericalFacingDirection, 0);
        dashTimer = stats.DashTime;
        anim.SetBool("IsDashing", true);
        if (!grounded) hasDashInAir = false;
    }

    #endregion

    #region Attack

    private float attackTimer;
    private bool IsAttacking => attackTimer > 0;
    private void HandleAttack()
    {
        if (IsAttacking) attackTimer -= Time.fixedDeltaTime;

        if (!attackDown) return;

        if (attackDown && !IsAttacking) ExecuteAttack();

        attackDown = false;
    }

    private void ExecuteAttack()
    {
        anim.SetTrigger("Attack");
        attackTimer = stats.TimeBetweenAttacks;
    }

    private void IncreaseAttack(float multiplier)
    {
        DamageComponent[] damageComponents = GetComponentsInChildren<DamageComponent>(includeInactive: true);

        for (int i = 0; i < damageComponents.Length; i++)
        {
            damageComponents[i].IncreaseDamageByMultiplier(multiplier);
        }
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

        if (IsInKnockback || IsDashing) return;

        if (grounded || CanUseCoyote) ExecuteJump();

        jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        jumpTimer = stats.JumpTime;
        velocity.y = 0;
        jumpSFXPlayer.Play();
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
        bool groundHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.down, stats.PlatformDistance, stats.SolidSurfaceLayer);
        RaycastHit2D platformHit = Physics2D.BoxCast(bodyCollider.bounds.center, bodyCollider.size, 0, Vector2.down, stats.PlatformDistance, stats.OneWayPlatformLayer);

        if (!groundHit && platformHit)
        {
            colliderToTurnOnOnceGrounded = platformHit.collider;
            Physics2D.IgnoreCollision(bodyCollider, colliderToTurnOnOnceGrounded, true);
            return true;
        }
        else return false;
    }

    #endregion

    #region Horizontal

    private int NumericalFacingDirection => transform.localEulerAngles.y == ROTATION_FACING_RIGHT ? 1 : -1;

    private void HandleDirection()
    {
        if (IsInKnockback || IsDashing) return;

        if (moveInput.x == 0)
        {
            var deceleration = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.fixedDeltaTime);

            anim.SetBool("IsMoving", false);
        }
        else
        {
            var maxSpeed = grounded ? stats.MaxGroundSpeed : stats.MaxAirSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * maxSpeed, stats.HorizontalAcceleration * Time.fixedDeltaTime);

            // Set Animator movement
            anim.SetBool("IsMoving", true);

            if (IsAttacking) return;

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

    private float knockbackTimer;
    private bool IsInKnockback => knockbackTimer > 0;

    private void HandleKnockback()
    {
        if (IsInKnockback)
        {
            knockbackTimer -= Time.fixedDeltaTime;
        }
    }

    private void Knockback(Vector2 force)
    {
        velocity += force;
        knockbackTimer = stats.KnockbackAppliedTime;
        
    }

#pragma warning disable IDE0051
    private void HitboxKnockbackHorizontal(Vector2 hitColliderDirection)
    {        
        velocity = new ((hitColliderDirection * stats.SurfaceKnockback).x, velocity.y);
        knockbackTimer = stats.KnockbackAppliedTime;
    }
    private void HitboxKnockbackVertical(Vector2 hitColliderDirection)
    {
        velocity = new(velocity.x, (hitColliderDirection * stats.SurfaceKnockback).y);
        //knockbackTimer = stats.KnockbackAppliedTime;
    }

    private void BounceKnockback(Vector2 hitColliderDirection)
    {
        StopVerticalMovement();
        velocity = new(velocity.x, (hitColliderDirection * stats.BounceKnockback).y);
    }
#pragma warning restore IDE0051

    private void KnockbackOnlyVertical(Vector2 force)
    {
        velocity.y = force.y;
    }

    private void StopVerticalMovement()
    {
        rb.velocity = new(rb.velocity.x, 0);
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (IsDashing) return;

        if (grounded && velocity.y <= 0f)
        {
            velocity.y = stats.GroundingForce;
            anim.SetInteger("VerticalSpeed", 0);
        }
        else
        {
            float inAirGravity = stats.FallAcceleration;
            if (endedJumpEarly && velocity.y > 0 && jumpTimer < stats.JumpTime - stats.MinJumpTime)
            {
                inAirGravity *= stats.JumpEndEarlyGravityModifier;
            }
            velocity.y = Mathf.MoveTowards(velocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);

            if (velocity.y > 0) anim.SetInteger("VerticalSpeed", 1);
            else if (velocity.y < 0) anim.SetInteger("VerticalSpeed", -1);
        }
    }

    #endregion

    #region Looking

    private float lookTimer;
    private float previousVertDirectionValue;

    private void HandleLooking()
    {
        Vector2 lookPos = camFocusTransform.localPosition;

        if (moveInput.x == 0 && moveInput.y != 0 && grounded)
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

    private void Heal(int healAmount)
    {
        playerHealthGainedEventSO.RaiseEvent(healAmount);
        playerHealedEventSO.RaiseEvent();

        health += healAmount;
        playerHealthUpdatedEventSO.RaiseEvent(health);
    }

    public void Damage(int dmgAmount)
    {
        playerHealthLossEventSO.RaiseEvent(dmgAmount);
        playerDamagedEventSO.RaiseEvent();


        health -= dmgAmount;
        playerHealthUpdatedEventSO.RaiseEvent(health);
        damageFlash.CallDamageFlash();
        damageImpulseSource.GenerateImpulse();

        if (health <= 0)
        {
            deathSFXPlayer.PlayClipAtPoint();
            playerDeathEventSO.RaiseEvent();
            Time.timeScale = 1;
            Destroy(gameObject);
        }
        else
        {
            damageSFXPlayer.Play();
        }
    }

    #endregion

    #region Currency

    private int currency;

    private void GainedCurrency(int amount)
    {
        currency += amount;
    }

    private void SpentCurrency(int amount)
    {
        currency -= amount;
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
    public LayerMask InteractLayer;

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

    [Header("Knockback")]
    public float SurfaceKnockback;
    public float BounceKnockback;
    public float KnockbackAppliedTime;

    [Header("Dash")]
    public float DashPower;
    public float DashTime;

    [Header("Looking")]
    public float LookDistance;
    public float TimeToLook;

    [Header("Health")]
    public int MaxHealth;
    public float InvincibleTime;
}
