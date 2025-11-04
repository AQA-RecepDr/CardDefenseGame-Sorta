using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Düşman Özellikleri")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public float baseSpeed = 2f;
    public int damageToPlayer = 1;
   
    public int laneIndex;
    public int zoneIndex;
    public enum EnemyType
    {
        White,   // Standart
        Black,   // Tank
        Yellow,   // Hızlı/Değerli
        Orange,  // Zigzag - YENİ!
        Blue,    // Üçlü - YENİ!
        Red      // Dash - YENİ!
    }
    
    [Header("Hareket Pattern")]
    public bool useZigzag = false;
    public float zigzagAmplitude = 2f; // Zigzag genişliği
    public float zigzagFrequency = 2f; // Zigzag hızı
    
    [Header("Grup Hareketi")]
    public int groupID = -1; // Hangi gruba ait (-1 = yalnız)
    public Vector3 groupOffset = Vector3.zero; // Grup içi pozisyon
    
    public bool useDash = false;
    public float dashSpeed = 8f; // Dash hızı
    public float dashCooldown = 2f; // Dash aralığı
    private float dashTimer = 0f;
    private Vector3 dashDirection = Vector3.zero;
    private bool isDashing = false;
    private float dashDuration = 0.3f; // Dash süresi
    private float dashTimeElapsed = 0f;
    
    private Vector3 originalSpawnPos; // Zigzag için başlangıç pozisyonu
    private float movementTime = 0f; // Zigzag için zaman sayacı
    
    public EnemyType enemyType;
    
    private SpriteRenderer spriteRenderer;
    private bool isDestroyed = false;
    private float currentSpeed;
    private bool hasNotifiedSpawner = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetupEnemyType();
        UpdateVisual();
    }

    void Update()
    {
        if (!isDestroyed)
        {
            currentSpeed = CalculateSpeed();
        
            Vector3 playerPos = Vector3.zero;
            Vector3 direction = (playerPos - transform.position).normalized;
        
            // NORMAL HAREKET
            Vector3 movement = direction * currentSpeed * Time.deltaTime;
        
            // ZİGZAG HAREKETİ EKLE - YENİ! ✅
            if (useZigzag)
            {
                movement += CalculateZigzagOffset();
            }
        
            // DASH HAREKETİ EKLE - YENİ! (Sonra ekleyeceğiz)
            if (useDash)
            {
                movement += CalculateDashMovement();
            }
        
            // Hareketi uygula
            transform.position += movement;
        
            // Zaman sayacını artır (zigzag için)
            movementTime += Time.deltaTime;
        
            // EKRAN SINIRI GÜVENLİĞİ - YENİ! ✅
            if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
            {
                Debug.LogWarning($"⚠️ Düşman ekrandan çıktı! {gameObject.name} Pos: {transform.position}");
            
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                if (spawner != null)
                {
                    spawner.OnEnemyReachedPlayer(); // Kayıp olarak say
                }
            
                Destroy(gameObject);
                return;
            }
        
            float distanceToCenter = Vector3.Distance(transform.position, playerPos);
            if (distanceToCenter < 1f)
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.TakeDamage(damageToPlayer);
                }
            
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                if (spawner != null)
                {
                    spawner.OnEnemyReachedPlayer();
                }
            
                Destroy(gameObject);
            }
        }
    }
    
    // Zigzag offset hesapla
    Vector3 CalculateZigzagOffset()
    {
        // Sin wave ile zigzag hareketi
        // movementTime arttıkça sağa-sola sallanır
        float zigzagX = Mathf.Sin(movementTime * zigzagFrequency) * zigzagAmplitude * Time.deltaTime;
    
        // Zone yönüne göre zigzag ekseni değişir
        Vector3 zigzagOffset = Vector3.zero;
    
        switch (zoneIndex)
        {
            case 0: // TOP (yukarıdan geliyor)
                // X ekseninde zigzag
                zigzagOffset = new Vector3(zigzagX, 0, 0);
                break;
        
            case 1: // RIGHT (sağdan geliyor)
                // Y ekseninde zigzag
                zigzagOffset = new Vector3(0, zigzagX, 0);
                break;
        
            case 2: // BOTTOM (aşağıdan geliyor)
                // X ekseninde zigzag
                zigzagOffset = new Vector3(zigzagX, 0, 0);
                break;
        
            case 3: // LEFT (soldan geliyor)
                // Y ekseninde zigzag
                zigzagOffset = new Vector3(0, zigzagX, 0);
                break;
        }
    
        return zigzagOffset;
    }

    // Dash hareketi (kırmızı düşman için)
    Vector3 CalculateDashMovement()
{
    // Dash timer güncelle
    dashTimer -= Time.deltaTime;
    
    // Dash durumundaysa
    if (isDashing)
    {
        dashTimeElapsed += Time.deltaTime;
        
        // Dash süresi doldu mu?
        if (dashTimeElapsed >= dashDuration)
        {
            // Dash bitti
            isDashing = false;
            dashTimeElapsed = 0f;
            dashTimer = dashCooldown; // Yeni cooldown başlat
            
            Debug.Log("🔴 Dash bitti!");
            
            return Vector3.zero;
        }
        
        // Dash hareketi (çok hızlı!)
        return dashDirection * dashSpeed * Time.deltaTime;
    }
    
    // Dash cooldown bitti mi? Yeni dash başlat!
    if (dashTimer <= 0f && !isDashing)
    {
        StartDash();
    }
    
    return Vector3.zero;
}

    // Yeni dash başlat
    void StartDash()
{
    isDashing = true;
    dashTimeElapsed = 0f;
    
    // Rastgele sağ veya sol yön seç
    dashDirection = GetDashDirection();
    
    Debug.Log($"🔴 Dash başladı! Yön: {dashDirection}");
    // TRAIL RENDERER EKLE - YENİ! ✅
    AddDashTrail();
}
    
    // Dash trail ekle
    void AddDashTrail()
    {
        // Zaten trail var mı kontrol et
        TrailRenderer trail = GetComponent<TrailRenderer>();
    
        if (trail == null)
        {
            // Trail yoksa ekle
            trail = gameObject.AddComponent<TrailRenderer>();
        
            // Trail ayarları
            trail.time = 0.3f; // 0.3 saniye iz kalır
            trail.startWidth = 0.5f;
            trail.endWidth = 0.1f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Kırmızı
            trail.endColor = new Color(1f, 0.2f, 0.2f, 0f); // Fade out
            trail.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    
        // Trail'i aktif et
        trail.enabled = true;
    
        // Dash bitince trail'i kapat
        StartCoroutine(DisableTrailAfterDash());
    }
    
    // Dash bitince trail'i kapat
    IEnumerator DisableTrailAfterDash()
    {
        yield return new WaitForSeconds(dashDuration);
    
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.enabled = false;
        }
    }
    
    // Dash yönünü belirle (zone'a göre sağ/sol)
    Vector3 GetDashDirection()
{
    // Rastgele sağ veya sol
    float randomDirection = Random.value > 0.5f ? 1f : -1f;
    
    Vector3 direction = Vector3.zero;
    
    switch (zoneIndex)
    {
        case 0: // TOP (yukarıdan geliyor)
            // X ekseninde sağ/sol dash
            direction = new Vector3(randomDirection, 0, 0);
            break;
        
        case 1: // RIGHT (sağdan geliyor)
            // Y ekseninde yukarı/aşağı dash
            direction = new Vector3(0, randomDirection, 0);
            break;
        
        case 2: // BOTTOM (aşağıdan geliyor)
            // X ekseninde sağ/sol dash
            direction = new Vector3(randomDirection, 0, 0);
            break;
        
        case 3: // LEFT (soldan geliyor)
            // Y ekseninde yukarı/aşağı dash
            direction = new Vector3(0, randomDirection, 0);
            break;
    }
    
    return direction;
}

    // Düşman tipine göre özellikleri ayarla
    void SetupEnemyType()
    {
        switch (enemyType)
        {
            case EnemyType.White:
                maxHealth = 100; // 100 can
                currentHealth = 100;
                baseSpeed = 2f;
                damageToPlayer = 1;
                break;
            
            case EnemyType.Black:
                maxHealth = 300; // 300 can
                currentHealth = 300;
                baseSpeed = 1f;
                damageToPlayer = 2;
                transform.localScale = Vector3.one * 1.2f;
                break;
            
            case EnemyType.Yellow:
                maxHealth = 50; // 50 can
                currentHealth = 50;
                baseSpeed = 4f;
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.8f; // Küçük
                break;
            
            case EnemyType.Orange:
                // ZIGZAG - YENİ! ✅
                maxHealth = 150;
                currentHealth = 150;
                baseSpeed = 2f; // 1x
                damageToPlayer = 1;
                useZigzag = true;
                zigzagAmplitude = 3.5f; // Zigzag genişliği (ayarlanabilir)
                zigzagFrequency = 4f; // Zigzag hızı (ayarlanabilir)
                transform.localScale = Vector3.one * 0.9f;
                break;
        
            case EnemyType.Blue:
                // ÜÇLÜ GRUP - YENİ! ✅
                maxHealth = 50;
                currentHealth = 50;
                baseSpeed = 3f; // 1.5x
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.7f; // Küçük
                // Not: Üçlü spawn EnemySpawner'da yapılacak
                break;
        
            case EnemyType.Red:
                // DASH - YENİ! ✅
                maxHealth = 100;
                currentHealth = 100;
                baseSpeed = 2f; // 1x normal
                damageToPlayer = 1;
                useDash = true;
                
                // DASH AYARLARI
                dashSpeed = 8f;         // Çok hızlı dash!
                dashCooldown = 0.8f;      // 2 saniyede bir dash
                dashDuration = 0.3f;    // 0.3 saniye dash süresi
                dashTimer = 0.5f;         // İlk dash 1 saniye sonra
                
                break;
        }
    }

    // Görsel güncelle
    void UpdateVisual()
    {
        switch (enemyType)
        {
            case EnemyType.White:
                spriteRenderer.color = new Color(0.9f, 0.9f, 0.9f); // Açık gri
                break;
            case EnemyType.Black:
                spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f); // Koyu siyah
                break;
            case EnemyType.Yellow:
                spriteRenderer.color = new Color(1f, 0.95f, 0.2f); // Parlak sarı
                break;
            case EnemyType.Orange:
                spriteRenderer.color = new Color(1f, 0.6f, 0f); // Turuncu - YENİ! ✅
                break;
            case EnemyType.Blue:
                spriteRenderer.color = new Color(0.2f, 0.5f, 1f); // Mavi - YENİ! ✅
                break;
            case EnemyType.Red:
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f); // Kırmızı - YENİ! ✅
                break;
        }
    }

    // Hızı hesapla (buff kontrolü ile)
    float CalculateSpeed()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
    
        foreach (Zone zone in allZones)
        {
            // Aynı zone'da mı?
            if (zone.zoneIndex == zoneIndex)
            {
                // Slow buff varsa yavaşlat
                if (zone.hasSlowBuff)
                {
                    Debug.Log($"❄️ Düşman yavaşlatıldı! Zone: {zoneIndex}, Hız: {baseSpeed * zone.slowMultiplier}");
                    return baseSpeed * zone.slowMultiplier;
                }
            }
        }
    
        return baseSpeed;
    }

    public void TakeDamage(int damage, bool isTurret = false)
    {
        int actualDamage = damage;
    
        // DEBUFF KONTROL - Turuncu kart varsa %50 fazla hasar!
        //actualDamage = ApplyDebuffMultiplier(actualDamage);
    
        currentHealth -= actualDamage;
    
        if (isTurret)
        {
            Debug.Log($"🔴 TURRET DAMAGE: {actualDamage} to {enemyType}");
        }
        else
        {
            Debug.Log($"🔵 LANE DAMAGE: {actualDamage} to {enemyType}");
        }
    
        // Damage text göster
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 0.5f;
            Color damageColor = isTurret ? Color.red : Color.cyan;
            DamageTextManager.Instance.ShowDamage(actualDamage, textPosition, damageColor);
        }

        StartCoroutine(DamageFlash());
        
        // SCREEN SHAKE EKLE - YENİ!
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.03f); // Hafif sarsıntı
        }
        // HIT SESİ - YENİ!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
        // HIT PARTICLE - YENİ!
        if (HitEffectManager.Instance != null)
        {
            // Düşman tipine göre renk
            Color hitColor = isTurret ? Color.red : Color.cyan;
            HitEffectManager.Instance.ShowHitEffect(transform.position, hitColor);
        }

        if (currentHealth <= 0)
        {
            DestroyEnemy();
        }
    }
    
    public void TakePlayerDamage(int damage)
    {
        
        // DEBUFF KONTROL - Player hasarı da debuff'tan etkilensin!
        int actualDamage = damage;
    
        currentHealth -= actualDamage;
        Debug.Log($"PLAYER DAMAGE: {actualDamage} to {enemyType} at {transform.position}");

        // Player damage - SARI
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 0.5f;
            DamageTextManager.Instance.ShowDamage(actualDamage, textPosition, Color.yellow);
        }

        StartCoroutine(DamageFlash());
        
        // HIT SESİ - YENİ!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.03f); // Hafif sarsıntı
        }
        
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.ShowHitEffect(transform.position, Color.cyan);
        }

        if (currentHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }
    
    public void DestroyEnemy()
    {
        if (isDestroyed) return;
    
        isDestroyed = true;
        
        if (CoinManager.Instance != null)
        {
            int coinAmount = CoinManager.Instance.coinsPerKill;
            CoinManager.Instance.AddCoins(coinAmount);
        }
        
        // Heal buff kontrolü
        CheckHealBuff();
    
        // Efektler
        if (CameraShake.Instance != null)
        {
            float shakeMagnitude = enemyType == EnemyType.Black ? 0.08f : 0.05f;
            CameraShake.Instance.Shake(0.15f, shakeMagnitude);
        }
    
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeath();
        }
    
        if (HitEffectManager.Instance != null)
        {
            Color explosionColor = Color.white;
        
            switch (enemyType)
            {
                case EnemyType.White:
                    explosionColor = Color.white;
                    break;
                case EnemyType.Black:
                    explosionColor = new Color(0.3f, 0.3f, 0.3f);
                    break;
                case EnemyType.Yellow:
                    explosionColor = Color.yellow;
                    break;
            }
        
            HitEffectManager.Instance.ShowHitEffect(transform.position, explosionColor);
        }
    
        // ÖNCE YOK ET! ✅
        Destroy(gameObject);
    
        // SONRA HABER VER! ✅
        // (GameObject yok olsa da kod çalışır - bir frame içinde)
        if (!hasNotifiedSpawner)
        {
            hasNotifiedSpawner = true;
        
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                Debug.Log($"💀 Spawner'a bildirim: {gameObject.name}");
                spawner.OnEnemyKilled(); // Artık FindObjectsOfType bu düşmanı bulamaz ✅
            }
        }
    }
   
    // Yeşil buff varsa can ver
    void CheckHealBuff()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
    
        foreach (Zone zone in allZones)
        {
            // Aynı zone'da mı ve heal buff var mı?
            if (zone.zoneIndex == zoneIndex && zone.hasHealBuff)
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.Heal(1);
                    Debug.Log($"💚 Yeşil buff! +1 can (Zone {zoneIndex})");
                }
                break;
            }
        }
    }
}