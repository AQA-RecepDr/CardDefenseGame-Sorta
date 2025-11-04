using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    public WaveConfig[] waveConfigs; // Inspector'dan ayarlanacak!
    public bool useWaveConfigs = true; // Config kullan mı?
    
    [Header("Düşman Ayarları")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints; // 4 spawn noktası (Top, Right, Bottom, Left)
    
    [Header("Wave Sistemi")]
    public int currentWave = 1;
    public int enemiesPerWave = 50; // Her wave'de kaç düşman
    public int enemiesSpawned = 0; // Kaç düşman spawn oldu
    public int enemiesKilled = 0; // Kaç düşman öldürüldü
    public int enemiesReachedPlayer = 0; //Oyuncuya ulaşan düşmanlar
    public float spawnInterval = 2f; // Düşmanlar arası süre
    
    [Header("Spawn Kontrolü")]
    public bool isSpawning = false;
    public bool hasStarted = false;
    public bool autoStart = false; // YENİ! Inspector'dan ayarlanabilir
    
    void Start()
    {
        Debug.Log("🎮 EnemySpawner hazır - Manuel başlatma bekleniyor...");
    }
    
   // Wave başlat - Sayaçları sıfırla!
   public void StartWave()
   {
       Debug.Log($"🌊 Wave {currentWave} başladı!");
    
       // Sayaçları sıfırla
       enemiesSpawned = 0;
       enemiesKilled = 0;
       enemiesReachedPlayer = 0;
    
       // WAVE CONFIG VARSA ONU KULLAN - YENİ! ✅
       if (useWaveConfigs && waveConfigs != null && currentWave - 1 < waveConfigs.Length)
       {
           WaveConfig config = waveConfigs[currentWave - 1];
           enemiesPerWave = config.totalEnemies;
           spawnInterval = config.spawnInterval;
           
           // SPAWN LİSTESİNİ OLUŞTUR VE KARIŞTIR - YENİ! ✅
           config.GenerateSpawnList();
        
           Debug.Log($"📋 Wave config kullanılıyor: {enemiesPerWave} düşman, {spawnInterval}s interval");
       }
       else
       {
           Debug.Log($"⚠️ Wave config yok, fallback kullanılıyor: {enemiesPerWave} düşman");
       }
    
       isSpawning = true;
    
       if (UIManager.Instance != null)
       {
           if (UIManager.Instance.waveText != null)
               UIManager.Instance.waveText.gameObject.SetActive(true);
        
           if (UIManager.Instance.enemyCountText != null)
               UIManager.Instance.enemyCountText.gameObject.SetActive(true);
        
           UIManager.Instance.UpdateWaveNumber(currentWave);
           UIManager.Instance.UpdateEnemyCount(0, enemiesPerWave, enemiesPerWave);
       }
    
       StartCoroutine(SpawnWave());
   }
   
   // Rastgele düşman tipi seç
      Enemy.EnemyType GetRandomEnemyType()
      {
          // WAVE CONFIG VARSA ONU KULLAN - YENİ! ✅
          if (useWaveConfigs && waveConfigs != null && currentWave - 1 < waveConfigs.Length)
          {
              WaveConfig config = waveConfigs[currentWave - 1];
              Enemy.EnemyType selectedType = config.GetRandomEnemyType(enemiesSpawned);
        
              Debug.Log($"📋 Config'den düşman: {selectedType}");
        
              return selectedType;
          }
          
          // FALLBACK - ESKİ SİSTEM
          Debug.Log("⚠️ Random fallback kullanılıyor!"); 
          int random = Random.Range(0, 100);
      
         if (random < 30)
              return Enemy.EnemyType.White;
          else if (random < 55)
              return Enemy.EnemyType.Black;
          else if (random < 75)
              return Enemy.EnemyType.Yellow;
          else if (random < 90)
              return Enemy.EnemyType.Orange;
          else if (random < 95)
              return Enemy.EnemyType.Blue;
          else
              return Enemy.EnemyType.Red;
      }     
      
    // Otomatik başlatma (countdown için bekleme)
    IEnumerator AutoStartWithDelay()
    {
        Debug.Log("⏳ EnemySpawner countdown bekliyor...");
    
        // Countdown süresi kadar bekle (MenuManager'dan sonra)
        yield return new WaitForSeconds(4f); // 3s countdown + 1s buffer
    
        Debug.Log("🚀 EnemySpawner otomatik başlatılıyor!");
        BeginSpawning();
    }
    
    // Manuel olarak dışarıdan çağrılacak
    public void BeginSpawning()
    {
        // ZATEN BAŞLADIYSAK TEKRAR BAŞLATMA! ✅
        if (hasStarted)
        {
            Debug.LogWarning("⚠️ BeginSpawning zaten çağrıldı, atlıyorum!");
            return;
        }
    
        hasStarted = true;
        Debug.Log("🌊 Spawn başlatıldı!");
        
        // WAVE UI'YI GÖSTER - YENİ! ✅
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveUI();
        }
        
        StartWave();
    }
    
    // Wave spawn coroutine
    IEnumerator SpawnWave()
    {
        Debug.Log($"🎬 SpawnWave başladı! Hedef: {enemiesPerWave}");
        while (enemiesSpawned < enemiesPerWave && isSpawning)
        {
            
            // SPAWN ÖNCESİ belirleme
            int nextZone = Random.Range(0, spawnPoints.Length);
            Vector3 nextSpawnPos = GetRandomSpawnPosition(spawnPoints[nextZone].position, nextZone);
        
            // ŞİMDİ SPAWN ET! - SpawnEnemyAt() KULLAN! ✅
            SpawnEnemyAt(nextSpawnPos, nextZone);
            enemiesSpawned++;
            
            Debug.Log($"✅ Spawn tamamlandı! Toplam: {enemiesSpawned}/{enemiesPerWave}");
            // Spawn interval bekle
            yield return new WaitForSeconds(spawnInterval);
        }
        
        Debug.Log($"✅ Wave {currentWave} - Tüm düşmanlar spawn oldu! ({enemiesSpawned}/{enemiesPerWave})");
        isSpawning = false;
    }
    
    // UI'yi güncelle (spawn sayısına göre)
    void UpdateSpawnUI()
    {
        if (UIManager.Instance != null)
        {
            int totalGone = enemiesKilled + enemiesReachedPlayer;
            int remaining = enemiesPerWave - totalGone;
            if (remaining < 0) remaining = 0;
        
            UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, remaining);
        
            Debug.Log($"📊 UI Güncellendi - Spawn: {enemiesSpawned}/{enemiesPerWave}, Öldü: {enemiesKilled}, Kalan: {remaining}");
        }
    }

