using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager ins;
    Controls input;
    public EventHandler<int> hitPerformed;
    public EventHandler pauseInputPerformed;

    Queue<QueuedInput> inputQueue;
    public int queueLimit;
    
    public float timeUntilNextInputProcessing;
    void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != ins)
        {
            Destroy(gameObject);
        }
        inputQueue = new Queue<QueuedInput>();
        input = new Controls();
        input.Enable();
        input.Game.hit1.performed += ctx => QueueAction(1);
        input.Game.hit2.performed += ctx => QueueAction(2);
        input.Game.hit3.performed += ctx => QueueAction(3);
        input.Game.hit4.performed += ctx => QueueAction(4);
        input.Game.hit5.performed += ctx => QueueAction(5);
        input.Game.hit6.performed += ctx => QueueAction(6);
        input.Game.hit7.performed += ctx => QueueAction(7);
        input.Game.hit8.performed += ctx => QueueAction(8);
        input.Game.hit9.performed += ctx => QueueAction(9);
        // Handle pause immediately due to its nature
        input.Game.pause.performed += ctx => pauseInputPerformed?.Invoke(this, EventArgs.Empty);
    }

    void QueueAction(int cellId)
    {
        if (!GameManager.GetState().isPlaying)
        {
            return;
        }
        if (inputQueue.Count < queueLimit)
        {
            inputQueue.Enqueue(new QueuedInput(cellId, GameManager.GetState().weapon));
        }
    }


    void Update()
    {
        // if has inputs to do
        if (inputQueue.Count > 0 && timeUntilNextInputProcessing <= 0)
        {
            // get next action
            QueuedInput nextInput = inputQueue.Dequeue();
            // check if action is doable
            Cell cell = GameManager.ins.GetCell(nextInput.cellId);
            if (cell.uninterruptable)
            {
                // put it back into queue
                if (inputQueue.Count < queueLimit)
                {
                    inputQueue.Enqueue(nextInput);
                }
            }
            else
            {
                GameManager.ins.OnInput(nextInput);
                timeUntilNextInputProcessing = nextInput.weaponData.actionDuration;
            }
        }
        if (timeUntilNextInputProcessing > 0)
        {
            timeUntilNextInputProcessing -= Time.deltaTime;
        }
        
    }
}

public struct QueuedInput
{
    public readonly int cellId;
    public readonly WeaponData weaponData;

    public QueuedInput(int cellId, WeaponData weaponData)
    {
        this.cellId = cellId;
        this.weaponData = weaponData;
    }
}