using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentBrain
{
    public Agent Agent { get; set; } // Agent "owning" this brain

    public Maze Maze { get; private set; }

    public List<Agent> AllAgents => GameManager.Instance.ActiveAgents;

    public List<Bomb> ActiveBombs => GameManager.Instance.ActiveBombs;

    public List<Pickup> ActivePickups => GameManager.Instance.ActivePickups;

    public virtual void Initialize(Agent ownerAgent, Maze maze)
    {
        Agent = ownerAgent;
        Maze = maze;
    }

    // If the brain desires to do some real-time computations, it can use this method
    // which is called by Agent in its MonoBehavior.Update()
    public virtual void Update() { }

    // When queried, returns the action the agent desires to perform
    public abstract AgentAction GetNextAction();

    // Returns the path (as a sequence of actions) from the Agent.CurrentTile location to destinationTile
    protected abstract AgentAction[] GetPathTo(Vector2Int destinationTile);
}
