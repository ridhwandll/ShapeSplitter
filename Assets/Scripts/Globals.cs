using UnityEngine;

public static class Globals
{
    public static readonly Color PlayerColor = new Color32(255,191,0, 255);
    public static readonly Color EnemyColor = new Color32(255,75,51, 255);
    public static readonly int InitialShopItemCost = 250;
    public static readonly float CostMultiplier = 1.4f;

    public static float PlayerDashCooldown = 8.0f;
        
    public static int OwnBulletDamage = 1;
    public static int DashDamage = 5;
    public static int PlayerMaxHealth = 50;

    public static bool Bloom = true;
    public static bool Vignette = true;
    public static bool Tonemapping = true;

    public static float MasterVolume = 1.0f;
    public static float SoundFXVolume = 1.0f;
    public static float MusicVolume = 0.2f;
    
    public static DifficultyLevel Difficulty = 0;
    
    // Sore
    public static int Coins = 999999;
    public static int Highscore = 69;
}
