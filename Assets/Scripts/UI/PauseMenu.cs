using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private InputReaderSO inputReader;

    [Header("Inspector Objects")]
    [SerializeField] private Button firstSelectButton;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Broadcast Events")]
    [SerializeField] private VoidEventSO gamePausedEventSO;
    [SerializeField] private VoidEventSO gameUnpausedEventSO;

    [Header("Listen Events")]
    [SerializeField] private VoidEventSO gameQuitEventSO;

    // Properties
    private PlayerControls.GameplayControlsActions Controls => inputReader.Controls;

    // Private Variables
    private bool isPaused;

    private void OnEnable()
    {
        Controls.Pause.performed += Pause;
        gameQuitEventSO.OnEventRaised += ResetPauseMenu;
    }

    private void OnDisable()
    {
        Controls.Pause.performed -= Pause;
        gameQuitEventSO.OnEventRaised -= ResetPauseMenu;
    }

    private void Pause(UnityEngine.InputSystem.InputAction.CallbackContext _)
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0 : 1;
        pauseMenuPanel.SetActive(isPaused);


        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectButton.gameObject);
            gamePausedEventSO.RaiseEvent();
        }
        else gameUnpausedEventSO.RaiseEvent();
    }

    public void Pause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0 : 1;
        pauseMenuPanel.SetActive(isPaused);


        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectButton.gameObject);
            gamePausedEventSO.RaiseEvent();
        }
        else gameUnpausedEventSO.RaiseEvent();
    }

    private void ResetPauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1;
    }
}
