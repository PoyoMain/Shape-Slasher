using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private UnityEvent onOptionsClose;

    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private void OnEnable()
    {
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        controls.Cancel.performed += Cancel;
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        onOptionsClose?.Invoke();
    }

    private void OnDisable()
    {
        controls.Cancel.performed -= Cancel;
        controls.Disable();
    }
}
