using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyBrain : AgentBrain
{
    // public Agent Agent { get; set; } // Agent "owning" this brain
    //
    // public Maze Maze { get; private set; }
    //
    // public List<Agent> AllAgents => GameManager.Instance.ActiveAgents;
    //
    // public List<Bomb> ActiveBombs => GameManager.Instance.ActiveBombs;
    //
    // public List<Pickup> ActivePickups => GameManager.Instance.ActivePickups;
    //
    // public virtual void Initialize(Agent ownerAgent, Maze maze)
    // {
    //     Agent = ownerAgent;
    //     Maze = maze;
    // }

    public override void Update() {}

    public override AgentAction GetNextAction()
    {
        return AgentAction.Stay;
    }

    protected override AgentAction[] GetPathTo(Vector2Int destinationTile)
    {
        List<Vector2Int> shortestPath = Agent.A_Star(Agent.CurrentTile, destinationTile);
        AgentAction[] path = new AgentAction[]{};
        Vector2Int current = Agent.CurrentTile;
        foreach (Vector2Int tile in shortestPath)
        {
            Vector2Int dir = tile - current;
            if (dir.x == 0)
            {
                path.Append(dir.y < 0 ? AgentAction.MoveUp : AgentAction.MoveDown);
            }
            else
            {
                path.Append(dir.x < 0 ? AgentAction.MoveLeft : AgentAction.MoveRight);
            }
                
        }
        return path;
    }
}
