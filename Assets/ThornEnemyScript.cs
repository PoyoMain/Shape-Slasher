using System.Collections;
using UnityEngine;

public class ThornEnemyScript : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject thornBallPrefab;  // Thornball
    public Transform firePoint;         // Spawn point for thornhall
    public float fireRate = 1.5f;       // Delay between shots
    public float shootAngle = 30f;      // Angle
    public float detectionRange = 5f;   // Range to detect player
    public float projectileSpeed = 5f;  // Speed of the thornball

    [Header("Detection")]
    public LayerMask playerLayer;       

    private Transform player;
    private bool canShoot = true;

    void Update()
    {
        DetectAndShootPlayer();
    }

    void DetectAndShootPlayer()
    {
        if (player == null)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
            if (hit != null) player = hit.transform;
        }

        if (player != null && IsPlayerInFront())
        {
            if (canShoot) StartCoroutine(ShootThornBall());
        }
    }

    bool IsPlayerInFront()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        return Vector2.Dot(transform.right, directionToPlayer) > 0; // Ensures player is in front
    }

    IEnumerator ShootThornBall()
    {
        canShoot = false;

        // Instantiate the thorn ball
        GameObject thornBall = Instantiate(thornBallPrefab, firePoint.position, Quaternion.identity);

        // Calculate shooting direction
        float angleRad = shootAngle * Mathf.Deg2Rad;
        Vector2 shootDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

        // Apply velocity
        Rigidbody2D rb = thornBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = shootDirection * projectileSpeed;
        }

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