// Rastgele spawn pozisyonu hesapla
    Vector3 GetRandomSpawnPosition(Vector3 basePosition, int zoneIndex)
    {
        float randomOffset = Random.Range(2f, 6f); // Rastgele offset miktarı
    
        Vector3 offset = Vector3.zero;
    
        switch (zoneIndex)
        {
            case 0: // TOP (üst)
                // X ekseninde rastgele
                offset = new Vector3(Random.Range(-randomOffset, randomOffset), 0, 0);
                break;
            
            case 1: // RIGHT (sağ)
                // Y ekseninde rastgele
                offset = new Vector3(0, Random.Range(-randomOffset, randomOffset), 0);
                break;
            
            case 2: // BOTTOM (alt)
                // X ekseninde rastgele
                offset = new Vector3(Random.Range(-randomOffset, randomOffset), 0, 0);
                break;
            
            case 3: // LEFT (sol)
                // Y ekseninde rastgele
                offset = new Vector3(0, Random.Range(-randomOffset, randomOffset), 0);
                break;
        }
    
        return basePosition + offset;
    }
    
   
    
    // Düşman öldürüldü (Enemy.cs'den çağrılacak)
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"💀 Düşman öldürüldü! ({enemiesKilled}/{enemiesPerWave})");
        
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
{
    // Bir frame bekle - tüm Destroy()'ler tamamlansın ✅
    StartCoroutine(CheckWaveCompleteDelayed());
}

