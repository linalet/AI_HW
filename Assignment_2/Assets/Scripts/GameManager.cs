using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class 
GameManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the instance of GameManager class.
    /// </summary>
    public static GameManager Instance { get; private set; }

    [Header("Object references")]
    [SerializeField]
    private Maze maze;

    [SerializeField]
    private GameObject agentPrefab;

    [SerializeField]
    private GameObject flagPrefab;
    
    [SerializeField]
    public bool euclidH;

    [Header("Other settings")]
    [SerializeField]
    private float movementSpeed = 5.0f;

    public System.Action<Vector2Int> DestinationChanged;

    /// <summary>
    /// Reference to the current Maze instance.
    /// </summary>
    public Maze Maze => maze;

    /// <summary>
    /// Reference to the game object representing the destination flag.
    /// </summary>
    public GameObject DestinationFlag { get; private set; }

    private Vector2Int _destinationTile;
    /// <summary>
    /// Corresponds to the tile index of the destination flag.
    /// </summary>
    public Vector2Int DestinationTile
    {
        get => _destinationTile;
        set
        {
            _destinationTile = value;
            DestinationChanged?.Invoke(_destinationTile);
        }
    }

    private Camera _mainCamera;
    public Camera MainCamera
    {
        get
        {
            if(_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }
    }

    private void Awake()
    {
        if(Instance == null)
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
    }

    private void Update()
    {
        CheckForNewDestination();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void InitializeObjects()
    {
        maze.BuildMaze();

        SpawnAgent(agentPrefab, maze.AgentSpawnTilePos);
        _destinationTile = maze.AgentSpawnTilePos;
        DestinationFlag = SpawnFlag(flagPrefab);
    }

    private Agent SpawnAgent(GameObject agentPrefab, Vector2Int tilePos)
    {
        var agentGo = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);

        var agentComp = agentGo.GetComponentInChildren<Agent>();

        if (agentComp == null)
        {
            Debug.LogError("Invalid agent prefab: " + agentPrefab.name);
        }
        else
        {
            agentComp.InitializeData(maze, movementSpeed, tilePos);
        }

        return agentComp;
    }

    private GameObject SpawnFlag(GameObject flagPrefab)
    {
        var flagGo = Instantiate(flagPrefab, Vector3.zero, Quaternion.identity);

        flagGo.transform.localScale = maze.GetElementsScale();
        flagGo.SetActive(false);

        return flagGo;
    }

    private void CheckForNewDestination()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = MainCamera.nearClipPlane;
            var mouseWorldPos = MainCamera.ScreenToWorldPoint(mousePos);

            var destTile = maze.GetMazeTileForWorldPosition(mouseWorldPos);

            if(maze.IsValidTileOfType(destTile, MazeTileType.Free))
            {
                DestinationFlag.transform.position = maze.GetWorldPositionForMazeTile(destTile);
                DestinationFlag.SetActive(true);
                DestinationTile = destTile;
            }
        }
    }
}
