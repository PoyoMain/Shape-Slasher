using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [SerializeField] private Animator[] doorParts;

    private void TogglePartSpin()
    {
        for (int i = 0; i < doorParts.Length; i++)
        {
            doorParts[i].SetTrigger("Toggle");
        }
    }
}
