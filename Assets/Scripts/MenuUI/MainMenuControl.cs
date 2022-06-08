using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    public MenuControl menuControl;
    
    public void OnClickSinglePlay()
    {
        //menuControl.MoveToMenu(transform.name, "CreateNewWorld");
        menuControl.MoveToMenu(transform.name, "SelectWorld");
    }

    public void OnClickSettings()
    {
        menuControl.MoveToMenu(transform.name, "SettingsMenu");
    }

    public void OnClickQuitGame()
    {
        GameManager.Mgr.QuitGame();
    }
}
