using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType type;
    public PickupType Type => type;

    public enum PickupType { Health, Currency}
}
