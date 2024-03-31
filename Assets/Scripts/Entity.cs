using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Entity : MonoBehaviour
{
    public AudioSource audioSource;
    public bool isInvincible;
    public bool isEntityPointing;
    public int hp;
    public Shatterable corpse;
    public EntityData data;
    public SpriteRenderer spriteRenderer;
    public int number;
    public int cellId;
    public List<Sprite> numbers;
    public SpriteRenderer numberDisplay;
    
    public void SetEntity(EntityData entityData)
    {
        data = entityData;
        isEntityPointing = false;
        number = 0;
        SetInvincible(false);
        spriteRenderer.sprite = null;
        spriteRenderer.enabled = false;
        numberDisplay.enabled = false;
        if (entityData != null)
        {
            Debug.Log("spawning new entity " + entityData.entityName + " on cell " + cellId);
            hp = entityData.hp;
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = entityData.graphic;
            // SET UP NUMBERFRIEND STUFF
            if (entityData.isNumber)
            {
                number = GameManager.ins.GetValidNumberForNumberFriend(cellId);
                if (number <= 0) throw new Exception("something fucked up in the get valid number code");
                Sprite numberSprite = numbers[number - 1];
                numberDisplay.sprite = numberSprite;
                // set as invincible first
                SetInvincible(true);
            }
        }
    }

    public void OnCellStateChange(CellState state)
    {
        switch (state)
        {
            case CellState.DOWN:
                OnExpired();
                break;
            case CellState.RISING:
                break;
            case CellState.UP:
                OnCellUp();
                break;
            case CellState.FALLING:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    
    public void OnCellUp()
    {
        if (data != null && data.isNumber)
        {
            MakeEntityPoint();
        }
    }

    void MakeEntityPoint()
    {
        isEntityPointing = true;
        SetInvincible(false);
        spriteRenderer.sprite = data.altGraphic;
        numberDisplay.enabled = true;
    }
    
    void SetInvincible(bool newIsInvincible)
    {
        isInvincible = newIsInvincible;
        spriteRenderer.color = isInvincible ? Color.cyan : Color.white;
    }
    
    public bool OnDeath(bool alwaysDropGoodie)
    {
        if (data == null) return false;
        // create corpse here
        corpse.originalSprite = data.graphic;
        corpse.Shatter();
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(data.deathSound);
        // spawn reward
        GameManager.GetState().AddScore(data.scoreReward);
        GameManager.GetState().AddCoin(data.coinReward);
        if (alwaysDropGoodie)
        {
            // drop reward if goodie
            if (data.reward != null && data.reward.isGoodie)
            {
                SetEntity(data.reward);
                return true;
            }
            else
            {
                // reward not goodie so not dropped
                return false;
            }
        }
        else
        {
            // roll for chance to drop reward
            if (data.reward != null && Random.value <= data.chanceToSpawnReward)
            {
                SetEntity(data.reward);
                return true;
            }
            else
            {
                // failed reward roll so not dropped
                SetEntity(null);
                return false;
            }
        }
    }
    
    void OnExpired()
    {
        if (data == null) return;
        if (data.expiredDamage > 0)
        {
            GameManager.ins.ApplyPlayerDamage(data.expiredDamage);
        }
        SetEntity(null);
    }

    public DamageOutcome ApplyDamage(WeaponData weaponData)
    {
        if (isInvincible)
        {
            return DamageOutcome.BOUNCED;
        }
        hp -= weaponData.damage;
        if (hp <= 0)
        {
            return DamageOutcome.KILLED;
        }

        return DamageOutcome.DAMAGED;
    }
    
    public void ForceKillAndDropReward()
    {
        if (data != null && !data.isGoodie)
        {
            OnDeath(true);
        }
    }
    
}

public enum DamageOutcome
{
    DAMAGED,
    KILLED,
    BOUNCED,
}