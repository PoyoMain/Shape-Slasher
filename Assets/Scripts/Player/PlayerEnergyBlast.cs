using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerEnergyBlast : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float timeToDestroy;

    private float Direction => transform.localRotation.y == -1 ? -1 : 1;
    private Rigidbody2D rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    private void OnEnable()
    {
        rb.velocity = new(speed * Direction, 0);

        Invoke(nameof(DestroyThis), timeToDestroy);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
