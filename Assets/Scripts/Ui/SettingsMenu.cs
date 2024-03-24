using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : Menu
{
    public override void Enter()
    {
        gameObject.SetActive(true);
    }

    public override void Exit()
    {
        gameObject.SetActive(false);
    }

    public void OnBackButton()
    {
        GameManager.ins.OpenMainMenu();
    }
}
