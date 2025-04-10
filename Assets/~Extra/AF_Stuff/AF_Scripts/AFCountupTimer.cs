using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AFCountupTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private VoidEventSO gameEndEventSO;

    private float timer;
    private bool gameRunning = true;

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
        gameRunning = false;
    }

    private void Update()
    {
        if (gameRunning)
        {
            timer += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            string t = string.Format("{0:0}:{1:00}", minutes, seconds);

            timeText.text = t;
        }

    }
}
