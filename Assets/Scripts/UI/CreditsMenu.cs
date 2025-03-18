using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private float maxContentPos;

    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private Vector2 navigationInput;
    private bool navigationHit;

    private Button[] buttons;
    private Button currentButton;
    private int currentButtonIndex = 1;

    private EventSystem eventSystem;

    private float Step => maxContentPos / (buttons.Length - 1);


    private void OnEnable()
    { 
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        eventSystem = EventSystem.current;

        buttons = GetComponentsInChildren<Button>();
        eventSystem.currentSelectedGameObject.TryGetComponent(out currentButton);
    }

    private void Update()
    {
        navigationInput = controls.Navigate.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (navigationInput.y == 0) return;

        if (!eventSystem.currentSelectedGameObject.TryGetComponent(out Button eventCurrentButton)) return;

        if (eventCurrentButton == currentButton) return;
        
        currentButton = eventCurrentButton;

        Vector2 newPos = content.anchoredPosition;
        if (navigationInput == Vector2.down)
        {
            if (currentButtonIndex == buttons.Length) return;

            currentButtonIndex++;
            newPos.y = Mathf.Clamp(newPos.y + Step, 0, maxContentPos);

        }
        else if (navigationInput == Vector2.up)
        {
            if (currentButtonIndex == 1) return;

            currentButtonIndex--;
            newPos.y = Mathf.Clamp(newPos.y - Step, 0, maxContentPos);
        }

        content.anchoredPosition = newPos;

    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
