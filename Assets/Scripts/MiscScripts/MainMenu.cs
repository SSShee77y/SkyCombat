using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void start()
    {
        SceneManager.LoadScene(1);
    }

    public void select(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void select(int num)
    {
        SceneManager.LoadScene(num);
    }

    public void quitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetChild(1).gameObject.activeInHierarchy == true) {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        }
        if (Time.timeScale != 1f) Time.timeScale = 1f;
        FindObjectOfType<AudioManager>().UnPause("MUSIC Danger Zone");
        FindObjectOfType<AudioManager>().Pause("JetEngine");
        FindObjectOfType<AudioManager>().Pause("GAU12");
    }
}
