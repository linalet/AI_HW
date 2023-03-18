using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingSteeringAgent : AbstractSteeringAgent
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // TODO Add your solution here. Feel free to add your own variables or helper methods.
        //      By accessing "Position" property, you can retrieve the world position of the agent.
        //      You can use "LookDirection" property to retrieve the direction in which the agent is facing.
        //      To move the agent, modify the "Velocity" property storing the direction in which the agent should go,
        //      whereas its magnitude should be equal to agent's speed per second (use "maxSpeed" variable).
        //      You can use "SetRotationImmediate" and "SetRotationTransition" functions to set the rotation / "LookDirection" of the agent.
        //
        //      Example code:
        //      SetRotationTransition(Vector3.right); // SetRotationImmediate(Vector3.right)
        //      Velocity = LookDirection * maxSpeed;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }
}