IEnumerator CheckWaveCompleteDelayed()
{
    // Bir frame bekle
    yield return null;
    
    int totalEnemiesGone = enemiesKilled + enemiesReachedPlayer;
    
    // Gerçek kalan düşman (Destroy'ler tamamlandı)
    Enemy[] aliveEnemies = FindObjectsOfType<Enemy>();
    int remainingInScene = aliveEnemies.Length;
    
    int expectedRemaining = enemiesPerWave - totalEnemiesGone;
    if (expectedRemaining < 0) expectedRemaining = 0;
    
    Debug.Log($"📊 ==================== WAVE KONTROL ====================");
    Debug.Log($"  - Spawn: {enemiesSpawned}/{enemiesPerWave} → Tamamlandı mı? {enemiesSpawned >= enemiesPerWave}");
    Debug.Log($"  - Öldü: {enemiesKilled}");
    Debug.Log($"  - Ulaştı: {enemiesReachedPlayer}");
    Debug.Log($"  - Toplam gitti: {totalEnemiesGone}/{enemiesPerWave} → Tamamlandı mı? {totalEnemiesGone >= enemiesPerWave}");
    Debug.Log($"  - Sahnede kalan: {remainingInScene} → Sıfır mı? {remainingInScene == 0}");
    Debug.Log($"  - Beklenen kalan: {expectedRemaining}");
    
    bool spawnComplete = enemiesSpawned >= enemiesPerWave;
    bool allGone = totalEnemiesGone >= enemiesPerWave;
    bool sceneEmpty = remainingInScene == 0;
    
    Debug.Log($"  ⚖️ ŞARTLAR:");
    Debug.Log($"     1. Spawn tamamlandı: {spawnComplete}");
    Debug.Log($"     2. Hepsi gitti: {allGone}");
    Debug.Log($"     3. Sahne boş: {sceneEmpty}");
    Debug.Log($"     → WAVE TamamLANACAK MI? {spawnComplete && allGone && sceneEmpty}");
    Debug.Log($"====================================================");
    
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, expectedRemaining);
    }
    
    if (spawnComplete && allGone && sceneEmpty)
    {
        Debug.Log("✅ ÜÇ ŞART DA SAĞLANDI - CompleteWave() ÇAĞRILIYOR!");
        CompleteWave();
    }
    else
    {
        Debug.LogWarning("❌ Şartlar sağlanmadı - Wave devam ediyor...");
    }
}
    
    // Düşman oyuncuya ulaştı
    public void OnEnemyReachedPlayer()
    {
        enemiesReachedPlayer++;
        Debug.Log($"💔 Düşman oyuncuya ulaştı! ({enemiesReachedPlayer})");
    
        CheckWaveComplete();
    }
    
    // Wave tamamlandı
    void CompleteWave()
    {
        Debug.Log($"🎉 Wave {currentWave} tamamlandı!");
        Debug.Log($"📊 İstatistikler - Öldürülen: {enemiesKilled}, Ulaşan: {enemiesReachedPlayer}");
        
        currentWave++;
        
        // 1 SANİYE BEKLE, SONRA REWARD GÖSTER - YENİ! ✅
        StartCoroutine(ShowRewardAfterDelay(1f));
    }
    
    // Bekleme sonrası reward göster
    IEnumerator ShowRewardAfterDelay(float delay)
    {
        Debug.Log($"⏳ {delay} saniye bekleniyor (cleanup)...");
    
        yield return new WaitForSeconds(delay);
    
        Debug.Log("✅ Bekleme bitti, reward gösteriliyor!");
    
        // LevelManager'a haber ver
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogWarning("⚠️ LevelManager bulunamadı!");
            StartCoroutine(StartNextWaveAfterDelay(2f));
        }
    }
    
    // Bekleme sonrası wave başlat
    public IEnumerator StartNextWaveAfterDelay(float delay)
    {
        Debug.Log($"⏳ {delay} saniye bekleniyor...");
        Debug.Log($"🔍 Time.timeScale: {Time.timeScale}"); // ← KONTROL!
        
        if (Time.timeScale == 0f)
        {
            Debug.LogError("❌ OYUN DONUK! Time.timeScale = 0");
            Time.timeScale = 1f; // Zorla düzelt
        }
        
        yield return new WaitForSeconds(delay);
    
        Debug.Log($"🌊 Wave {currentWave} başlatılıyor!");
        StartWave();
    }


    
    // Tek bir düşman spawn et
