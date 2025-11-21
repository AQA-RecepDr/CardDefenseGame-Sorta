using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    public WaveConfig[] waveConfigs; // Inspector'dan ayarlanacak!
    public bool useWaveConfigs = true; // Config kullan mÄ±?
    
    [Header("DÃ¼ÅŸman AyarlarÄ±")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints; // 4 spawn noktasÄ± (Top, Right, Bottom, Left)
    
    [Header("Wave Sistemi")]
    public int currentWave = 1;
    public int enemiesPerWave = 50; // Her wave'de kaÃ§ dÃ¼ÅŸman
    public int enemiesSpawned = 0; // KaÃ§ dÃ¼ÅŸman spawn oldu
    public int enemiesKilled = 0; // KaÃ§ dÃ¼ÅŸman Ã¶ldÃ¼rÃ¼ldÃ¼
    public int enemiesReachedPlayer = 0; //Oyuncuya ulaÅŸan dÃ¼ÅŸmanlar
    public float spawnInterval = 2f; // DÃ¼ÅŸmanlar arasÄ± sÃ¼re
    
    [Header("Spawn KontrolÃ¼")]
    public bool isSpawning = false;
    public bool hasStarted = false;
    public bool autoStart = false; // Inspector'dan ayarlanabilir
    
    [Header("Test Ayarları")]
    public bool testMode = false; // ← Inspector'dan aç/kapat!
    public int testWaveNumber = 5; // ← Hangi wave'de başlasın
    
    void Start()
    {
        // TEST MODU - YENİ! 🧪
        if (testMode)
        {
            currentWave = testWaveNumber;
            Debug.Log($"🧪 TEST MODE: Wave {currentWave} başlatılıyor!");
        }
        
        Debug.Log(" EnemySpawner hazır - Manuel başlatma bekleniyor...");
    }
    
   // Wave baÅŸlat - SayaÃ§larÄ± sÄ±fÄ±rla!
   public void StartWave()
   {
       
       enemiesSpawned = 0;
       enemiesKilled = 0;
       enemiesReachedPlayer = 0;
    
       // WAVE CONFIG VARSA ONU KULLAN - YENÄ°! âœ…
       if (useWaveConfigs && waveConfigs != null && currentWave - 1 < waveConfigs.Length)
       {
           WaveConfig config = waveConfigs[currentWave - 1];
           enemiesPerWave = config.totalEnemies;
           spawnInterval = config.spawnInterval;
           
           // SPAWN LÄ°STESÄ°NÄ° OLUÅžTUR VE KARIÅžTIR - YENÄ°! âœ…
           config.GenerateSpawnList();
        
           Debug.Log($"ðŸ“‹ Wave config kullanÄ±lÄ±yor: {enemiesPerWave} dÃ¼ÅŸman, {spawnInterval}s interval");
       }
       else
       {
           Debug.Log($"âš ï¸ Wave config yok, fallback kullanÄ±lÄ±yor: {enemiesPerWave} dÃ¼ÅŸman");
       }
       // Normal wave'lerde normal müzik
       if (currentWave < 5 && SoundManager.Instance != null)
       {
           SoundManager.Instance.PlayNormalMusic();
       }
       
       // WAVE 5 = BOSS WAVE! 
       if (currentWave == 5)
       {
           Debug.Log("👾 === BOSS WAVE! ===" );
           SpawnBoss();
           return; // Normal wave spawn yapma!
       }
       
       // WAVE 10 = FINAL BOSS WAVE! ✅ BUNU EKLE
       if (currentWave == 10)
       {
           Debug.Log("💀 === FINAL BOSS WAVE! ===" );
           SpawnFinalBoss();
           return;
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
   
   // Rastgele dÃ¼ÅŸman tipi seÃ§
      Enemy.EnemyType GetRandomEnemyType()
      {
          // WAVE CONFIG VARSA ONU KULLAN - YENÄ°! âœ…
          if (useWaveConfigs && waveConfigs != null && currentWave - 1 < waveConfigs.Length)
          {
              WaveConfig config = waveConfigs[currentWave - 1];
              Enemy.EnemyType selectedType = config.GetRandomEnemyType(enemiesSpawned);
        
              return selectedType;
          }
          
          // FALLBACK - ESKÄ° SÄ°STEM
          Debug.Log("Random fallback!"); 
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
      
    // Otomatik baÅŸlatma (countdown iÃ§in bekleme)
    IEnumerator AutoStartWithDelay()
    {
        Debug.Log("EnemySpawner countdown bekliyor...");
    
        // Countdown sÃ¼resi kadar bekle (MenuManager'dan sonra)
        yield return new WaitForSeconds(4f); // 3s countdown + 1s buffer
        
        BeginSpawning();
    }
    
    // Manuel olarak dÄ±ÅŸarÄ±dan Ã§aÄŸrÄ±lacak
    public void BeginSpawning()
    {
        if (hasStarted)
        {
            return;
        }
    
        hasStarted = true;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveUI();
        }
        
        StartWave();
    }
    
    // Wave spawn coroutine
    IEnumerator SpawnWave()
    {
        Debug.Log($"SpawnWave başladı Hedef: {enemiesPerWave}");
        while (enemiesSpawned < enemiesPerWave && isSpawning)
        {
            
            // SPAWN belirleme
            int nextZone = Random.Range(0, spawnPoints.Length);
            Vector3 nextSpawnPos = GetRandomSpawnPosition(spawnPoints[nextZone].position, nextZone);
        
            // SPAWN ET! - SpawnEnemyAt() KULLAN!
            SpawnEnemyAt(nextSpawnPos, nextZone);
            enemiesSpawned++;
            
            Debug.Log($"Spawn tamamlandı! Toplam: {enemiesSpawned}/{enemiesPerWave}");
            // Spawn interval bekle
            yield return new WaitForSeconds(spawnInterval);
        }
        
        isSpawning = false;
    }
    
    // UI'yi gücelle (spawn sayısına göre)
    void UpdateSpawnUI()
    {
        if (UIManager.Instance != null)
        {
            int totalGone = enemiesKilled + enemiesReachedPlayer;
            int remaining = enemiesPerWave - totalGone;
            if (remaining < 0) remaining = 0;
        
            UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, remaining);
        }
    }

// Rastgele spawn pozisyonu hesapla
    Vector3 GetRandomSpawnPosition(Vector3 basePosition, int zoneIndex)
    {
        float randomOffset = Random.Range(2f, 6f); // Rastgele offset miktarÄ±
    
        Vector3 offset = Vector3.zero;
    
        switch (zoneIndex)
        {
            case 0: // TOP (Ã¼st)
                // X ekseninde rastgele
                offset = new Vector3(Random.Range(-randomOffset, randomOffset), 0, 0);
                break;
            
            case 1: // RIGHT (saÄŸ)
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
    
   
    
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
{
    // Bir frame bekle
    StartCoroutine(CheckWaveCompleteDelayed());
}

IEnumerator CheckWaveCompleteDelayed()
{
    // Bir frame bekle
    yield return null;
    
    int totalEnemiesGone = enemiesKilled + enemiesReachedPlayer;
    
    Enemy[] aliveEnemies = FindObjectsOfType<Enemy>();
    int remainingInScene = aliveEnemies.Length;
    
    int expectedRemaining = enemiesPerWave - totalEnemiesGone;
    if (expectedRemaining < 0) expectedRemaining = 0;
    
    bool spawnComplete = enemiesSpawned >= enemiesPerWave;
    bool allGone = totalEnemiesGone >= enemiesPerWave;
    bool sceneEmpty = remainingInScene == 0;
    
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, expectedRemaining);
    }
    
    if (spawnComplete && allGone && sceneEmpty)
    {
        CompleteWave();
    }
    }
    
    
    public void OnEnemyReachedPlayer()
    {
        enemiesReachedPlayer++;
        CheckWaveComplete();
    }
    
    // Wave tamamlandÄ±
    void CompleteWave()
    {
        currentWave++;
        
        StartCoroutine(ShowRewardAfterDelay(1f));
    }
    
    // Bekleme sonrasÄ± reward gÃ¶ster
    IEnumerator ShowRewardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        // LevelManager'a haber ver
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
        else
        {
            StartCoroutine(StartNextWaveAfterDelay(2f));
        }
    }
    
    // Bekleme sonrasÄ± wave baÅŸlat
    public IEnumerator StartNextWaveAfterDelay(float delay)
    {
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f; // Zorla dÃ¼zelt
        }
        
        yield return new WaitForSeconds(delay);
        
        StartWave();
    }


    
    // Tek bir düşman spawn et
