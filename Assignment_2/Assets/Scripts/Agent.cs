using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using UnityEngine.UIElements;

public class Agent : MonoBehaviour
{
    public Vector2Int CurrentTile { get; private set; }

    private Sprite _sprite;
    public Sprite Sprite
    {
        get
        {
            if(_sprite == null)
            {
                _sprite = GetComponentInChildren<SpriteRenderer>()?.sprite;
            }

            return _sprite;
        }
    }

    protected float movementSpeed; // in "tile" units
    protected Maze parentMaze;
    protected bool isInitialized = false;

    private List<Vector3> shortestPath;
    private int tileIndex = 0;
    private Coroutine astar = null;

    protected virtual void Start()
    {
        GameManager.Instance.DestinationChanged += OnDestinationChanged;
        shortestPath = new List<Vector3>();
    }

    protected virtual void Update()
    {
        // TODO Assignment 2 ... this function might be of your interest. :-)
        // You are free to add new functions, create new classes, etc.
        // ---
        // The CurrentTile property should held the current location (tile-based) of an agent
        //
        // Have a look at Maze class, it contains several useful properties and functions.
        // For example, Maze.MazeTiles stores the information about the tiles of the maze.
        // Then, there are several functions for conversion/retrieval of tile positions, as well as for changing tile colors.
        // 
        // Finally, you can also have a look at GameManager to see what it provides.

        // NOTE
        // The code below is just a simple demonstration of some of the functionality / functions
        // You will need to replace it / change it
        if (tileIndex < shortestPath.Count && shortestPath.Count!=0)
        {
            var nextTile = shortestPath[tileIndex];
            var cur = parentMaze.GetWorldPositionForMazeTile(CurrentTile);
            Vector3 direction = new Vector3(nextTile.x - cur.x, nextTile.y - cur.y, 0);
            direction.Normalize();
            transform.Translate(direction* movementSpeed * Time.deltaTime);
        
            var oldTile = CurrentTile;
            var afterTranslTile = parentMaze.GetMazeTileForWorldPosition(transform.position);

            if(oldTile != afterTranslTile)
            {
                CurrentTile = afterTranslTile;
            }

            if(CurrentTile == parentMaze.GetMazeTileForWorldPosition(nextTile))
            {
                tileIndex++;
            }
        }
        

    }

    // This function is called every time the user sets a new destination using a left mouse button
    protected virtual void OnDestinationChanged(Vector2Int newDestinationTile)
    {
        // TODO Assignment 2 ... this function might be of your interest. :-)
        // The destination tile index is also accessible via GameManager.Instance.DestinationTile
        if (astar != null)
            StopCoroutine(astar);
        tileIndex = 0;
        parentMaze.ResetTileColors();
        shortestPath = new List<Vector3>();
        astar = StartCoroutine(AStar(parentMaze.GetWorldPositionForMazeTile(CurrentTile), 
                                   parentMaze.GetWorldPositionForMazeTile(GameManager.Instance.DestinationTile)));

    }

    public virtual void InitializeData(Maze parentMaze, float movementSpeed, Vector2Int spawnTilePos)
    {
        this.parentMaze = parentMaze;

        // The multiplication below ensures that movement speed is considered in tile-units so it stays
        // consistent across different scales of the maze
        this.movementSpeed = movementSpeed * parentMaze.GetElementsScale().x; 

        transform.position = parentMaze.GetWorldPositionForMazeTile(spawnTilePos.x, spawnTilePos.y);
        transform.localScale = parentMaze.GetElementsScale();

        CurrentTile = spawnTilePos;

        isInitialized = true;
    }

