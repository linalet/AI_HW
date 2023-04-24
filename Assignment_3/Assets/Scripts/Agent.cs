using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentAction
{
    Stay,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    PlaceBomb
}

public class Agent : MonoBehaviour
{
    private const float destinationTileDistanceTolerance = 0.01f * 0.01f;

    [SerializeField]
    private GameObject bombPrefab;

    public Vector2Int CurrentTile { get; private set; }

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            return _spriteRenderer;
        }
    }

    protected Sprite _sprite;
    public Sprite Sprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = SpriteRenderer.sprite;
            }

            return _sprite;
        }
    }

    protected bool _isDead;
    public bool IsDead
    {
        get => _isDead;
        set
        {
            var prevValue = _isDead;
            _isDead = value;

            if(value && !prevValue)
            {
                AgentDied?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }

    protected float _movementSpeed;
    public float MovementSpeed
    {
        get => _movementSpeed;
        set => _movementSpeed = value;
    }

    protected int _bombsStrength;
    public int BombsStrength
    {
        get => _bombsStrength;
        set => _bombsStrength = value;
    }

    protected float _bombTimeToExplode;
    public float BombTimeToExplode
    {
        get => _bombTimeToExplode;
        set => _bombTimeToExplode = value;
    }

    protected int _bombsMaxCount;
    public int BombsMaxCount
    {
        get => _bombsMaxCount;
        set => _bombsMaxCount = value;
    }

    public int BombsPlacedCount
    {
        get => placedBombs.Count;
    }

    public System.Action<Agent> AgentDied;

    protected AgentBrain brain;
    protected Maze parentMaze;
    protected bool isInitialized = false;
    protected HashSet<Bomb> placedBombs = new HashSet<Bomb>();

    protected bool isInMovementTransition = false;

    protected Vector3 transitionTarget;

    protected bool CanQueryNextAction => !isInMovementTransition;

    protected virtual void Awake() { }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        brain.Update();

        if (CanQueryNextAction)
        {
            HandleAgentAction(brain.GetNextAction());
        }

        if (isInMovementTransition)
        {
            transform.position = Vector3.MoveTowards(transform.position, transitionTarget, MovementSpeed * Time.deltaTime);

            if((transform.position - transitionTarget).sqrMagnitude <= destinationTileDistanceTolerance)
            {
                transform.position = transitionTarget;
                isInMovementTransition = false;
                CurrentTile = parentMaze.GetMazeTileForWorldPosition(transitionTarget);
            }
        }
    }

    public virtual void Initialize(Maze parentMaze, float movementSpeed, Vector2Int spawnTilePos, AgentBrain brain, Color color,
        int initBombsCount, int initBombsStrength, float initBombsTimeToExplode)
    {
        if (brain == null)
        {
            Debug.Log("No brain provided!");
        }

        this.parentMaze = parentMaze;

        // The multiplication below ensures that movement speed is considered in tile-units so it stays
        // consistent across different scales of the maze
        MovementSpeed = movementSpeed * parentMaze.GetElementsScale().x;

        transform.position = parentMaze.GetWorldPositionForMazeTile(spawnTilePos.x, spawnTilePos.y);
        transform.localScale = parentMaze.GetElementsScale();

        CurrentTile = spawnTilePos;
        BombsMaxCount = initBombsCount;
        BombsStrength = initBombsStrength;
        BombTimeToExplode = initBombsTimeToExplode;

        this.brain = brain;
        SpriteRenderer.color = color;

        this.brain?.Initialize(this, parentMaze);
        isInitialized = true;
    }

    protected virtual void OnBombExploded(Bomb bomb)
    {
        placedBombs.Remove(bomb);
    }

    protected virtual void PlaceBomb()
    {
        var bomb = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity);
        var bombComp = bomb.GetComponent<Bomb>();

        bombComp.Place(parentMaze, CurrentTile, BombsStrength, BombTimeToExplode);
        bombComp.Destroyed += OnBombExploded;
        placedBombs.Add(bombComp);
    }

    protected virtual void HandleAgentAction(AgentAction action)
    {
        switch (action)
        {
            case AgentAction.PlaceBomb:
                if (BombsPlacedCount < BombsMaxCount)
                {
                    PlaceBomb();
                }
                break;
            case AgentAction.MoveDown:
                StartTransitionToNeighbouringTile(new Vector2Int(CurrentTile.x, CurrentTile.y + 1));
                break;
            case AgentAction.MoveUp:
                StartTransitionToNeighbouringTile(new Vector2Int(CurrentTile.x, CurrentTile.y - 1));
                break;
            case AgentAction.MoveLeft:
                StartTransitionToNeighbouringTile(new Vector2Int(CurrentTile.x - 1, CurrentTile.y));
                break;
            case AgentAction.MoveRight:
                StartTransitionToNeighbouringTile(new Vector2Int(CurrentTile.x + 1, CurrentTile.y));
                break;
            case AgentAction.Stay:
                // Do nothing
                break;
            default:
                break;

        }
    }

    protected virtual void StartTransitionToNeighbouringTile(Vector2Int tile)
    {
        if(isInMovementTransition) { return; }

        // Agent is not allowed to go to walls ...
        if(!parentMaze.IsValidTileOfType(tile, MazeTileType.Free))
        {
            return;
        }

        // ... and placed bombs are also not passable
        for(int i = 0; i < GameManager.Instance.ActiveBombs.Count; ++i)
        {
            if(GameManager.Instance.ActiveBombs[i].TileLocation == tile)
            {
                return;
            }
        }
        
        transitionTarget = parentMaze.GetWorldPositionForMazeTile(tile.x, tile.y);
        isInMovementTransition = true;
    }
}
