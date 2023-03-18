using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Sphere
{
    public Vector3 WorldCenter { get; set; }

    public float Radius { get; set; }
}

public class CombinedSteeringAgent : AbstractSteeringAgent
{
    [SerializeField]
    private SphereCollider sphereCollider;
        
    [SerializeField]
    private Path pathToFollow;

    [SerializeField]
    private LayerMask obstacleLayer;

    private Sphere[] staticObstacles;
    private CombinedSteeringAgent[] otherAgents;
    private int currentPathNode = 0;

    // The AI agent can be considered a sphere
    // with center at "Position" and radius equal to this property
    public float Radius => sphereCollider.bounds.extents.x; // .bounds is used instead of .radius since the radius is in local coordinates

    protected override void Awake()
    {
        base.Awake();

        SetInitialLocation();
    }

    protected override void Start()
    {
        base.Start();

        InitializeObstaclesArrays();
    }

    protected override void Update()
    {
        base.Update();

        // TODO Add your solution here. Feel free to add your own variables or helper functions.
        //      Use "pathToFollow.GetPathVertices()" to access the points of the path.
        //      Variables "staticObstacles" and "otherAgents" contain information about obstacles to avoid, and agents to avoid.
        //      However, your solution does not necessarily have to use these arrays – but they are here to help you in case of need.
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private void InitializeObstaclesArrays()
    {
        // NOTE for a curious reader:
        // It is not a best practice to use LINQ in Unity because of the performance & garbage collection issues.
        // However, in this case, this is not a big deal as the performance is far from being an issue (hopefully). 
        // But in general, it is better to stay away from LINQ, mainly in Update() function and other computations repeated every frame.
        staticObstacles = FindObjectsOfType<CapsuleCollider>()
            .Where(x => obstacleLayer == (obstacleLayer | (1 << x.gameObject.layer)))
            .Select(x => new Sphere
            {
                WorldCenter = x.transform.TransformPoint(x.center),
                Radius = x.bounds.extents.x
            })
            .ToArray();

        otherAgents = FindObjectsOfType<CombinedSteeringAgent>()
            .Where(x => x != this)
            .ToArray();
    }

    private void SetInitialLocation()
    {
        var pathVertices = pathToFollow.GetPathVertices();
        Position = pathVertices[currentPathNode];
        if (pathVertices.Length > 1)
        {
            SetRotationImmediate(pathVertices[currentPathNode + 1] - Position);
        }
    }
}
