using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPause : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hitPauseTime;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO hitPauseEventSO;
    [SerializeField] private VoidEventSO playerDeathEventSO;

    private Coroutine freezeCoroutine;

    #region OnEnable/OnDisable

    private void OnEnable()
    {
        hitPauseEventSO.OnEventRaised += PauseMethod;
        playerDeathEventSO.OnEventRaised += PlayerDeathEventSO_OnEventRaised;
    }

    private void PlayerDeathEventSO_OnEventRaised()
    {
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        hitPauseEventSO.OnEventRaised -= PauseMethod;
        playerDeathEventSO.OnEventRaised -= PlayerDeathEventSO_OnEventRaised;
    }

    #endregion

    #region Death Methods

    private void PauseMethod()
    {
        Freeze(hitPauseTime);
    }

    #endregion

    #region Freezing

    private void Freeze(float duration)
    {
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);

        freezeCoroutine = StartCoroutine(DoFreeze(duration));
    }

    private IEnumerator DoFreeze(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1;
    }

    #endregion
}
