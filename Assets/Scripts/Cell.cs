using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cell : MonoBehaviour
{
    public int id;
    public CellState state;
    public GameObject platform;
    public Shatterable corpse;
    
    //public GameObject currentEntity;
    public Entity entity;
    public GameObject weaponSprite;
    public Vector3 downPlatformPos;
    public Vector3 upPlatformPos;
    
    float weaponStartAngle = 0;
    float weaponEndAngle = 90;
    public Transform weaponOrigin;
    public Transform weaponTarget;
    public bool uninterruptable;
    float upDuration;
    float upTime;
    float upSpeed = 2f;
    float downSpeed = 1f;
    //Coroutine platformTimer;
    
    void Awake()
    {
        Init();
    }

    public void Init()
    {
        StopAllCoroutines();
        //platformTimer = null;
        upTime = 0;
        uninterruptable = false;
        entity.SetEntity(null);
        platform.transform.localPosition = downPlatformPos;
        ChangeState(CellState.DOWN);
        isResetting = false;
    }
    
    
    void Update()
    {
        switch (state)
        {
            case CellState.RISING:
                MoveUp();
                break;
            case CellState.FALLING:
                MoveDown();
                break;
            case CellState.DOWN:
                break;
            case CellState.UP:
                upTime -= Time.deltaTime;
                if (upTime <= 0)
                {
                    ChangeState(CellState.FALLING);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void ChangeState(CellState newState)
    {
        state = newState;
        switch (state)
        {
            case CellState.DOWN:
                entity.OnExpired();
                // stop platform
                break;
            case CellState.RISING:
                // raise platform
                break;
            case CellState.UP:
                upTime = upDuration;
                break;
            case CellState.FALLING:
                // drop platform
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ActivateCellWithEntity(EntityData entityData, float duration)
    {
        AddEntity(entityData);
        upDuration = duration;
        ChangeState(CellState.RISING);
    }

    public void AddEntity(EntityData entityData)
    {
        //GameObject newEntityPrefab = Instantiate(entityPrefab, spawnPoint.transform);
        //newEntityPrefab.GetComponent<Entity>().SetEntity(entityData);
        entity.SetEntity(entityData);
        corpse.originalSprite = entityData.graphic;
        // TODO: dynamically generate a new corpse for each entity 
        //return newEntityPrefab;
    }

    public void MoveUp()
    {
        float step = upSpeed * Time.deltaTime;
        platform.transform.localPosition = Vector3.MoveTowards(platform.transform.localPosition, upPlatformPos, step);
        if (Vector3.Distance(platform.transform.localPosition, upPlatformPos) < 0.01f)
        {
            platform.transform.localPosition = upPlatformPos; // Snap to the exact target position to avoid overshooting.
            ChangeState(CellState.UP); // Update the state as the movement is complete.
        }
    }

    public void MoveDown()
    {
        float step = downSpeed * Time.deltaTime;
        platform.transform.localPosition = Vector3.MoveTowards(platform.transform.localPosition, downPlatformPos, step);
        if (Vector3.Distance(platform.transform.localPosition, downPlatformPos) < 0.01f)
        {
            platform.transform.localPosition = downPlatformPos; // Snap to the exact target position.
            ChangeState(CellState.DOWN); // Update the state as the movement is complete.
        }
    }

    
    public void OnAnyCellHit(int cellId, WeaponData weaponData)
    {
       
        if (id == cellId)
        {
            if (uninterruptable)
            {
                Debug.Log("GameManager needs to check if this cell is uninterruptable first");
                return;
            }
            uninterruptable = true;
            weaponSprite.GetComponent<SpriteRenderer>().enabled = true;
            
            // Start from the weaponStartAngle by directly setting the local rotation
            weaponSprite.transform.localRotation = Quaternion.Euler(0, 0, weaponStartAngle);
            weaponSprite.transform.position = weaponOrigin.position;
            // Tween to the weaponEndAngle around the Z-axis in local space
            weaponSprite.transform.DOLocalRotate(new Vector3(0, 0, weaponEndAngle), weaponData.actionDuration);
            // Move the weaponSprite to the target position 
            weaponSprite.transform.DOMove(weaponTarget.position, weaponData.actionDuration).OnComplete(() => OnWeaponAnimationComplete(weaponData));
        }
    }
    
    public void OnWeaponAnimationComplete(WeaponData weaponData)
    {
        weaponSprite.GetComponent<SpriteRenderer>().enabled = false;
        uninterruptable = false;
        if (entity.data != null)
        {
            bool entityDied = entity.ApplyDamage(weaponData);
            if (entityDied)
            {
                corpse.Shatter();
                if (entity.data == null)
                {
                    ChangeState(CellState.FALLING);
                }
            }
        }
        else
        {
            OnWhiff();
        }
    }

    void OnWhiff()
    {
        GameManager.ins.ApplyPlayerDamage(1);
    }
    
    bool isResetting;
    public void ResetCell()
    {
        Init();
    }

    public void KillEntityOnCellAndDropReward()
    {
        if (entity.data != null)
        {
            EntityData reward = entity.data.reward;
            bool droppedReward = entity.OnDeath();
            corpse.Shatter();
            if (!droppedReward)
            {
                entity.SetEntity(reward);
            }
        }
    }
}


public enum CellState
{
    DOWN,
    RISING,
    UP,
    FALLING
}