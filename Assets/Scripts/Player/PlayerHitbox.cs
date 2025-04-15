using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private Axis axis;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private SFXPlayer hitSFXPlayer;

    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;
    private const float COOLDOWN_TIME = 0.1f;

    private bool OnSurfaceCooldown => surfaceHitTimer > 0;
    private float surfaceHitTimer;

    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        if (player == null) Debug.LogError("Player Hitbox script doesnt have an associated Player script in parent");
    }

    private void LateUpdate()
    {
        if (surfaceHitTimer > 0) surfaceHitTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<CutableProp>()) return;
        if (OnSurfaceCooldown) return;

        if (axis == Axis.Horizontal)
        {
            if (transform.eulerAngles.y == ROTATION_FACINGLEFT) player.HitboxKnockbackHorizontal(Vector2.right);
            else if (transform.eulerAngles.y == ROTATION_FACINGRIGHT) player.HitboxKnockbackHorizontal(Vector2.left);
        }

        if (collision.contacts.Length > 0) Instantiate(hitEffect, collision.contacts[^1].point, Random.rotation);

        hitSFXPlayer.Play();
        surfaceHitTimer = COOLDOWN_TIME;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CutableProp>()) return;

        if (axis == Axis.Horizontal)
        {
            if (transform.eulerAngles.y == ROTATION_FACINGLEFT) player.HitboxKnockbackHorizontal(Vector2.right);
            else if (transform.eulerAngles.y == ROTATION_FACINGRIGHT) player.HitboxKnockbackHorizontal(Vector2.left);
        }
        else if (axis == Axis.Down) player.BounceKnockback(Vector2.up);

        int energy = collision.GetComponentInParent<IHasEnergy>().EnergyAmountOnHit;
        player.GainedEnergy(energy);
    }

    private enum Axis { Horizontal, Up, Down }
}
