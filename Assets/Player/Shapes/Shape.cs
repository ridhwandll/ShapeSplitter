using UnityEngine;
public enum ShapeRole
{
    ANCHOR,  // Controller
    SHELL,   // Sentinel
    SPLINTER // Duelist
}

[CreateAssetMenu(fileName = "Shape", menuName = "ScriptableObjects/Shape")]
public class Shape : ScriptableObject
{
    public string Name;

    [Header("Settings")]
    public ShapeRole Role;
    public int SplitShapeCount;
    public float MaxSpreadAngle;
    public float MinSpreadAngle;
    public float SplitShapeShootSpeed;
    public float CoreMoveSpeed;
    public float SplitShapeReturnSpeed;

    public Color ShapeThemeColorOne;
    public Color ShapeThemeColorTwo;

    [Header("Shape Visuals")]
    public Sprite ShapeCoreSprite;
    public Sprite ShapeUnitedSprite;
    public Sprite ShapeSplitSprite;

    [Header("Abilities")]
    public Ability Ability1;
    public Ability Ability2;
    public Ability Ultimate;
}
