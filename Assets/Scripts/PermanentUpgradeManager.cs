using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PermanentUpgrade
{
    public string upgradeName;
    public int currentLevel = 0;
    public int maxLevel = 10;
    public int baseCost = 100;
    
    public float baseValue;
    public float maxValue;
    public float valuePerLevel;
    
    // Mevcut değeri hesapla
    public float GetCurrentValue()
    {
        return baseValue + (valuePerLevel * currentLevel);
    }
    
    // Bir sonraki upgrade maliyeti (artan maliyet)
    public int GetUpgradeCost()
    {
        return baseCost + (currentLevel * 50); // Her level +50 coin
    }
    
    // Max level'de mi?
    public bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }
}

public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager Instance;
    
    [Header("Upgrades")]
    public PermanentUpgrade maxHealthUpgrade;
    public PermanentUpgrade projectileSpeedUpgrade;
    public PermanentUpgrade primaryDamageUpgrade;
    public PermanentUpgrade ultiCooldownUpgrade;
    public PermanentUpgrade criticalChanceUpgrade;
    public PermanentUpgrade criticalDamageUpgrade;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // DEBUG - Tüm upgrade'leri sıfırla
    public void ResetAllUpgrades()
    {
        Debug.Log("🔄 TÜM UPGRADE'LER SIFIRLANACAK!");
    
        // PlayerPrefs'i temizle
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.DeleteKey("MaxHealthLevel");
        PlayerPrefs.DeleteKey("ProjectileSpeedLevel");
        PlayerPrefs.DeleteKey("PrimaryDamageLevel");
        PlayerPrefs.DeleteKey("UltiCooldownLevel");
        PlayerPrefs.DeleteKey("CriticalChanceLevel");
        PlayerPrefs.DeleteKey("CriticalDamageLevel");
        PlayerPrefs.Save();
    
        // Coin'i sıfırla
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.totalCoins = 0;
            CoinManager.Instance.currentSessionCoins = 0;
        }
    
        // Upgrade level'ları sıfırla
        if (maxHealthUpgrade != null)
            maxHealthUpgrade.currentLevel = 0;
        if (projectileSpeedUpgrade != null)
            projectileSpeedUpgrade.currentLevel = 0;
        if (primaryDamageUpgrade != null)
            primaryDamageUpgrade.currentLevel = 0;
        if (ultiCooldownUpgrade != null)
            ultiCooldownUpgrade.currentLevel = 0;
        if (criticalChanceUpgrade != null)
            criticalChanceUpgrade.currentLevel = 0;
        if (criticalDamageUpgrade != null)
            criticalDamageUpgrade.currentLevel = 0;
    
        Debug.Log("✅ Tüm upgrade'ler ve coin'ler sıfırlandı!");
    }
    
    void Start()
    {
        InitializeUpgrades();
        LoadUpgrades();
    }
    
    // Upgrade'leri başlat
    void InitializeUpgrades()
    {
        // Max Health: 10 → 50 (her level +4)
        maxHealthUpgrade = new PermanentUpgrade
        {
            upgradeName = "Max Health",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 100,
            baseValue = 10f,
            maxValue = 50f,
            valuePerLevel = 4f
        };
        
        // Projectile Speed: 5 → 20 (her level +1.5)
        projectileSpeedUpgrade = new PermanentUpgrade
        {
            upgradeName = "Projectile Speed",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 80,
            baseValue = 5f,
            maxValue = 20f,
            valuePerLevel = 1.5f
        };
        
        // Primary Damage: 5 → 30 (her level +2.5)
        primaryDamageUpgrade = new PermanentUpgrade
        {
            upgradeName = "Primary Damage",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 120,
            baseValue = 5f,
            maxValue = 30f,
            valuePerLevel = 2.5f
        };
        
        // Ulti Cooldown: 20 → 5 (her level -1.5)
        ultiCooldownUpgrade = new PermanentUpgrade
        {
            upgradeName = "Ulti Cooldown",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 150,
            baseValue = 20f,
            maxValue = 5f,
            valuePerLevel = -1.5f
        };
        
        // Critical Chance: 5% → 30% (her level +2.5%)
        criticalChanceUpgrade = new PermanentUpgrade
        {
            upgradeName = "Critical Chance",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 200,
            baseValue = 5f,
            maxValue = 30f,
            valuePerLevel = 2.5f
        };
        
        // Critical Damage: 30% → 100% (her level +7%)
        criticalDamageUpgrade = new PermanentUpgrade
        {
            upgradeName = "Critical Damage",
            currentLevel = 0,
            maxLevel = 10,
            baseCost = 250,
            baseValue = 30f,
            maxValue = 100f,
            valuePerLevel = 7f
        };
    }
    
    // Upgrade yap
    public bool TryUpgrade(PermanentUpgrade upgrade)
    {
        if (upgrade.IsMaxLevel())
        {
            Debug.LogWarning($" {upgrade.upgradeName} max level!");
            return false;
        }
        
        int cost = upgrade.GetUpgradeCost();
        
        if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(cost))
        {
            upgrade.currentLevel++;
            SaveUpgrades();
            
            Debug.Log($"✅ {upgrade.upgradeName} upgraded! Level: {upgrade.currentLevel}, Value: {upgrade.GetCurrentValue()}");
            
            return true;
        }
        
        return false;
    }
    
    // Upgrade'leri kaydet
    void SaveUpgrades()
    {
        PlayerPrefs.SetInt("MaxHealthLevel", maxHealthUpgrade.currentLevel);
        PlayerPrefs.SetInt("ProjectileSpeedLevel", projectileSpeedUpgrade.currentLevel);
        PlayerPrefs.SetInt("PrimaryDamageLevel", primaryDamageUpgrade.currentLevel);
        PlayerPrefs.SetInt("UltiCooldownLevel", ultiCooldownUpgrade.currentLevel);
        PlayerPrefs.SetInt("CriticalChanceLevel", criticalChanceUpgrade.currentLevel);
        PlayerPrefs.SetInt("CriticalDamageLevel", criticalDamageUpgrade.currentLevel);
        
        PlayerPrefs.Save();
        
        Debug.Log("💾 Permanent upgrades kaydedildi!");
    }
    
    // Upgrade'leri yükle
    void LoadUpgrades()
    {
        maxHealthUpgrade.currentLevel = PlayerPrefs.GetInt("MaxHealthLevel", 0);
        projectileSpeedUpgrade.currentLevel = PlayerPrefs.GetInt("ProjectileSpeedLevel", 0);
        primaryDamageUpgrade.currentLevel = PlayerPrefs.GetInt("PrimaryDamageLevel", 0);
        ultiCooldownUpgrade.currentLevel = PlayerPrefs.GetInt("UltiCooldownLevel", 0);
        criticalChanceUpgrade.currentLevel = PlayerPrefs.GetInt("CriticalChanceLevel", 0);
        criticalDamageUpgrade.currentLevel = PlayerPrefs.GetInt("CriticalDamageLevel", 0);
        
        Debug.Log("💎 Permanent upgrades yüklendi!");
    }
    
    // Getter'lar (diğer sistemler için)
    public float GetMaxHealthBonus() => maxHealthUpgrade.GetCurrentValue();
    public float GetProjectileSpeedBonus() => projectileSpeedUpgrade.GetCurrentValue();
    public float GetPrimaryDamageBonus() => primaryDamageUpgrade.GetCurrentValue();
    public float GetUltiCooldownReduction() => Mathf.Abs(ultiCooldownUpgrade.GetCurrentValue() - 20f);
    public float GetCriticalChance() => criticalChanceUpgrade.GetCurrentValue();
    public float GetCriticalDamage() => criticalDamageUpgrade.GetCurrentValue();
}