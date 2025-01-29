using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input Reader")]
public class InputReaderSO : ScriptableObject
{
    private PlayerControls _playerControls;
    public PlayerControls.GameplayControlsActions Controls { get; private set; }

    private void OnEnable()
    {
        _playerControls = new();
        Controls = _playerControls.GameplayControls;
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }
}