void SpawnEnemyAt(Vector3 spawnPos, int zoneIndex)
{
    Enemy.EnemyType randomType = GetRandomEnemyType();
    
    // MAVİ DÜŞMAN ÜÇLÜ SPAWN - YENİ! ✅
    if (randomType == Enemy.EnemyType.Blue)
    {
        SpawnTripleBlue(spawnPos, zoneIndex);
        return; // Üçlü spawn yaptık, bitir
    }
    
    // Normal tek spawn
    GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    Enemy enemy = enemyObj.GetComponent<Enemy>();
    
    if (enemy != null)
    {
        enemy.zoneIndex = zoneIndex;
        enemy.enemyType = randomType;
        enemy.gameObject.name = $"Enemy_{enemiesSpawned}_{randomType}";
        
        Debug.Log($"👾 Düşman spawn! #{enemiesSpawned} Zone:{zoneIndex}, Tip:{randomType}");
    }
    
    UpdateSpawnUI();
}

// Üçlü mavi düşman spawn et
void SpawnTripleBlue(Vector3 centerPos, int zoneIndex)
{
    // 3 düşman yan yana spawn olacak
    Vector3[] offsets = GetTripleOffsets(zoneIndex);
    
    int groupID = Random.Range(1000, 9999);
    
    for (int i = 0; i < 3; i++)
    {
        Vector3 spawnPos = centerPos + offsets[i];
        
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.zoneIndex = zoneIndex;
            enemy.enemyType = Enemy.EnemyType.Blue;
            enemy.gameObject.name = $"BlueEnemy_{i}_{enemiesSpawned}";
            
            // GRUP BİLGİSİ - YENİ! ✅
            enemy.groupID = groupID;
            enemy.groupOffset = offsets[i]; // İlk spawn offset'ini kaydet
            
            if (i == 1) // Orta düşman biraz daha büyük
            {
                enemy.transform.localScale = Vector3.one * 0.8f; // Biraz büyük
            }
        }
    }
    
    UpdateSpawnUI();
}

// Üçlü için offset pozisyonları
Vector3[] GetTripleOffsets(int zoneIndex)
{
    Vector3[] offsets = new Vector3[3];
    float spacing = 0.7f; // Aralarındaki mesafe
    
    switch (zoneIndex)
    {
        case 0: // TOP (yukarıdan geliyor)
            // Yan yana (X ekseninde)
            offsets[0] = new Vector3(-spacing, 0, 0); // Sol
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(spacing, 0, 0);   // Sağ
            break;
        
        case 1: // RIGHT (sağdan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // Üst
            break;
        
        case 2: // BOTTOM (aşağıdan geliyor)
            // Yan yana (X ekseninde)
            offsets[0] = new Vector3(-spacing, 0, 0); // Sol
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(spacing, 0, 0);   // Sağ
            break;
        
        case 3: // LEFT (soldan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // Üst
            break;
    }
    
    return offsets;
}
}