using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item", order = 1)]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite graphic;
}
