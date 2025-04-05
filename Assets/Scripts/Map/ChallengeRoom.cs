using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeRoom : MonoBehaviour
{
    [SerializeField] private Animator[] autoDoors;
    [SerializeField] private EnemyDeathEvent[] enemies;
    [SerializeField] private bool isBossRoom;

    private int EnemiesInRoom => enemies.Length;
    private int enemiesKilled;

    private void Awake()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].OnDeath += EnemyDied;
        }
    }

    private void EnemyDied()
    {
        enemiesKilled++;

        if (enemiesKilled >= EnemiesInRoom)
        {
            LockRoom();
        }
    }

    public void LockRoom()
    {
        if (EnemiesInRoom == 0 && !isBossRoom) return;
        //Freeze();

        for (int i = 0; i < autoDoors.Length; i++)
        {
            autoDoors[i].SetTrigger("Toggle");
        }

        //Invoke(nameof(UnFreeze), 1);
    }

    private void Freeze()
    {
        Time.timeScale = 0f;
    }

    private void UnFreeze()
    {
        Time.timeScale = 1f;
    }
}
