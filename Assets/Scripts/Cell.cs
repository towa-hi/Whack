using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int id;
    public CellState state;
    public GameObject platform;
    public Entity currentEntity;

    public GameObject entityPrefab;
    
    // Start is called before the first frame update
    void Awake()
    {
        ChangeState(CellState.DOWN);
    }

    public void RaisePlatformWithEntity(EntityData entityData)
    {
        GameObject newEntityPrefab = GameObject.Instantiate(entityPrefab);
        newEntityPrefab.GetComponent<Entity>().SetEntity(entityData);
    }

    void ChangeState(CellState newState)
    {
        state = newState;
        switch (state)
        {
            case CellState.DOWN:
                // stop platform
                break;
            case CellState.RISING:
                // raise platform
                break;
            case CellState.UP:
                // stop platform
                break;
            case CellState.FALLING:
                // drop platform
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void OnAnyCellHit(int cellId)
    {
        if (id == cellId)
        {
            Debug.Log(cellId + " received event");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}


public enum CellState
{
    DOWN,
    RISING,
    UP,
    FALLING
}