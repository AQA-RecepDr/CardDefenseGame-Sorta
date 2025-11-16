using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    
    [Header("Mevcut Upgrade'ler")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();
    
    [Header("Seçilen Upgrade'ler")]
    public List<UpgradeData> selectedUpgrades = new List<UpgradeData>();
    
    [Header("Aktif Buff'lar")]
    public float damageMultiplier = 1f;        // x1.0 (normal)
    public float ultiCooldownReduction = 0f;   // -0s
    public float turretDamageMultiplier = 1f;  // x1.0
    public int maxHealthBonus = 0;             // +0
    public float fireRateMultiplier = 1f;      // x1.0
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 5 upgrade tanımla
        DefineUpgrades();
    }
    
    // Upgrade'leri tanımla
    void DefineUpgrades()
    {
        allUpgrades.Clear();
        
        // 1. Hasar Artışı
        allUpgrades.Add(new UpgradeData
        {
            upgradeName = "Güçlü Vuruş",
            description = "Oyuncu ateş hasarı %20 artar",
            type = UpgradeData.UpgradeType.IncreaseDamage,
            value = 0.2f // +20%
        });
        
        // 2. Ulti Cooldown Azaltma
        allUpgrades.Add(new UpgradeData
        {
            upgradeName = "Hızlı Şarj",
            description = "Ulti cooldown süresi 2 saniye azalır",
            type = UpgradeData.UpgradeType.DecreaseUltiCooldown,
            value = 2f // -2s
        });
        
        // 3. Turret Güçlendirme
        allUpgrades.Add(new UpgradeData
        {
            upgradeName = "Gelişmiş Turret",
            description = "Kırmızı turret hasarı %50 artar",
            type = UpgradeData.UpgradeType.IncreaseTurretDamage,
            value = 0.5f // +50%
        });
        
        // 4. Maksimum Can Artışı
        allUpgrades.Add(new UpgradeData
        {
            upgradeName = "Sağlam Zırh",
            description = "Maksimum can 10 artar",
            type = UpgradeData.UpgradeType.IncreaseMaxHealth,
            value = 10f // +10 can
        });
        
        // 5. Ateş Hızı Artışı
        allUpgrades.Add(new UpgradeData
        {
            upgradeName = "Hızlı Tetik",
            description = "Ateş hızı %30 artar",
            type = UpgradeData.UpgradeType.IncreaseFireRate,
            value = 0.3f // +30%
        });
        
        Debug.Log($"✅ {allUpgrades.Count} upgrade tanımlandı!");
    }
    
    // Rastgele 3 upgrade seç
    public List<UpgradeData> GetRandomUpgrades(int count = 3)
    {
        List<UpgradeData> available = new List<UpgradeData>(allUpgrades);
        List<UpgradeData> selected = new List<UpgradeData>();
        
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            selected.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
        
        return selected;
    }
    
    // Upgrade'i uygula
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        selectedUpgrades.Add(upgrade);
        
        switch (upgrade.type)
        {
            case UpgradeData.UpgradeType.IncreaseDamage:
                damageMultiplier += upgrade.value;
                Debug.Log($" Hasar çarpanı: x{damageMultiplier}");
                break;
                
            case UpgradeData.UpgradeType.DecreaseUltiCooldown:
                ultiCooldownReduction += upgrade.value;
                Debug.Log($" Ulti cooldown azaltma: -{ultiCooldownReduction}s");
                break;
                
            case UpgradeData.UpgradeType.IncreaseTurretDamage:
                turretDamageMultiplier += upgrade.value;
                Debug.Log($" Turret hasar çarpanı: x{turretDamageMultiplier}");
                break;
                
            case UpgradeData.UpgradeType.IncreaseMaxHealth:
                maxHealthBonus += (int)upgrade.value;
                
                // Oyuncunun max canını artır
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.maxHealth += (int)upgrade.value;
                    player.currentHealth += (int)upgrade.value; // Mevcut canı da artır
                }
                Debug.Log($"Max can bonusu: +{maxHealthBonus}");
                break;
                
            case UpgradeData.UpgradeType.IncreaseFireRate:
                fireRateMultiplier += upgrade.value;
                Debug.Log($" Ateş hızı çarpanı: x{fireRateMultiplier}");
                break;
        }
    }
}