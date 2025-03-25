using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShaker : MonoBehaviour
{
    [SerializeField] private VoidEventSO startShakingEvent;
    [SerializeField] private VoidEventSO stopShakingEvent;

    private CinemachineImpulseSource impulseSource;
    private bool isShaking;

    private void Awake()
    {
        TryGetComponent(out impulseSource);
    }

    private void OnEnable()
    {
        startShakingEvent.OnEventRaised += StartShaking;
        stopShakingEvent.OnEventRaised += StopShaking;
    }

    private void OnDisable()
    {
        startShakingEvent.OnEventRaised -= StartShaking;
        stopShakingEvent.OnEventRaised -= StopShaking;
    }

    private void Update()
    {
        if (isShaking)
        {
            impulseSource.GenerateImpulse();
        }
    }

    private void StartShaking()
    {
        isShaking = true;
    }

    private void StopShaking()
    {
        isShaking = false;
    }
}
