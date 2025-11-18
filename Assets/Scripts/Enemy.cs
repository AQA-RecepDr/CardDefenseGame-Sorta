using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("DÃ¼ÅŸman Ã–zellikleri")]
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
        Yellow,   // 
        Orange,  // Zigzag 
        Blue,    // 
        Red,     // Dash 
        Boss,     // BOSS 
        Sniper,     // YENI! Uzaktan saldiri - Yesil koni
        Debuffer,   // YENI! Atak hizi dusurucu - Bordo altigen
        ZoneBuffer  // YENI! Elite - Kart etkisiz + can buff - Koyu gri daire
    }
    
    [Header("Coin Drop")]
    public GameObject coinPrefab;
    
    [Header("Hareket Pattern")]
    public bool useZigzag = false;
    public float zigzagAmplitude = 2f; // Zigzag geniÅŸliÄŸi
    public float zigzagFrequency = 2f; // Zigzag hÄ±zÄ±
    
    [Header("Knockback")]
    public bool isKnockbacked = false;
    private float knockbackTimer = 0f;
    public float knockbackDuration = 0.1f; // 0.1 saniye geriye gider
    private Vector3 knockbackVelocity = Vector3.zero;
    
    [Header("Hit Reaction")]
    public bool enableHitReaction = true;
    private Vector3 originalScale;
    private bool isHitAnimating = false;
    
    [Header("Grup Hareketi")]
    public int groupID = -1; // Hangi gruba ait (-1 = yalnÄ±z)
    public Vector3 groupOffset = Vector3.zero; // Grup iÃ§i pozisyon
    
    public bool useDash = false;
    public float dashSpeed = 8f; // Dash hÄ±zÄ±
    public float dashCooldown = 2f; // Dash aralÄ±ÄŸÄ±
    private float dashTimer = 0f;
    private Vector3 dashDirection = Vector3.zero;
    private bool isDashing = false;
    private float dashDuration = 0.3f; // Dash sÃ¼resi
    private float dashTimeElapsed = 0f;
    
    [Header("Sniper - Uzaktan Saldiri")]
    public bool useSniper = false;
    public float sniperFireRate = 2f; // Ates hizi
    private float sniperFireTimer = 0f;
    public int sniperShotCount = 0; // Kac atis yapti
    public int sniperMaxShots = 3; // Max atis sonra hareket
    public GameObject sniperProjectilePrefab;
    private Vector3 sniperPosition; // Bekledigi pozisyon
    
    [Header("Debuffer - Atak Hizi Dusurucu")]
    public bool useDebuffer = false;
    public float debuffRange = 4f; // Etki mesafesi
    public float debuffSlowMultiplier = 0.2f; // Yavaslatma carpani
    private bool isDebuffActive = false;
    private Vector3 debufferStopPosition; // YENI - Duracagi pozisyon
    private bool hasReachedPosition = false; // YENI - Pozisyona ulasti mi?
    
    [Header("Zone Buffer (Elite) - Kart Etkisiz + Can Buff")]
    public bool useZoneBuffer = false;
    public float zoneBuffHealRate = 5f; // Saniyede +HP
    private float zoneBuffHealTimer = 0f;
    private bool hasDisabledCard = false; // Karti devre disi birakti mi?
    
    private Vector3 originalSpawnPos; // Zigzag iÃ§in baÅŸlangÄ±Ã§ pozisyonu
    private float movementTime = 0f; // Zigzag iÃ§in zaman sayacÄ±
    
    public EnemyType enemyType;
    
    private SpriteRenderer spriteRenderer;
    public bool isDestroyed = false;
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
        
        CreateEnemyShape();
    
        // SpriteRenderer'ı gizle (artık shape kullanıyoruz)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Orijinal scale'i kaydet
        originalScale = transform.localScale;
    }

   void Update()
{
    if (!isDestroyed)
    {
        // BOSS kontrolu
        if (enemyType == EnemyType.Boss)
        {
            return;
        }
        
        // DEBUFFER kontrolu - HAREKET KODUNDAN ONCE! ✅
        if (enemyType == EnemyType.Debuffer)
        {
            DebufferUpdate();
            return; // Normal hareket yapma!
        }
        
        // SNIPER kontrolu - HAREKET KODUNDAN ONCE! ✅
        if (enemyType == EnemyType.Sniper)
        {
            SniperUpdate();
            return; // Normal hareket yapma!
        }
        
        // ZONE BUFFER kontrolu - HAREKET KODUNDAN ONCE! ✅
        if (enemyType == EnemyType.ZoneBuffer)
        {
            ZoneBufferUpdate();
            return; // Normal hareket yapma!
        }
        
        // Knockback kontrolu
        if (isKnockbacked)
        {
            knockbackTimer -= Time.deltaTime;
        
            // Knockback hareketi
            transform.position += knockbackVelocity * Time.deltaTime;
        
            // Knockback bitti mi?
            if (knockbackTimer <= 0f)
            {
                isKnockbacked = false;
                knockbackVelocity = Vector3.zero;
            }
        
            return; // Normal hareket yapma!
        }
        
        // NORMAL DUSMAN HAREKETI (White, Black, Yellow, Orange, Blue, Red)
        currentSpeed = CalculateSpeed();
    
        Vector3 playerPos = Vector3.zero;
        Vector3 direction = (playerPos - transform.position).normalized;
    
        // NORMAL HAREKET
        Vector3 movement = direction * currentSpeed * Time.deltaTime;
        
        if (useZigzag)
        {
            movement += CalculateZigzagOffset();
        }
    
        if (useDash)
        {
            movement += CalculateDashMovement();
        }
    
        // Hareketi uygula
        transform.position += movement;
        
        movementTime += Time.deltaTime;
        
        // Ekran sinirlari kontrolu
        if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
        {
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyReachedPlayer();
            }
        
            Destroy(gameObject);
            return;
        }
    
        // Player'a carpisma kontrolu
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
        // movementTime arttÄ±kÃ§a saÄŸa-sola sallanÄ±r
        float zigzagX = Mathf.Sin(movementTime * zigzagFrequency) * zigzagAmplitude * Time.deltaTime;
    
        // Zone yÃ¶nÃ¼ne gÃ¶re zigzag ekseni deÄŸiÅŸir
        Vector3 zigzagOffset = Vector3.zero;
    
        switch (zoneIndex)
        {
            case 0: // TOP (yukarÄ±dan geliyor)
                // X ekseninde zigzag
                zigzagOffset = new Vector3(zigzagX, 0, 0);
                break;
        
            case 1: // RIGHT (saÄŸdan geliyor)
                // Y ekseninde zigzag
                zigzagOffset = new Vector3(0, zigzagX, 0);
                break;
        
            case 2: // BOTTOM (aÅŸaÄŸÄ±dan geliyor)
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

    
    Vector3 CalculateDashMovement()
{
    // Dash timer gÃ¼ncelle
    dashTimer -= Time.deltaTime;
    
    // Dash durumundaysa
    if (isDashing)
    {
        dashTimeElapsed += Time.deltaTime;
        
        // Dash sÃ¼resi doldu mu?
        if (dashTimeElapsed >= dashDuration)
        {
            // Dash bitti
            isDashing = false;
            dashTimeElapsed = 0f;
            dashTimer = dashCooldown; // Yeni cooldown baÅŸlat
            
            Debug.Log("ðŸ”´ Dash bitti!");
            
            return Vector3.zero;
        }
        
        // Dash hareketi (Ã§ok hÄ±zlÄ±!)
        return dashDirection * dashSpeed * Time.deltaTime;
    }
    
    // Dash cooldown bitti mi? Yeni dash baÅŸlat!
    if (dashTimer <= 0f && !isDashing)
    {
        StartDash();
    }
    
    return Vector3.zero;
}

    // Yeni dash baÅŸlat
    void StartDash()
{
    isDashing = true;
    dashTimeElapsed = 0f;
    
    // Rastgele saÄŸ veya sol yÃ¶n seÃ§
    dashDirection = GetDashDirection();
    
    Debug.Log($"ðŸ”´ Dash baÅŸladÄ±! YÃ¶n: {dashDirection}");
    // TRAIL RENDERER EKLE - YENÄ°! âœ…
    AddDashTrail();
}
    
    // Dash trail ekle
    void AddDashTrail()
    {
        // Zaten trail var mÄ± kontrol et
        TrailRenderer trail = GetComponent<TrailRenderer>();
    
        if (trail == null)
        {
            // Trail yoksa ekle
            trail = gameObject.AddComponent<TrailRenderer>();
        
            // Trail ayarlarÄ±
            trail.time = 0.3f; // 0.3 saniye iz kalÄ±r
            trail.startWidth = 0.5f;
            trail.endWidth = 0.1f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 0.2f, 0.2f, 0.8f); // KÄ±rmÄ±zÄ±
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
    // Rastgele saÄŸ veya sol
    float randomDirection = Random.value > 0.5f ? 1f : -1f;
    
    Vector3 direction = Vector3.zero;
    
    switch (zoneIndex)
    {
        case 0: // TOP (yukarÄ±dan geliyor)
            // X ekseninde saÄŸ/sol dash
            direction = new Vector3(randomDirection, 0, 0);
            break;
        
        case 1: // RIGHT (saÄŸdan geliyor)
            // Y ekseninde yukarÄ±/aÅŸaÄŸÄ± dash
            direction = new Vector3(0, randomDirection, 0);
            break;
        
        case 2: // BOTTOM (aÅŸaÄŸÄ±dan geliyor)
            // X ekseninde saÄŸ/sol dash
            direction = new Vector3(randomDirection, 0, 0);
            break;
        
        case 3: // LEFT (soldan geliyor)
            // Y ekseninde yukarÄ±/aÅŸaÄŸÄ± dash
            direction = new Vector3(0, randomDirection, 0);
            break;
    }
    
    return direction;
}

    // Düşman tipine göre özellikler
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
                transform.localScale = Vector3.one * 0.8f; // KÃ¼Ã§Ã¼k
                break;
            
            case EnemyType.Orange:
                maxHealth = 150;
                currentHealth = 150;
                baseSpeed = 2f;
                damageToPlayer = 1;
                useZigzag = true;
                zigzagAmplitude = 3.5f; 
                zigzagFrequency = 4f;
                transform.localScale = Vector3.one * 0.9f;
                break;
        
            case EnemyType.Blue:
                maxHealth = 50;
                currentHealth = 50;
                baseSpeed = 3f; // 1.5x
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.7f; 
                break;
        
            case EnemyType.Red:
                maxHealth = 100;
                currentHealth = 100;
                baseSpeed = 2f; // 1x normal
                damageToPlayer = 1;
                useDash = true;
                
               dashSpeed = 8f;         
                dashCooldown = 0.8f;      
                dashDuration = 0.3f;    
                dashTimer = 0.5f;         
                
                break;
            
            case EnemyType.Boss:
                // BOSS 
                maxHealth = 6000;
                currentHealth = 6000;
                baseSpeed = 0f;
                damageToPlayer = 3;
                transform.localScale = Vector3.one * 2.5f;
                
                // Boss Controller ekle
                BossController bossAI = gameObject.AddComponent<BossController>();
                bossAI.enemyPrefab = FindObjectOfType<EnemySpawner>().enemyPrefab;
                
                Debug.Log("👾 BOSS INITIALIZED!");
                break;
            case EnemyType.Sniper:
                // SNIPER - Uzaktan saldiri!
                maxHealth = 150;
                currentHealth = 150;
                baseSpeed = 0f; // Hareket etmez, pozisyonda bekler
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.9f;
                
                // Sniper ozellikleri
                useSniper = true;
                sniperFireRate = 2f; // 2 saniyede bir ates
                sniperShotCount = 0;
                sniperMaxShots = 3; // 3 atis sonra hareket
                
                Debug.Log("Sniper initialized!");
                break;
            
            case EnemyType.Debuffer:
                // DEBUFFER - Atak hizi dusurucu!
                maxHealth = 170;
                currentHealth = 170;
                baseSpeed = 1.5f; // Yavas hareket
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.85f;
                
                // Debuffer ozellikleri
                useDebuffer = true;
                debuffRange = 3f; // 3 birim mesafede etkili
                debuffSlowMultiplier = 0.6f; // %80 yavas atak
                
                // SPAWN POZISYONU AYARLA - YENI!
                Vector3 spawnCorner = GetZoneCornerPosition(zoneIndex);
                transform.position = spawnCorner;
                
                Debug.Log("Debuffer initialized!");
                break;
            
            case EnemyType.ZoneBuffer:
                // ZONE BUFFER (ELITE) - Kart etkisiz + can buff!
                maxHealth = 600;
                currentHealth = 600;
                baseSpeed = 0f; // Hareket etmez, kosede bekler
                damageToPlayer = 2;
                transform.localScale = Vector3.one * 1.5f; // Buyuk!
                
                // Zone Buffer ozellikleri
                useZoneBuffer = true;
                zoneBuffHealRate = 5f; // Saniyede +5 HP
                
                Debug.Log("ZoneBuffer (ELITE) initialized!");
                break;
        }
    }

    // GÃ¶rsel gÃ¼ncelle
    public void UpdateVisual()
    {
        switch (enemyType)
        {
           case EnemyType.White:
                spriteRenderer.color = new Color(1f, 1f, 1f); // TAM BEYAZ (glow için)
                break;
            case EnemyType.Black:
                spriteRenderer.color = new Color(0.5f, 0f, 1f); // MOR NEON
                break;
            case EnemyType.Yellow:
                spriteRenderer.color = new Color(1f, 1f, 0f); // SARI NEON
                break;
            case EnemyType.Orange:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // TURUNCU NEON
                break;
            case EnemyType.Blue:
                spriteRenderer.color = new Color(0f, 0.5f, 1f); // MAVİ NEON
                break;
            case EnemyType.Red:
                spriteRenderer.color = new Color(1f, 0f, 0.3f); // KIRMIZI NEON
                break;
            case EnemyType.Boss:
                spriteRenderer.color = new Color(1f, 0f, 1f); // PEMBE NEON
                break;
            case EnemyType.Sniper:
                spriteRenderer.color = new Color(0.3f, 1f, 0.3f); // PARLAK YESIL NEON
                break;
            case EnemyType.Debuffer:
                spriteRenderer.color = new Color(0.6f, 0f, 0.2f); // BORDO NEON
                break;
            case EnemyType.ZoneBuffer:
                spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f); // KOYU GRI
                break;
        }
    }
    
    float CalculateSpeed()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
    
        foreach (Zone zone in allZones)
        {
            // AynÄ± zone'da mÄ±?
            if (zone.zoneIndex == zoneIndex)
            {
                // Slow buff varsa yavaÅŸlat
                if (zone.hasSlowBuff)
                {
                    Debug.Log($"â„ï¸ DÃ¼ÅŸman yavaÅŸlatÄ±ldÄ±! Zone: {zoneIndex}, HÄ±z: {baseSpeed * zone.slowMultiplier}");
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
        actualDamage = ApplyDebuffMultiplier(actualDamage);
    
        currentHealth -= actualDamage;
        
        // BOSS HP GÖSTERGESİ - YENİ! 👾
        if (enemyType == EnemyType.Boss && currentHealth > 0)
        {
            UpdateBossVisual();
        }
    
        if (isTurret)
        {
            Debug.Log($"ðŸ”´ TURRET DAMAGE: {actualDamage} to {enemyType}");
        }
        else
        {
            Debug.Log($"ðŸ”µ LANE DAMAGE: {actualDamage} to {enemyType}");
        }
    
        // Damage text
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 0.5f;
            Color damageColor = isTurret ? Color.red : Color.cyan;
            DamageTextManager.Instance.ShowDamage(actualDamage, textPosition, damageColor);
        }

        StartCoroutine(DamageFlash());
        
        // SCREEN SHAKE EKLE 
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.03f); // Hafif sarsÄ±ntÄ±
        }
        // HIT
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
        // HIT PARTICLE 
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
        
        // DEBUFF KONTROL - Player hasarÄ± da debuff'tan etkilensin!
        int actualDamage = damage;
        
        // DEBUFF KONTROL - Player hasarı da debuff'tan etkilensin!
        actualDamage = ApplyDebuffMultiplier(actualDamage);  // Debuff eklendi!
    
        currentHealth -= actualDamage;
        
        // BOSS HP GÖSTERGESİ - YENİ! 👾
        if (enemyType == EnemyType.Boss && currentHealth > 0)
        {
            UpdateBossVisual();
        }
        
        Debug.Log($"PLAYER DAMAGE: {actualDamage} to {enemyType} at {transform.position}");
        
        // KNOCKBACK UYGULA
        ApplyKnockback();
        
        // Player damage - SARI
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 0.5f;
            DamageTextManager.Instance.ShowDamage(actualDamage, textPosition, Color.yellow);
        }

        StartCoroutine(DamageFlash());
        
        // HIT SESÄ° - YENÄ°!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.03f); // Hafif sarsÄ±ntÄ±
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
    
    void ApplyKnockback()
    {
        // Merkezden dışarı doğru (vuruş yönü)
        Vector3 knockbackDirection = (transform.position - Vector3.zero).normalized;
    
        // nockback gücü (düşman tipine göre)
        float knockbackForce = 3f; // Base knockback
    
        switch (enemyType)
        {
            case EnemyType.White:
                knockbackForce = 3f; // Normal
                break;
            case EnemyType.Black:
                knockbackForce = 1.5f; // Tank - daha az geriye gider
                break;
            case EnemyType.Yellow:
                knockbackForce = 4f; // Hafif - daha Ã§ok geriye gider
                break;
            case EnemyType.Orange:
                knockbackForce = 3.5f; // ðŸŸ  Normal+
                break;
            case EnemyType.Blue:
                knockbackForce = 3.5f; // ðŸ”µ Normal+
                break;
            case EnemyType.Red:
                knockbackForce = 2.5f; // ðŸ”´ Dash - Biraz zor
                break;
        }
    
        // Knockback aktif et
        isKnockbacked = true;
        knockbackTimer = knockbackDuration;
        knockbackVelocity = knockbackDirection * knockbackForce;
    
        Debug.Log($"ðŸ’¥ Knockback! Direction: {knockbackDirection}, Force: {knockbackForce}");
        
        if (enableHitReaction && !isHitAnimating)
        {
            StartCoroutine(HitReactionAnimation(knockbackDirection));
        }
    }
    
    // Hit reaction animasyonu (squash & stretch)
