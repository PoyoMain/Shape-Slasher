using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractionTrigger : MonoBehaviour
{
    [SerializeField] private bool onlyDoEnterOnce;
    public UnityEvent OnTriggerEnter;
    public UnityEvent OnTriggerExit;
    private bool enteredOnce;

    private Collider2D coll;

    private void Awake()
    {
        TryGetComponent(out coll);
        if (coll != null) coll.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (onlyDoEnterOnce && enteredOnce) return;
            
            OnTriggerEnter?.Invoke();
            if (onlyDoEnterOnce) enteredOnce = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnTriggerExit?.Invoke();
        }
    }
}
