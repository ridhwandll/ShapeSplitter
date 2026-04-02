using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Abilities/CoreLock")]
public class Ability_CoreLock : Ability
{
    public float LockDuration = 8.0f;
    public ParticleSystem ParticleSystemPrefab;
    private ParticleSystem _psObject = null;
    private float prevCoreMoveSpeed;

    protected override void Activate(PlayerController shape)
    {
        // Halt the current movement
        shape.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;

        Shape shapeData = shape.GetShapeData();
        prevCoreMoveSpeed = shapeData.CoreMoveSpeed;
        shapeData.CoreMoveSpeed = 0.0f;

        if(_psObject == null)
            _psObject = Instantiate(ParticleSystemPrefab, shape.transform);        

        _psObject.Play();
    }

    // Undo What was done in Activation
    protected override void Deactivate(PlayerController shape)
    {
        Shape shapeData = shape.GetShapeData();
        shapeData.CoreMoveSpeed = prevCoreMoveSpeed;
        _psObject.Stop();
    }
}