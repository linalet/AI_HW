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
        // List<Vector3> path = AStar.A_Star(current, destinationTile);
        return new AgentAction[] { AgentAction.Stay };
    }
}
