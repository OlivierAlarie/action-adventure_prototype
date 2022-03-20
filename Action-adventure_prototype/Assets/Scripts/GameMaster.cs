using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance { get; private set; }

    public GameObject PauseMenu;
    public GameObject DefeatMenu;
    public GameObject ResumeButton;
    public GameObject RetryButton;
    public BasicPlayerController Player;
    public MaterialSwitcher NormalWord;
    public MaterialSwitcher RedWorld;
    public BasicCameraController Camera;
    public bool GameIsPaused = false;

    private GameMasterInput _gmInputs;
    private Scene _activeScene;

    // Start is called before the first frame update
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            _gmInputs = new GameMasterInput();
            _gmInputs.GameMasterControls.SwitchWorld.started += SwitchWorld;
            _gmInputs.GameMasterControls.Pause.started += SwitchPauseMenu;
        }
        _activeScene = SceneManager.GetActiveScene();
    }
    private void OnEnable()
    {
        _gmInputs.Enable();
    }

    private void OnDisable()
    {
        _gmInputs.Disable();
    }
    // Update is called once per frame
    private void Update()
    {
        if(Player.Health <= 0 && !DefeatMenu.activeSelf)
        {
            //Show Defeat Screen
            Defeat();
        }
    }

    private void SwitchWorld(InputAction.CallbackContext context)
    {
        NormalWord.ApplyMaterial();
        RedWorld.ApplyMaterial();
        Camera.SwitchWorld();
    }

    private void SwitchPauseMenu(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton()) return;

        if (Player.Health <= 0) return;

        if (GameIsPaused) { Resume(); }
        else { Pause(); }
    }

    public void Defeat()
    {
        Cursor.lockState = CursorLockMode.Confined;
        DefeatMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(RetryButton);
    }
    public void Retry()
    {
        Cursor.lockState = CursorLockMode.Locked;
        DefeatMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        Player.Health = 6;
        SceneManager.LoadScene(_activeScene.name);
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        PauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.Confined;
        EventSystem.current.SetSelectedGameObject(ResumeButton);
        PauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}