IEnumerator HitReactionAnimation(Vector3 hitDirection)
{
    isHitAnimating = true;
    
    Debug.Log($" HIT REACTION BAŞLADI! Enemy: {enemyType}, Direction: {hitDirection}");
    Debug.Log($" Original Scale: {originalScale}, Current Scale: {transform.localScale}");
    
    Quaternion originalRotation = transform.rotation;
    
    // 1. AŞAMA: SQUASH (Ezilme)
    // Vuruş yönünde ezil
    float squashDuration = 0.2f;
    float elapsed = 0f;
    
    // Vuruş yönünü hesapla (normalize edilmiş)
    Vector3 impactAxis = hitDirection.normalized;
    
    // Ezilme miktarı (vuruş yönünde küçül, diğer yönde büyü)
    float squashAmount = 0.5f; // %30 küçülme
    float stretchAmount = 1.4f; // %15 büyüme
    
    // Rotation wobble miktarı
    float maxRotation = 30f; // 15 derece
    
    while (elapsed < squashDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / squashDuration;
        
        // Ease out cubic (yumuşak geçiş)
        float ease = 1f - Mathf.Pow(1f - t, 3f);
        
        // Vuruş yönünde ezil
        float currentSquash = Mathf.Lerp(1f, squashAmount, ease);
        float currentStretch = Mathf.Lerp(1f, stretchAmount, ease);
        
        // Scale hesapla
        Vector3 newScale = originalScale;
        
        // X ve Y eksenlerinde farklı scale
        if (Mathf.Abs(impactAxis.x) > Mathf.Abs(impactAxis.y))
        {
            // Yatay vuruş
            newScale.x *= currentSquash; // X ezilir
            newScale.y *= currentStretch; // Y uzar
        }
        else
        {
            // Dikey vuruş
            newScale.y *= currentSquash; // Y ezilir
            newScale.x *= currentStretch; // X uzar
        }
        
        transform.localScale = newScale;
        // ROTATION - YENİ! 🔄
        float rotationAngle = Mathf.Lerp(0f, maxRotation, ease) * Mathf.Sign(impactAxis.x);
        transform.rotation = originalRotation * Quaternion.Euler(0, 0, rotationAngle);
        
        yield return null;
    }
    
    // 2. AŞAMA: STRETCH (Geri Esneme) 🎯
    float stretchDuration = 0.3f;
    elapsed = 0f;
    
    while (elapsed < stretchDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / stretchDuration;
        
        // Elastic ease out (elastik geri dönüş)
        float ease = Mathf.Sin(-13f * (t + 1f) * Mathf.PI * 0.5f) * Mathf.Pow(2f, -10f * t) + 1f;
        
        // Normal scale'e geri dön
        Vector3 newScale = Vector3.Lerp(transform.localScale, originalScale, ease);
        transform.localScale = newScale;
        // Rotation geri dön
        transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, ease);
        
        yield return null;
    }
    
    // Son dokunuş: Kesinlikle orijinal scale
    transform.localScale = originalScale;
    transform.rotation = originalRotation;
    isHitAnimating = false;
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
        
       // BOSS öldü mü? Özel ödül ve KAZANMA!
        if (enemyType == EnemyType.Boss)
        {
            Debug.Log("💾 === BOSS ÖLDÜRÜLDÜ! EPIC SEQUENCE BAŞLIYOR! ===");
            
            SpawnCoins();
            
            // BOSS EPIC DEATH COROUTINE! 💥
            StartCoroutine(BossEpicDeathSequence());

            return;
        }
        
        SpawnCoins();
        
        // Heal buff kontrolÃ¼
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
        }
    
        // ÖNCE YOK ET! 
        Destroy(gameObject);
    
        // SONRA HABER VER!
        if (!hasNotifiedSpawner)
        {
            hasNotifiedSpawner = true;
        
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                Debug.Log($"ðŸ’€ Spawner'a bildirim: {gameObject.name}");
                spawner.OnEnemyKilled(); // ArtÄ±k FindObjectsOfType bu dÃ¼ÅŸmanÄ± bulamaz âœ…
            }
        }
    }
    
    // Düşman tipine göre coin spawn et
    void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("⚠️ Coin prefab atanmamış!");
            return;
        }
    
        // Düşman tipine göre coin sayısı
        int coinCount = GetCoinCountByType();
    
        // Coinleri spawn et
        for (int i = 0; i < coinCount; i++)
        {
            GameObject coinObj = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        
            // CoinPickup script'i varsa değer ata
            CoinPickup coin = coinObj.GetComponent<CoinPickup>();
            if (coin != null)
            {
                coin.coinValue = 1; // Her coin 1 değerinde
            }
        }
    
        Debug.Log($"💰 {coinCount} coin spawn edildi! (Type: {enemyType})");
    }

