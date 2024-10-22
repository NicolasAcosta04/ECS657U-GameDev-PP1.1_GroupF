using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class MainMenuScript : MonoBehaviour
{
    [SerializeField] 
    GameObject MainMenuPanel;
    [SerializeField] 
    GameObject PlayGamePanel;
    [SerializeField] 
    GameObject CustomSettingsPanel;
    [SerializeField] 
    GameObject SettingsPanel;
    [SerializeField]
    TMP_InputField seedInputField;
    [SerializeField]
    TMP_InputField roomCountInputField;
    [SerializeField]
    TMP_InputField enemyRoomCountInputField;
    [SerializeField]
    TMP_InputField itemRoomCountInputField;
    [SerializeField]
    Slider mouseXSlider;

    [SerializeField]
    Slider mouseYSlider;

    //navigate menuee
    public void MainMenu(){
        MainMenuPanel.SetActive(true);
        PlayGamePanel.SetActive(false);
        CustomSettingsPanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }
    public void PlayGame(){
        MainMenuPanel.SetActive(false);
        PlayGamePanel.SetActive(true);
        CustomSettingsPanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

    public void CustomSettings(){
        MainMenuPanel.SetActive(false);
        PlayGamePanel.SetActive(false);
        CustomSettingsPanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }

    public void Settings(){
        MainMenuPanel.SetActive(false);
        PlayGamePanel.SetActive(false);
        CustomSettingsPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    // store items in general settings

    public void EasyButton(){
        if (GeneralSettings.Instance != null){
            
            GeneralSettings.Instance.sensX = 5;
            GeneralSettings.Instance.sensY = 5;
            GeneralSettings.Instance.Seed = 0;
            GeneralSettings.Instance.RoomCount = 75;
            GeneralSettings.Instance.EnemyRoomCount = 1;
            GeneralSettings.Instance.ItemRoomCount = 5;

        }
    }
    public void MediumButton(){
        if (GeneralSettings.Instance != null){

            GeneralSettings.Instance.sensX = 5;
            GeneralSettings.Instance.sensY = 5;
            GeneralSettings.Instance.Seed = 0;
            GeneralSettings.Instance.RoomCount = 150;
            GeneralSettings.Instance.EnemyRoomCount = 5;
            GeneralSettings.Instance.ItemRoomCount = 10;

        }
    }
    public void HardButton(){
        if (GeneralSettings.Instance != null){

            GeneralSettings.Instance.sensX = 5;
            GeneralSettings.Instance.sensY = 5;
            GeneralSettings.Instance.Seed = 0;
            GeneralSettings.Instance.RoomCount = 300;
            GeneralSettings.Instance.EnemyRoomCount = 20;
            GeneralSettings.Instance.ItemRoomCount = 20;

        }
    }

    public void StoreGeneralSettings(){
        if (GeneralSettings.Instance != null){
            GeneralSettings.Instance.sensX = mouseXSlider.value;
            GeneralSettings.Instance.sensY = mouseYSlider.value;
        }
    }

    public void StoreCustomSettings(){
        if (GeneralSettings.Instance != null){

        GeneralSettings.Instance.Seed = int.Parse(seedInputField.text);
        GeneralSettings.Instance.RoomCount = int.Parse(roomCountInputField.text);
        GeneralSettings.Instance.EnemyRoomCount = int.Parse(enemyRoomCountInputField.text);
        GeneralSettings.Instance.ItemRoomCount = int.Parse(itemRoomCountInputField.text);

        }
    }
}
