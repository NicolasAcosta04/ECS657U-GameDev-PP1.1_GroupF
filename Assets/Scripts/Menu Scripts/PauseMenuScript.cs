using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject PauseMenuPanel;


    [SerializeField]
    private GameObject SettingsPanel;

    [SerializeField]
    Slider mouseXSlider;

    [SerializeField]
    Slider mouseYSlider;


    [SerializeField]
    private GameObject HelpPanel;
    [SerializeField]
    private GameObject MainMenuConfirmationPanel;
    [SerializeField]
    private GameObject PlayerUI; // Reference to the player UI


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
        PauseMenuPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
        PlayerUI.SetActive(true); // Re-enable player UI


        Time.timeScale = 1f; // Unpause the game
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the game
        Cursor.visible = false; // Hide the cursor
        isPaused = false;
    }

    public void Pause()
    {
        PauseMenuPanel.SetActive(true);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
        PlayerUI.SetActive(false); // Disable player UI


        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Canvas.ForceUpdateCanvases(); // Recalculate UI
        isPaused = true;
    }

    public void PauseMenu()
    {
        PauseMenuPanel.SetActive(true);

        SettingsPanel.SetActive(false);
        HelpPanel.SetActive(false);

        MainMenuConfirmationPanel.SetActive(false);
    }

    public void Settings()
    {
        PauseMenuPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(true);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void StoreGeneralSettings()
    {
        if (GeneralSettings.Instance != null)
        {
            GeneralSettings.Instance.sensX = mouseXSlider.value;
            GeneralSettings.Instance.sensY = mouseYSlider.value;
        }
    }

    public void Help()
    {
        PauseMenuPanel.SetActive(false);
        HelpPanel.SetActive(true);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(false);
    }

    public void ReturnToMainMenuConfirmation()
    {
        PauseMenuPanel.SetActive(false);
        HelpPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        MainMenuConfirmationPanel.SetActive(true);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // Reset time before changing scene
        SceneManager.LoadScene("Main Menu");
    }
}
