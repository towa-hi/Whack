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
    public Queue<int> hitQueue;
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

        input = new Controls();
        input.Enable();
        hitQueue = new Queue<int>();
        input.Game.hit1.performed += OnInput;
        input.Game.hit2.performed += OnInput;
        input.Game.hit3.performed += OnInput;
        input.Game.hit4.performed += OnInput;
        input.Game.hit5.performed += OnInput;
        input.Game.hit6.performed += OnInput;
        input.Game.hit7.performed += OnInput;
        input.Game.hit8.performed += OnInput;
        input.Game.hit9.performed += OnInput;
        input.Game.pause.performed += OnInput;
    }

    void OnInput(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "hit1":
                hitQueue.Enqueue(1);
                break;
            case "hit2":
                hitQueue.Enqueue(2);
                break;
            case "hit3":
                hitQueue.Enqueue(3);
                break;
            case "hit4":
                hitQueue.Enqueue(4);
                break;
            case "hit5":
                hitQueue.Enqueue(5);
                break;
            case "hit6":
                hitQueue.Enqueue(6);
                break;
            case "hit7":
                hitQueue.Enqueue(7);
                break;
            case "hit8":
                hitQueue.Enqueue(8);
                break;
            case "hit9":
                hitQueue.Enqueue(9);
                break;
            case "pause":
                pauseInputPerformed?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    int queueLimit = 3;
    
    void Update()
    {
        if (hitQueue.Count > 0)
        {
            int numpadKey = hitQueue.Dequeue();
            hitPerformed?.Invoke(this, numpadKey);

            if (hitQueue.Count > queueLimit)
            {
                Debug.LogWarning("Input queue limit exceeded. Clearing the queue.");
                hitQueue.Clear();
            }
        }
    }
}
