using UnityEngine;

public static class Constants
{
    public static readonly Color PlayerColor = new Color32(255,191,0, 255);
    public static readonly Color EnemyColor = new Color32(255,75,51, 255);
    public static bool IsPlayerAlive = true;
    
    public static int OwnBulletDamage = 1;
    public static int DashDamage = 5;
    public static int PlayerMaxHealth = 50;
}
