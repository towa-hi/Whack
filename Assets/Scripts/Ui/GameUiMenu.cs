using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUiMenu : Menu
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    public RectTransform healthBar;
    public RectTransform levelTimeBar;
    public Image weaponImage;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponDamageText;
    public TextMeshProUGUI weaponSpeedText;
    public TextMeshProUGUI coinText;
    public PausePanel pausePanel;
    public NextLevelPanel nextLevelPanel;
    public override void Enter()
    {
        gameObject.SetActive(true);
    }

    public override void Exit()
    {
        Clear();
        gameObject.SetActive(false);
    }

    void Update()
    {
        GetGameState();
    }
    public void GetGameState()
    {
        if (GameManager.GetState().isPlaying)
        {
            levelText.text = "Level " + GameManager.GetState().level;
            scoreText.text = GameManager.GetState().score.ToString();
            hpText.text = GameManager.GetState().hp + "/" + GameManager.GetState().maxHp;
            weaponImage.sprite = GameManager.GetState().weapon.graphic;
            weaponNameText.text = GameManager.GetState().weapon.weaponName;
            weaponDamageText.text = "Damage: " + GameManager.GetState().weapon.damage;
            weaponSpeedText.text = "Speed: " + GameManager.GetState().weapon.actionDuration;
            UpdateHealthBar(GameManager.GetState().hp, GameManager.GetState().maxHp);
            UpdateLevelTimeBar(GameManager.GetState().levelTimeRemaining, GameManager.GetState().GetLevelDuration());
            coinText.text = "x" + GameManager.GetState().coins;
        }
    }


    public void OnPause(bool paused)
    {
        if (paused)
        {
            pausePanel.gameObject.SetActive(true);
            pausePanel.SetText("gaym is paus");
        }
        else
        {
            pausePanel.gameObject.SetActive(false);
        }
    }
    
    public void Clear()
    {
        
    }
    
    void UpdateHealthBar(float currentHp, float maxHp)
    {
        // Calculate the current HP percentage
        float hpPercentage = currentHp / maxHp;
        healthBar.localScale = new Vector3(hpPercentage, 1, 1);
    }

    void UpdateLevelTimeBar(float currentTime, float maxTime)
    {
        float timePercentage = currentTime / maxTime;
        levelTimeBar.localScale = new Vector3(timePercentage, 1, 1);
    }
    
}
