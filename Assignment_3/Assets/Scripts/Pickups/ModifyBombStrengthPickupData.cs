using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyBombStrengthPickupData", menuName = "ScriptableObjects/Modify Bomb Strength Pickup Data")]
public class ModifyBombStrengthPickupData : PickupData
{
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private int bombStrengthToAdd = 1;

    public override PickupType Type => PickupType.IncreaseBombStrength;

    public override Sprite Sprite => sprite;

    public override void Apply(Agent agent)
    {
        agent.BombsStrength += bombStrengthToAdd;
    }
}
