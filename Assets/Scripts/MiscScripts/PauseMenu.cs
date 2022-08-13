using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;
    private bool pastLoadScreen;
    public GameObject loadMenuUI;
    public GameObject pauseMenuUI;
    public GameObject gameHud;
    public GameObject tDgameHud;
    public GameObject centerHud;
    public bool gameOverBool;
    public GameObject pauseTitle;
    public GameObject resumeButton;

    void Start() {
        pause();
        gamePaused = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameOverBool != true && pastLoadScreen == true) {
            if(gamePaused) resume();
            else pause();
        }

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape)) && pastLoadScreen != true) {
            resume();
            pastLoadScreen = true;
            loadMenuUI.SetActive(false);
        }

        if (gamePaused == true) {
            AudioManager am = FindObjectOfType<AudioManager>();
            foreach (Sound s in am.sounds)
                am.Pause(s.name);
            FindObjectOfType<AudioManager>().Stop("JetEngine");
            FindObjectOfType<AudioManager>().Stop("GAU12");
        } else {
            AudioManager am = FindObjectOfType<AudioManager>();
            foreach (Sound s in am.sounds)
                am.UnPause(s.name);
        }
    }
    
    public void resume()
    {
        pauseMenuUI.SetActive(false);
        gameHud.SetActive(true);
        tDgameHud.SetActive(true);
        centerHud.SetActive(true);
        foreach (Canvas cavnasObject in GameObject.FindObjectsOfType<Canvas>()) {
            if (cavnasObject.renderMode == RenderMode.ScreenSpaceOverlay) {
                cavnasObject.enabled = true;
            }
        }
        Time.timeScale = 1f;
        gamePaused = false;
    }

    public void pause()
    {
        pauseMenuUI.SetActive(true);
        gameHud.SetActive(false);
        tDgameHud.SetActive(false);
        centerHud.SetActive(false);
        foreach (Canvas cavnasObject in GameObject.FindObjectsOfType<Canvas>()) {
            if (cavnasObject.renderMode == RenderMode.ScreenSpaceOverlay) {
                cavnasObject.enabled = false;
            }
        }
        Time.timeScale = 0f;
        gamePaused = true;
    }

    public void gameOver() {
        if (gameOverBool != true) {
            pauseTitle.GetComponent<TextMeshProUGUI>().text = string.Format("[ MISSION FAILED ]");
            resumeButton.SetActive(false);
            pauseMenuUI.SetActive(true);
            gameHud.SetActive(false);
            tDgameHud.SetActive(false);
            centerHud.SetActive(false);
            foreach (Canvas cavnasObject in GameObject.FindObjectsOfType<Canvas>()) {
                if (cavnasObject.renderMode == RenderMode.ScreenSpaceOverlay) {
                    cavnasObject.enabled = false;
                }
            }
            Time.timeScale = 0f;
            gamePaused = true;
        }
    }

    public bool getGamePaused() {
        return gamePaused;
    }

    public void restartGame()
    {
        resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        AudioManager am = FindObjectOfType<AudioManager>();
        foreach (Sound s in am.sounds)
            if (s.name != "MUSIC Danger Zone") am.Stop(s.name);
    }

    public void quitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void quitToMenu()
    {
        resume();
        SceneManager.LoadScene(0);
        Debug.Log("Quit");
        AudioManager am = FindObjectOfType<AudioManager>();
        foreach (Sound s in am.sounds)
            if (s.name != "MUSIC Danger Zone") am.Stop(s.name);
    }
}