// Düşman tipine göre coin sayısı
    int GetCoinCountByType()
    {
        switch (enemyType)
        {
            case EnemyType.White:
                return 5; // 5 coin
            
            case EnemyType.Yellow:
                return 6; // 6 coin
            
            case EnemyType.Black:
                return 10; // 10 coin (tank)
            
            case EnemyType.Blue:
                return 3; // 3 coin (küçük)
            
            case EnemyType.Red:
                return 10; // 10 coin
            
            case EnemyType.Orange:
                return 7; // 7 coin
            
            case EnemyType.Boss:
                return 100; // 100 coin!
            
            case EnemyType.Sniper:
                return 12; // 12 coin (tehlikeli sniper)
            
            case EnemyType.Debuffer:
                return 15; // 15 coin (yuksek deger)
            
            case EnemyType.ZoneBuffer:
                return 50; // 50 coin! (ELITE)
            default:
                return 5;
        }
    }
   
    // YeÅŸil buff varsa can ver
    void CheckHealBuff()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
    
        foreach (Zone zone in allZones)
        {
            // AynÄ± zone'da mÄ± ve heal buff var mÄ±?
            if (zone.zoneIndex == zoneIndex && zone.hasHealBuff)
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.Heal(1);
                    Debug.Log($"ðŸ’š YeÅŸil buff! +1 can (Zone {zoneIndex})");
                }
                break;
            }
        }
    }
    
    // Boss görsel güncellemesi
    void UpdateBossVisual()
    {
        // BOSS HASAR SESİ - YENİ! 👾
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossHurt();
        }
        
        float healthPercent = (float)currentHealth / (float)maxHealth;
    
        // Boyut küçülmesi (2.5x -> 1.5x)
        float targetScale = 1.5f + (healthPercent * 1.0f);
        transform.localScale = Vector3.one * targetScale;
    
        // Renk solması
        Color healthColor = Color.Lerp(
            new Color(1f, 1f, 1f), // Koyu mor (düşük HP)
            new Color(0.8f, 0.3f, 1.0f), // Parlak mor (full HP)
            healthPercent
        );
        spriteRenderer.color = healthColor;
    
        Debug.Log($"👾 Boss HP: {healthPercent:P0} - Scale: {targetScale:F2}");
    }
    
    // BOSS EPIC DEATH SEQUENCE!
    System.Collections.IEnumerator BossEpicDeathSequence()
    {
        Debug.Log("🎬 === BOSS EPIC DEATH BAŞLIYOR! ===");
        
        // 1. BOSS ÖLÜM SESİ! 💀
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossDeath();
            SoundManager.Instance.StopMusic();
        }
        
        // 2. UZUN SÜRELI CAMERA SHAKE (2 saniye!) 📷💥
        if (CameraShake.Instance != null)
        {
            StartCoroutine(LongCameraShake(5f));
        }
        
        // 3. BÜYÜK PATLAMA EFEKTİ (2 saniye devam eder!) 💥
        StartCoroutine(BossExplosionEffect(5f));
        
        // 4. TÜM MİNYONLARI YOK ET! 👻
        DestroyAllMinions();
        
        // 5. TÜM COİNLERİ OTOMATİK TOPLA! 💰
        StartCoroutine(AutoCollectAllCoins());
        
        // 6. Bonus coin ekle
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(800);
            Debug.Log("💰 Boss bonus: +800 coin!");
        }
        
        // 2 saniye bekle (efektler devam ediyor)
        yield return new WaitForSeconds(5f);
        
        Debug.Log("🎬 === BOSS EPIC DEATH BİTTİ! ===");
        
        // 7. OYUNU KAZAN!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
        
        // 8. ŞİMDİ BOSS GAMEOBJECT'İNİ YOK ET!
        Destroy(gameObject);
        
        // 9. Spawner'a bildirim
        if (!hasNotifiedSpawner)
        {
            hasNotifiedSpawner = true;
        
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyKilled();
            }
        }
    }
    
    // 2 saniye sürekli sarsıntı! 📷💥
    System.Collections.IEnumerator LongCameraShake(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (CameraShake.Instance != null)
            {
                // Başta güçlü, sonra azalan
                float intensity = Mathf.Lerp(0.4f, 0.1f, elapsed / duration);
                CameraShake.Instance.Shake(0.1f, intensity);
            }
            
            elapsed += 0.08f; // Her 0.1 saniyede bir shake
            yield return new WaitForSeconds(0.08f);
        }
        
        Debug.Log("📷 Camera shake bitti!");
    }
    
    // 2 saniye devam eden patlama efekti! 💥
    System.Collections.IEnumerator BossExplosionEffect(float duration)
    {
        float elapsed = 0f;
        Vector3 bossPos = transform.position;
        
        while (elapsed < duration)
        {
            // Her 0.15 saniyede bir patlama!
            if (HitEffectManager.Instance != null)
            {
                // Rastgele pozisyon (boss'un etrafında)
                Vector3 randomOffset = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f),
                    0
                );
                
                Vector3 explosionPos = bossPos + randomOffset;
                
                // Rastgele renk (mor, sarı, beyaz, kırmızı)
                Color[] colors = new Color[] {
                    new Color(0.9f, 0.3f, 1f), // Mor 💜
                    Color.yellow,              // Sarı ⚡
                    Color.white,               // Beyaz ✨
                    Color.red,                 // Kırmızı 🔥
                    Color.cyan                 // Cyan 💙
                };
                Color randomColor = colors[Random.Range(0, colors.Length)];
                
                HitEffectManager.Instance.ShowHitEffect(explosionPos, randomColor);
            }
            
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("💥 Patlama efektleri bitti!");
    }
    
    // Tüm minyonları yok et! 👻
    void DestroyAllMinions()
    {
        // Tüm Enemy'leri bul
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        int minionCount = 0;
        
        foreach (Enemy enemy in allEnemies)
        {
            // Boss değilse ve zaten ölmemişse
            if (enemy.enemyType != EnemyType.Boss && !enemy.isDestroyed && enemy != this)
            {
                // Minyon yok olsun!
                enemy.isDestroyed = true;
                
                // Küçük patlama efekti
                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.ShowHitEffect(enemy.transform.position, Color.white);
                }
                
                Destroy(enemy.gameObject);
                minionCount++;
            }
        }
        
        Debug.Log($"👻 {minionCount} minyon yok edildi!");
    }
    
    // Tüm coinleri otomatik topla! 💰
    System.Collections.IEnumerator AutoCollectAllCoins()
    {
        // Kısa bekleme (coinler spawn olsun)
        yield return new WaitForSeconds(0.2f);
        
        // Tüm coinleri bul
        CoinPickup[] allCoins = FindObjectsOfType<CoinPickup>();
        
        Debug.Log($"💰 {allCoins.Length} coin otomatik toplanıyor!");
        
        // Player pozisyonunu bul
        GameObject player = GameObject.Find("WeaponCenter");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player == null)
        {
            Debug.LogWarning("⚠️ Player bulunamadı!");
            yield break;
        }
        
        Transform playerTransform = player.transform;
        
        // Her coini player'a çek!
        foreach (CoinPickup coin in allCoins)
        {
            if (coin != null && !coin.isCollected)
            {
                StartCoroutine(PullCoinToPlayer(coin, playerTransform));
            }
        }
        
        Debug.Log("💰 Tüm coinler çekilmeye başladı!");
    }
    
    // Tek bir coini player'a çek
    System.Collections.IEnumerator PullCoinToPlayer(CoinPickup coin, Transform playerTransform)
    {
        if (coin == null) yield break;
        
        // Coin'i vakum moduna al
        coin.isBeingPulled = true;
        
        // Player'a doğru hareket et
        float speed = 15f; // Hızlı çekim!
        
        while (coin != null && !coin.isCollected)
        {
            if (playerTransform == null) yield break;
            
            // Player'a doğru hareket
            Vector3 direction = (playerTransform.position - coin.transform.position).normalized;
            coin.transform.position += direction * speed * Time.deltaTime;
            
            // Player'a çok yakınsa otomatik topla
            float distance = Vector3.Distance(coin.transform.position, playerTransform.position);
            if (distance < 0.5f)
            {
                // Coin toplansın
                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.AddCoins(coin.coinValue);
                }
                
                coin.isCollected = true;
                Destroy(coin.gameObject);
                yield break;
            }
            
            yield return null;
        }
    }
    
    // Boss öldükten sonra kazanma
    IEnumerator WinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }
    
    /// <summary>
    /// Enemy tipine göre geometrik şekil oluştur
    /// </summary>
    void CreateEnemyShape()
    {
        // EnemyShapeRenderer ekle
        EnemyShapeRenderer shapeRenderer = gameObject.AddComponent<EnemyShapeRenderer>();
        
        // Düşman tipine göre şekil ve renk ayarla
        switch (enemyType)
        {
            case EnemyType.White:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Triangle;
                shapeRenderer.size = 0.6f; // Küçük (hızlı)
                shapeRenderer.shapeColor = new Color(0.9f, 0.9f, 1f); // Beyaz-mavi
                shapeRenderer.pulseSpeed = 3f; // Hızlı pulse (hızlı düşman)
                // ANIMATED CORE! 🔮
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.RotatingShape,
                    EnemyShapeRenderer.ShapeType.Triangle,
                    0.15f, // Küçük core
                    new Color(1f, 1f, 1f), // Beyaz
                    180f // Hızlı dönüş (ters yön)
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
                
            case EnemyType.Black:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Square;
                shapeRenderer.size = 0.6f; // Büyük (tank)
                shapeRenderer.shapeColor = new Color(0.3f, 0.3f, 0.4f); // Koyu
                shapeRenderer.pulseSpeed = 1f; // Yavaş pulse (ağır tank)
                // ANIMATED CORE! 🔮
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.SpinningRing,
                    EnemyShapeRenderer.ShapeType.Circle,
                    0.2f, // Orta boy
                    new Color(0.5f, 0.5f, 0.7f), // Açık gri-mavi
                    60f // Yavaş dönüş (tank)
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
                
            case EnemyType.Yellow:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Pentagon;
                shapeRenderer.size = 0.5f;
                shapeRenderer.shapeColor = new Color(1f, 0.95f, 0.3f); // Sarı
                shapeRenderer.pulseSpeed = 4f; // Hızlı pulse (enerji)
                shapeRenderer.glowIntensity = 2f; // Ekstra parlak (charge)
                // ANIMATED CORE! 🔮
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.PulsingOrb,
                    EnemyShapeRenderer.ShapeType.Circle,
                    0.18f,
                    new Color(1f, 1f, 0.5f), // Parlak sarı
                    0f, // Dönmez (sadece pulse)
                    5f // Hızlı pulse
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
                
            case EnemyType.Orange:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Hexagon;
                shapeRenderer.size = 0.5f;
                shapeRenderer.shapeColor = new Color(1f, 0.6f, 0.2f); // Turuncu
                shapeRenderer.pulseSpeed = 2f;
                // ANIMATED CORE! 🔮
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.RotatingCross,
                    EnemyShapeRenderer.ShapeType.Square,
                    0.2f,
                    new Color(1f, 0.7f, 0.3f), // Turuncu
                    120f // Orta hız
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
                
            case EnemyType.Blue:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Diamond;
                shapeRenderer.size = 0.5f;
                shapeRenderer.shapeColor = new Color(0.3f, 0.7f, 1f); // Açık mavi
                shapeRenderer.pulseSpeed = 1.5f; // Yavaş pulse (slow enemy)
                // ANIMATED CORE! 🔮
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.RotatingShape,
                    EnemyShapeRenderer.ShapeType.Diamond,
                    0.15f,
                    new Color(0.5f, 0.9f, 1f), // Açık mavi
                    90f // Yavaş dönüş
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
                
            case EnemyType.Red:
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Star;
                shapeRenderer.size = 0.8f; // ÇOK BÜYÜK (BOSS!)
                shapeRenderer.shapeColor = new Color(1f, 0.2f, 0.2f); // Kırmızı
                shapeRenderer.pulseSpeed = 2.5f;
                shapeRenderer.glowIntensity = 2.5f; // MEGA GLOW (boss)
                // ANIMATED CORE! 🔮 BOSS MEGA CORE!
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.RotatingShape,
                    EnemyShapeRenderer.ShapeType.Pentagon,
                    0.3f, // BÜYÜK CORE
                    new Color(1f, 0.4f, 0.4f), // Kırmızı
                    150f, // Hızlı dönüş
                    4f // Güçlü pulse
                );
                // Gradient ve glow her zaman aktif
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
        
                Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
        
                // TRAIL EFFECT EKLE! 💨
                CreateTrailEffect();
                break;
            case EnemyType.Sniper:
                // SNIPER - Parlak yesil koni/triangle
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Triangle;
                shapeRenderer.size = 0.55f;
                shapeRenderer.shapeColor = new Color(0.3f, 1f, 0.3f); // Parlak yesil
                shapeRenderer.pulseSpeed = 2f;
                shapeRenderer.glowIntensity = 2f; // Parlak glow
                // ANIMATED CORE!
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.PulsingOrb,
                    EnemyShapeRenderer.ShapeType.Circle,
                    0.15f,
                    new Color(0.5f, 1f, 0.5f), // Acik yesil
                    0f, // Donmez
                    3f // Orta hiz pulse
                );
                // Gradient ve glow
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
                
                Debug.Log($"{enemyType} sekli olusturuldu: {shapeRenderer.shapeType}");
                CreateTrailEffect();
                break;
            case EnemyType.Debuffer:
                // DEBUFFER - Bordo 
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Hexagon;
                shapeRenderer.size = 0.5f;
                shapeRenderer.shapeColor = new Color(0.6f, 0f, 0.2f); // Bordo
                shapeRenderer.pulseSpeed = 1.8f;
                shapeRenderer.glowIntensity = 1.5f;
                // ANIMATED CORE!
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.RotatingShape,
                    EnemyShapeRenderer.ShapeType.Square,
                    0.18f,
                    new Color(0.8f, 0.2f, 0.4f), // Kirmizimsi
                    100f // Orta donu
                );
                // Gradient ve glow
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
                
                Debug.Log($"{enemyType} sekli olusturuldu: {shapeRenderer.shapeType}");
                CreateTrailEffect();
                break;
                
            case EnemyType.ZoneBuffer:
                // ZONE BUFFER (ELITE) - Koyu gri buyuk daire
                shapeRenderer.shapeType = EnemyShapeRenderer.ShapeType.Circle;
                shapeRenderer.size = 0.8f; // BUYUK!
                shapeRenderer.shapeColor = new Color(0.3f, 0.3f, 0.3f); // Koyu gri
                shapeRenderer.pulseSpeed = 1f; // Yavas pulse
                shapeRenderer.glowIntensity = 1.8f;
                // ANIMATED CORE! (Elite core)
                CreateAnimatedCore(
                    EnemyAnimatedCore.CoreType.SpinningRing,
                    EnemyShapeRenderer.ShapeType.Circle,
                    0.35f, // Buyuk core
                    new Color(0.5f, 0.5f, 0.5f), // Acik gri
                    80f // Yavas donux
                );
                // Gradient ve glow
                shapeRenderer.enableGradient = true;
                shapeRenderer.enableGlow = true;
                shapeRenderer.enablePulse = true;
                
                Debug.Log($"{enemyType} (ELITE) sekli olusturuldu: {shapeRenderer.shapeType}");
                CreateTrailEffect();
                break;
        }
        
        // Gradient ve glow her zaman aktif
        shapeRenderer.enableGradient = true;
        shapeRenderer.enableGlow = true;
        shapeRenderer.enablePulse = true;
        
        Debug.Log($"🎨 {enemyType} şekli oluşturuldu: {shapeRenderer.shapeType}");
    }
    
    /// <summary>
    /// Animated core oluştur
    /// </summary>
    void CreateAnimatedCore(
        EnemyAnimatedCore.CoreType coreType, 
        EnemyShapeRenderer.ShapeType coreShape,
        float coreSize,
        Color coreColor,
        float rotationSpeed = 90f,
        float pulseSpeed = 3f)
    {
        // Core objesi oluştur
        GameObject coreObj = new GameObject("AnimatedCore");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        
        // EnemyAnimatedCore component ekle
        EnemyAnimatedCore core = coreObj.AddComponent<EnemyAnimatedCore>();
        
        // Ayarları yap
        core.coreType = coreType;
        core.coreShape = coreShape;
        core.coreSize = coreSize;
        core.coreColor = coreColor;
        core.rotationSpeed = rotationSpeed;
        core.pulseSpeed = pulseSpeed;
        core.enablePulse = true;
        core.glowIntensity = 1.8f;
        
        Debug.Log($"🔮 {enemyType} animated core oluşturuldu: {coreType}");
    }
    
    /// <summary>
    /// Trail effect oluştur (her düşman tipine özel)
    /// </summary>
    void CreateTrailEffect()
    {
        // EnemyTrailEffect component ekle
        EnemyTrailEffect trailEffect = gameObject.AddComponent<EnemyTrailEffect>();
        
        // Düşman tipine göre trail ayarları
        switch (enemyType)
        {
            case EnemyType.White:
                // Hızlı düşman - uzun, ince trail
                trailEffect.trailColor = new Color(0.9f, 0.9f, 1f, 0.8f); // Beyaz-mavi
                trailEffect.trailDuration = 1f; // Uzun trail (hız hissi!)
                trailEffect.trailStartWidth = 0.25f;
                trailEffect.trailEndWidth = 0.05f;
                trailEffect.glowIntensity = 1.8f;
                break;
                
            case EnemyType.Black:
                // Tank - kısa, kalın trail
                trailEffect.trailColor = new Color(0.4f, 0.4f, 0.5f, 0.7f); // Koyu gri
                trailEffect.trailDuration = 0.2f; // Kısa trail (yavaş)
                trailEffect.trailStartWidth = 0.4f; // Kalın (tank)
                trailEffect.trailEndWidth = 0.1f;
                trailEffect.glowIntensity = 1.2f;
                break;
                
            case EnemyType.Yellow:
                // Charge - parlak, titreyen trail
                trailEffect.trailColor = new Color(1f, 0.95f, 0.3f, 0.9f); // Parlak sarı
                trailEffect.trailDuration = 0.35f;
                trailEffect.trailStartWidth = 0.3f;
                trailEffect.trailEndWidth = 0.05f;
                trailEffect.glowIntensity = 2.2f; // Ekstra parlak!
                break;
                
            case EnemyType.Orange:
                // Minion - orta trail
                trailEffect.trailColor = new Color(1f, 0.6f, 0.2f, 0.8f); // Turuncu
                trailEffect.trailDuration = 0.3f;
                trailEffect.trailStartWidth = 0.3f;
                trailEffect.trailEndWidth = 0.06f;
                trailEffect.glowIntensity = 1.6f;
                break;
                
            case EnemyType.Blue:
                // Slow - kristal trail
                trailEffect.trailColor = new Color(0.3f, 0.7f, 1f, 0.8f); // Açık mavi
                trailEffect.trailDuration = 0.45f; // Uzun trail (slow ama görünür)
                trailEffect.trailStartWidth = 0.28f;
                trailEffect.trailEndWidth = 0.05f;
                trailEffect.glowIntensity = 1.7f;
                break;
                
            case EnemyType.Red:
                // BOSS - MEGA TRAIL!
                trailEffect.trailColor = new Color(1f, 0.2f, 0.2f, 1f); // Kırmızı
                trailEffect.trailDuration = 0.5f; // ÇOK UZUN
                trailEffect.trailStartWidth = 0.5f; // ÇOK KALIN
                trailEffect.trailEndWidth = 0.1f;
                trailEffect.glowIntensity = 2.5f; // MEGA PARLAK!
                break;
            case EnemyType.Sniper:
                // Sniper - Ince, parlak yesil trail
                trailEffect.trailColor = new Color(0.3f, 1f, 0.3f, 0.8f); // Parlak yesil
                trailEffect.trailDuration = 0.25f; // Kisa trail (statik dusman)
                trailEffect.trailStartWidth = 0.25f;
                trailEffect.trailEndWidth = 0.05f;
                trailEffect.glowIntensity = 2f; // Parlak!
                break;
                
            case EnemyType.Debuffer:
                // Debuffer - Karanlik, suren trail
                trailEffect.trailColor = new Color(0.6f, 0f, 0.2f, 0.7f); // Bordo
                trailEffect.trailDuration = 0.4f; // Orta trail
                trailEffect.trailStartWidth = 0.3f;
                trailEffect.trailEndWidth = 0.06f;
                trailEffect.glowIntensity = 1.5f;
                break;
                
            case EnemyType.ZoneBuffer:
                // ZoneBuffer - Kalin, koyu gri trail
                trailEffect.trailColor = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Koyu gri
                trailEffect.trailDuration = 0.3f; // Kisa (statik)
                trailEffect.trailStartWidth = 0.45f; // Kalin (buyuk dusman)
                trailEffect.trailEndWidth = 0.1f;
                trailEffect.glowIntensity = 1.8f;
                break;
        }
        
        // Her zaman additive blend (neon efekt)
        trailEffect.useAdditiveBlend = true;
        
        Debug.Log($"💨 {enemyType} trail effect oluşturuldu!");
    }
    
    /// <summary>
    /// Debuff multiplier uygula (mor kart - zone debuff)
    /// </summary>
    int ApplyDebuffMultiplier(int baseDamage)
    {
        // Bu zone'da debuff var mı?
        Zone[] allZones = FindObjectsOfType<Zone>();
        
        foreach (Zone zone in allZones)
        {
            // Aynı zone'da mı ve debuff aktif mi?
            if (zone.zoneIndex == zoneIndex && zone.hasDebuff)
            {
                float multipliedDamage = baseDamage * zone.debuffMultiplier;
                int finalDamage = Mathf.RoundToInt(multipliedDamage);
                
                Debug.Log($"💜 DEBUFF! {baseDamage} → {finalDamage} damage (x{zone.debuffMultiplier})");
                
                return finalDamage;
            }
        }
        
        return baseDamage; // Debuff yoksa normal damage
    }
    // ========== YENI DUSMAN TIPLERI - OZEL FONKSIYONLAR ==========
    
    /// <summary>
    /// SNIPER - Zone kosesinde bekleyip uzaktan ates eder
    /// </summary>
    void SniperUpdate()
    {
        // Ilk spawn'da pozisyon ayarla (zone kosesine git)
        if (sniperPosition == Vector3.zero)
        {
            SetSniperPosition();
        }
        
        // Pozisyonda bekle (hareket etme)
        transform.position = sniperPosition;
        
        // Ates zamani
        sniperFireTimer += Time.deltaTime;
        
        if (sniperFireTimer >= sniperFireRate)
        {
            sniperFireTimer = 0f;
            SniperFire();
            
            sniperShotCount++;
            
            // 2-3 atis sonra pozisyon degistir
            if (sniperShotCount >= sniperMaxShots)
            {
                sniperShotCount = 0;
                ShiftSniperPosition(); // Yana kay
            }
        }
    }
    
    /// <summary>
    /// Sniper pozisyonunu ayarla (zone kosesinde)
    /// </summary>
    void SetSniperPosition()
    {
        Vector3 zoneCorner = GetZoneCornerPosition(zoneIndex);
        sniperPosition = zoneCorner;
        transform.position = sniperPosition;
        
        Debug.Log($"Sniper pozisyon ayarlandi: {sniperPosition} (Zone {zoneIndex})");
    }
    
    /// <summary>
    /// Sniper pozisyonunu degistir (sag/sol)
    /// </summary>
    void ShiftSniperPosition()
    {
        // Rastgele sag veya sol tarafa kay
        float shift = Random.Range(0, 2) == 0 ? -1.5f : 1.5f;
        
        // Zone yonune gore kaydirma ekseni
        if (zoneIndex == 0 || zoneIndex == 2) // TOP/BOTTOM
        {
            sniperPosition += new Vector3(shift, 0, 0); // X ekseninde kay
        }
        else // LEFT/RIGHT
        {
            sniperPosition += new Vector3(0, shift, 0); // Y ekseninde kay
        }
        
        // Ekran sinirlari icinde kal
        sniperPosition.x = Mathf.Clamp(sniperPosition.x, -8f, 8f);
        sniperPosition.y = Mathf.Clamp(sniperPosition.y, -5f, 5f);
        
        Debug.Log($"Sniper pozisyon degisti: {sniperPosition}");
    }
    
    /// <summary>
    /// Sniper ates et (player'a projectile)
    /// </summary>
    void SniperFire()
    {
        if (sniperProjectilePrefab == null)
        {
            Debug.LogWarning("Sniper projectile prefab yok!");
            return;
        }
        
        Vector3 playerPos = Vector3.zero;
        Vector3 direction = (playerPos - transform.position).normalized;
        
        // Muzzle flash
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.ShowMuzzleFlash(transform.position, direction, new Color(0.3f, 1f, 0.3f));
        }
        
        // Projectile olustur (TurretProjectile kullanabiliriz)
        GameObject projectile = Instantiate(sniperProjectilePrefab, transform.position, Quaternion.identity);
        TurretProjectile projectileScript = projectile.GetComponent<TurretProjectile>();
        
        if (projectileScript != null)
        {
            // Player'i target al
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                projectileScript.target = player.transform;
                projectileScript.damage = 1; // Sniper hasar
            }
        }
        
        Debug.Log($"Sniper ates etti! Shot count: {sniperShotCount + 1}/{sniperMaxShots}");
    }
    
 /// <summary>
