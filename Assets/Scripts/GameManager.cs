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
    
    public LevelState levelState;
    Menu currentMenu;
    [SerializeField] PlayerState state;
    public MainMenu mainMenu;
    public SettingsMenu settingsMenu;
    public GameUiMenu gameMenu;
    public DeathMenu deathMenu;
    public bool canAttack;
    
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
        SetCurrentMenu(mainMenu);
        entityDatas = new List<EntityData>(Resources.LoadAll<EntityData>("Data/Entities"));
        itemDatas = new List<ItemData>(Resources.LoadAll<ItemData>("Data/Items"));
        weaponDatas = new List<WeaponData>(Resources.LoadAll<WeaponData>("Data/Weapons"));
        // Example to show that objects are loaded
        Debug.Log($"Loaded {entityDatas.Count} entities, {itemDatas.Count} items, {weaponDatas.Count} weapons.");
        SetLevelState(LevelState.PREGAME);

    }

    
    
    void Update()
    {
        switch (levelState)
        {
            
            case LevelState.PREGAME:
                // dnt do anything just wait for StartGame()
                break;
            case LevelState.PLAYING:
                if (GetState().levelTimeRemaining > 0)
                {
                    GetState().levelTimeRemaining -= Time.deltaTime;
                    GetState().timeUntilNextActivation -= Time.deltaTime;
                    if (GetState().timeUntilNextActivation <= 0)
                    {
                        // get the number of cells that should be activated
                        int maxNumberOfCellsToActivate = Mathf.Max(1, GetState().GetLevelMaxSimultaneousCellActivations());
                        int numberOfCellsToActivate = Random.Range(1, maxNumberOfCellsToActivate + 1);
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
                        GetState().timeUntilNextActivation = GetState().GetLevelTimeBetweenCellActivation();
                    }
                }
                else
                {
                    SetLevelState(LevelState.WAITINGFORCLEANUP);
                }
                break;
            case LevelState.WAITINGFORCLEANUP:
                if (AllCellsDown())
                {
                    SetLevelState(LevelState.WAITINGFORNEXT);
                }
                break;
            case LevelState.WAITINGFORNEXT:
                break;
            case LevelState.LOST:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void SetLevelState(LevelState newState)
    {
        switch (newState)
        {
            case LevelState.PREGAME:
                canAttack = false;
                // new game initialization here
                state = new PlayerState(startingHp, weaponDatas[0]);
                // immediately go into playing state
                break;
            case LevelState.PLAYING:
                canAttack = true;
                // init all cells
                foreach (Cell cell in cells)
                {
                    cell.Init();
                }
                break;
            case LevelState.WAITINGFORCLEANUP:
                canAttack = true;
                foreach (Cell cell in cells)
                {
                    cell.KillEntityOnCellAndDropReward();
                }
                break;
            case LevelState.WAITINGFORNEXT:
                canAttack = false;
                break;
            case LevelState.LOST:
                canAttack = false;
                SetCurrentMenu(deathMenu);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        levelState = newState;
        
    }

    public void OnNextLevelButton()
    {
        GetState().GoLevel(GetState().level + 1);
        SetLevelState(LevelState.PLAYING);
    }
    
    bool AllCellsDown()
    {
        // Check if all cells are in the DOWN state
        return cells.All(cell => cell.state == CellState.DOWN);
    }
    
    public static PlayerState GetState()
    {
        return ins.state ?? null;
    }
    
    public void StartGame()
    {
        SetCurrentMenu(gameMenu);
        state = new PlayerState(startingHp, weaponDatas[0]);
        SetLevelState(LevelState.PLAYING);
        //GetState().SetIsPlaying(true);
    }
    
    List<EntityData> GetPossibleEntities()
    {
        return entityDatas.Where(entity => entity.minimumLevel <= GetState().level && entity.isNaturallySpawnable).ToList();
    }
    
    public void ApplyPlayerDamage(int damage)
    {
        if (levelState == LevelState.LOST)
        {
            return;
        }
        GetState().hp -= damage;
        CameraEffects.ins.ShakeCamera(0.3f, 0.1f);
        if (GetState().hp <= 0)
        {
            LoseGame();
        }
    }

    public void LoseGame()
    {
        Debug.Log("you just lost the game");
        SetLevelState(LevelState.LOST);
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
            EntityData entityData = entities[randomEntityIndex];
            if (entityData.isNumber && GetValidNumberForNumberFriend(randomIndex) == -1)
            {
                int newEntityIndex;
                do
                {
                    newEntityIndex = Random.Range(0, entities.Count);
                } while (newEntityIndex == randomEntityIndex);
                entityData = entities[newEntityIndex];
                Debug.Log("rerolled because numberfriend couldnt be spawned");
            }
            
            // activate cell
            selectedCell.ActivateCellWithEntity(entityData, GetState().GetLevelCellUpTime());
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
    

    public void OnPauseInput()
    {

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

    public int GetValidNumberForNumberFriend(int numberFriendCell)
    {
        
        HashSet<int> invalidCells = new HashSet<int>() {numberFriendCell};
        foreach (Cell cell in from cell in cells where cell.entity.number != 0 where cell.entity.data != null where cell.entity.data.isNumber select cell)
        {
            Debug.Log("invalid num added " + cell.entity.number);
            invalidCells.Add(cell.entity.number);
        }
        List<int> validCells = new List<int>();
        for (int i = 1; i <= 9; i++)
        {
            if (!invalidCells.Contains(i))
            {
                validCells.Add(i);
            }
        }

        if (validCells.Count == 0)
        {
            Debug.LogWarning("could not find a suitable number for number friend on pos " + numberFriendCell);
            return -1;
        }
        else
        {
            int index = Random.Range(0, validCells.Count);
            return validCells[index];
        }
    }

    public int NumberFriendPointingAtThisCell(int cellId)
    {
        foreach (Cell cell in cells)
        {
            if (cell.entity.data != null)
            {
                if (cell.entity.data.isNumber)
                {
                    if (cell.entity.number == cellId)
                    {
                        if (cell.entity.altState = true)
                        {
                            Debug.Log("NumberFriendPointingAtThisCell found friend on cell " + cell.id);
                            return cell.id;
                        }
                    }
                }
            }
        }

        return -1;
    }
}

public abstract class Menu : MonoBehaviour
{
    public abstract void Enter();
    public abstract void Exit();
}

[Serializable]
public class PlayerState
{
    //public bool isPlaying;
    //public bool isWaitingForNextLevel;
    
    public int level;
    public int hp;
    public int maxHp;
    public int coins;
    public int score;
    public WeaponData weapon;
    public float levelTimeRemaining;
    public float timeUntilNextActivation;
    public bool isPaused;
    
    public PlayerState(int startingHp, WeaponData startingWeapon)
    {
        //isPlaying = false;
        maxHp = startingHp;
        hp = startingHp;
        coins = 0;
        weapon = startingWeapon;
        GoLevel(1);
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

    public void GoLevel(int newLevel)
    {
        level = newLevel;
        levelTimeRemaining = GetLevelDuration();
        timeUntilNextActivation = GetLevelTimeBetweenCellActivation();
    }
}

public enum LevelState
{
    PREGAME,
    PLAYING,
    WAITINGFORCLEANUP,
    WAITINGFORNEXT,
    LOST
}