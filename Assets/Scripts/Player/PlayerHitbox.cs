using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private Axis axis;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private SFXPlayer hitSFXPlayer;

    private const int ROTATION_FACINGRIGHT = 0;
    private const int ROTATION_FACINGLEFT = 180;
    private const float COOLDOWN_TIME = 0.1f;

    private bool OnCooldown => timer > 0;

    private float timer;


    private void LateUpdate()
    {
        if (timer > 0) timer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (axis == Axis.Horizontal)
        {
            if (OnCooldown) return;

            if (transform.eulerAngles.y == ROTATION_FACINGLEFT) SendMessageUpwards("HitboxKnockback", Vector2.right);
            else if (transform.eulerAngles.y == ROTATION_FACINGRIGHT) SendMessageUpwards("HitboxKnockback", Vector2.left);

            if (collision.contacts.Length > 0)
            {
                Instantiate(hitEffect, collision.contacts[0].point, Random.rotation);
            }

            hitSFXPlayer.Play();
            timer = COOLDOWN_TIME;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (axis == Axis.Horizontal)
        {
            if (OnCooldown) return;

            if (transform.eulerAngles.y == ROTATION_FACINGLEFT) SendMessageUpwards("HitboxKnockback", Vector2.right);
            else if (transform.eulerAngles.y == ROTATION_FACINGRIGHT) SendMessageUpwards("HitboxKnockback", Vector2.left);

            timer = COOLDOWN_TIME;
        }
    }

    private enum Axis { Horizontal, Vertical }
}
