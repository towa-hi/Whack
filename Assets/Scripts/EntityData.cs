using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EntityData", menuName = "Game/Entity Data", order = 1)]
public class EntityData : ScriptableObject
{
    public int id;
    public bool isNaturallySpawnable;
    public bool isBoss;
    public string entityName;
    public float chanceToSpawnReward;
    
    public int minimumLevel;
    public int hp;
    public int expiredDamage;
    public int scoreReward;
    public int coinReward;
    public Sprite graphic;
    public EntityData reward;
}
