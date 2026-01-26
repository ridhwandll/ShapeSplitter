using UnityEngine;

public static class Globals
{
    public static readonly Color PlayerColor = new Color32(255,191,0, 255);
    public static readonly Color EnemyColor = new Color32(255,75,51, 255);
    public static readonly int InitialShopItemCost = 100;
    public static readonly float CostMultiplier = 1.4f;

    public static float RepulsorCooldown = 65f;
    public static float RepulsorCooldownDecreasePerLevel = 1.5f;
    
    public static float PlayerDashCooldown = 12.0f;
    public static float DashCooldownDecreasePerLevel = 0.3f;

    public static int HealthIncreasePerLevel = 5; //5 HP
        
    public static int OwnBulletDamage = 1;
    public static int DashDamage = 5;
    public static int PlayerMaxHealth = 50;

    public static bool Bloom = true;
    public static bool Vignette = true;
    public static bool Tonemapping = true;

    public static float MasterVolume = 1.0f;
    public static float SoundFXVolume = 1.0f;
    public static float MusicVolume = 0.1f;
    
    public static DifficultyLevel Difficulty = DifficultyLevel.Medium;
    
    // Sore
    public static int Coins = 1000;
    public static int Highscore = 00;

    //Shop
    public static ShopElement[] ShopElements = new ShopElement[5];
    public static int GetShopElementLevel(ShopElementType type)
    {
        if (ShopElements[(int)type] == null)
            return 0;
        
        foreach (var shopElement in ShopElements)
        {
            if (shopElement.ElementType == type)
                return shopElement.Level;
        }
        return 0;
    }
    
    // Score Awards
    public static readonly int KillUnitShortRangedScore = 25;
    public static readonly int KillUnitLongRangedScore = 30;
    public static readonly int KillUnitBigChonkScore = 60;

    public static int ScoreToCoinConv(int score)
    {
        return Mathf.RoundToInt(score / 12f);
    }

    public static readonly int MaxShopItemLevel = 15;
    
    public static bool DashTipShown = false;
    public static bool RepulseTipShown = false;
}
