using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ConformationPanel : MonoBehaviour
{
    [SerializeField] private UnityEvent onConfirm;
    [SerializeField] private UnityEvent onDeny;

    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private void OnEnable()
    {
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        controls.Submit.performed += Confirm;
        controls.Cancel.performed += Deny;
    }

    private void Confirm(InputAction.CallbackContext context)
    {
        onConfirm?.Invoke();
    }

    private void Deny(InputAction.CallbackContext context)
    {
        onDeny?.Invoke();
    }
}
