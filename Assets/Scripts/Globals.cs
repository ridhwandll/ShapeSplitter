using UnityEngine;

public static class Globals
{
    public static readonly Color PlayerColor = new Color32(255,191,0, 255);
    public static readonly Color EnemyColor = new Color32(255,75,51, 255);
    public static readonly int InitialShopItemCost = 100;
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
    public static float MusicVolume = 0.1f;
    
    public static DifficultyLevel Difficulty = 0;
    
    // Sore
    public static int Coins = 00;
    public static int Highscore = 00;

    // Score Awards
    public static readonly int KillUnitShortRangedScore = 25;
    public static readonly int KillUnitLongRangedScore = 30;
    public static readonly int KillUnitBigChonkScore = 60;

    public static int ScoreToCoinConv(int score)
    {
        return Mathf.RoundToInt(score / 12f);
    }
}
