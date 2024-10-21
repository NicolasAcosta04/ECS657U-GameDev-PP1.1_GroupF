using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScript : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("Level 1");
    }
        public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}

