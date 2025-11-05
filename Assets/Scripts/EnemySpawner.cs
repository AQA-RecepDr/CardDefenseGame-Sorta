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
    public bool autoStart = false; // YENÄ°! Inspector'dan ayarlanabilir
    
    void Start()
    {
        Debug.Log("ðŸŽ® EnemySpawner hazÄ±r - Manuel baÅŸlatma bekleniyor...");
    }
    
   // Wave baÅŸlat - SayaÃ§larÄ± sÄ±fÄ±rla!
   public void StartWave()
   {
       Debug.Log($"ðŸŒŠ Wave {currentWave} baÅŸladÄ±!");
    
       // SayaÃ§larÄ± sÄ±fÄ±rla
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
       // WAVE 5 = BOSS WAVE! 
       if (currentWave == 5)
       {
           Debug.Log("👾 === BOSS WAVE! ===" );
           SpawnBoss();
           return; // Normal wave spawn yapma!
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
        
              Debug.Log($"ðŸ“‹ Config'den dÃ¼ÅŸman: {selectedType}");
        
              return selectedType;
          }
          
          // FALLBACK - ESKÄ° SÄ°STEM
          Debug.Log("âš ï¸ Random fallback kullanÄ±lÄ±yor!"); 
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
        Debug.Log("â³ EnemySpawner countdown bekliyor...");
    
        // Countdown sÃ¼resi kadar bekle (MenuManager'dan sonra)
        yield return new WaitForSeconds(4f); // 3s countdown + 1s buffer
    
        Debug.Log("ðŸš€ EnemySpawner otomatik baÅŸlatÄ±lÄ±yor!");
        BeginSpawning();
    }
    
    // Manuel olarak dÄ±ÅŸarÄ±dan Ã§aÄŸrÄ±lacak
    public void BeginSpawning()
    {
        // ZATEN BAÅžLADIYSAK TEKRAR BAÅžLATMA! âœ…
        if (hasStarted)
        {
            Debug.LogWarning("âš ï¸ BeginSpawning zaten Ã§aÄŸrÄ±ldÄ±, atlÄ±yorum!");
            return;
        }
    
        hasStarted = true;
        Debug.Log("ðŸŒŠ Spawn baÅŸlatÄ±ldÄ±!");
        
        // WAVE UI'YI GÃ–STER - YENÄ°! âœ…
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveUI();
        }
        
        StartWave();
    }
    
    // Wave spawn coroutine
    IEnumerator SpawnWave()
    {
        Debug.Log($"ðŸŽ¬ SpawnWave baÅŸladÄ±! Hedef: {enemiesPerWave}");
        while (enemiesSpawned < enemiesPerWave && isSpawning)
        {
            
            // SPAWN Ã–NCESÄ° belirleme
            int nextZone = Random.Range(0, spawnPoints.Length);
            Vector3 nextSpawnPos = GetRandomSpawnPosition(spawnPoints[nextZone].position, nextZone);
        
            // ÅžÄ°MDÄ° SPAWN ET! - SpawnEnemyAt() KULLAN! âœ…
            SpawnEnemyAt(nextSpawnPos, nextZone);
            enemiesSpawned++;
            
            Debug.Log($"âœ… Spawn tamamlandÄ±! Toplam: {enemiesSpawned}/{enemiesPerWave}");
            // Spawn interval bekle
            yield return new WaitForSeconds(spawnInterval);
        }
        
        Debug.Log($"âœ… Wave {currentWave} - TÃ¼m dÃ¼ÅŸmanlar spawn oldu! ({enemiesSpawned}/{enemiesPerWave})");
        isSpawning = false;
    }
    
    // UI'yi gÃ¼ncelle (spawn sayÄ±sÄ±na gÃ¶re)
    void UpdateSpawnUI()
    {
        if (UIManager.Instance != null)
        {
            int totalGone = enemiesKilled + enemiesReachedPlayer;
            int remaining = enemiesPerWave - totalGone;
            if (remaining < 0) remaining = 0;
        
            UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, remaining);
        
            Debug.Log($"ðŸ“Š UI GÃ¼ncellendi - Spawn: {enemiesSpawned}/{enemiesPerWave}, Ã–ldÃ¼: {enemiesKilled}, Kalan: {remaining}");
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
    
   
    
    // DÃ¼ÅŸman Ã¶ldÃ¼rÃ¼ldÃ¼ (Enemy.cs'den Ã§aÄŸrÄ±lacak)
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"ðŸ’€ DÃ¼ÅŸman Ã¶ldÃ¼rÃ¼ldÃ¼! ({enemiesKilled}/{enemiesPerWave})");
        
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
{
    // Bir frame bekle - tÃ¼m Destroy()'ler tamamlansÄ±n âœ…
    StartCoroutine(CheckWaveCompleteDelayed());
}

