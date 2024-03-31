using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUiMenu : Menu
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    public RectTransform healthBar;
    public RectTransform whiteHealthBar;
    public RectTransform levelTimeBar;
    public Image weaponImage;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponDamageText;
    public TextMeshProUGUI weaponSpeedText;
    public TextMeshProUGUI coinText;
    public PausePanel pausePanel;
    public NextLevelPanel nextLevelPanel;
    
    public float whiteHealthBarTweenDuration = 1f;
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
        if (GameManager.GetState() != null)
        {
            levelText.text = "Level " + GameManager.GetState().level;
            scoreText.text = GameManager.GetState().score.ToString();
            hpText.text = GameManager.GetState().hp + "/" + GameManager.GetState().maxHp;
            weaponImage.sprite = GameManager.GetState().weapon.graphic;
            weaponNameText.text = GameManager.GetState().weapon.weaponName;
            weaponDamageText.text = "Damage: " + GameManager.GetState().weapon.damage;
            weaponSpeedText.text = "Speed: " + GameManager.GetState().weapon.actionDuration;
            if (GameManager.GetState().hp != currentlyDisplayedHp ||
                GameManager.GetState().maxHp != currentlyDisplayedMaxHp)
            {
                UpdateHealthBar(GameManager.GetState().hp, GameManager.GetState().maxHp);
            }
            
            UpdateLevelTimeBar(GameManager.GetState().levelTimeRemaining, GameManager.GetState().GetLevelDuration());
            coinText.text = "x" + GameManager.GetState().coins;
            nextLevelPanel.gameObject.SetActive(GameManager.ins.levelState == LevelState.WAITINGFORNEXT);
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

    float currentlyDisplayedMaxHp;
    float currentlyDisplayedHp;
    void UpdateHealthBar(float currentHp, float maxHp)
    {
        // Calculate the current HP percentage
        float hpPercentage = currentHp / maxHp;
        healthBar.localScale = new Vector3(hpPercentage, 1, 1);
        float targetScaleX = hpPercentage;
        whiteHealthBar.DOKill(); // Kill existing tweens on whiteHealthBar to avoid conflicts
        whiteHealthBar.DOScaleX(targetScaleX, whiteHealthBarTweenDuration).SetEase(Ease.OutQuad); // Adjust this to change the animation curve

        currentlyDisplayedHp = currentHp;
        currentlyDisplayedMaxHp = maxHp;

    }

    IEnumerator LerpWhiteHealthBar(float targetScaleX)
    {
        while (!Mathf.Approximately(whiteHealthBar.localScale.x, targetScaleX))
        {
            float newXScale = Mathf.Lerp(whiteHealthBar.localScale.x, targetScaleX, whiteHealthBarTweenDuration * Time.deltaTime);
            whiteHealthBar.localScale = new Vector3(newXScale, 1, 1);
            yield return null;
        }
    }

    void UpdateLevelTimeBar(float currentTime, float maxTime)
    {
        float timePercentage = currentTime / maxTime;
        levelTimeBar.localScale = new Vector3(timePercentage, 1, 1);
    }
    
}
