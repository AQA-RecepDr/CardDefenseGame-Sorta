using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager Instance;
  
    [Header("Aktif Upgrade'ler")]
    public bool hasTripleShot = false;
    public bool hasSpreadShot = false;
    public bool hasPierceShot = false;
    public bool hasAutoTarget = false;
    public bool hasRapidFire = false;
    public bool hasPowerShot = false;
    
    [Header("Upgrade Değerleri")]
    public float rapidFireMultiplier = 1.5f; // %50 daha hızlı
    public float powerShotMultiplier = 1.5f; // %50 daha fazla hasar
    public float tripleShotDamageMultiplier = 0.6f;
    public float powerShotFireRateMultiplier = 1.5f; // Power Shot yavaşlatma ← YENİ!
    public int pierceShotMaxTargets = 3; // Maksimum kaç düşman delebilir ← YENİ!
    
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
    
    // Upgrade uygula
    public void ApplyWeaponUpgrade(WeaponUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case WeaponUpgradeType.TripleShot:
                hasTripleShot = true;
                Debug.Log("🔫 Triple Shot aktif!");
                break;
                
            case WeaponUpgradeType.SpreadShot:
                hasSpreadShot = true;
                Debug.Log("💥 Spread Shot aktif!");
                break;
                
            case WeaponUpgradeType.PierceShot:
                hasPierceShot = true;
                Debug.Log("⚔️ Pierce Shot aktif!");
                break;
                
            case WeaponUpgradeType.AutoTarget:
                hasAutoTarget = true;
                Debug.Log("🎯 Auto-Target aktif!");
                break;
                
            case WeaponUpgradeType.RapidFire:
                hasRapidFire = true;
                Debug.Log("⚡ Rapid Fire aktif!");
                break;
                
            case WeaponUpgradeType.PowerShot:
                hasPowerShot = true;
                Debug.Log("💪 Power Shot aktif!");
                break;
        }
    }
    
    // Upgrade tipleri
    public enum WeaponUpgradeType
    {
        TripleShot,
        SpreadShot,
        PierceShot,
        AutoTarget,
        RapidFire,
        PowerShot
    }
}