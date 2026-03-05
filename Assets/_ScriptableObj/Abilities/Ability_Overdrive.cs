using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Abilities/Overdrive")]
public class Ability_Overdrive : Ability
{
    public float ShootPowerMultiplier = 2.0f;

    protected override void Activate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();
        shapeData.SplitShapeShootSpeed = shapeData.SplitShapeShootSpeed * ShootPowerMultiplier;
    }

    // Undo What was done in Activation
    protected override void Deactivate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();
        shapeData.SplitShapeShootSpeed = shapeData.SplitShapeShootSpeed / ShootPowerMultiplier;
    }
}