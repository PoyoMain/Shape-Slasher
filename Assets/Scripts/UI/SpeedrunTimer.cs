using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
    [SerializeField] private OptionsSO options;
    [SerializeField] private TextMeshProUGUI timerTextBox;
    [SerializeField] private VoidEventSO timerStartEventSO;
    [SerializeField] private VoidEventSO timerSucceedEventSO;
    [SerializeField] private VoidEventSO[] timerFailEventSOs;

    public string TimerValue { get; private set; }

    private bool active;
    private float timer;

    private void Awake()
    {
        if (options == null) { Debug.LogError("Options not set in speedrun timer script"); return; }
        if (timerTextBox == null) { Debug.LogError("Timer textbox not set in speedrun timer script"); return; }

        if (options.SpeedrunMode) timerTextBox.gameObject.SetActive(true);
        else DestoyThis();
    }

    private void OnEnable()
    {
        timerStartEventSO.OnEventRaised += StartTimer;
        timerSucceedEventSO.OnEventRaised += StopTimer;

        for (int i = 0; i < timerFailEventSOs.Length; i++)
        {
            timerFailEventSOs[i].OnEventRaised += DestoyThis;
        }
    }

    private void OnDisable()
    {
        timerStartEventSO.OnEventRaised -= StartTimer;
        timerSucceedEventSO.OnEventRaised -= StopTimer;


        for (int i = 0; i < timerFailEventSOs.Length; i++)
        {
            timerFailEventSOs[i].OnEventRaised -= DestoyThis;
        }
    }

    private void FixedUpdate()
    {
        if (active) CountUpTimer();
    }

    private void CountUpTimer()
    {
        timer += Time.fixedDeltaTime;
        timerTextBox.text = TimeSpan.FromSeconds(timer).ToString("mm\\:ss\\.fff");

    }

    private void StartTimer()
    {
        if (options.SpeedrunMode)
        {
            timer = 0f;
            active = true;
            DontDestroyOnLoad(gameObject);
        }
        else DestoyThis();
    }

    private void StopTimer()
    {
        active = false;
        TimerValue = timerTextBox.text;
    }

    public void DestoyThis()
    {
        Destroy(gameObject);
    }
}
