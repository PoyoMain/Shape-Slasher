using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private float maxContentPos;

    [SerializeField] private UnityEvent onCreditsClose;
    [SerializeField] private UnityEvent onConceptOpen;
    [SerializeField] private UnityEvent onConceptClose;

    private PlayerControls _playerControls;
    private PlayerControls.UIControlsActions controls;

    private Vector2 navigationInput;

    private Button[] buttons;
    private Button currentButton;
    private int currentButtonIndex = 1;

    private GameObject prevSelectedObject;
    private EventSystem eventSystem;

    private bool conceptOpen = false;

    private float Step => maxContentPos / (buttons.Length - 1);


    private void OnEnable()
    { 
        _playerControls = new();
        controls = _playerControls.UIControls;
        controls.Enable();

        eventSystem = EventSystem.current;

        buttons = GetComponentsInChildren<Button>();
        eventSystem.currentSelectedGameObject.TryGetComponent(out currentButton);

        controls.Cancel.performed += Cancel;
    }

    private void Update()
    {
        navigationInput = controls.Navigate.ReadValue<Vector2>();
    }

    private void Cancel(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        if (conceptOpen)
        {
            eventSystem.SetSelectedGameObject(prevSelectedObject);
            conceptOpen = false;
            onConceptClose?.Invoke();
        }
        else
        {
            onCreditsClose?.Invoke();

            Vector2 newPos = content.anchoredPosition;
            newPos.y = 0;
            content.anchoredPosition = newPos;

            currentButtonIndex = 1;
        }
    }

    private void FixedUpdate()
    {
        HandleNavigation();
    }

    private void HandleNavigation()
    {
        if (navigationInput.y == 0) return;

        if (conceptOpen) return;

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

    public void OpenConceptArt()
    {
        prevSelectedObject = eventSystem.currentSelectedGameObject;
        conceptOpen = true;
        onConceptOpen?.Invoke();
    }

    private void OnDisable()
    {
        controls.Cancel.performed -= Cancel;
        controls.Disable();
    }
}
