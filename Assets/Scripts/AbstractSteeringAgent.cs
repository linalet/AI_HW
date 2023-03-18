using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSteeringAgent : MonoBehaviour
{
    [SerializeField]
    protected MeshRenderer meshRenderer;

    [SerializeField]
    protected Collider movementBounds;

    [SerializeField]
    protected Color color = Color.white;

    [SerializeField]
    protected float maxSpeed = 3.0f;

    [SerializeField]
    protected float rotationSpeedDegPerSec = 90;

    /// <summary>
    /// Defines the (material's) color of this agent
    /// </summary>
    public virtual Color Color
    {
        get => color;
        set
        {
            color = value;
            UpdateMaterialColor(color);
        }
    }

    /// <summary>
    /// Defines the position of this agent
    /// </summary>
    public virtual Vector3 Position
    {
        get => transform.position;
        protected set => transform.position = value;
    }

    /// <summary>
    /// Determines the velocity of this agent
    /// </summary>
    public virtual Vector3 Velocity { get; protected set; }

    /// <summary>
    /// Defines the "ahead direction" for this agent
    /// </summary>
    public virtual Vector3 LookDirection
    {
        get => transform.forward;
        private set => transform.forward = value;
    }

    private Vector3? targetLookDirection = null;

    protected virtual void Awake()
    {
        Velocity = LookDirection * maxSpeed;
    }

    protected virtual void Start()
    {
        Color = color;
    }

    protected virtual void Update() { }

    protected virtual void LateUpdate()
    {
        Position += Velocity * Time.deltaTime;
        ClampPositionToBounds();

        if (targetLookDirection.HasValue)
        {
            Vector3 newDir = Vector3.RotateTowards(LookDirection, targetLookDirection.Value, 
                Mathf.Deg2Rad * rotationSpeedDegPerSec * Time.deltaTime, 0.0f);

            transform.rotation = Quaternion.LookRotation(newDir);

            if (newDir == targetLookDirection)
            {
                targetLookDirection = null;
            }
        }

        Debug.DrawRay(Position, Velocity, Color);
    }

    /// <summary>
    /// Immediately makes the agent face the given direction
    /// </summary>
    public virtual void SetRotationImmediate(Vector3 newLookDirection)
    {
        LookDirection = newLookDirection.normalized;
        targetLookDirection = null;
    }

    /// <summary>
    /// Gradually rotates the agent towards the given direction
    /// </summary>
    public virtual void SetRotationTransition(Vector3 newLookDirection)
    {
        targetLookDirection = newLookDirection.normalized;
    }

    /// <summary>
    /// Returns axis-aligned distance to bounds.
    /// You can use this method to detect that the agent is close to the bounds
    /// in order to "redirect" him somewhere else.
    /// </summary>
    protected virtual float DistanceToBounds()
    {
        return Mathf.Min(
            Position.x - movementBounds.bounds.min.x,
            movementBounds.bounds.max.x - Position.x,
            Position.z - movementBounds.bounds.min.z,
            movementBounds.bounds.max.z - Position.z);
    }

    protected virtual void ClampPositionToBounds()
    {
        Position = new Vector3(
            Mathf.Clamp(Position.x, movementBounds.bounds.min.x, movementBounds.bounds.max.x),
            Mathf.Clamp(Position.y, movementBounds.bounds.min.y, movementBounds.bounds.max.y),
            Mathf.Clamp(Position.z, movementBounds.bounds.min.z, movementBounds.bounds.max.z)
            );
    }

    protected virtual Vector3 GenerateRandomPointInBounds()
    {
        return new Vector3(
            Random.Range(movementBounds.bounds.min.x, movementBounds.bounds.max.x),
            0.0f,
            Random.Range(movementBounds.bounds.min.z, movementBounds.bounds.max.z));
    }

    protected virtual void UpdateMaterialColor(Color newColor)
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        meshRenderer.material.SetColor("_BaseColor", newColor);
    }
}
