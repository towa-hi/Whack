using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon", order = 1)]
public class WeaponData : ScriptableObject
{
    public int id;
    public string weaponName;
    public int damage;
    public Sprite graphic;
    public float cellFreezeDuration;
    public float actionDuration;
    public AudioClip weaponSound;
    public AudioClip weaponWhiffSound;
}
