using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private GameObject _pauseMenu;
    private GameObject _defeatMenu;
    private GameObject _hud;

    public bool isPausedMenuVisible()
    {
        return _pauseMenu.activeSelf;
    }

    public bool isDefeatMenuVisible()
    {
        return _defeatMenu.activeSelf;
    }

    public bool isHUDVisible()
    {
        return _hud.activeSelf;
    }

    public void PauseMenuSetActive(bool value)
    {
        _pauseMenu.SetActive(value);
    }

    public void DefeatMenuSetActive(bool value)
    {
        _defeatMenu.SetActive(value);
    }

    public void HUDSetActive(bool value)
    {
        _hud.SetActive(value);
    }
}
