using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SpeedrunTimerDisplay : MonoBehaviour
{
    private TextMeshProUGUI displayTextBox;
    private void Awake()
    {
        TryGetComponent(out displayTextBox);
    }

    private void Start()
    {
        SpeedrunTimer timer = FindObjectOfType<SpeedrunTimer>(includeInactive: true);

        if (timer == null)
        {
            displayTextBox.text = "";
            return;
        }

        displayTextBox.text = timer.TimerValue;
    }
}
