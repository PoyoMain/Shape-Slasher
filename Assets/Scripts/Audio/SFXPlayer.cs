using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    private AudioSource audioSource;

    private void Awake()
    {
        TryGetComponent(out audioSource);
    }

    public void Play()
    {
        if (clips == null) return;
        if (clips.Length == 0) return;

        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];

        audioSource.PlayOneShot(clipToPlay);
    }
}
