using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStar : MonoBehaviour
{
    public Vector2Int CurrentTile { get; private set; }

    protected float movementSpeed;
    protected Maze parentMaze;
    protected bool isInitialized = false;
    private int tileIndex = 0;

    protected virtual void Update()
    {
        if (tileIndex < shortestPath.Count && shortestPath.Count!=0)
        {
            var nextTile = shortestPath[tileIndex];
            var cur = parentMaze.GetWorldPositionForMazeTile(CurrentTile);
            Vector3 direction = new Vector3(nextTile.x - cur.x, nextTile.y - cur.y, 0);
            direction.Normalize();
            transform.Translate(direction* movementSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, nextTile) < 0.1f)
                CurrentTile = parentMaze.GetMazeTileForWorldPosition(nextTile);

            if(CurrentTile == parentMaze.GetMazeTileForWorldPosition(nextTile))
            {
                tileIndex++;
            }
        }
        

    }
    private IEnumerator A_Star(Vector3 start, Vector3 goal)
    {
        List<Vector3> closedSet = new List<Vector3>();
        SimplePriorityQueue<Vector3> openSet = new SimplePriorityQueue<Vector3>();
        openSet.Enqueue(start, EuclidHeuristic(start, goal));
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
                return ReconstructPath(cameFrom, current, start);
            }
            closedSet.Add(current);
            foreach (Vector3 neighbor in FindNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (!openSet.Contains(neighbor))
                {
                    openSet.Enqueue(neighbor, gScore[neighbor] + EuclidHeuristic(neighbor, goal));
                }
                float tentative_gScore = gScore[current] + EuclidHeuristic(current, neighbor);
                if (tentative_gScore >= gScore[neighbor]) continue;
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;
                openSet.UpdatePriority(neighbor, gScore[neighbor] + EuclidHeuristic(neighbor, goal));
            }
        }
    }
    private float EuclidHeuristic(Vector3 cur, Vector3 goal)
    {
        var dx = (cur.x - goal.x);
        var dy = (cur.y - goal.y);
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current, Vector3 start)
    {
        List<Vector3> total_path = new List<Vector3>();
        total_path.Add(current);
        while (current != start)
        {
            foreach (Vector3 wp in cameFrom.Keys)
            {
                if (wp == current)
                {
                    current = cameFrom[wp];
                    total_path.Add(current);
                }
            }
        }
        total_path.Reverse();
        return total_path;
    }

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
        
        return res;
    }

}
