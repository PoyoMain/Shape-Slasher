using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnergyBar : MonoBehaviour
{
    [SerializeField] private IntEventSO energyUpdateEventSO;

    private Slider slider;

    private void Awake()
    {
        TryGetComponent(out slider);
    }

    private void OnEnable()
    {
        energyUpdateEventSO.OnEventRaised += UpdateEnergyBar;
    }

    private void OnDisable()
    {
        energyUpdateEventSO.OnEventRaised -= UpdateEnergyBar;
    }

    private void UpdateEnergyBar(int arg0)
    {
        slider.value = arg0;
    }
}
