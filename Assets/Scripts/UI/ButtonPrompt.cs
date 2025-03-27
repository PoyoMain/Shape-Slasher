using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonPrompt : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [Space(20)]
    [SerializeField] private Image promptImage;
    [SerializeField] private Sprite keyboardButtonSprite;
    [SerializeField] private Sprite controllerButtonSprite;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI promptText;

    private void OnEnable()
    {
        ReplaceImage();
        InputSystem.onDeviceChange += ReplaceImage;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= ReplaceImage;
    }

    private void ReplaceImage()
    {
        if (Gamepad.current != null) promptImage.sprite = controllerButtonSprite;
        else promptImage.sprite = keyboardButtonSprite;

        promptText.text = action.action.controls[^1].displayName;
    }

    private void ReplaceImage(InputDevice device, InputDeviceChange change)
    {
        if (Gamepad.current != null) promptImage.sprite = controllerButtonSprite;
        else promptImage.sprite = keyboardButtonSprite;

        promptText.text = action.action.controls[^1].displayName;
    }
}
