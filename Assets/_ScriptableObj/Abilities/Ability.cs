using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string AbilityName;
    public Sprite Icon;
    public float Cooldown;

    // Is Ultimate Ability
    public bool IsUltimate;
    public int UltimateActivationPoints;

    // Override this per ability
    public abstract void Activate(PlayerController controller);
}