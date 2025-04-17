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

        if (audioSource == null)
        {
            if (!TryGetComponent(out audioSource)) return;
            if (!audioSource.isActiveAndEnabled) return;
        }

        if (clipToPlay != null) audioSource.PlayOneShot(clipToPlay);
    }

    public void PlayClipAtPoint()
    {
        if (clips == null) return;
        if (clips.Length == 0) return;

        AudioClip clipToPlay = clips[Random.Range(0, clips.Length)];

        if (clipToPlay != null) AudioSource.PlayClipAtPoint(clipToPlay, transform.position, 1);
    }
}
