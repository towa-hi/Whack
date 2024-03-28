using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMenu : Menu
{
    public override void Enter()
    {
        gameObject.SetActive(true);
    }

    public override void Exit()
    {
        gameObject.SetActive(false);
    }
    
    public void OnRestartButton()
    {
        GameManager.ins.StartGame();
    }

    public void OnTitleButton()
    {
        GameManager.ins.OpenMainMenu();
    }
}
