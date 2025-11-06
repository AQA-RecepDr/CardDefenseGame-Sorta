using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    //[Header("Boss Durumu")]
    public enum BossState 
    { 
        Approaching,  // Merkeze yaklaşıyor
        Spawning,     // Minion spawn ediyor (DURMUŞ)
        Retreating,   // Geri çekiliyor
        Cooldown      // Ekran dışında bekliyor
    }
    
    public BossState currentState = BossState.Cooldown;
    
    [Header("Boss Ayarları")]
    public float approachSpeed = 3f;        // İleri hız
    public float retreatSpeed = 5f;         // Geri hız (daha hızlı!)
    public float stopDistance = 9f;         // Merkeze olan minimum mesafe
    public int minionsToSpawn = 8;          // Her gelişte 8 beyaz düşman
    public float minionSpawnInterval = 0.25f; // Minionlar arası süre
    public float cooldownDuration = 3f;     // Ekran dışında bekleme süresi
    
    [Header("Spawn Referansları")]
    public GameObject enemyPrefab;          // Enemy prefab referansı
    private EnemySpawner spawner;
    
    [Header("Pozisyon Bilgileri")]
    private int currentZone = -1;           // Şu anki zone (0-3)
    private int lastZone = -1;              // Son kullanılan zone (tekrar gelmesin)
    private Vector3 targetPosition;         // Hedef pozisyon
    private Vector3 spawnPosition;          // Başlangıç pozisyonu
    private Transform[] spawnPoints;        // Zone spawn noktaları
    
    [Header("Spawn Sayaçları")]
    private int spawnedMinionCount = 0;
    private float spawnTimer = 0f;
    private float cooldownTimer = 0f;
    
    private Enemy enemyComponent;
    private bool isInitialized = false;

    void Start()
    {
        enemyComponent = GetComponent<Enemy>();
        spawner = FindObjectOfType<EnemySpawner>();
        
        if (spawner != null)
        {
            spawnPoints = spawner.spawnPoints;
        }
        
        // İlk state: Cooldown (bekle, sonra başla)
        currentState = BossState.Cooldown;
        cooldownTimer = 2f; // 2 saniye bekle başlamadan önce
        
        Debug.Log("👾 BOSS SPAWN! İlk saldırı için hazırlanıyor...");
        
        // BOSS MÜZİĞİ BAŞLAT! 🎵
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossMusic();
            SoundManager.Instance.PlayBossSpawn();
        }
    }

    void Update()
    {
        if (enemyComponent == null || enemyComponent.isDestroyed) return;
        
        // İlk kez çalışıyorsa zone seç
        if (!isInitialized && currentState == BossState.Cooldown && cooldownTimer <= 0f)
        {
            SelectNewZone();
            isInitialized = true;
        }
        
        // State'e göre davranış
        switch (currentState)
        {
            case BossState.Approaching:
                ApproachBehavior();
                break;
                
            case BossState.Spawning:
                SpawnBehavior();
                break;
                
            case BossState.Retreating:
                RetreatBehavior();
                break;
                
            case BossState.Cooldown:
                CooldownBehavior();
                break;
        }
    }
    
    // DURUM 1: Merkeze Yaklaşma
    void ApproachBehavior()
    {
        // Hedefe doğru hareket
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * approachSpeed * Time.deltaTime;
        
        // Hedefe ulaştık mı?
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance < 0.5f)
        {
            // DUR ve minion spawn'a geç!
            currentState = BossState.Spawning;
            spawnedMinionCount = 0;
            spawnTimer = 0f;
            
            Debug.Log($"👾 BOSS DURDU! Minion spawn başlıyor... (Zone: {currentZone})");
            
            // Görsel: Kırmızıya dön (tehlikeli!)
            if (enemyComponent != null)
            {
                StartCoroutine(PulseColor(Color.red));
            }
        }
    }
    
    // DURUM 2: Minion Spawn (DURMUŞ HALDE)
    void SpawnBehavior()
    {
        spawnTimer += Time.deltaTime;
        
        // Minion spawn zamanı geldi mi?
        if (spawnTimer >= minionSpawnInterval && spawnedMinionCount < minionsToSpawn)
        {
            SpawnMinion();
            spawnedMinionCount++;
            spawnTimer = 0f;
            
            Debug.Log($"🔵 Minion spawn! ({spawnedMinionCount}/{minionsToSpawn})");
        }
        
        // Tüm minionlar spawn oldu mu?
        if (spawnedMinionCount >= minionsToSpawn)
        {
            // Geri çekilmeye başla!
            currentState = BossState.Retreating;
            
            Debug.Log($"👾 BOSS GERİ ÇEKİLİYOR! (Zone: {currentZone})");
            
            // Görsel: Normal renge dön
            if (enemyComponent != null)
            {
                enemyComponent.UpdateVisual();
            }
        }
    }
    
    // DURUM 3: Geri Çekilme
    void RetreatBehavior()
    {
        // Spawn pozisyonuna geri dön (HIZLI!)
        Vector3 direction = (spawnPosition - transform.position).normalized;
        transform.position += direction * retreatSpeed * Time.deltaTime;
        
        // Spawn pozisyonuna ulaştık mı?
        float distance = Vector3.Distance(transform.position, spawnPosition);
        
        if (distance < 1f)
        {
            // Cooldown'a geç
            currentState = BossState.Cooldown;
            cooldownTimer = cooldownDuration;
            
            Debug.Log($"👾 BOSS EKRAN DIŞINDA! {cooldownDuration}s bekliyor...");
        }
    }
    
    // DURUM 4: Bekleme (Ekran Dışında)
    void CooldownBehavior()
    {
        cooldownTimer -= Time.deltaTime;
        
        if (cooldownTimer <= 0f)
        {
            // Yeni zone seç ve tekrar saldır!
            SelectNewZone();
        }
    }
    
    // Yeni zone seç ve saldırıya başla
    void SelectNewZone()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ Spawn points bulunamadı!");
            return;
        }
        
        // Yeni random zone (son zone hariç!)
        int newZone;
        do
        {
            newZone = Random.Range(0, spawnPoints.Length);
        } 
        while (newZone == lastZone && spawnPoints.Length > 1);
        
        currentZone = newZone;
        lastZone = newZone;
        
        // Spawn pozisyonu (ekran dışı)
        spawnPosition = spawnPoints[currentZone].position;
        transform.position = spawnPosition;
        
        // Hedef pozisyon (merkeze yakın ama tam değil)
        Vector3 centerPos = Vector3.zero;
        Vector3 direction = (centerPos - spawnPosition).normalized;
        //targetPosition = centerPos - (direction * stopDistance);
        
        // Zone'a göre farklı stopDistance! (Ekran şekline göre)
        float zoneStopDistance = stopDistance;

        switch (currentZone)
        {
            case 0: // TOP (üst)
                zoneStopDistance = 6.5f; // Daha yakın (ekran dar)
                break;
            case 1: // RIGHT (sağ)
                zoneStopDistance = 10f; // Daha uzak (ekran geniş)
                break;
            case 2: // BOTTOM (alt)
                zoneStopDistance = 6.5f; // Daha yakın (ekran dar)
                break;
            case 3: // LEFT (sol)
                zoneStopDistance = 10f; // Daha uzak (ekran geniş)
                break;
        }

        targetPosition = centerPos - (direction * zoneStopDistance);

        Debug.Log($"👾 Zone {currentZone} - Stop mesafesi: {zoneStopDistance}");
        
        // Saldırıya başla!
        currentState = BossState.Approaching;
        
        Debug.Log($"👾 BOSS YENİ SALDIRI! Zone: {currentZone} → Hedef: {targetPosition}");
        
        // Teleport sesi!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossTeleport();
        }
        
        // Görsel: Parlak glow
        if (enemyComponent != null)
        {
            StartCoroutine(FlashEffect());
        }
    }
    
    // White düşman spawn et
    void SpawnMinion()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ Enemy prefab null!");
            return;
        }
        
        // Boss'un yanında spawn (rastgele offset)
        //Vector3 offset = Random.insideUnitCircle * 1.5f; // 1.5f yarıçapında
        //Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);
        
        // Boss'un bulunduğu zone'a göre spawn pozisyonu belirle
        Vector3 offset = Vector3.zero;

        switch (currentZone)
        {
            case 0: // TOP (üstten geliyor)
                // X ekseninde dağıtık, Y'de boss'tan UZAK
                offset = new Vector3(Random.Range(-4f, 4f), Random.Range(2f, 4f), 0f);
                break;
    
            case 1: // RIGHT (sağdan geliyor)
                // Y ekseninde dağıtık, X'te boss'tan UZAK
                offset = new Vector3(Random.Range(2f, 4f), Random.Range(-4f, 4f), 0f);
                break;
    
            case 2: // BOTTOM (alttan geliyor)
                // X ekseninde dağıtık, Y'de boss'tan UZAK
                offset = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, -2f), 0f);
                break;
    
            case 3: // LEFT (soldan geliyor)
                // Y ekseninde dağıtık, X'te boss'tan UZAK
                offset = new Vector3(Random.Range(-4f, -2f), Random.Range(-4f, 4f), 0f);
                break;
        }

        Vector3 spawnPos = transform.position + offset;
        
        // Minion spawn!
        GameObject minionObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Enemy minion = minionObj.GetComponent<Enemy>();
        
        if (minion != null)
        {
            minion.enemyType = Enemy.EnemyType.White;
            minion.zoneIndex = currentZone;
            minion.gameObject.name = $"BossMinion_{spawnedMinionCount}";
            
            Debug.Log($"🔵 Boss minion spawn! Pos: {spawnPos}, Zone: {currentZone}");
        }
        
        // Minion spawn sesi
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossMinionSpawn();
        }
        
        // Spawn efekti
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.ShowHitEffect(spawnPos, Color.cyan);
        }
        
        // Spawn sesi
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
    }
    
    // Renk pulse efekti
    IEnumerator PulseColor(Color targetColor)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        
        Color originalColor = sr.color;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 2f, 1f);
            sr.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }
        
        sr.color = originalColor;
    }
    
    // Flash efekti (zone değiştirince)
    IEnumerator FlashEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        
        Color originalColor = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;
    }
    
    // Boss öldüğünde
    void OnDestroy()
    {
        Debug.Log("👾 BOSS ÖLDÜ!");
    }
}
