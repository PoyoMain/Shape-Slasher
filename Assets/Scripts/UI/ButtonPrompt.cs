using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class ButtonPrompt : MonoBehaviour
{
    [SerializeField] private InputActionReference action;
    [Space(20)]
    [SerializeField] private Image promptImage;
    [SerializeField] private Sprite keyboardSquareButtonSprite;
    [SerializeField] private Sprite keyboardRectButtonSprite;
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
        else promptImage.sprite = keyboardSquareButtonSprite;

        string text = action.action.controls[^1].displayName;

        if (Gamepad.current is DualShockGamepad)
        {
            text = text switch
            {
                "Triangle" => "<sprite=\"Icon_PlaystationButtons\" index=0>",
                "Circle" => "<sprite=\"Icon_PlaystationButtons\" index=1>",
                "Cross" => "<sprite=\"Icon_PlaystationButtons\" index=2>",
                "Square" => "<sprite=\"Icon_PlaystationButtons\" index=3>",
                "Options" => "<sprite=\"Icon_Playstation_Options\" index=0>",
                _ => text
            }; 
        }
        else if (Gamepad.current is XInputController)
        {
            text = text switch
            {
                "Start" => "<sprite=\"Icon_Xbox_Start\", index=0>",
                _ => text
            };
        }
        else if (Gamepad.current == null)
        {
            promptImage.sprite = text switch
            {
                "Enter" => keyboardRectButtonSprite,
                "Shift" => keyboardRectButtonSprite,
                "Space" => keyboardRectButtonSprite,
                _ => keyboardSquareButtonSprite,
            };
        }
        else
        {
            text = text switch
            {
                "Plus" => "<sprite=\"Icon_Switch_Plus\" index=0>",
                _ => text
            };
        }

        promptText.text = text;
    }

    private void ReplaceImage(InputDevice _, InputDeviceChange __)
    {
        ReplaceImage();
    }
}
