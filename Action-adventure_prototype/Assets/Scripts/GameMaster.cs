using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance { get; private set; }

    public PauseMenu PauseMenu;
    public BasicPlayerController Player;
    public MaterialSwitcher NormalWord;
    public MaterialSwitcher RedWorld;
    public BasicCameraController Camera;

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
        
    }

    private void SwitchWorld(InputAction.CallbackContext context)
    {
        NormalWord.ApplyMaterial();
        RedWorld.ApplyMaterial();
        Camera.SwitchWorld();
    }

    private void Reload()
    {
        SceneManager.LoadScene(_activeScene.name);
        PauseMenu.Resume();
    }
    private void QuitGame()
    {
        Application.Quit();
    }
}