IEnumerator CheckWaveCompleteDelayed()
{
    // Bir frame bekle
    yield return null;
    
    int totalEnemiesGone = enemiesKilled + enemiesReachedPlayer;
    
    // GerÃ§ek kalan dÃ¼ÅŸman (Destroy'ler tamamlandÄ±)
    Enemy[] aliveEnemies = FindObjectsOfType<Enemy>();
    int remainingInScene = aliveEnemies.Length;
    
    int expectedRemaining = enemiesPerWave - totalEnemiesGone;
    if (expectedRemaining < 0) expectedRemaining = 0;
    
    Debug.Log($"ðŸ“Š ==================== WAVE KONTROL ====================");
    Debug.Log($"  - Spawn: {enemiesSpawned}/{enemiesPerWave} â†’ TamamlandÄ± mÄ±? {enemiesSpawned >= enemiesPerWave}");
    Debug.Log($"  - Ã–ldÃ¼: {enemiesKilled}");
    Debug.Log($"  - UlaÅŸtÄ±: {enemiesReachedPlayer}");
    Debug.Log($"  - Toplam gitti: {totalEnemiesGone}/{enemiesPerWave} â†’ TamamlandÄ± mÄ±? {totalEnemiesGone >= enemiesPerWave}");
    Debug.Log($"  - Sahnede kalan: {remainingInScene} â†’ SÄ±fÄ±r mÄ±? {remainingInScene == 0}");
    Debug.Log($"  - Beklenen kalan: {expectedRemaining}");
    
    bool spawnComplete = enemiesSpawned >= enemiesPerWave;
    bool allGone = totalEnemiesGone >= enemiesPerWave;
    bool sceneEmpty = remainingInScene == 0;
    
    Debug.Log($"  âš–ï¸ ÅžARTLAR:");
    Debug.Log($"     1. Spawn tamamlandÄ±: {spawnComplete}");
    Debug.Log($"     2. Hepsi gitti: {allGone}");
    Debug.Log($"     3. Sahne boÅŸ: {sceneEmpty}");
    Debug.Log($"     â†’ WAVE TamamLANACAK MI? {spawnComplete && allGone && sceneEmpty}");
    Debug.Log($"====================================================");
    
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateEnemyCount(enemiesKilled, enemiesPerWave, expectedRemaining);
    }
    
    if (spawnComplete && allGone && sceneEmpty)
    {
        Debug.Log("âœ… ÃœÃ‡ ÅžART DA SAÄžLANDI - CompleteWave() Ã‡AÄžRILIYOR!");
        CompleteWave();
    }
    else
    {
        Debug.LogWarning("âŒ Åžartlar saÄŸlanmadÄ± - Wave devam ediyor...");
    }
}
    
    // DÃ¼ÅŸman oyuncuya ulaÅŸtÄ±
    public void OnEnemyReachedPlayer()
    {
        enemiesReachedPlayer++;
        Debug.Log($"ðŸ’” DÃ¼ÅŸman oyuncuya ulaÅŸtÄ±! ({enemiesReachedPlayer})");
    
        CheckWaveComplete();
    }
    
    // Wave tamamlandÄ±
    void CompleteWave()
    {
        Debug.Log($"ðŸŽ‰ Wave {currentWave} tamamlandÄ±!");
        Debug.Log($"ðŸ“Š Ä°statistikler - Ã–ldÃ¼rÃ¼len: {enemiesKilled}, UlaÅŸan: {enemiesReachedPlayer}");
        
        currentWave++;
        
        // 1 SANÄ°YE BEKLE, SONRA REWARD GÃ–STER - YENÄ°! âœ…
        StartCoroutine(ShowRewardAfterDelay(1f));
    }
    
    // Bekleme sonrasÄ± reward gÃ¶ster
    IEnumerator ShowRewardAfterDelay(float delay)
    {
        Debug.Log($"â³ {delay} saniye bekleniyor (cleanup)...");
    
        yield return new WaitForSeconds(delay);
    
        Debug.Log("âœ… Bekleme bitti, reward gÃ¶steriliyor!");
    
        // LevelManager'a haber ver
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogWarning("âš ï¸ LevelManager bulunamadÄ±!");
            StartCoroutine(StartNextWaveAfterDelay(2f));
        }
    }
    
    // Bekleme sonrasÄ± wave baÅŸlat
    public IEnumerator StartNextWaveAfterDelay(float delay)
    {
        Debug.Log($"â³ {delay} saniye bekleniyor...");
        Debug.Log($"ðŸ” Time.timeScale: {Time.timeScale}"); // â† KONTROL!
        
        if (Time.timeScale == 0f)
        {
            Debug.LogError("âŒ OYUN DONUK! Time.timeScale = 0");
            Time.timeScale = 1f; // Zorla dÃ¼zelt
        }
        
        yield return new WaitForSeconds(delay);
    
        Debug.Log($"ðŸŒŠ Wave {currentWave} baÅŸlatÄ±lÄ±yor!");
        StartWave();
    }


    
    // Tek bir dÃ¼ÅŸman spawn et
