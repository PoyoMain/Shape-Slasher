using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private Vector2 navigationInput;
    private bool navigationHit;

    private EventSystem eventSystem;

    private void OnEnable()
    { 
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        eventSystem = EventSystem.current;
    }

    private void Update()
    {
        navigationInput = controls.Navigate.ReadValue<Vector2>();
        navigationHit = controls.Navigate.WasPerformedThisFrame();
    }

    private void FixedUpdate()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (navigationInput == Vector2.zero || !navigationHit) return;

        print(eventSystem.currentSelectedGameObject.transform.position.y);
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Print()
    {

    }
}
