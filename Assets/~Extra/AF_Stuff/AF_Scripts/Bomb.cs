using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float minStartForce = 10f;
    [SerializeField] private float maxStartForce = 15f;
    [SerializeField] private VoidEventSO bombHitEventSO;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * Random.Range(minStartForce, maxStartForce), ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out Blade _))
        {
            bombHitEventSO.RaiseEvent();
            Destroy(gameObject);
        }
    }
}
