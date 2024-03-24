using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Menu
{
    public override void Enter()
    {
        gameObject.SetActive(true);
    }

    public override void Exit()
    {
        gameObject.SetActive(false);
    }

    public void OnStartGameButton()
    {
        GameManager.ins.StartGame();
    }

    public void OnSettingsButton()
    {
        GameManager.ins.OpenSettingsMenu();
    }

    public void OnCreditsButton()
    {
        //TODO: credits
    }
}
