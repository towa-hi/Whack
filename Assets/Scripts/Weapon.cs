using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    int damage;
    public WeaponData data;

    public void SetWeapon(WeaponData weaponData)
    {
        data = weaponData;
        damage = weaponData.damage;
    }
}

