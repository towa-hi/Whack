using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager ins;
    Menu currentMenu;

    public MainMenu mainMenu;
    public SettingsMenu settingsMenu;
    public GameObject background;
    public List<Cell> cells;
    
    void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != ins)
        {
            Destroy(gameObject);
        }
        SetCurrentMenu(mainMenu);

    }

    void Start()
    {
        InputManager.ins.hitPerformed += HandleHit;
        InputManager.ins.pauseInputPerformed += HandlePauseInput;
    }

    public Cell GetCell(int id)
    {
        foreach (Cell cell in cells)
        {
            if (cell.id == id)
            {
                return cell;
            }
        }
        throw new Exception("invalid id");
    }
    
    void HandleHit(object sender, int cellId)
    {
        Debug.Log(cellId + " pressed");
        foreach (Cell cell in cells)
        {
            cell.OnAnyCellHit(cellId);
        }
    }

    void HandlePauseInput(object sender, EventArgs e)
    {
        Debug.Log("pause or unpause game");
    }
    
    void SetCurrentMenu(Menu newMenu)
    {
        if (currentMenu != null)
        {
            currentMenu.Exit();
        }
        currentMenu = newMenu;
        if (currentMenu != null)
        {
            currentMenu.Enter();
        }
        background.SetActive(currentMenu != null);
    }

    public void StartGame()
    {
        SetCurrentMenu(null);
    }

    public void OpenMainMenu()
    {
        SetCurrentMenu(mainMenu);
    }

    public void OpenSettingsMenu()
    {
        SetCurrentMenu(settingsMenu);
    }

    public void OnPauseButton()
    {
        Debug.Log("OnPauseButton()");
    }
}

public abstract class Menu : MonoBehaviour
{
    public abstract void Enter();
    public abstract void Exit();

    void Awake()
    {
    }
}