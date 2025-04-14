using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private IntEventSO currencyUpdateEventSO;
    [SerializeField] private TextMeshProUGUI currencyText;

    private void OnEnable()
    {
        currencyUpdateEventSO.OnEventRaised += UpdateCurrency;
    }

    private void OnDisable()
    {
        currencyUpdateEventSO.OnEventRaised -= UpdateCurrency;
    }

    private void UpdateCurrency(int arg0)
    {
        currencyText.text = arg0.ToString();
    }
}
