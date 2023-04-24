using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Object references")]
    [SerializeField]
    private Maze maze;

    [SerializeField]
    private GameObject agentPrefab;

    [SerializeField]
    private Color[] agentsColors;

    [SerializeField]
    private GameObject pickupPrefab;

    [SerializeField]
    private PickupData[] pickupTypes;

    [SerializeField]
    private UnityEngine.UI.Text timerText;

    [SerializeField]
    private GameObject gameOverCanvas;

    [SerializeField]
    private UnityEngine.UI.Text gameOverText;

    [Header("Other settings")]
    [SerializeField]
    private float matchTimeSeconds = 300.0f;

    [SerializeField]
    private float initMovementSpeed = 5.0f;

    [SerializeField]
    private int initBombsCount = 1;

    [SerializeField]
    private int initBombsStrength = 1;

    [SerializeField]
    private float initBombsTimeToExplode = 3.0f;

    [SerializeField]
    private float pickupsProbability = 0.1f;

    public Maze Maze => maze;

    private Camera _mainCamera;
    public Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }
    }

    public List<Agent> ActiveAgents { get; } = new List<Agent>();

    public List<Bomb> ActiveBombs { get; } = new List<Bomb>();

    public List<Pickup> ActivePickups { get; } = new List<Pickup>();

    private List<Pickup> placedPickups = new List<Pickup>();
    private Coroutine timerCoroutine = null;
    private WaitForSeconds secondWaiter = new WaitForSeconds(1.0f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of game manager occured.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeObjects();
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void Update()
    {
        // Not much effective approach but should be sufficient in this case
        CheckForPickups();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void InitializeObjects()
    {
        gameOverCanvas.SetActive(false);

        maze.BuildMaze();
        maze.WallDestroyed += OnMazeWallDestroy;

        var brains = AgentBrainRegistry.GetAllBrains();

        if (maze.AgentsSpawnTilePositions.Count > brains.Length)
        {
            Debug.LogError("Number of agents must not exceed number of available brains. We do not want brainless agents.");
        }

        SpawnPickups();

        for (int i = 0; i < Mathf.Min(brains.Length, maze.AgentsSpawnTilePositions.Count); ++i)
        {
            SpawnAgent(agentPrefab, maze.AgentsSpawnTilePositions[i], brains[i], agentsColors[i % agentsColors.Length]);
        }
    }

    private Agent SpawnAgent(GameObject agentPrefab, Vector2Int tilePos, AgentBrain brain, Color color)
    {
        var agentGo = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
        agentGo.name = "Agent_" + ColorUtility.ToHtmlStringRGB(color);

        var agentComp = agentGo.GetComponentInChildren<Agent>();

        if (agentComp == null)
        {
            Debug.LogError("Invalid agent prefab: " + agentPrefab.name);
        }
        else
        {
            agentComp.Initialize(maze, initMovementSpeed, tilePos, brain, color,
                initBombsCount, initBombsStrength, initBombsTimeToExplode);

            ActiveAgents.Add(agentComp);
            agentComp.AgentDied += OnAgentDeath;
        }

        return agentComp;
    }

    private void SpawnPickups()
    {
        var mazeTiles = maze.GetMazeTiles();

        for (int i = 0; i < mazeTiles.Count; ++i)
        {
            for (int j = 0; j < mazeTiles[i].Count; ++j)
            {
                if (mazeTiles[i][j] == MazeTileType.DestructibleWall && Random.Range(0.0f, 1.0f) <= pickupsProbability)
                {
                    PlaceRandomPickup(new Vector2Int(j, i));
                }
            }
        }
    }

    private void OnMazeWallDestroy(Vector2Int location)
    {
        for (int i = placedPickups.Count - 1; i >= 0; --i)
        {
            if (placedPickups[i].TileLocation == location)
            {
                placedPickups[i].MakeVisible();
                ActivePickups.Add(placedPickups[i]);
                placedPickups.Remove(placedPickups[i]);

            }
        }
    }

    private void CheckForPickups()
    {
        for (int i = 0; i < ActivePickups.Count; ++i)
        {
            for (int j = 0; j < ActiveAgents.Count; ++j)
            {
                ActivePickups[i].CheckForPickUpByAgent(ActiveAgents[j]);
            }
        }
    }

    private void PlaceRandomPickup(Vector2Int location)
    {
        var pickupObj = Instantiate(pickupPrefab, Vector3.zero, Quaternion.identity);
        var pickupComp = pickupObj.GetComponent<Pickup>();

        pickupComp.Destroyed += OnPickupDestroyed;
        pickupComp.Place(maze, location, pickupTypes[Random.Range(0, pickupTypes.Length)]);

        placedPickups.Add(pickupComp);
    }

    private void OnPickupDestroyed(Pickup pickup)
    {
        ActivePickups.Remove(pickup);
    }

    private void OnAgentDeath(Agent agent)
    {
        ActiveAgents.Remove(agent);

        if (ActiveAgents.Count == 1)
        {
            StopCoroutine(timerCoroutine);
            GameOver(string.Format("Game over! {0} is a winner.", ActiveAgents[0].gameObject.name));
        }
    }

    private IEnumerator TimerRoutine()
    {
        float currTimeSec = matchTimeSeconds;

        do
        {
            var timespan = System.TimeSpan.FromSeconds(currTimeSec);
            timerText.text = string.Format("Timer: {0}m:{1}s", Mathf.Round(timespan.Minutes), Mathf.Round(timespan.Seconds));
            currTimeSec -= 1.0f;
            // This is very approximate solution but precision is not a big deal in this scenario
            yield return secondWaiter;
        }
        while (currTimeSec > 0.0f);

        timerText.text = "Time out!";
        GameOver("Draw! No winners!");
    }

    private void GameOver(string message)
    {
        if(gameOverCanvas.activeInHierarchy) { return; }

        Time.timeScale = 0.0001f;
        gameOverText.text = message;
        gameOverCanvas.SetActive(true);
    }
}
