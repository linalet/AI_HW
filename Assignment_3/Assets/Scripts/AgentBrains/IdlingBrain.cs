using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlingBrain : AgentBrain
{
    public override AgentAction GetNextAction()
    {
        return AgentAction.Stay;
    }

    protected override AgentAction[] GetPathTo(Vector2Int destinationTile)
    {
        // Idling brain is too lazy to do any meaningful computations
        return new AgentAction[0];
    }
}
