using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupType
{
    AddBomb,
    IncreaseBombStrength,
    IncreaseAgentSpeed
}

public abstract class PickupData : ScriptableObject
{
    public abstract PickupType Type { get; }

    public abstract Sprite Sprite { get; }

    public abstract void Apply(Agent agent);
}
