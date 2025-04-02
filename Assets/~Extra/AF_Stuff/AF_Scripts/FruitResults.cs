using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FruitResults : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO fruitSlicedEventSO;

    int score;

    private void OnEnable()
    {
        fruitSlicedEventSO.OnEventRaised += IncreaseScore;
    }

    private void OnDisable()
    {
        fruitSlicedEventSO.OnEventRaised -= IncreaseScore;
    }

    private void IncreaseScore()
    {
        score += 1;
        scoreText.text = "Score: " + score;
    }
}
