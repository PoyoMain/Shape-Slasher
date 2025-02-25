using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO soundFXPlayEventSO;

    private AudioSource audioSource;

    private void Awake()
    {
        TryGetComponent(out audioSource);
    }

    private void OnEnable()
    {
        soundFXPlayEventSO.OnEventRaised += PlayRandomClip;
    }

    private void OnDisable()
    {
        soundFXPlayEventSO.OnEventRaised -= PlayRandomClip;
    }

    private void PlayRandomClip()
    {
        if (clips == null) return;
        if (clips.Length == 0) return;

        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];

        audioSource.PlayOneShot(clipToPlay);
    }
}
