using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FruitPiece : MonoBehaviour
{
    [SerializeField] private FruitDirection direction;
    [SerializeField] private float spawnSpeed;

    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void OnEnable()
    {
        float dir = direction == FruitDirection.Left ? -1 : 1;
        Vector2 force = (transform.right * dir) * spawnSpeed;
        rb.velocity = force;
    }

    public enum FruitDirection { Left, Right }
}
