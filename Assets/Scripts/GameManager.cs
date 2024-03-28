using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager ins;
    
    Menu currentMenu;
    [SerializeField] PlayerState state;
    public float timeUntilNextActivation;
    public MainMenu mainMenu;
    public SettingsMenu settingsMenu;
    public GameUiMenu gameMenu;
    public DeathMenu deathMenu;

    public bool overtime;
    
    public List<Cell> cells;
    public int startingHp;
    public AnimationCurve levelCellUpTimeCurve;
    public AnimationCurve levelTimeBetweenCellActivationCurve;
    public AnimationCurve levelMaxSimultaneousCellActivationsCurve;
    public AnimationCurve levelDurationCurve;
    
    public List<EntityData> entityDatas;
    public List<ItemData> itemDatas;
    public List<WeaponData> weaponDatas;
    
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
        SetCurrentMenu(mainMenu);
        entityDatas = new List<EntityData>(Resources.LoadAll<EntityData>("Data/Entities"));
        itemDatas = new List<ItemData>(Resources.LoadAll<ItemData>("Data/Items"));
        weaponDatas = new List<WeaponData>(Resources.LoadAll<WeaponData>("Data/Weapons"));

        // Example to show that objects are loaded
        Debug.Log($"Loaded {entityDatas.Count} entities, {itemDatas.Count} items, {weaponDatas.Count} weapons.");

    }

    void Start()
    {
        InputManager.ins.hitPerformed += HandleHitInput;
        InputManager.ins.pauseInputPerformed += HandlePauseInput;
    }
    
    void Update()
    {
        if (GetState() == null || !GetState().isPlaying)
        {
            return;
        }
        if (GetState().levelTimeRemaining >= 0)
        {
            GetState().levelTimeRemaining -= Time.deltaTime;
        }
        else
        {
            overtime = true;
        }
        if (overtime)
        {
            bool allCellsDown = true;
            foreach (Cell cell in cells.Where(cell => cell.state != CellState.DOWN))
            {
                allCellsDown = false;
            }
            if (allCellsDown)
            {
                OnLevelEnd();
                overtime = false;
            }
        }
        else
        {
            // loop for cell activations
            timeUntilNextActivation -= Time.deltaTime;
            if (timeUntilNextActivation <= 0)
            {
                // get the number of cells that should be activated
                int maxNumberOfCellsToActivate = Mathf.Max(1, GetState().GetLevelMaxSimultaneousCellActivations());
                int numberOfCellsToActivate = UnityEngine.Random.Range(1, maxNumberOfCellsToActivate + 1);
                //List<int> activatedCellIds = new List<int>();
                for (int i = 0; i < numberOfCellsToActivate; i++)
                {
                    Cell activatedCell = RandomlyActivateUnusedCell();
                    if (activatedCell != null)
                    {
                        //activatedCellIds.Add(activatedCell.id);
                    }
                }
                //Debug.Log(activatedCellIds.Count > 0 ? $"Activated these cells: {string.Join(", ", activatedCellIds)}" : "No cells activated this cycle.");
                timeUntilNextActivation = GetState().GetLevelTimeBetweenCellActivation();
            }
        }

    }

    public void OnLevelEnd()
    {
        GetState().GoNextLevel();
    }
    
    public static PlayerState GetState()
    {
        return ins.state ?? null;
    }
    
    public void StartGame()
    {
        SetCurrentMenu(gameMenu);
        state = new PlayerState(startingHp, weaponDatas[0]);
        // init all cells
        foreach (Cell cell in cells)
        {
            cell.Init();
        }
        GetState().SetIsPlaying(true);
    }
    
    List<EntityData> GetPossibleEntities()
    {
        return entityDatas.Where(entity => entity.minimumLevel <= GetState().level && entity.isNaturallySpawnable).ToList();
    }
    
    public void ApplyPlayerDamage(int damage)
    {
        GetState().hp -= damage;
        if (GetState().hp <= 0)
        {
            LoseGame();
        }
    }

    public void LoseGame()
    {
        Debug.Log("you just lost the game");
        SetCurrentMenu(deathMenu);
        GetState().SetIsPlaying(false);
        foreach (var cell in cells)
        {
            cell.ResetCell();
        }
    }
    
    public void OnInput(QueuedInput input)
    {
        // tell all cells an input happened
        foreach (Cell currentCell in GameManager.ins.cells)
        {
            currentCell.OnAnyCellHit(input.cellId, input.weaponData);
        }
    }
    
    public Cell RandomlyActivateUnusedCell()
    {
        List<Cell> unusedCells = GetUnusedCells();
        if (unusedCells.Count > 0)
        {
            // get a random cell
            int randomIndex = Random.Range(0, unusedCells.Count);
            Cell selectedCell = unusedCells[randomIndex];
            // get a random entity
            List<EntityData> entities = GetPossibleEntities();
            if (entities.Count == 0)
            {
                throw new Exception("no valid entities to feed into RaisePlatformWithEntity");
            }
            int randomEntityIndex = Random.Range(0, entities.Count);
            // activate cell
            selectedCell.ActivateCellWithEntity(entities[randomEntityIndex], GetState().GetLevelCellUpTime());
            return selectedCell;
        }
        else
        {
            Debug.Log("nothing to randomly activate");
            return null;
        }
    }

    List<Cell> GetUnusedCells()
    {
        return cells.Where(cell => cell.state == CellState.DOWN).ToList();
    }
    
    public Cell GetCell(int id)
    {
        foreach (Cell cell in cells)
        {
            if (cell.id == id)
            {
                return cell;
            }
        }
        throw new Exception("invalid id");
    }
    
    void HandleHitInput(object sender, int cellId)
    {
        Debug.Log(cellId + " pressed");
        if (GetCell(cellId).uninterruptable)
        {
            // send input into queue
        }
        foreach (Cell cell in cells)
        {
            cell.OnAnyCellHit(cellId, GetState().weapon);
        }
    }

    void HandlePauseInput(object sender, EventArgs e)
    {
        Debug.Log("pause or unpause game");
    }
    
    void SetCurrentMenu(Menu newMenu)
    {
        if (currentMenu != null)
        {
            currentMenu.Exit();
        }
        currentMenu = newMenu;
        if (currentMenu != null)
        {
            currentMenu.Enter();
        }
        //background.SetActive(currentMenu == null);
    }

    public void OpenMainMenu()
    {
        SetCurrentMenu(mainMenu);
    }

    public void OpenSettingsMenu()
    {
        SetCurrentMenu(settingsMenu);
    }

    public void OnPauseButton()
    {
        Debug.Log("OnPauseButton()");
    }
}

