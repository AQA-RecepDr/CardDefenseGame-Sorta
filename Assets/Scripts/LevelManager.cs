using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [System.Serializable]
    public class LevelData
    {
        [Header("Level Bilgisi")]
        public int levelNumber;
        [Header("Reward Ayarları")]
        public bool showUpgradeReward = false;
        public RewardScreen.RewardType rewardType;
    }
    
    [Header("Level Ayarları")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;
    
    [Header("Referanslar")]
    public CardManager cardManager;
  
    [Header("Countdown")]
    public float countdownTime = 3f; // 3 saniye geri sayım
    public bool isCountdownActive = false;
    private float countdownTimer = 0f;
    
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
    void Update()
    {
        if (isCountdownActive)
        {
            // Time.unscaledDeltaTime kullan - Time.timeScale'den bağımsız!
            countdownTimer -= Time.unscaledDeltaTime;
        
            // UI'da göster
            if (UIManager.Instance != null)
            {
                int displayTime = Mathf.CeilToInt(countdownTimer);
                UIManager.Instance.ShowCountdown(displayTime);
            }
        
            if (countdownTimer <= 0)
            {
                // Countdown bitti, oyunu başlat!
                isCountdownActive = false;
                StartLevel();
            }
        }
    }

    void Start()
    {
        // İlk level başlarken başlangıç kartlarını ver
        Debug.Log("🎮 LevelManager hazır - Start bekleniyor...");
    }
   
    // Level'i yükle
    public void LoadLevel(int levelIndex)
    {
        // Eski düşmanları temizle
        ClearAllEnemies();
        
        // YENİ - Level başlarken lane'leri temizle
        ClearAllZones();
        ResetUltiCooldown();
        
        if (levelIndex >= levels.Length)
        {
            Debug.Log("🎉 TÜM LEVELLER TAMAMLANDI!");
            return;
        }
    
        currentLevelIndex = levelIndex;
        LevelData level = levels[levelIndex];
    
        Debug.Log($"📚 Level {level.levelNumber} yükleniyor...");
        // Countdown'u başlat
        StartCountdown();
    }
    
    // Ulti cooldown'ını sıfırla
    void ResetUltiCooldown()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.ResetUltiCooldown();
        }
    }
    
   // Sonraki level'e geç
    public void NextLevel()
    {
        // ZONE'LARI TEMİZLE - YENİ!
        ClearAllZones();
        
      // Mevcut level'in reward'ını kontrol et
        if (currentLevelIndex < levels.Length)
        {
            LevelData completedLevel = levels[currentLevelIndex];
        
            if (completedLevel.showUpgradeReward)
            {
                // Upgrade seçim ekranını göster
                ShowUpgradeReward();
                return; // RewardScreen devam ettirecek
            }
        }
    
        Debug.Log("⏭️ Reward yok, sonraki level yükleniyor...");
        ContinueToNextLevel();
    }
    
  // Tüm zone'ları temizle
    void ClearAllZones()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
    
        foreach (Zone zone in allZones)
        {
            zone.ClearZone();
        }
    
        Debug.Log($"🧹 {allZones.Length} zone temizlendi!");
    }
    
    public void ContinueToNextLevel()
    {
        currentLevelIndex++;
        
        // OYUNU DEVAM ETTİR - GÜVENLİK! ✅
        Time.timeScale = 1f;
    
        if (currentLevelIndex >= levels.Length)
        {
            Debug.Log("🎉 TÜM LEVELLER TAMAMLANDI!");
            // Oyun bitti ekranı göster
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
            return;
        }
    
        // Sonraki wave'i başlat (LoadLevel çağırma - sadece wave devam etsin)
        Debug.Log($"🌊 Sonraki wave başlatılıyor... (Level {currentLevelIndex})");
    
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StartCoroutine(spawner.StartNextWaveAfterDelay(2f));
        }
    }
    
    // Upgrade reward ekranını göster
    void ShowUpgradeReward()
    {
        if (RewardScreen.Instance != null)
        {
            LevelData completedLevel = levels[currentLevelIndex];
        
            // Reward tipine göre doğru ekranı göster
            switch (completedLevel.rewardType)
            {
                case RewardScreen.RewardType.Upgrade:
                    RewardScreen.Instance.ShowUpgradeSelection();
                    break;
                
                case RewardScreen.RewardType.Card:
                    RewardScreen.Instance.ShowCardSelection();
                    break;
                
                case RewardScreen.RewardType.Weapon:
                    RewardScreen.Instance.ShowWeaponUpgradeSelection();
                    break;
            }
        }
        else
        {
            Debug.LogWarning("RewardScreen bulunamadı!");
            LoadLevel(currentLevelIndex + 1);
        }
    }
    
    void ClearAllEnemies()
    {
        // Sahnedeki tüm Enemy objelerini bul
        Enemy[] enemies = FindObjectsOfType<Enemy>();
    
        // Her birini yok et
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    
        Debug.Log($"🧹 {enemies.Length} düşman temizlendi!");
        ClearAllProjectiles();
    }
    
    void ClearAllProjectiles()
    {
        int totalCleared = 0;
    
        // Primary projectile'ları temizle
        PrimaryProjectile[] primaryProjectiles = FindObjectsOfType<PrimaryProjectile>();
        foreach (PrimaryProjectile proj in primaryProjectiles)
        {
            Destroy(proj.gameObject);
            totalCleared++;
        }
    
        // Secondary projectile'ları temizle
        SecondaryProjectile[] secondaryProjectiles = FindObjectsOfType<SecondaryProjectile>();
        foreach (SecondaryProjectile proj in secondaryProjectiles)
        {
            Destroy(proj.gameObject);
            totalCleared++;
        }
    
        // Card projectile'ları temizle
        CardProjectile[] cardProjectiles = FindObjectsOfType<CardProjectile>();
        foreach (CardProjectile proj in cardProjectiles)
        {
            Destroy(proj.gameObject);
            totalCleared++;
        }
    
        // Turret projectile'ları temizle
        TurretProjectile[] turretProjectiles = FindObjectsOfType<TurretProjectile>();
        foreach (TurretProjectile proj in turretProjectiles)
        {
            Destroy(proj.gameObject);
            totalCleared++;
        }
    
        // Lightning projectile'ları temizle
        LightningProjectile[] lightningProjectiles = FindObjectsOfType<LightningProjectile>();
        foreach (LightningProjectile proj in lightningProjectiles)
        {
            Destroy(proj.gameObject);
            totalCleared++;
        }
    
        Debug.Log($"🧹 {totalCleared} projectile temizlendi!");
    }
    
    public void StartCountdown()
    {
        Time.timeScale = 0f;
        
        isCountdownActive = true;
        countdownTimer = countdownTime;
    
       Debug.Log("⏱️ Countdown başladı!");
    }
    void StartLevel()
    {
        // Oyunu tekrar başlat!
        Time.timeScale = 1f;
        
        // Countdown UI'ını gizle
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideCountdown();
        }
        
        // SPAWNER'I BAŞLAT - YENİ! ✅
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.BeginSpawning();
            Debug.Log("🌊 Wave sistemi başlatıldı!");
        }
        else
        {
            Debug.LogWarning("⚠️ EnemySpawner bulunamadı!");
        }

        Debug.Log("🎮 Level başladı!");
    }
}