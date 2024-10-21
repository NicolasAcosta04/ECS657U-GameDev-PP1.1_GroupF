using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManage : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadScene("Level 1");
    }

    public void Restart(){
        SceneManager.LoadScene("Level 1");
    }

    public void MainMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    private void OnTriggerEnter(Collider other){

        if (other.CompareTag("Player")){

            if (SceneManager.GetActiveScene().name == "Level 1"){
            SceneManager.LoadScene("Level 2");
            }

            else if (SceneManager.GetActiveScene().name == "Level 2"){
            SceneManager.LoadScene("Victory Screen");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            }

        }
    }
}
