using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public enum ShopElementType
{
    Life = 0,
    Dash = 1,
    Speed = 2,
    LifeRegen = 3,
    Repulsor = 4
}

[System.Serializable]
public class ShopElement
{
    public ShopElementType ElementType;
    public int Level;
}

[System.Serializable]
public class ShopElementsSaveData
{
    public ShopElement[] shopElements;
}

public class ShopElementUI
{
    public Label LevelLabel;
    public Label CostLabel;
    public Button BuyButton;
}

public class ShopMenuManager
{
    private Label _coinsLabel;
    private ShopElementUI[] _shopElementsUIs;
    private ShopElement[] _shopElements;
    private string SavePath => Path.Combine(Application.persistentDataPath, "ShapeSplitter_Shop_Beta001.json");
    
    public void Initialize(VisualElement root)
    {
        _coinsLabel = root.Q<Label>("CoinsLabel");
        
        _shopElements = new ShopElement[5];
        _shopElementsUIs = new ShopElementUI[5];
        
        for (int i = 0; i < _shopElementsUIs.Length; i++)
        {
            _shopElementsUIs[i] = new ShopElementUI();
            ShopElementUI shopElementUI = _shopElementsUIs[i];
            
            shopElementUI.LevelLabel = root.Q<Label>("LevelLabel_" + i);
            shopElementUI.CostLabel = root.Q<Label>("CostLabel_" + i);
            shopElementUI.BuyButton = root.Q<Button>("BuyButton_" + i);
            
            var i1 = i;
            shopElementUI.BuyButton.clicked += () => { OnBuyButtonPressed((ShopElementType)i1); };
        }
        for (int i = 0; i < _shopElements.Length; i++)
        {
            _shopElements[i] = new ShopElement();
            _shopElements[i].ElementType = (ShopElementType)i;
            _shopElements[i].Level = 1;
        }
        
        Load();
        InvalidateShopElementsUI();
    }

    private void InvalidateShopElementsUI()
    {
        for (int i = 0; i < _shopElementsUIs.Length; i++)
        {
            ShopElementUI shopElementUI = _shopElementsUIs[i];

            int costOfNextLevel = FindCost(_shopElements[i].Level);
            shopElementUI.LevelLabel.text = "LEVEL: 0" + _shopElements[i].Level;
            shopElementUI.CostLabel.text = "COST: " + costOfNextLevel;
            
            if (costOfNextLevel > Globals.Coins)
                shopElementUI.BuyButton.SetEnabled(false);
            else
                shopElementUI.BuyButton.SetEnabled(true);
        }
        _coinsLabel.text = "COINS: " + Globals.Coins;
    }

    private int FindCost(int elementLevel)
    {
        return Mathf.RoundToInt(Globals.InitialShopItemCost * Mathf.Pow(Globals.CostMultiplier, elementLevel - 1));
    }
    
    private void OnBuyButtonPressed(ShopElementType elementType)
    {
        // Check for coins, take coins
        int currentItemLevel = _shopElements[(int)elementType].Level; 
        int costToUpgrade = FindCost(currentItemLevel);

        if (costToUpgrade <= Globals.Coins)
        {
            _shopElements[(int)elementType].Level++;
            Globals.Coins -= costToUpgrade;
        }
        
        InvalidateShopElementsUI();
        Save();
        PlayerProgress.Instance.Save();
    }
    
    public void Save()
    {
        ShopElementsSaveData save = new ShopElementsSaveData();
        
        save.shopElements = new ShopElement[5];
        
        for (int i = 0; i < save.shopElements.Length; i++)
            save.shopElements[i] = _shopElements[i];
        

        string json = JsonUtility.ToJson(save, true);
        //string encrypted = SaveCrypto.Encrypt(json);
        File.WriteAllText(SavePath, json);

        Debug.Log("Saved to: " + SavePath);
    }

    private bool SavedDataExists()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return false;
        }
        return true;
    }
    
    private bool Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file");
            return false;
        }
        
        string encrypted = File.ReadAllText(SavePath);
        //string json = SaveCrypto.Decrypt(encrypted);
        ShopElementsSaveData save =  JsonUtility.FromJson<ShopElementsSaveData>(encrypted);
        
        for (int i = 0; i < 5; i++)
        {
            _shopElements[i] = save.shopElements[i];
            InvalidateShopElementsUI();
        }
        
        Debug.Log("Loaded Shop");
        return true;
    }

}
