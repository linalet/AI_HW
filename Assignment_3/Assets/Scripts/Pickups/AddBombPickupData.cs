using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddBombPickupData", menuName = "ScriptableObjects/Add Bomb Pickup Data")]
public class AddBombPickupData : PickupData
{
    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private int bombsToAdd = 1;

    public override PickupType Type => PickupType.AddBomb;

    public  override Sprite Sprite => sprite;

    public override void Apply(Agent agent)
    {
        agent.BombsMaxCount += bombsToAdd;
    }
}
