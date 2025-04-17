using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControllerHapticsHandler : MonoBehaviour
{
    public static ControllerHapticsHandler Instance { get; private set; }

    [SerializeField] private OptionsSO options;

    private Coroutine controllerShakeCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    private void GroundForce()
    {
        ShakeController(0.005f, 0.005f, 1f);
    }



    public void ShakeController(float low = 0.4f, float high = 0.4f, float timeTillStop = 0.01f)
    {
        if (!options.ControllerRumble) return;

        if (controllerShakeCoroutine != null) StopCoroutine(controllerShakeCoroutine);

        controllerShakeCoroutine = StartCoroutine(ShakingController(low, high, timeTillStop));

    }

    private IEnumerator ShakingController(float low, float high, float timeTillStop)
    {
        Gamepad.current.SetMotorSpeeds(low, high);

        float initLow = low;
        float initHigh = high;

        float shakeTimer = timeTillStop;
        while (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            low = initLow * (shakeTimer / timeTillStop);
            high = initHigh * (shakeTimer / timeTillStop);

            Gamepad.current.SetMotorSpeeds(low, high);

            yield return null;
        }

        Gamepad.current.SetMotorSpeeds(0, 0);
        Gamepad.current.ResetHaptics();
        


        yield break;
    }
}
