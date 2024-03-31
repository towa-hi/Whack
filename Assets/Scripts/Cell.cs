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
    
    //public GameObject currentEntity;
    public AudioSource weaponAudioSource;
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
    Coroutine resetPitch;
    
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
        entity.cellId = id;
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
                if (entity.data != null)
                {
                    if (entity.data.isNumber)
                    {
                        entity.SetAltState(true);
                    }
                }
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
        //corpse.originalSprite = entityData.graphic;
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
            
            // AUDIO STUFF
            if (resetPitch != null)
            {
                StopCoroutine(resetPitch);
            }
            // Adjust the pitch of the AudioSource to match the action duration
            float originalSoundDuration = weaponData.weaponWhiffSound.length; // Assuming this is the natural duration of the sound
            float desiredDuration = weaponData.actionDuration;
            weaponAudioSource.pitch = originalSoundDuration / desiredDuration;
            // Play the sound with the adjusted pitch
            weaponAudioSource.PlayOneShot(AudioManager.ins.whiffSound);
            // Reset pitch after the sound has played
            resetPitch = StartCoroutine(ResetPitchAfterDelay(weaponAudioSource, desiredDuration));
            
            // MOVEMENT STUFF
            // Start from the weaponStartAngle by directly setting the local rotation
            weaponSprite.transform.localRotation = Quaternion.Euler(0, 0, weaponStartAngle);
            weaponSprite.transform.position = weaponOrigin.position;
            // Tween to the weaponEndAngle around the Z-axis in local space
            weaponSprite.transform.DOLocalRotate(new Vector3(0, 0, weaponEndAngle), weaponData.actionDuration);
            // Move the weaponSprite to the target position 
            weaponSprite.transform.DOMove(weaponTarget.position, weaponData.actionDuration).OnComplete(() => OnWeaponAnimationComplete(weaponData));
        }
    }
    IEnumerator ResetPitchAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.pitch = 1.0f; // Reset pitch to normal
    }
    
    void OnWeaponAnimationComplete(WeaponData weaponData)
    {
        weaponSprite.GetComponent<SpriteRenderer>().enabled = false;
        uninterruptable = false;
        CameraEffects.ins.BounceCamera(0.1f, 1.5f);
        if (entity.data != null)
        {
            if (entity.isInvincible)
            {
                weaponAudioSource.PlayOneShot(AudioManager.ins.bounceSound);
                Debug.Log("bounce");
            }
            else
            {
                weaponAudioSource.PlayOneShot(GameManager.GetState().weapon.weaponSound);
                bool entityDied = entity.ApplyDamage(weaponData);
                if (entityDied)
                {
                    if (entity.data == null)
                    {
                        ChangeState(CellState.FALLING);
                    }
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