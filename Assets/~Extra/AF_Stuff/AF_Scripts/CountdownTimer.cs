using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float startTimeInSeconds;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Broadcast Event")]
    [SerializeField] private VoidEventSO gameEndEventSO;

    private float timer;

    private void Awake()
    {
        timer = startTimeInSeconds;      
    }

    private void OnEnable()
    {
        gameEndEventSO.OnEventRaised += StopTimer;
    }

    private void OnDisable()
    {
        gameEndEventSO.OnEventRaised -= StopTimer;
    }

    private void StopTimer()
    {
        timer = 0;
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            string t = string.Format("{0:0}:{1:00}", minutes, seconds);

            timerText.text = t;

            if (timer < 0)
            {
                gameEndEventSO.RaiseEvent();
            }
        }
    }
}
