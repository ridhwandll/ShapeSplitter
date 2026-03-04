using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Abilities/Overdrive")]
public class Ability_Overdrive : Ability
{
    public float ShootPowerMultiplier = 2.0f;

    public override void Activate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();

        shapeData.ShootPower = shapeData.ShootPower * ShootPowerMultiplier;
    }
}