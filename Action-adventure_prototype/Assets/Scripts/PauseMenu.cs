using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    Scene _activeSceneName;

    private void Awake() {
        _activeSceneName = SceneManager.GetActiveScene();
    }

    private void Update() {
        if (Input.GetButtonDown("Pause")) {
            if (GameIsPaused) {
                Resume();
            } else {
                Pause();
            }
        }
        Debug.Log(_activeSceneName);
    }
    public void Resume() {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    void Pause() {
        Cursor.lockState = CursorLockMode.Confined;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Reload() {
        SceneManager.LoadScene(_activeSceneName.name);
        Resume();
    }
    public void QuitGame() {
        Application.Quit();
    }
}
