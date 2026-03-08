using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Abilities/Overdrive")]
public class Ability_Overdrive : Ability
{
    public float ShootPowerMultiplier = 2.0f;
    public ParticleSystem ParticleSystemPrefab;
    private ParticleSystem _psObject = null;

    protected override void Activate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();
        shapeData.SplitShapeShootSpeed = shapeData.SplitShapeShootSpeed * ShootPowerMultiplier;

        if(_psObject == null)
            _psObject = Instantiate(ParticleSystemPrefab, shape.transform);        

        _psObject.Play();
    }

    // Undo What was done in Activation
    protected override void Deactivate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();
        shapeData.SplitShapeShootSpeed = shapeData.SplitShapeShootSpeed / ShootPowerMultiplier;
        _psObject.Stop();
    }
}