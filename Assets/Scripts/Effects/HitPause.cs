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
    private float stunTimer;

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
        if (Time.timeScale == 0)
        {
            stunTimer += hitPauseTime;
            return;
        }
        else stunTimer = hitPauseTime;

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
        Time.timeScale = 0f;

        while (stunTimer > 0)
        {
            stunTimer -= Time.fixedUnscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1;
        stunTimer = 0;

        yield break;
    }

    #endregion
}
