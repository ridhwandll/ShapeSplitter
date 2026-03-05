using System.Collections;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string AbilityName;
    public float Cooldown = 5.0f;
    public bool IsReady { get; private set; } = true;

    // Is Ultimate Ability
    public bool IsUltimate;
    public int UltimateActivationPoints;

    public bool TryActivate(PlayerController shape)
    {
        if (!IsReady)
            return false;

        shape.StartCoroutine(Run(shape));
        return true;
    }

    private IEnumerator Run(PlayerController shape)
    {
        IsReady = false;
        Activate(shape);

        yield return new WaitForSeconds(Cooldown);

        Deactivate(shape);
        IsReady = true;
    }


    // Override this per ability
    protected abstract void Activate(PlayerController controller);
    protected abstract void Deactivate(PlayerController controller);
}