/// DEBUFFER - Player'a yaklasip belirli mesafede durup atak hizini dusurur
/// </summary>
void DebufferUpdate()
{
    Vector3 playerPos = Vector3.zero;
    float currentDistanceToPlayer = Vector3.Distance(transform.position, playerPos);
    
    Debug.Log($"[DEBUFFER UPDATE] Mesafe: {currentDistanceToPlayer:F2}, HasReached: {hasReachedPosition}, IsActive: {isDebuffActive}");
    
    // FAZE 1: Hedef pozisyona gitme
    if (!hasReachedPosition)
    {
        Debug.Log($"[DEBUFFER FAZE 1] Player'a yaklasiyor... Pos: {transform.position}, Target Range: {debuffRange}");
        
        // Player'a dogru yaklas, ama debuffRange mesafede dur
        Vector3 directionToPlayer = (playerPos - transform.position).normalized;
        
        // Player ne kadar uzakta?
        // ✅ DÜZELTME: debuffRange * 1.1 yerine sadece debuffRange kullan
        if (currentDistanceToPlayer > debuffRange)
        {
            // Henuz yeterince yakin degil, yaklasmeya devam
            transform.position += directionToPlayer * baseSpeed * Time.deltaTime;
            Debug.Log($"[DEBUFFER] Yaklasmeya devam... Yeni Pos: {transform.position}");
        }
        else
        {
            // Yeterince yaklastik! Artik dur
            hasReachedPosition = true;
            debufferStopPosition = transform.position;
            
            Debug.Log($"✅✅✅ [DEBUFFER] POZISYONA ULASTI! Mesafe: {currentDistanceToPlayer:F2}");
            
            // Gorsel efekt goster (buyuk daire)
            ShowDebuffCircle();
            
            // ✅ HEM POZISYONA ULAŞINCA HEM DEBUFF UYGULA!
            ApplyDebuffToPlayer();
            isDebuffActive = true;
        }
        
        return; // Bu frame'de daha fazla bir sey yapma
    }
    
    // FAZE 2: Pozisyonda sabit kal ve debuff uygula
    Debug.Log($"[DEBUFFER FAZE 2] Pozisyonda! Mesafe: {currentDistanceToPlayer:F2}, Range: {debuffRange}");
    
    // ✅ DÜZELTME: Biraz daha geniş range kullan (debuffRange * 1.5)
    if (currentDistanceToPlayer <= debuffRange * 1.5f)
    {
        Debug.Log($"[DEBUFFER] MESAFE ICINDE! {currentDistanceToPlayer:F2} <= {debuffRange * 1.5f}");
        
        // Mesafe icinde - debuff aktif et (zaten aktif olmalı ama kontrol edelim)
        if (!isDebuffActive)
        {
            Debug.Log($"🔴🔴🔴 DEBUFF UYGULANACAK! Calling ApplyDebuffToPlayer()...");
            ApplyDebuffToPlayer();
            isDebuffActive = true;
        }
        else
        {
            Debug.Log($"[DEBUFFER] Debuff zaten aktif.");
        }
    }
    else
    {
        Debug.Log($"[DEBUFFER] MESAFE DISINDA! {currentDistanceToPlayer:F2} > {debuffRange * 1.5f}");
        
        // Player uzaklasti - debuff kaldir
        if (isDebuffActive)
        {
            RemoveDebuffFromPlayer();
            isDebuffActive = false;
            Debug.Log("🟢 DEBUFF KALDIRILDI!");
        }
        
        // Player cok uzaklastiysa, yeniden yaklasma moduna gec
        if (currentDistanceToPlayer > debuffRange * 3f)
        {
            hasReachedPosition = false;
            Debug.Log("[DEBUFFER] Player cok uzaklasti, yeniden yaklasiyor...");
        }
    }
    
    // Pozisyonda statik kal
    transform.position = debufferStopPosition;
}

    void ShowDebuffCircle()
    {
        Debug.Log($"🔴 [DEBUFFER] Debuff dairesi olusturuluyor! Range: {debuffRange}");
    
        // LineRenderer ile büyük kırmızı daire çiz
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
    
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    
        // LineRenderer ayarları
        lineRenderer.positionCount = 60; // 60 nokta ile yumuşak daire
        lineRenderer.loop = true; // Daire kapansın
        lineRenderer.startWidth = 0.15f; // Daha kalın çizgi
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.2f, 0.2f, 0.8f); // Parlak kırmızı
        lineRenderer.endColor = new Color(1f, 0.2f, 0.2f, 0.8f);
        lineRenderer.sortingOrder = -1; // Düşmanın arkasında
        lineRenderer.useWorldSpace = false; // Local space kullan
    
        // DAHA BÜYÜK DAİRE! (debuffRange * 1.5 = daha belirgin)
        float visualRadius = debuffRange * 1.2f;
    
        // Daire noktalarını hesapla
        float angleStep = 360f / 60f;
    
        for (int i = 0; i < 60; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * visualRadius;
            float y = Mathf.Sin(angle) * visualRadius;
        
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    
        Debug.Log($"Büyük kırmızı debuff dairesi gösteriliyor! Radius: {visualRadius}");
    }
    
    /// <summary>
    /// Player'a debuff uygula (atak hizi dusurme)
    /// </summary>
    void ApplyDebuffToPlayer()
    {
        Debug.Log($"[ApplyDebuffToPlayer] CAGRILDI! Multiplier: {debuffSlowMultiplier}");
    
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
    
        if (weaponManager == null)
        {
            Debug.LogError("❌ WeaponManager BULUNAMADI!");
            return;
        }
    
        Debug.Log($"[ApplyDebuffToPlayer] WeaponManager bulundu! AddDebuffer cagiriliyor...");
    
        // Debuffer sayisini artir
        weaponManager.AddDebuffer(debuffSlowMultiplier);
    
        Debug.Log($"✅ Debuff uygulandi! Atak hizi: x{debuffSlowMultiplier}");
    }
    
    /// <summary>
    /// Player'dan debuff kaldir
    /// </summary>
    void RemoveDebuffFromPlayer()
    {
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.RemoveDebuffer();
            Debug.Log("Debuff kaldirildi!");
        }
    }
    
    /// <summary>
    /// ZONE BUFFER (ELITE) - Karti etkisiz yapar, zone'daki dusmanlarin canini artirir
    /// </summary>
    void ZoneBufferUpdate()
    {
        // Ilk spawn'da pozisyon ayarla (zone kosesinde statik kal)
        if (sniperPosition == Vector3.zero)
        {
            Vector3 zoneCorner = GetZoneCornerPosition(zoneIndex);
            sniperPosition = zoneCorner;
            transform.position = sniperPosition;
            
            Debug.Log($"ZoneBuffer (ELITE) pozisyon ayarlandi: {sniperPosition} (Zone {zoneIndex})");
        }
        
        // Pozisyonda statik kal (hareket etme)
        transform.position = sniperPosition;
        
        // Karti etkisiz yap (bir kere)
        if (!hasDisabledCard)
        {
            DisableZoneCard();
            hasDisabledCard = true;
        }
        
        // Zone'daki dusmanlaracanlari arttir (saniyede +5 HP)
        zoneBuffHealTimer += Time.deltaTime;
        
        if (zoneBuffHealTimer >= 1f) // Her saniye
        {
            zoneBuffHealTimer = 0f;
            BuffZoneEnemies();
        }
    }
    
    /// <summary>
    /// Zone'daki karti etkisiz yap
    /// </summary>
    void DisableZoneCard()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
        
        foreach (Zone zone in allZones)
        {
            if (zone.zoneIndex == zoneIndex)
            {
                zone.RemoveBuff(); // Tum buff'lari kaldir
                Debug.Log($"[ELITE] Zone {zoneIndex} karti etkisiz yapildi!");
                break;
            }
        }
    }
    
    /// <summary>
    /// Zone'daki dusmanların canlarini arttir
    /// </summary>
    void BuffZoneEnemies()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
    
        int buffedCount = 0;
        foreach (Enemy enemy in allEnemies)
        {
            // Ayni zone'da ve kendisi degil
            if (enemy.zoneIndex == zoneIndex && enemy != this && !enemy.isDestroyed)
            {
                // +5 HP ver (max HP'yi gecmeden)
                int healAmount = (int)zoneBuffHealRate;
                enemy.currentHealth += healAmount;
                enemy.currentHealth = Mathf.Min(enemy.currentHealth, enemy.maxHealth);
            
                // ✅ YEŞİL HEAL TEXT GÖSTER!
                if (DamageTextManager.Instance != null)
                {
                    Color healColor = new Color(0.2f, 1f, 0.2f); // Parlak yeşil! 💚
                    DamageTextManager.Instance.ShowDamage(
                        healAmount, 
                        enemy.transform.position, 
                        healColor
                    );
                }
            
                buffedCount++;
            }
        }
    
        if (buffedCount > 0)
        {
            Debug.Log($"[ELITE] Zone {zoneIndex}: {buffedCount} dusman +{zoneBuffHealRate} HP aldi!");
        }
    }
    
    /// <summary>
    /// Zone kosesinin pozisyonunu al
    /// </summary>
    Vector3 GetZoneCornerPosition(int zone)
    {
        // Zone'a gore kose pozisyonlari
        switch (zone)
        {
            case 0: // TOP
                return new Vector3(Random.Range(-6f, 6f), 7f, 0);
            case 1: // RIGHT
                return new Vector3(8f, Random.Range(-4f, 4f), 0);
            case 2: // BOTTOM
                return new Vector3(Random.Range(-6f, 6f), -6f, 0);
            case 3: // LEFT
                return new Vector3(-8f, Random.Range(-4f, 4f), 0);
            default:
                return new Vector3(0, 7f, 0);
        }
    }
    
    // ZoneBuffer yok edildiginde - debuff'u kaldir
    void OnDestroy()
    {
        // Debuffer yok olurken debuff'u kaldir
        if (useDebuffer && isDebuffActive)
        {
            RemoveDebuffFromPlayer();
        }
        
        // LineRenderer varsa temizle
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
}