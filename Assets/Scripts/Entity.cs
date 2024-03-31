using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public AudioSource audioSource;
    public bool isBoss;
    public int hp;
    public Shatterable corpse;
    public EntityData data;
    public SpriteRenderer spriteRenderer;
    public int number;
    public int cellId;
    public List<Sprite> numbers;
    public SpriteRenderer numberDisplay;
    public bool altState;
    public bool isInvincible;
    
    public void SetEntity(EntityData entityData)
    {
        data = entityData;
        if (entityData != null)
        {
            isBoss = entityData.isBoss;
            hp = entityData.hp;
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = entityData.graphic;
        }
        else
        {
            spriteRenderer.sprite = null;
            spriteRenderer.enabled = false;
        }
        altState = false;
        if (entityData != null)
        {
            if (entityData.entityName == "Number Friend")
            {
                Debug.Log("number friend spawned");
            }
            number = entityData.isNumber ? GameManager.ins.GetValidNumberForNumberFriend(this.cellId) : 0;
            if (number != 0)
            {
                Debug.Log("Setting numbers display");
                Sprite numberSprite = numbers[number - 1];
                numberDisplay.sprite = numberSprite;
            }
        }
        else
        {
            number = 0;
        }
        SetAltState(false);
    }

    public void SetAltState(bool newAltState)
    {
        numberDisplay.enabled = false;
        if (data == null)
        {
            altState = false;
            return;
        }

        if (newAltState)
        {
            altState = true;
            // change sprite to alt graphic if exists
            spriteRenderer.sprite = data.altGraphic != null ? data.altGraphic : data.graphic;
            // if isNumberFriend
            if (data.isNumber)
            {
                numberDisplay.enabled = true;
            }
            else
            {
                
            }
        }
        else
        {
            spriteRenderer.sprite = data.graphic;
        }

        if (data.isNumber)
        {
            SetInvincible(!newAltState);
        }
    }

    public void SetInvincible(bool newIsInvincible)
    {
        Debug.Log("invincible set to " + newIsInvincible);
        isInvincible = newIsInvincible;
        if (isInvincible)
        {
            spriteRenderer.color = Color.cyan;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
    public bool OnDeath()
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
        if (data.reward != null && Random.value <= data.chanceToSpawnReward)
        {
            SetEntity(data.reward);
            return true;
        }
        else
        {
            SetEntity(null);
            return false;
        }
    }

    public void OnExpired()
    {
        if (data == null) return;
        if (data.expiredDamage > 0)
        {
            GameManager.ins.ApplyPlayerDamage(data.expiredDamage);
        }
        SetEntity(null);
    }

    public bool ApplyDamage(WeaponData weaponData)
    {
        hp -= weaponData.damage;
        if (hp <= 0)
        {
            OnDeath();
            return true;
        }

        return false;
    }
}

