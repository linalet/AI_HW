using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Vector2Int TileLocation { get; set; }

    public bool Visible 
    {
        get => SpriteRenderer.enabled;
        private set => SpriteRenderer.enabled = value;
    }

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            return _spriteRenderer;
        }
    }

    public PickupType PickupType => pickupData.Type;

    public System.Action<Pickup> Destroyed;

    private Maze maze;
    private PickupData pickupData;
    private bool wasPickedUp = false;

    public void OnDestroy()
    {
        Destroyed?.Invoke(this);
    }

    public void Place(Maze maze, Vector2Int location, PickupData pickupData, bool spawnInvisible = true)
    {
        this.maze = maze;
        TileLocation = location;
        this.pickupData = pickupData;

        transform.position = maze.GetWorldPositionForMazeTile(location.x, location.y);
        transform.localScale = maze.GetElementsScale();

        SpriteRenderer.sprite = pickupData.Sprite;
        Visible = !spawnInvisible;
    }

    public void MakeVisible()
    {
        Visible = true;
    }

    public void CheckForPickUpByAgent(Agent agent)
    {
        if(!wasPickedUp && agent.CurrentTile == TileLocation)
        {
            pickupData.Apply(agent);
            wasPickedUp = true;
            Destroy(gameObject);
        }
    }
}
