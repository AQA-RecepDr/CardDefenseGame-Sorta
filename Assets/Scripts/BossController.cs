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
        
        CreateBossShape();
        
        // SIMPLE BOSS HEALTH BAR GÖSTER! 💜
        if (SimpleBossHealthBar.Instance != null && enemyComponent != null)
        {
            SimpleBossHealthBar.Instance.ResetBar(enemyComponent.maxHealth);
        }
    }

    void Update()
    {
        if (enemyComponent == null || enemyComponent.isDestroyed) return;
        
        // SIMPLE HEALTH BAR GÜNCELLE! 💜
        if (SimpleBossHealthBar.Instance != null)
        {
            SimpleBossHealthBar.Instance.UpdateHealth(enemyComponent.currentHealth, enemyComponent.maxHealth);
        }
        
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
    
    /// <summary>
    /// Boss için özel geometrik şekil sistemi
    /// </summary>
    void CreateBossShape()
    {
        // SpriteRenderer'ı gizle (shape kullanacağız)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }
        
        // 1) SHAPE RENDERER - YILDIZ! ⭐
        EnemyShapeRenderer shapeRenderer = gameObject.AddComponent<EnemyShapeRenderer>();
        shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Star;
        shapeRenderer.size = 1.0f; // ÇOK BÜYÜK! (Boss!)
        shapeRenderer.shapeColor = new Color(0.9f, 0.3f, 1f); //Mor
        shapeRenderer.pulseSpeed = 2.5f;
        shapeRenderer.glowIntensity = 1.5f; 
        shapeRenderer.gradientStrength = 0.7f; // Merkez daha koyu! (0.4 → 0.5)
        shapeRenderer.enableGradient = true;
        shapeRenderer.enableGlow = true;
        shapeRenderer.enablePulse = true;
        
        Debug.Log("⭐ BOSS SHAPE: Yıldız oluşturuldu!");
        
        // 2) ANIMATED CORE - Dönen beşgen! 🔮
        GameObject coreObj = new GameObject("BossAnimatedCore");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        
        EnemyAnimatedCore core = coreObj.AddComponent<EnemyAnimatedCore>();
        core.coreType = EnemyAnimatedCore.CoreType.RotatingShape;
        core.coreShape = EnemyShapeRenderer.ShapeType.Pentagon;
        core.coreSize = 0.3f; // Biraz küçült (0.4 → 0.35)
        core.coreColor = new Color(1f, 0.9f, 0.3f); // SARI! (Mor ile kontrast) ⚡
        core.rotationSpeed = 150f; // Biraz yavaşlat (180 → 150)
        core.pulseSpeed = 3.5f;
        core.enablePulse = true;
        core.glowIntensity = 1.2f; // Daha düşük (2.5 → 1.5)
        
        Debug.Log("🔮 BOSS CORE: Dönen beşgen oluşturuldu!");
        
        // 3) TRAIL EFFECT - MOR TRAIL! 💨
        EnemyTrailEffect trailEffect = gameObject.AddComponent<EnemyTrailEffect>();
        trailEffect.trailColor = new Color(0.9f, 0.3f, 1f, 0.7f); // MOR + biraz şeffaf
        trailEffect.trailDuration = 0.5f; // Biraz kısalt (0.6 → 0.5)
        trailEffect.trailStartWidth = 0.5f; // Biraz incelt (0.6 → 0.5)
        trailEffect.trailEndWidth = 0.1f;
        trailEffect.glowIntensity = 1.4f; // Daha düşük (3.0 → 1.6)
        trailEffect.useAdditiveBlend = true;
        
        Debug.Log("💨 BOSS TRAIL: Mega trail oluşturuldu!");
        
        Debug.Log("🌟 BOSS GÖRSEL SİSTEM TAMAMLANDI! Yıldız + Dönen Core + Mega Trail!");
    }
    
    // Boss öldüğünde
    void OnDestroy()
    {
        Debug.Log("👾 BOSS ÖLDÜ!");
        
        // SIMPLE HEALTH BAR'I GİZLE! 💜
        if (SimpleBossHealthBar.Instance != null)
        {
            SimpleBossHealthBar.Instance.HideBar();
        }
    }
}
