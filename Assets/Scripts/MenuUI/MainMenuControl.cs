using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    public MenuControl menuControl;
    
    public void OnClickSinglePlay()
    {
        menuControl.MoveToMenu(transform.name, "CreateNewWorld");
    }

    public void OnClickQuitGame()
    {
        GameManager.Mgr.QuitGame();
    }
}
