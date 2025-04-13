using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutableProp : MonoBehaviour
{
    [SerializeField] private GameObject partToCut;
    [SerializeField] private GameObject particleEffect;

    private Collider2D coll;

    private void Awake()
    {
        TryGetComponent(out coll);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerHitbox _))
        {
            partToCut.SetActive(false);

            if (particleEffect != null) particleEffect.SetActive(true);

            coll.enabled = false;
        }
    }
}
