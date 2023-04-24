using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseAgentSpeedPickupData", menuName = "ScriptableObjects/Increase Agent Speed Pickup Data")]
public class IncreaseAgentSpeedPickupData : PickupData
{
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private float speedMultiplier = 1.1f;

    public override PickupType Type => PickupType.IncreaseAgentSpeed;

    public override Sprite Sprite => sprite;

    public override void Apply(Agent agent)
    {
        agent.MovementSpeed *= speedMultiplier;
    }
}