void SpawnEnemyAt(Vector3 spawnPos, int zoneIndex)
{
    Enemy.EnemyType randomType = GetRandomEnemyType();
    
    if (randomType == Enemy.EnemyType.Blue)
    {
        SpawnTripleBlue(spawnPos, zoneIndex);
        return;
    }
    
    // Normal tek spawn
    GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    Enemy enemy = enemyObj.GetComponent<Enemy>();
    
    if (enemy != null)
    {
        enemy.zoneIndex = zoneIndex;
        enemy.enemyType = randomType;
        enemy.gameObject.name = $"Enemy_{enemiesSpawned}_{randomType}";
        
        Debug.Log($"Düşman spawn! #{enemiesSpawned} Zone:{zoneIndex}, Tip:{randomType}");
    }
    
    UpdateSpawnUI();
}

void SpawnTripleBlue(Vector3 centerPos, int zoneIndex)
{
    // 3 dÃ¼ÅŸman yan yana spawn olacak
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
            
            
            enemy.groupID = groupID;
            enemy.groupOffset = offsets[i]; 
            
            if (i == 1) 
            {
                enemy.transform.localScale = Vector3.one * 0.8f; 
            }
        }
    }
    
    UpdateSpawnUI();
}


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
        
        case 1: // RIGHT (saÄŸdan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // üst
            break;
        
        case 2: // BOTTOM (aÅŸaÄŸÄ±dan geliyor)
            // Yan yana (X ekseninde)
            offsets[0] = new Vector3(-spacing, 0, 0); // Sol
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(spacing, 0, 0);   // Sağ
            break;
        
        case 3: // LEFT (soldan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // üst
            break;
    }
    
    return offsets;
}