public abstract class Menu : MonoBehaviour
{
    public abstract void Enter();
    public abstract void Exit();

    void Awake()
    {
    }
}

[Serializable]
public class PlayerState
{
    public bool isPlaying;
    public int level;
    public int hp;
    public int maxHp;
    public int coins;
    public int score;
    public WeaponData weapon;
    public float levelTimeRemaining;
    
    public PlayerState(int startingHp, WeaponData startingWeapon)
    {
        isPlaying = false;
        level = 1;
        maxHp = startingHp;
        hp = startingHp;
        coins = 0;
        weapon = startingWeapon;
        levelTimeRemaining = GetLevelDuration();
    }

    public void SetIsPlaying(bool newIsPlaying)
    {
        isPlaying = newIsPlaying;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        if (coins < 0)
        {
            coins = 0;
        }
    }

    public void SetWeapon(WeaponData newWeapon)
    {
        weapon = newWeapon;
    }

    public float GetLevelCellUpTime()
    {
        return GameManager.ins.levelCellUpTimeCurve.Evaluate(level);
    }

    public float GetLevelTimeBetweenCellActivation()
    {
        return GameManager.ins.levelTimeBetweenCellActivationCurve.Evaluate(level);
    }

    public int GetLevelMaxSimultaneousCellActivations()
    {
        return (int)GameManager.ins.levelMaxSimultaneousCellActivationsCurve.Evaluate(level);
    }

    public float GetLevelDuration()
    {
        return GameManager.ins.levelDurationCurve.Evaluate(level);
    }

    public void GoNextLevel()
    {
        level += 1;
        levelTimeRemaining = GetLevelDuration();
        isPlaying = false;
    }
}