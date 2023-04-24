using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private GameObject explosionEffectPrefab;

    [SerializeField]
    private float effectDuration = 0.5f;

    public Vector2Int TileLocation { get; set; }

    public float TimeInitialToExplode { get; private set; }

    public float TimeLeftToExplode { get; private set; }

    public int Strength { get; private set; }

    public System.Action<Bomb> Destroyed;

    private Maze maze;

    private bool isExploded = false;

    private void Update()
    {
        if(maze != null && !isExploded)
        {
            TimeLeftToExplode -= Time.deltaTime;

            if(TimeLeftToExplode <= 0.0f)
            {
                Explode();
            }
        }
    }

    public void Place(Maze maze, Vector2Int location, int strength, float timeToExplode)
    {
        this.maze = maze;
        TileLocation = location;
        Strength = strength;

        TimeInitialToExplode = timeToExplode;
        TimeLeftToExplode = TimeInitialToExplode;

        transform.position = maze.GetWorldPositionForMazeTile(location.x, location.y);
        transform.localScale = maze.GetElementsScale();

        GameManager.Instance.ActiveBombs.Add(this);
    }

    public void Explode()
    {
        if(isExploded) { return; }

        isExploded = true;

        DestroyTile(TileLocation);
        bool canExpandLeft = true, canExpandRight = true, canExpandUp = true, canExpandDown = true;
        for(int i = 1; i <= Strength; ++i)
        {
            if (canExpandRight)
            {
                canExpandRight = DestroyTile(new Vector2Int(TileLocation.x + i, TileLocation.y));
            }

            if (canExpandLeft)
            {
                canExpandLeft = DestroyTile(new Vector2Int(TileLocation.x - i, TileLocation.y));
            }

            if (canExpandUp)
            {
                canExpandUp = DestroyTile(new Vector2Int(TileLocation.x, TileLocation.y - i));
            }

            if (canExpandDown)
            {
                canExpandDown = DestroyTile(new Vector2Int(TileLocation.x, TileLocation.y + i));
            }
        }

        GameManager.Instance.ActiveBombs.Remove(this);
        Destroyed?.Invoke(this);
    
        Destroy(gameObject);
    }

    // Returns true if the bomb can continue expanding after this tile
    private bool DestroyTile(Vector2Int tileLoc)
    {
        if(!maze.IsInBoundsTile(tileLoc)) { return false; }

        var tileType = maze.GetTileType(tileLoc);

        if(tileType == MazeTileType.Wall) { return false; }

        bool canBombContinueToExpand = tileType == MazeTileType.Free;

        // Destroy pickups
        for(int i = 0; i < GameManager.Instance.ActivePickups.Count; ++i)
        {
            if(GameManager.Instance.ActivePickups[i].TileLocation == tileLoc)
            {
                Destroy(GameManager.Instance.ActivePickups[i].gameObject);
                canBombContinueToExpand &= false;
            }
        }
        
        // Remove destructible walls
        if(tileType == MazeTileType.DestructibleWall)
        {
            maze.SetTileType(tileLoc, MazeTileType.Free);
        }

        // Spawn visual effect and schedule it for destruction
        var effect = Instantiate(explosionEffectPrefab, Vector3.zero, Quaternion.identity);
        effect.transform.position = maze.GetWorldPositionForMazeTile(tileLoc.x, tileLoc.y);
        effect.transform.localScale = maze.GetElementsScale();
        Destroy(effect, effectDuration);

        // Lit agents on fire (aka kill them)
        for (int i = 0; i < GameManager.Instance.ActiveAgents.Count; ++i)
        {
            if(GameManager.Instance.ActiveAgents[i].CurrentTile == tileLoc)
            {
                GameManager.Instance.ActiveAgents[i].IsDead = true;
                canBombContinueToExpand &= false;
            }
        }

        // Start a chain reaction
        for (int i = 0; i < GameManager.Instance.ActiveBombs.Count; ++i)
        {
            if (GameManager.Instance.ActiveBombs[i] != this && 
                GameManager.Instance.ActiveBombs[i].TileLocation == tileLoc)
            {
                GameManager.Instance.ActiveBombs[i].Explode();
                canBombContinueToExpand &= false;
            }
        }

        return canBombContinueToExpand;
    }
}
