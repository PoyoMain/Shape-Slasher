using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornBall : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;
    public Vector2 direction = Vector2.left;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction.normalized * speed; 
    }

    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
