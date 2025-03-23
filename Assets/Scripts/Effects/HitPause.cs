using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPause : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float playerDeathTime;
    [SerializeField] private float enemyDeathTime;
    [SerializeField] private float bossDeathTime;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO playerDeathEventSO;
    [SerializeField] private VoidEventSO enemyDeathEventSO;
    [SerializeField] private VoidEventSO bossDeathEventSO;

    private Coroutine freezeCoroutine;

    #region OnEnable/OnDisable

    private void OnEnable()
    {
        playerDeathEventSO.OnEventRaised += PlayerDeathFreeze;
        enemyDeathEventSO.OnEventRaised += EnemyDeathFreeze;
        bossDeathEventSO.OnEventRaised += BossDeathFreeze;
    }

    private void OnDisable()
    {
        playerDeathEventSO.OnEventRaised -= PlayerDeathFreeze;
        enemyDeathEventSO.OnEventRaised -= EnemyDeathFreeze;
        bossDeathEventSO.OnEventRaised -= BossDeathFreeze;
    }

    #endregion

    #region Death Methods

    private void PlayerDeathFreeze()
    {
        Freeze(playerDeathTime);
    }

    private void EnemyDeathFreeze()
    {
        Freeze(enemyDeathTime);
    }

    private void BossDeathFreeze()
    {
        Freeze(bossDeathTime);
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

        Time.timeScale = originalTimeScale;
    }

    #endregion
}
