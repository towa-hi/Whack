using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool isBoss;
    
    public int hp;

    public EntityData data;
    public SpriteRenderer spriteRenderer;
    
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
    }

    public void OnDeath()
    {
        if (data == null) return;
        // create corpse here
        // spawn reward
        GameManager.GetState().AddScore(data.scoreReward);
        GameManager.GetState().AddCoin(data.coinReward);
        if (data.reward != null && Random.value <= data.chanceToSpawnReward)
        {
            SetEntity(data.reward);
        }
        else
        {
            SetEntity(null);
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
