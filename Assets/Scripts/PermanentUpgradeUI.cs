using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermanentUpgradeUI : MonoBehaviour
{
    public static PermanentUpgradeUI Instance;
    
    [Header("UI Panel")]
    public GameObject upgradePanel;
    
    [Header("Coin Display")]
    public TextMeshProUGUI totalCoinText;
    
    [Header("Upgrade Buttons")]
    public Button maxHealthButton;
    public Button projectileSpeedButton;
    public Button primaryDamageButton;
    public Button ultiCooldownButton;
    public Button criticalChanceButton;
    public Button criticalDamageButton;
    
    [Header("Upgrade Info Texts")]
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI projectileSpeedText;
    public TextMeshProUGUI primaryDamageText;
    public TextMeshProUGUI ultiCooldownText;
    public TextMeshProUGUI criticalChanceText;
    public TextMeshProUGUI criticalDamageText;
    
    [Header("Close Button")]
    public Button closeButton;
    
    [Header("DEBUG - Reset Button")] // YENİ! 🔄✅
    public Button resetButton;
    
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
        // Panel başlangıçta kapalı
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        // Buton listener'ları ekle
        SetupButtons();
    }
    
    void SetupButtons()
    {
        if (maxHealthButton != null)
            maxHealthButton.onClick.AddListener(() => UpgradeMaxHealth());
        
        if (projectileSpeedButton != null)
            projectileSpeedButton.onClick.AddListener(() => UpgradeProjectileSpeed());
        
        if (primaryDamageButton != null)
            primaryDamageButton.onClick.AddListener(() => UpgradePrimaryDamage());
        
        if (ultiCooldownButton != null)
            ultiCooldownButton.onClick.AddListener(() => UpgradeUltiCooldown());
        
        if (criticalChanceButton != null)
            criticalChanceButton.onClick.AddListener(() => UpgradeCriticalChance());
        
        if (criticalDamageButton != null)
            criticalDamageButton.onClick.AddListener(() => UpgradeCriticalDamage());
        
        if (closeButton != null)
            closeButton.onClick.AddListener(() => ClosePanel());
        
        // DEBUG - RESET BUTTON! 🔄✅
        if (resetButton != null)
            resetButton.onClick.AddListener(() => ResetAllUpgrades());
    }
    
    // DEBUG - Reset all upgrades
    void ResetAllUpgrades()
    {
        if (PermanentUpgradeManager.Instance != null)
        {
            // Onay popup'ı (opsiyonel)
            Debug.Log("⚠️ RESET! Tüm upgrade'ler silinecek!");
        
            PermanentUpgradeManager.Instance.ResetAllUpgrades();
        
            // UI'yi güncelle
            UpdateUI();
        
            Debug.Log("✅ Reset tamamlandı! Oyunu restart et.");
        }
    }
    
    // Paneli aç
    public void OpenPanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // Oyunu durdur
            Time.timeScale = 0f;
            
            // UI'yi güncelle
            UpdateUI();
        }
    }
    
    // Paneli kapat
    public void ClosePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    // UI güncelle
    void UpdateUI()
    {
        if (CoinManager.Instance == null || PermanentUpgradeManager.Instance == null)
            return;
        
        // Toplam coin
        int totalCoins = CoinManager.Instance.GetTotalCoins();
        if (totalCoinText != null)
        {
            totalCoinText.text = $"{totalCoins} COIN";
        }
        
        // Her upgrade için bilgi güncelle
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.maxHealthUpgrade, maxHealthText);
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.projectileSpeedUpgrade, projectileSpeedText);
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.primaryDamageUpgrade, primaryDamageText);
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.ultiCooldownUpgrade, ultiCooldownText);
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.criticalChanceUpgrade, criticalChanceText);
        UpdateUpgradeInfo(PermanentUpgradeManager.Instance.criticalDamageUpgrade, criticalDamageText);
    }
    
    // Upgrade bilgisini güncelle
    void UpdateUpgradeInfo(PermanentUpgrade upgrade, TextMeshProUGUI text)
    {
        if (text == null) return;
        
        int currentLevel = upgrade.currentLevel;
        int maxLevel = upgrade.maxLevel;
        float currentValue = upgrade.GetCurrentValue();
        int cost = upgrade.GetUpgradeCost();
        
        string levelText = upgrade.IsMaxLevel() ? "MAX" : $"{currentLevel}/{maxLevel}";
        string costText = upgrade.IsMaxLevel() ? "" : $"\n[{cost}💰]";
        
        text.text = $"{upgrade.upgradeName}\n" +
                    $"Level: {levelText}\n" +
                    $"Value: {currentValue:F1}" +
                    $"{costText}";
    }
    
    // Upgrade fonksiyonları
    void UpgradeMaxHealth()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.maxHealthUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    void UpgradeProjectileSpeed()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.projectileSpeedUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    void UpgradePrimaryDamage()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.primaryDamageUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    void UpgradeUltiCooldown()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.ultiCooldownUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    void UpgradeCriticalChance()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.criticalChanceUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    void UpgradeCriticalDamage()
    {
        if (PermanentUpgradeManager.Instance.TryUpgrade(PermanentUpgradeManager.Instance.criticalDamageUpgrade))
        {
            UpdateUI();
            PlayUpgradeSound();
        }
    }
    
    // Upgrade sesi
    void PlayUpgradeSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLevelUp();
        }
    }
}