    private IEnumerator AStar(Vector3 start, Vector3 goal)
    {
        List<Vector3> closedSet = new List<Vector3>();
        SimplePriorityQueue<Vector3> openSet = new SimplePriorityQueue<Vector3>();
        openSet.Enqueue(start, HeuristicF(start, goal));
        parentMaze.SetFreeTileColor(parentMaze.GetMazeTileForWorldPosition(start), Color.green);
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>();

        for (int i = 0; i < parentMaze.MazeTiles.Count; i++)
        {
            for (int j = 0; j < parentMaze.MazeTiles[0].Count; j++)
            {
                gScore.Add(parentMaze.GetWorldPositionForMazeTile(i,j), Mathf.Infinity);
            }
        }
        gScore[start] = 0;
        
        while (openSet.Count > 0)
        {
            Vector3 current = openSet.Dequeue();
            if (current == goal)
            {
                ReconstructPath(cameFrom, current, start);
                yield break;
            }
            closedSet.Add(current);
            parentMaze.SetFreeTileColor(parentMaze.GetMazeTileForWorldPosition(current), Color.red);
            foreach (Vector3 neighbor in FindNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (!openSet.Contains(neighbor))
                {
                    openSet.Enqueue(neighbor, gScore[neighbor] + HeuristicF(neighbor, goal));
                    parentMaze.SetFreeTileColor(parentMaze.GetMazeTileForWorldPosition(neighbor), Color.green);
                }
                float tentative_gScore = gScore[current] + HeuristicF(current, neighbor);
                if (tentative_gScore >= gScore[neighbor]) continue;
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;
                openSet.UpdatePriority(neighbor, gScore[neighbor] + HeuristicF(neighbor, goal));
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    private float HeuristicF(Vector3 cur, Vector3 goal)
    {
        if (GameManager.Instance.euclidH) return EuclidHeuristic(cur, goal);
        return 0;
    }
    private float EuclidHeuristic(Vector3 cur, Vector3 goal)
    {
        var dx = (cur.x - goal.x);
        var dy = (cur.y - goal.y);
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    private void ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current, Vector3 start)
    {
        List<Vector3> total_path = new List<Vector3>();
        total_path.Add(current);
        parentMaze.SetFreeTileColor(parentMaze.GetMazeTileForWorldPosition(current), Color.blue);
        while (current != start)
        {
            foreach (Vector3 wp in cameFrom.Keys)
            {
                if (wp == current)
                {
                    current = cameFrom[wp];
                    total_path.Add(current);
                    parentMaze.SetFreeTileColor(parentMaze.GetMazeTileForWorldPosition(current), Color.blue);
                }
            }
        }
        total_path.Reverse();
        shortestPath = total_path;
    }

    //this function is so ugly im so so sorry
    private List<Vector3> FindNeighbors(Vector3 tile)
    {
        List<Vector3> res = new List<Vector3>();
        var center = parentMaze.GetMazeTileForWorldPosition(tile);
        List<Vector2Int> sides = new List<Vector2Int>();
        sides.Add(new Vector2Int(center.x - 1, center.y));
        sides.Add(new Vector2Int(center.x + 1, center.y));
        sides.Add(new Vector2Int(center.x, center.y - 1));
        sides.Add(new Vector2Int(center.x, center.y + 1));
        
        foreach (Vector2Int pos in sides)
        {
            if (parentMaze.IsValidTileOfType(pos, MazeTileType.Free))
            {
                res.Add(parentMaze.GetWorldPositionForMazeTile(pos));
            }
        }
        
        Vector2Int cur = new Vector2Int(center.x - 1, center.y - 1);
        if (CheckCorners(cur, new Vector2Int(center.x - 1, center.y), new Vector2Int(center.x, center.y - 1)))
            res.Add(parentMaze.GetWorldPositionForMazeTile(cur));
        cur = new Vector2Int(center.x - 1, center.y + 1);
        if (CheckCorners(cur, new Vector2Int(center.x - 1, center.y), new Vector2Int(center.x, center.y + 1)))
            res.Add(parentMaze.GetWorldPositionForMazeTile(cur));
        cur = new Vector2Int(center.x + 1, center.y - 1);
        if (CheckCorners(cur, new Vector2Int(center.x + 1, center.y), new Vector2Int(center.x, center.y - 1)))
            res.Add(parentMaze.GetWorldPositionForMazeTile(cur));
        cur = new Vector2Int(center.x + 1, center.y + 1);
        if (CheckCorners(cur, new Vector2Int(center.x + 1, center.y), new Vector2Int(center.x, center.y + 1)))
            res.Add(parentMaze.GetWorldPositionForMazeTile(cur));
        
        return res;
    }

    private bool CheckCorners(Vector2Int cur, Vector2Int side1, Vector2Int side2)
    {
        return parentMaze.IsValidTileOfType(cur, MazeTileType.Free) &&
               parentMaze.IsValidTileOfType(side1, MazeTileType.Free) &&
               parentMaze.IsValidTileOfType(side2, MazeTileType.Free);
    }
}
