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
        Yellow,   // HÄ±zlÄ±/DeÄŸerli
        Orange,  // Zigzag - YENÄ°!
        Blue,    // ÃœÃ§lÃ¼ - YENÄ°!
        Red,     // Dash - YENI!
        Boss     // BOSS - YENI! 👾
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
    
    [Header("Hit Reaction - YENİ! 💥")]
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
        
        // Orijinal scale'i kaydet
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (!isDestroyed)
            // BOSS ise kendi AI hareket eder - normal hareketi atla
            if (enemyType == EnemyType.Boss)
            {
                return;
            }
        {
            // KNOCKBACK AKTÄ°FSE SADECE KNOCKBACK HAREKETÄ°! ðŸ’¥âœ…
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
            
            currentSpeed = CalculateSpeed();
        
            Vector3 playerPos = Vector3.zero;
            Vector3 direction = (playerPos - transform.position).normalized;
        
            // NORMAL HAREKET
            Vector3 movement = direction * currentSpeed * Time.deltaTime;
        
            // ZÄ°GZAG HAREKETÄ° EKLE - YENÄ°! âœ…
            if (useZigzag)
            {
                movement += CalculateZigzagOffset();
            }
        
            // DASH HAREKETÄ° EKLE - YENÄ°! (Sonra ekleyeceÄŸiz)
            if (useDash)
            {
                movement += CalculateDashMovement();
            }
        
            // Hareketi uygula
            transform.position += movement;
        
            // Zaman sayacÄ±nÄ± artÄ±r (zigzag iÃ§in)
            movementTime += Time.deltaTime;
        
            // EKRAN SINIRI GÃœVENLÄ°ÄžÄ° - YENÄ°! âœ…
            if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
            {
                Debug.LogWarning($"âš ï¸ DÃ¼ÅŸman ekrandan Ã§Ä±ktÄ±! {gameObject.name} Pos: {transform.position}");
            
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                if (spawner != null)
                {
                    spawner.OnEnemyReachedPlayer(); // KayÄ±p olarak say
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

    // Dash hareketi (kÄ±rmÄ±zÄ± dÃ¼ÅŸman iÃ§in)
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
                // ZIGZAG - YENÄ°! âœ…
                maxHealth = 150;
                currentHealth = 150;
                baseSpeed = 2f; // 1x
                damageToPlayer = 1;
                useZigzag = true;
                zigzagAmplitude = 3.5f; // Zigzag geniÅŸliÄŸi (ayarlanabilir)
                zigzagFrequency = 4f; // Zigzag hÄ±zÄ± (ayarlanabilir)
                transform.localScale = Vector3.one * 0.9f;
                break;
        
            case EnemyType.Blue:
                // ÃœÃ‡LÃœ GRUP - YENÄ°! âœ…
                maxHealth = 50;
                currentHealth = 50;
                baseSpeed = 3f; // 1.5x
                damageToPlayer = 1;
                transform.localScale = Vector3.one * 0.7f; // KÃ¼Ã§Ã¼k
                // Not: ÃœÃ§lÃ¼ spawn EnemySpawner'da yapÄ±lacak
                break;
        
            case EnemyType.Red:
                // DASH - YENÄ°! âœ…
                maxHealth = 100;
                currentHealth = 100;
                baseSpeed = 2f; // 1x normal
                damageToPlayer = 1;
                useDash = true;
                
                // DASH AYARLARI
                dashSpeed = 8f;         // Ã‡ok hÄ±zlÄ± dash!
                dashCooldown = 0.8f;      // 2 saniyede bir dash
                dashDuration = 0.3f;    // 0.3 saniye dash sÃ¼resi
                dashTimer = 0.5f;         // Ä°lk dash 1 saniye sonra
                
                break;
            
            case EnemyType.Boss:
                // BOSS - YENİ! 👾
                maxHealth = 6000; // 6000 HP!
                currentHealth = 6000;
                baseSpeed = 0f; // Boss kendi hareketini kontrol eder
                damageToPlayer = 3; // Çok tehlikeli!
                transform.localScale = Vector3.one * 2.5f; // Büyük!
                
                // Boss Controller ekle
                BossController bossAI = gameObject.AddComponent<BossController>();
                bossAI.enemyPrefab = FindObjectOfType<EnemySpawner>().enemyPrefab;
                
                Debug.Log("👾 BOSS INITIALIZED!");
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
        }
    }

    // HÄ±zÄ± hesapla (buff kontrolÃ¼ ile)
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
        //actualDamage = ApplyDebuffMultiplier(actualDamage);
    
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
    
        // Damage text gÃ¶ster
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 0.5f;
            Color damageColor = isTurret ? Color.red : Color.cyan;
            DamageTextManager.Instance.ShowDamage(actualDamage, textPosition, damageColor);
        }

        StartCoroutine(DamageFlash());
        
        // SCREEN SHAKE EKLE - YENÄ°!
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.03f); // Hafif sarsÄ±ntÄ±
        }
        // HIT SESÄ° - YENÄ°!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHit();
        }
        // HIT PARTICLE - YENÄ°!
        if (HitEffectManager.Instance != null)
        {
            // DÃ¼ÅŸman tipine gÃ¶re renk
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
    
        currentHealth -= actualDamage;
        
        // BOSS HP GÖSTERGESİ - YENİ! 👾
        if (enemyType == EnemyType.Boss && currentHealth > 0)
        {
            UpdateBossVisual();
        }
        
        Debug.Log($"PLAYER DAMAGE: {actualDamage} to {enemyType} at {transform.position}");
        
        // KNOCKBACK UYGULA - YENÄ°! ðŸ’¥âœ…
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
        
        //if (CoinManager.Instance != null)
        //{
        //    int coinAmount = CoinManager.Instance.coinsPerKill;
        //    CoinManager.Instance.AddCoins(coinAmount);
        //}
        
        SpawnCoins();
        
        // BOSS öldü mü? Özel ödül ve KAZANMA!
        if (enemyType == EnemyType.Boss)
        {
            Debug.Log("👾 === BOSS ÖLDÜRÜLDÜ! ===");
            
            // BOSS ÖLÜM SESİ! 💀
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBossDeath();
                SoundManager.Instance.StopMusic(); // Müziği durdur
            }

            // Büyük screen shake!
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(1.0f, 0.3f); // ÇOK BÜYÜK!
            }
            
            // Bonus coin!
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(800); // Ekstra 800 coin!
                Debug.Log("💰 Boss bonus: +800 coin!");
            }
            
            // OYUNU KAZAN!
            if (GameManager.Instance != null)
            {
                // 2 saniye bekle, sonra kazanma ekranı
                StartCoroutine(WinAfterDelay(2f));
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
                    return 100; // 100 coin! 🎉
            
                default:
                    return 5;
            }
        }
        
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
                    //HitEffectManager.Instance.ShowHitEffect(transform.position, explosionColor);
                case EnemyType.Boss:
                    explosionColor = new Color(0.8f, 0.2f, 1f); // Parlak mor
                    CameraShake.Instance.Shake(0.5f, 0.2f); // Güçlü sarsıntı!
                    break;
            }
        
            
        }
    
        // ÖNCE YOK ET! 
        Destroy(gameObject);
    
        // SONRA HABER VER!
        // (GameObject yok olsa da kod Ã§alÄ±ÅŸÄ±r - bir frame iÃ§inde)
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
    
    // Boss öldükten sonra kazanma
    IEnumerator WinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
    }
}