// BOSS SPAWN
void SpawnBoss()
{
    Debug.Log("👾 BOSS SPAWN BAŞLIYOR!");
    
    // Boss için özel ayarlar
    enemiesPerWave = 1; // Sadece boss
    enemiesSpawned = 1; // Boss spawn oldu sayıyoruz
    
    // Random zone seç (boss buradan başlayacak)
    int bossZone = Random.Range(0, spawnPoints.Length);
    Vector3 bossSpawnPos = spawnPoints[bossZone].position;
    
    // Boss spawn!
    GameObject bossObj = Instantiate(enemyPrefab, bossSpawnPos, Quaternion.identity);
    Enemy boss = bossObj.GetComponent<Enemy>();
    
    if (boss != null)
    {
        boss.enemyType = Enemy.EnemyType.Boss;
        boss.zoneIndex = bossZone;
        boss.gameObject.name = "BOSS_TheSummoner";
        
        Debug.Log($"👾 BOSS SPAWNED! Zone: {bossZone}, Pos: {bossSpawnPos}");
    }
    
    // UI güncelle
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateWaveNumber(currentWave);
        UIManager.Instance.UpdateEnemyCount(0, 1, 1); // 1 boss
    }
    
    Debug.Log("👾 BOSS WAVE BAŞLADI! Oyuncu hazır olsun!");
}

public void SpawnSpecificEnemy(Enemy.EnemyType type)
{
    // Random spawn point (4 köşeden biri)
    int randomZone = Random.Range(0, spawnPoints.Length);
    Vector3 spawnPos = GetRandomSpawnPosition(spawnPoints[randomZone].position, randomZone);
    
    GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    Enemy enemy = enemyObj.GetComponent<Enemy>();
    
    if (enemy != null)
    {
        enemy.enemyType = type;
        enemy.zoneIndex = randomZone;
        enemy.SetupEnemyType(); // ✅ Doğru method ismi
        enemy.gameObject.name = $"BossMinion_{type}";
    }
}

// FINAL BOSS SPAWN (Level 10)
void SpawnFinalBoss()
{
    Debug.Log("💀 FINAL BOSS SPAWN BAŞLIYOR!");
    
    // Final Boss için özel ayarlar
    enemiesPerWave = 1;
    enemiesSpawned = 1;
    
    // Merkez pozisyon
    int bossZone = Random.Range(0, spawnPoints.Length);
    Vector3 finalBossSpawnPos = spawnPoints[bossZone].position;
    
    // Final Boss spawn!
    GameObject finalBossObj = Instantiate(enemyPrefab, finalBossSpawnPos, Quaternion.identity);
    Enemy finalBoss = finalBossObj.GetComponent<Enemy>();
    
    if (finalBoss != null)
    {
        finalBoss.enemyType = Enemy.EnemyType.FinalBoss;
        finalBoss.zoneIndex = 0;
        finalBoss.gameObject.name = "FINALBOSS_TheDestroyer";
        
        Debug.Log($"💀 FINAL BOSS SPAWNED! Pos: {finalBossSpawnPos}");
    }
    
    // UI güncelle
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateWaveNumber(currentWave);
        UIManager.Instance.UpdateEnemyCount(0, 1, 1);
    }
    
    Debug.Log("💀 FINAL BOSS WAVE BAŞLADI!");
}
}