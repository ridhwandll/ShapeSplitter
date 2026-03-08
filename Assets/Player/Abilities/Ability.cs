using System.Collections;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string AbilityName;
    public float Cooldown = 5.0f;
    public bool CanActivate { get; private set; } = true;

    // Is Ultimate Ability
    public bool IsUltimate;
    public int UltimateActivationPoints;

    public bool TryActivate(PlayerController shape)
    {
        if (CanActivate == false)
            return false;

        shape.StartCoroutine(Run(shape));
        return true;
    }

    public void TryDeactivate(PlayerController shape)
    {
        if (CanActivate == false)
        {
            Deactivate(shape);
            CanActivate = true;
        }
    }

    private IEnumerator Run(PlayerController shape)
    {
        Activate(shape);
        CanActivate = false;

        yield return new WaitForSeconds(Cooldown);

        CanActivate = true;
        Deactivate(shape);
    }


    // Override this per ability
    protected abstract void Activate(PlayerController controller);
    protected abstract void Deactivate(PlayerController controller);
}