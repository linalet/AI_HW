using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleCpuBrain : AgentBrain
{
    private int dir;

    private bool runFromBomb = false;
    private bool shouldReturnFromAvoidance = false;

    public SimpleCpuBrain(int dir)
    {
        this.dir = dir;
    }

    public override AgentAction GetNextAction()
    {
        // This is a simple (and not very nice) code which gives
        // the agent some very negligible intelligence.
        // Thanks to that, you can see the agents doing at least some kind of action and not just remain static. :-)
        // 
        // The code is obviously not using behavior trees or any other advanced techniques.
        // Besides that, the "simple brained" agents will probably commit suicide sooner or later.
        // It's up to you to create your own "brains" making the agents more clever. :-)

        int x = Agent.CurrentTile.x;
        int y = Agent.CurrentTile.y;
        
        if (!runFromBomb && Maze.IsValidTileOfType(new Vector2Int(x + 1 * dir, y), MazeTileType.DestructibleWall))
        {
            runFromBomb = true;
            return AgentAction.PlaceBomb;
        }
        else if (!runFromBomb)
        {
            return dir > 0 ? AgentAction.MoveRight : AgentAction.MoveLeft;
        }
        else if (runFromBomb && shouldReturnFromAvoidance)
        {
            if (!ActiveBombs.Any(b => ((dir > 0 && b.TileLocation.x >= Agent.CurrentTile.x) || (dir < 0 && b.TileLocation.x <= Agent.CurrentTile.x)) 
            && b.TileLocation.y == Agent.CurrentTile.y + 1 * dir))
            {
                shouldReturnFromAvoidance = false;
                return dir > 0 ? AgentAction.MoveDown : AgentAction.MoveUp;
            }
            return AgentAction.Stay;
        }
        else
        {
            if (ActiveBombs.Any(b => ((dir > 0 && b.TileLocation.x >= Agent.CurrentTile.x) || (dir < 0 && b.TileLocation.x <= Agent.CurrentTile.x)) 
            && b.TileLocation.y == Agent.CurrentTile.y) && Maze.IsValidTileOfType(new Vector2Int(x, y - 1 * dir), MazeTileType.Free))
            {
                shouldReturnFromAvoidance = true;
                return dir > 0 ? AgentAction.MoveUp : AgentAction.MoveDown;
            }

            if (!Maze.IsValidTileOfType(new Vector2Int(x - 1 * dir, y), MazeTileType.Free))
            {
                runFromBomb = false;
            }
            return dir > 0 ? AgentAction.MoveLeft : AgentAction.MoveRight;
        }
    }

    protected override AgentAction[] GetPathTo(Vector2Int destinationTile)
    {
        // Path to happiness is what I sought and what I found. It is right here.
        // So do not expect me to give you a meaningful output from this function.
        // I am a simple brain with a great vision. At least they say so.
        return new AgentAction[] { AgentAction.Stay };
    }
}
