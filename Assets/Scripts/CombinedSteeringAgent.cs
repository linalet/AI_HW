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
    private Vector3[] points;

    private Vector3 targetPosition;
    private bool back = false;
    
    // [SerializeField] 
    private float avoidDistance = 2f;
    // [SerializeField] 
    private float lookahead = 1f;
    // [SerializeField] 
    private float sideViewAngle = 45f;

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
        
        //path
        points = pathToFollow.GetPathVertices();
        if (currentPathNode == 0)
        {
            back = false;
        }
        if (currentPathNode == points.Length - 1)
        {
            back = true;
        }
        
        if (Vector3.Distance(points[currentPathNode], Position) < 0.1)
        {
            if (back)
            {
                currentPathNode -= 1;
            }
            else
            {
                currentPathNode += 1;
            }
        }
        targetPosition = points[currentPathNode] - Position;
        
        //agents
        Vector3 colissionTarget = CollisionAvoidance();

        //obstacles
        Vector3 obstacleTarget = ObstacleAvoidance();

        // final direction
        // targetPosition = (5*targetPosition + colissionTarget + obstacleTarget)/3;
        float obstacleDistance = Vector3.Distance(Position, ClosestObstacle().WorldCenter);

        if (obstacleDistance < ClosestObstacle().Radius + Radius+0.1f)
        {
            // targetPosition = obstacleTarget - Position; 
            if (obstacleTarget != Vector3.zero)
                targetPosition = obstacleTarget - Position;
             // targetPosition = obstacleTarget; 
        }
        // else
        // {
            // SetRotationImmediate(targetPosition);
        // }
        // targetPosition = colissionTarget - Position;
        // if (colissionTarget != Vector3.zero)
        // {
            // SetRotationImmediate(colissionTarget); 
        // }
        // else
        // {
            SetRotationImmediate(targetPosition); 
        // }
        
        Velocity = LookDirection * maxSpeed;
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


    private Vector3 ObstacleAvoidance()
    {
        Vector3 target = Vector3.zero;
        Vector3[] rayVector = new Vector3[3];
        rayVector[0] = Velocity;
        rayVector[0].Normalize();
        rayVector[0] *= lookahead;
        float rayOrientation = Mathf.Atan2(  Velocity.x,  Velocity.z);
        float rightRayOrientation = rayOrientation + (sideViewAngle *  Mathf.Deg2Rad);
        float leftRayOrientation = rayOrientation - (sideViewAngle *  Mathf.Deg2Rad);
        rayVector[1] = new Vector3(Mathf.Cos(rightRayOrientation), 0,  Mathf.Sin(rightRayOrientation));
        rayVector[1].Normalize();
        rayVector[1] *= lookahead;
        rayVector[2] = new Vector3(Mathf.Cos(leftRayOrientation), 0,  Mathf.Sin(leftRayOrientation));
        rayVector[2].Normalize();
        rayVector[2] *= lookahead;
        
        for (int i = 0; i < rayVector.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(Position, rayVector[i], out hit, lookahead,obstacleLayer))
            {
                target = hit.point + (hit.normal * avoidDistance);
                target.Normalize();
                break;
            }
        }
        return new Vector3(target.x,0,target.z);
    }

    private Vector3 CollisionAvoidance()
    {
        Vector3 target = Vector3.zero;
        float shortestTime = float.PositiveInfinity;
        CombinedSteeringAgent firstTarget = null;
        float firstMinSeparation = 0, firstDistance = 0, firstRadius = 0;
        Vector3 firstRelativePos = Vector3.zero, firstRelativeVel = Vector3.zero;
        foreach (CombinedSteeringAgent agent in otherAgents)
        {
            Vector3 relativePos = Position - agent.Position;
            Vector3 relativeVel = (LookDirection - agent.LookDirection);
            float distance = relativePos.magnitude;
            float relativeSpeed = relativeVel.magnitude;
            if (relativeSpeed == 0)
                continue;

            float timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

            Vector3 separation = relativePos + relativeVel * timeToCollision;
            float minSeparation = separation.magnitude;

            if (minSeparation > Radius + agent.Radius)
                continue;

            if ((timeToCollision > 0) && (timeToCollision < shortestTime))
            {
                shortestTime = timeToCollision;
                firstTarget = agent;
                firstMinSeparation = minSeparation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
                firstRadius = agent.Radius;
            }
        }

        if (firstTarget == null)
        {
            return target;
        }
        if (firstMinSeparation <= 0 || firstDistance < Radius + firstRadius)
            target = Position - firstTarget.Position;
        else
            target = firstRelativePos + firstRelativeVel * shortestTime;
        target.Normalize();
        return target;
    }
    private Sphere ClosestObstacle()
    {
        Sphere res = staticObstacles[0];
        for (int i = 1; i < staticObstacles.Length; i++)
        {
            if (Vector3.Distance(staticObstacles[i].WorldCenter, Position) <
                Vector3.Distance(res.WorldCenter, Position))
            {
                res = staticObstacles[i];
            }
        }

        return res;
    }
}
