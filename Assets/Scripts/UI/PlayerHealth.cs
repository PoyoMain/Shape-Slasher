using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int startingHealth;
    [SerializeField] private Animator healthPrefab;
    [SerializeField] private Transform healthParent;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO playerDeathEventSO;

    [Header("Listen Events")]
    [SerializeField] private IntEventSO playerDamagedEventSO;

    private List<Animator> healthImages;
    private int Health => healthImages.Count;

    private void Awake()
    {
        healthImages = new();

        for (int i = 0; i < startingHealth; i++)
        {
            Animator spawnedHealth = Instantiate(healthPrefab, healthParent);
            healthImages.Add(spawnedHealth);
        }
    }

    private void OnEnable()
    {
        playerDamagedEventSO.OnEventRaised += TakeDamage;
    }

    private void OnDisable()
    {
        playerDamagedEventSO.OnEventRaised -= TakeDamage;
    }

    private void TakeDamage(int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            if (Health > 0)
            {
                healthImages[^1].SetTrigger("Disappear");
                healthImages.RemoveAt(healthImages.Count - 1);
            }
        }
        

        if (Health <= 0)
        {
            playerDeathEventSO.RaiseEvent();
        }
    }
}
