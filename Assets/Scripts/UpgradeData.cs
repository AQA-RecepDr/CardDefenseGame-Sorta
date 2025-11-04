using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    [Header("Upgrade Bilgileri")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    public UpgradeType type;
    
    [Header("Değerler")]
    public float value; // Artış miktarı
    
    public enum UpgradeType
    {
        IncreaseDamage,        // Hasar artışı
        DecreaseUltiCooldown,  // Ulti cooldown azaltma
        IncreaseTurretDamage,  // Kırmızı turret güçlendirme
        IncreaseMaxHealth,     // Maksimum can artışı
        IncreaseFireRate       // Ateş hızı artışı
    }
}