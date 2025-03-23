using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbiencePlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] ambienceClips;

    private AudioSource audioSource;
    private float ambienceTimer;

    private void Awake()
    {
        TryGetComponent(out audioSource);

        PlayAmbienceClip();
    }

    private void Update()
    {
        if (ambienceTimer > 0)
        {
            ambienceTimer -= Time.deltaTime;

            if (ambienceTimer <= 0)
            {
                PlayAmbienceClip();
            }
        }
    }

    private void PlayAmbienceClip()
    {
        if (ambienceClips.Length == 0) return;

        AudioClip clipToPlay = ambienceClips[Random.Range(0, ambienceClips.Length)];

        audioSource.clip = clipToPlay;
        audioSource.Play();
        ambienceTimer = clipToPlay.length;
    }
}