void SpawnEnemyAt(Vector3 spawnPos, int zoneIndex)
{
    Enemy.EnemyType randomType = GetRandomEnemyType();
    
    // MAVÄ° DÃœÅžMAN ÃœÃ‡LÃœ SPAWN - YENÄ°! âœ…
    if (randomType == Enemy.EnemyType.Blue)
    {
        SpawnTripleBlue(spawnPos, zoneIndex);
        return; // ÃœÃ§lÃ¼ spawn yaptÄ±k, bitir
    }
    
    // Normal tek spawn
    GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    Enemy enemy = enemyObj.GetComponent<Enemy>();
    
    if (enemy != null)
    {
        enemy.zoneIndex = zoneIndex;
        enemy.enemyType = randomType;
        enemy.gameObject.name = $"Enemy_{enemiesSpawned}_{randomType}";
        
        Debug.Log($"ðŸ‘¾ DÃ¼ÅŸman spawn! #{enemiesSpawned} Zone:{zoneIndex}, Tip:{randomType}");
    }
    
    UpdateSpawnUI();
}

// ÃœÃ§lÃ¼ mavi dÃ¼ÅŸman spawn et
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
            
            // GRUP BÄ°LGÄ°SÄ° - YENÄ°! âœ…
            enemy.groupID = groupID;
            enemy.groupOffset = offsets[i]; // Ä°lk spawn offset'ini kaydet
            
            if (i == 1) // Orta dÃ¼ÅŸman biraz daha bÃ¼yÃ¼k
            {
                enemy.transform.localScale = Vector3.one * 0.8f; // Biraz bÃ¼yÃ¼k
            }
        }
    }
    
    UpdateSpawnUI();
}

// ÃœÃ§lÃ¼ iÃ§in offset pozisyonlarÄ±
Vector3[] GetTripleOffsets(int zoneIndex)
{
    Vector3[] offsets = new Vector3[3];
    float spacing = 0.7f; // AralarÄ±ndaki mesafe
    
    switch (zoneIndex)
    {
        case 0: // TOP (yukarÄ±dan geliyor)
            // Yan yana (X ekseninde)
            offsets[0] = new Vector3(-spacing, 0, 0); // Sol
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(spacing, 0, 0);   // SaÄŸ
            break;
        
        case 1: // RIGHT (saÄŸdan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // Ãœst
            break;
        
        case 2: // BOTTOM (aÅŸaÄŸÄ±dan geliyor)
            // Yan yana (X ekseninde)
            offsets[0] = new Vector3(-spacing, 0, 0); // Sol
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(spacing, 0, 0);   // SaÄŸ
            break;
        
        case 3: // LEFT (soldan geliyor)
            // Yan yana (Y ekseninde)
            offsets[0] = new Vector3(0, -spacing, 0); // Alt
            offsets[1] = new Vector3(0, 0, 0);         // Orta
            offsets[2] = new Vector3(0, spacing, 0);   // Ãœst
            break;
    }
    
    return offsets;
}

// BOSS SPAWN - YENİ!
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
}