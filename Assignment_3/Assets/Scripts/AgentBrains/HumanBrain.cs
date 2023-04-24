using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBrain : AgentBrain
{
    private AgentAction mostRecentAction = AgentAction.Stay;

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.W))
        {
            mostRecentAction = AgentAction.MoveUp;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            mostRecentAction = AgentAction.MoveDown;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            mostRecentAction = AgentAction.MoveLeft;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            mostRecentAction = AgentAction.MoveRight;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            mostRecentAction = AgentAction.PlaceBomb;
        }
    }

    public override AgentAction GetNextAction()
    {
        var actionToReturn = mostRecentAction;
        mostRecentAction = AgentAction.Stay;
        return actionToReturn;
    }

    protected override AgentAction[] GetPathTo(Vector2Int destinationTile)
    {
        // You can do this computation in your head, human!
        // What am I for you? Just a machine?!
        return new AgentAction[0];
    }
}
