using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    [SerializeField] private UnityEvent action;
    [SerializeField] private float delay;
    [SerializeField] private bool activateOnEnable;

    private void OnEnable()
    {
        if (activateOnEnable) Invoke(nameof(Activate), delay);
    }

    public void Activate()
    {
        action?.Invoke();
    }
}
