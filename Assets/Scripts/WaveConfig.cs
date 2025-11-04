using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Header("Düşman Tipi")]
    public Enemy.EnemyType enemyType;
    
    [Header("Spawn Modu")]
    public bool usePercentage = true; // true = yüzde, false = tam sayı
    
    [Header("Yüzde Modu (usePercentage = true)")]
    [Range(0f, 100f)]
    public float spawnPercentage = 0f; // %0-100
    
    [Header("Manuel Mod (usePercentage = false)")]
    public int exactCount = 0; // Tam sayı
}

[System.Serializable]
public class WaveConfig
{
    [Header("Wave Bilgisi")]
    public int waveNumber = 1;
    
    [Header("Düşman Sayısı")]
    public int totalEnemies = 30; // Toplam düşman
    
    [Header("Spawn Hızı")]
    public float spawnInterval = 2f; // Spawn aralığı (saniye)
    
    [Header("Düşman Dağılımı")]
    public EnemySpawnConfig[] enemyConfigs;
    
    private List<Enemy.EnemyType> spawnList = new List<Enemy.EnemyType>();
    private bool isListGenerated = false;
    
    // Wave başında listeyi oluştur ve karıştır
    public void GenerateSpawnList()
    {
        spawnList.Clear();
        isListGenerated = true;
        
        if (usePercentageMode)
        {
            // YÜZDE MODUNDA: Random seçim yapacağız (şu anki gibi)
            Debug.Log($"📊 Yüzde modu - Random spawn kullanılacak");
        }
        else
        {
            // MANUEL MODDA: Liste oluştur ve karıştır!
            Debug.Log($"📋 Manuel mod - Spawn listesi oluşturuluyor...");
            
            foreach (var config in enemyConfigs)
            {
                // Her düşman tipinden exactCount kadar ekle
                for (int i = 0; i < config.exactCount; i++)
                {
                    spawnList.Add(config.enemyType);
                }
            }
            
            // LİSTEYİ KARIŞTIR! ✅
            ShuffleList(spawnList);
            
            Debug.Log($"✅ Spawn listesi oluşturuldu ve karıştırıldı! Toplam: {spawnList.Count}");
        }
    }
    
    // Listeyi karıştır (Fisher-Yates shuffle)
    void ShuffleList(List<Enemy.EnemyType> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            
            // Swap
            Enemy.EnemyType temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    // Düşman tipini seç (yüzde veya manuel moda göre)
    public Enemy.EnemyType GetRandomEnemyType(int currentSpawnIndex)
    {
        // Liste henüz oluşturulmadıysa oluştur
        if (!isListGenerated)
        {
            GenerateSpawnList();
        }
        
        if (usePercentageMode)
        {
            return GetEnemyByPercentage();
        }
        else
        {
            // MANUEL MOD: Listeden sırayla al
            if (currentSpawnIndex < spawnList.Count)
            {
                Enemy.EnemyType selectedType = spawnList[currentSpawnIndex];
                Debug.Log($"📋 Manuel spawn #{currentSpawnIndex}: {selectedType}");
                return selectedType;
            }
            
            // Fallback
            Debug.LogWarning($"⚠️ Spawn index {currentSpawnIndex} liste dışında!");
            return Enemy.EnemyType.White;
        }
    }
    
    // Yüzde modunda mı?
    bool usePercentageMode
    {
        get
        {
            if (enemyConfigs == null || enemyConfigs.Length == 0)
                return true;
            
            return enemyConfigs[0].usePercentage;
        }
    }
    
    // Yüzde modunda düşman seç
    Enemy.EnemyType GetEnemyByPercentage()
    {
        float random = Random.Range(0f, 100f);
        float cumulative = 0f;
        
        foreach (var config in enemyConfigs)
        {
            cumulative += config.spawnPercentage;
            
            if (random <= cumulative)
            {
                return config.enemyType;
            }
        }
        
        // Fallback
        return Enemy.EnemyType.White;
    }
    
    // Manuel modda düşman seç
    Enemy.EnemyType GetEnemyByExactCount(int spawnIndex)
    {
        int currentIndex = 0;
        
        foreach (var config in enemyConfigs)
        {
            currentIndex += config.exactCount;
            
            if (spawnIndex < currentIndex)
            {
                return config.enemyType;
            }
        }
        
        // Fallback
        return Enemy.EnemyType.White;
    }
}