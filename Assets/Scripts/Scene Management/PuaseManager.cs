using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainPanel;
    [SerializeField] private GameObject HelpPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject MainMenuConfirmationPanel;
    [SerializeField] private GameObject PlayerUI; // Reference to the player UI

    [SerializeField] private Slider sensXSlider; // Slider for sensitivity X
    [SerializeField] private Slider sensYSlider; // Slider for sensitivity Y


    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        MainPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
        PlayerUI.SetActive(true); // Re-enable player UI

        Time.timeScale = 1f; // Unpause the game
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the game
        Cursor.visible = false; // Hide the cursor
        isPaused = false;

        // Notify PlayerCam
        PlayerCam playerCam = Object.FindFirstObjectByType<PlayerCam>();
        if (playerCam != null)
        {
            playerCam.isPaused = false;
        }
    }

    public void Pause()
    {
        MainPanel.SetActive(true);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
        PlayerUI.SetActive(false); // Disable player UI

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Canvas.ForceUpdateCanvases(); // Recalculate UI
        isPaused = true;

        // Notify PlayerCam
        PlayerCam playerCam = Object.FindFirstObjectByType<PlayerCam>();
        if (playerCam != null)
        {
            playerCam.isPaused = true;
        }
    }

    public void Back()
    {
        MainPanel.SetActive(true);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void Help()
    {
        MainPanel.SetActive(false);
        HelpPanel.SetActive(true);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void Settings()
    {
        MainPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(true);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void ReturnToMainMenuConfirmation()
    {
        MainPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(true);
    }

    public void ConfirmReturnToMainMenu()
    {
        Time.timeScale = 1f; // Reset time before changing scene
        SceneManager.LoadScene("Main Menu");
    }

    public void CancelReturnToMainMenu()
    {
        MainPanel.SetActive(true);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void SaveSettings()
    {
        if (GeneralSettings.Instance != null)
        {
            GeneralSettings.Instance.sensX = sensXSlider.value;
            GeneralSettings.Instance.sensY = sensYSlider.value;

            Debug.Log($"Saved Sensitivity - X: {sensXSlider.value}, Y: {sensYSlider.value}");

            // Notify PlayerCam of the updated settings
            PlayerCam playerCam = Object.FindFirstObjectByType<PlayerCam>(); // Updated to use the new method
            if (playerCam != null)
            {
                playerCam.UpdateSensitivity(GeneralSettings.Instance.sensX, GeneralSettings.Instance.sensY);
            }
            else
            {
                Debug.LogWarning("PlayerCam not found!");
            }
        }
    }

}
