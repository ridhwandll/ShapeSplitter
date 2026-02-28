using UnityEngine;

[CreateAssetMenu(fileName = "Shape", menuName = "ScriptableObjects/Shape")]
public class Shape : ScriptableObject
{
    public string Name;

    [Header("Settings")]
    public int SplitShapeCount;
    public float MaxSpreadAngle;
    public float MinSpreadAngle;
    public float ShootPower;

    public Color ShapeThemeColorOne;
    public Color ShapeThemeColorTwo;

    [Header("Shape Visuals")]
    public Sprite PlayerCore;
    public Sprite PlayerUnited;
    public Sprite PlayerSplit;
}
