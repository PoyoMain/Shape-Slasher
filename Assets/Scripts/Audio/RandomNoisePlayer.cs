using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SFXPlayer))]
public class RandomNoisePlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float noiseFrequencyMin = 5;
    [SerializeField] private float noiseFrequencyMax = 10;

    private SFXPlayer sfxPlayer;
    private float randomNouseTimer;

    private void Awake()
    {
        TryGetComponent(out sfxPlayer);

        randomNouseTimer = SetRandomNoiseTimer();
    }
    private void Update()
    {
        if (randomNouseTimer > 0)
        {
            randomNouseTimer -= Time.deltaTime;

            if (randomNouseTimer <= 0)
            {
                randomNouseTimer = SetRandomNoiseTimer();
                sfxPlayer.Play();
            }
        }
    }

    private float SetRandomNoiseTimer()
    {
        return Random.Range(noiseFrequencyMin, noiseFrequencyMax);
    }
}
