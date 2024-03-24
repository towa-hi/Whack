using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool isBoss;
    
    public int hp;

    public EntityData data;

    public void SetEntity(EntityData entityData)
    {
        data = entityData;
        isBoss = entityData.isBoss;
        hp = entityData.hp;
        
    }
}

public class EntityData : ScriptableObject
{
    public int id;
    public bool isBoss;
    public string name;
    public int hp;
    public Sprite graphic;
    
}
