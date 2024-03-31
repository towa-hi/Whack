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
    
    public Transform weaponOrigin;
    public Transform weaponTarget;
    public bool uninterruptable;
    float upDuration;
    public float upTime;
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
        entity.OnCellStateChange(newState);
        switch (state)
        {
            case CellState.DOWN:
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
        entity.SetEntity(entityData);
        upDuration = duration;
        ChangeState(CellState.RISING);
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
            float weaponStartAngle = 0;
            float weaponEndAngle = 90;
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
            DamageOutcome outcome = entity.ApplyDamage(weaponData);
            switch (outcome)
            {
                case DamageOutcome.DAMAGED:
                    weaponAudioSource.PlayOneShot(GameManager.GetState().weapon.weaponSound);
                    break;
                case DamageOutcome.KILLED:
                    weaponAudioSource.PlayOneShot(GameManager.GetState().weapon.weaponSound);
                    entity.OnDeath(false);
                    if (entity.data == null)
                    {
                        upTime = 0;
                    }
                    break;
                case DamageOutcome.BOUNCED:
                    weaponAudioSource.PlayOneShot(AudioManager.ins.bounceSound);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
}


public enum CellState
{
    DOWN,
    RISING,
    UP,
    FALLING
}