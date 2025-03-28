using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonPrompt : MonoBehaviour
{
    [SerializeField] private InputActionReference action;


    private void OnEnable()
    {
        print(action.action.bindings);
        print(action.action.controls);
        print(action.action.expectedControlType);
        print(action.action.type);
    }
}
