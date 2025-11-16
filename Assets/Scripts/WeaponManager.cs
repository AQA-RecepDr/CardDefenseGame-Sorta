using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;
    
    [Header("Trajectory Ayarları")]
    public int trajectoryDotCount = 15; // Nokta sayısı
    public float trajectoryDotSpacing = 0.3f; // Noktalar arası mesafe
    public float startDotSize = 0.15f;
    public float endDotSize = 0.05f;
    private GameObject[] trajectoryDots; // Nokta objeleri
    
    [Header("Ateş Ayarları")]
    public GameObject primaryProjectilePrefab; // Birincil mermi
    public Transform firePoint; // Merminin çıkış noktası
    public float basePrimaryFireRate = 0.6f;
    private float primaryCooldownTimer = 0f;
    
    [Header("Birincil Ateş Özellikleri")]
    public float projectileSpeed = 10f; // Sabit hız
    public float basePrimaryDamage = 50f; // Base hasar
    
    [Header("Ulti - Orbital Burst")]
    public GameObject ultiProjectilePrefab; // Yeni prefab
    public int ultiProjectileCount = 10; // 10 mermi
    public float ultiCooldown = 20f;
    private float ultiCooldownTimer = 0f;
    public bool isUltiReady = false;
    
    [Header("Aiming")]
    public bool isAiming = false;
    public LineRenderer trajectoryLine;
    public float trajectoryLength = 5f; // Sabit uzunluk
    
    [Header("Görsel Feedback")]
    public SpriteRenderer weaponRenderer; // Silah dairesi
    public Color normalColor = new Color(1f, 1f, 1f); // Gri
    public Color readyColor = new Color(1f, 0.5f, 0f); // Turuncu
    public float shakeAmount = 0.05f; // Titreşim miktarı
    private Vector3 originalWeaponPos;
    public float shakeDuration = 1f; // 1 saniye titreşim - YENİ!
    private float shakeTimer = 0f; // Titreşim sayacı - YENİ!
    //private bool wasReady = false; // Önceki frame'de hazır mıydı? - YENİ!
    
    //[Header("Orbital Strike")]
    //public GameObject targetIndicatorPrefab; // Hedef göstergesi (opsiyonel)
    
    [Header("Auto-Target Ayarları")]
    public bool showAutoTargetIndicator = true; // Hedef göstergesi göster
    public float autoTargetRange = 15f; // Auto-target menzili
    
    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        isAiming = true;
        SetupTrajectory();
        
        // Silah renderer'ını al
        if (weaponRenderer == null)
        {
            weaponRenderer = GetComponent<SpriteRenderer>();
        }
    
        // Orijinal pozisyonu sakla
        originalWeaponPos = transform.localPosition;
    
        // OYUN BAŞINDA ULTİ COOLDOWN BAŞLAT ✅
        ultiCooldownTimer = ultiCooldown;
        isUltiReady = false;
    }
    
    // Primary damage hesapla (upgrade dahil) - YENİ FONKSIYON! 💥✅
    public float CalculateFinalPrimaryDamage()
    {
        float damage = basePrimaryDamage; // 50
    
        // PERMANENT UPGRADE - YENİ! 💥✅
        if (PermanentUpgradeManager.Instance != null)
        {
            float damageBonus = PermanentUpgradeManager.Instance.GetPrimaryDamageBonus();
            damage += damageBonus;
        
            Debug.Log($"💥 Primary Damage: {basePrimaryDamage} + {damageBonus} = {damage}");
        }
    
        // Wave sırasındaki upgrade multiplier (UpgradeManager)
        if (UpgradeManager.Instance != null)
        {
            damage *= UpgradeManager.Instance.damageMultiplier;
        }
    
        // Triple Shot varsa hasar azalt (balance)
        if (WeaponUpgradeManager.Instance != null && 
            WeaponUpgradeManager.Instance.hasTripleShot)
        {
            damage *= WeaponUpgradeManager.Instance.tripleShotDamageMultiplier;
        }
    
        // Power Shot upgrade'i varsa hasar artar
        if (WeaponUpgradeManager.Instance != null && 
            WeaponUpgradeManager.Instance.hasPowerShot)
        {
            damage *= WeaponUpgradeManager.Instance.powerShotMultiplier;
        }
    
        return damage;
    }

    void Update()
    {
        // OYUN BAŞLAMADIYSA INPUT ALMA! 🚫
        if (GameManager.Instance != null && !GameManager.Instance.isGameStarted)
        {
            return; // Menüdeyken hiçbir şey yapma!
        }
        
        // SİLAHI MOUSE YÖNÜNE DÖNDÜR - YENİ!
        RotateWeaponTowardsMouse();
        
        if (Time.timeScale == 0f && isAiming)
        {
            isAiming = false;
            HideTrajectory();
        } 
        
        // Fire rate hesapla
        float currentFireRate = CalculateFireRate();
    
        // Cooldown sayaçları
        if (primaryCooldownTimer > 0)
        {
            primaryCooldownTimer -= Time.deltaTime;
        }
    
        // Ulti cooldown
        if (ultiCooldownTimer > 0)
        {
            ultiCooldownTimer -= Time.deltaTime;
            isUltiReady = false;
        }
        else
        { 
            if (!isUltiReady) 
            {
                isUltiReady = true;
                shakeTimer = shakeDuration;
                Debug.Log("⚡ ULTI HAZIR!");
            }
        }
        
        // ULTI BAR UI GÜNCELLE! ⚡
        if (UltiBarUI.Instance != null)
        {
            // Doluluk oranı hesapla
            float currentCooldown = CalculateFinalUltiCooldown();
            float fillRatio = 1f - Mathf.Clamp01(ultiCooldownTimer / currentCooldown);
        
            UltiBarUI.Instance.UpdateUltiFill(fillRatio, isUltiReady);
        }
    
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
        }

        UpdateWeaponVisuals();

        // AUTO-TARGET VARSA OTOMATİK ATEŞ - YENİ!
        bool hasAutoTarget = WeaponUpgradeManager.Instance != null && 
                             WeaponUpgradeManager.Instance.hasAutoTarget;

        if (hasAutoTarget)
        {
            // Otomatik ateş (aiming moduna gerek yok!)
            if (primaryCooldownTimer <= 0)
            {
                Enemy target = FindClosestEnemy();
                if (target != null)
                {
                    FirePrimary();
                }
            }
        
            // Auto-Target göstergesi
            if (showAutoTargetIndicator)
            {
                ShowAutoTargetIndicator();
            }
            
            // Ulti için sağ tık (hala manuel)
            if (Input.GetKeyDown(KeyCode.F) && isUltiReady)
            {
                FireSecondary();
            }
        }
        else
        {
            // MANUEL MOD (Auto-Target yok)
            
                UpdateTrajectory();

                // Sol tık BASILI TUTUNCA - Otomatik ateş!
                if (Input.GetMouseButton(0) && primaryCooldownTimer <= 0)
                {
                    FirePrimary();
                }

                // SAĞ TIK - İkincil ateş
                if (Input.GetKeyDown(KeyCode.F) && isUltiReady)
                {
                    FireSecondary();
                } 
        }
    }
    
    // Ulti cooldown hesapla (upgrade dahil) - YENİ FONKSIYON! ⏱️✅
    float CalculateFinalUltiCooldown()
    {
        float cooldown = ultiCooldown; // Base: 12s
    
        // PERMANENT UPGRADE - YENİ! ⏱️✅
        if (PermanentUpgradeManager.Instance != null)
        {
            float reduction = PermanentUpgradeManager.Instance.GetUltiCooldownReduction();
            cooldown -= reduction;
        
            // Minimum 5 saniye
            if (cooldown < 5f)
                cooldown = 5f;
        
            Debug.Log($"⏱️ Ulti Cooldown: {ultiCooldown} - {reduction} = {cooldown}");
        }
    
        // Wave upgrade (UpgradeManager)
        if (UpgradeManager.Instance != null)
        {
            cooldown -= UpgradeManager.Instance.ultiCooldownReduction;
        }
    
        return cooldown;
    }
    
    
    void RotateWeaponTowardsMouse()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
        float spriteOffset = -90f;
    
        // Silahı döndür
        transform.rotation = Quaternion.Euler(0, 0, angle + spriteOffset);

        // SPRITE'LARI SCALE İLE FLIP ET - YENİ! 🔄
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
    
        foreach (SpriteRenderer sr in allSprites)
        {
            if (sr != null)
            {
                Vector3 scale = sr.transform.localScale;
            
                // Sol tarafa bakıyorsa
                if (angle > 90 || angle < -90)
                {
                    scale.x = -Mathf.Abs(scale.x); // Negatif (ters)
                }
                else
                {
                    scale.x = Mathf.Abs(scale.x); // Pozitif (normal)
                }
            
                sr.transform.localScale = scale;
            }
        }
    }
    
    // Auto-Target hedef göstergesi
    void ShowAutoTargetIndicator()
    {
        Enemy target = FindClosestEnemy();
    
        if (target != null)
        {
            Vector3 targetPos = target.transform.position;
        
            // Hedefin etrafında kırmızı kare çiz (Debug)
            Debug.DrawLine(targetPos + new Vector3(-0.5f, 0.5f, 0), 
                targetPos + new Vector3(0.5f, 0.5f, 0), 
                Color.red);
            Debug.DrawLine(targetPos + new Vector3(0.5f, 0.5f, 0), 
                targetPos + new Vector3(0.5f, -0.5f, 0), 
                Color.red);
            Debug.DrawLine(targetPos + new Vector3(0.5f, -0.5f, 0), 
                targetPos + new Vector3(-0.5f, -0.5f, 0), 
                Color.red);
            Debug.DrawLine(targetPos + new Vector3(-0.5f, -0.5f, 0), 
                targetPos + new Vector3(-0.5f, 0.5f, 0), 
                Color.red);
        
            // Silahtan hedefe çizgi
            Debug.DrawLine(transform.position, targetPos, Color.yellow);
        }
    }
    
    // En tehlikeli düşmanı bul (sola en yakın = en tehlikeli)
    Enemy FindMostDangerousEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy mostDangerous = null;
        float closestToLeft = float.MaxValue; // En soldaki düşman
    
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
        
            // Silaha uzaklık kontrolü (menzil içinde mi?)
            float distanceToWeapon = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToWeapon > autoTargetRange) continue;
        
            // X pozisyonuna bak (en soldaki = en tehlikeli)
            if (enemy.transform.position.x < closestToLeft)
            {
                closestToLeft = enemy.transform.position.x;
                mostDangerous = enemy;
            }
        }
    
        return mostDangerous;
    }
    
    // Fire rate hesapla (upgrade'ler dahil)
    float CalculateFireRate()
    {
        float fireRate = basePrimaryFireRate; // 0.2
    
        // Power Shot varsa ateş hızı AZALIR (daha yavaş)
        if (WeaponUpgradeManager.Instance != null && 
            WeaponUpgradeManager.Instance.hasPowerShot)
        {
            fireRate *= 1.5f; // %50 daha yavaş (0.2 → 0.3)
        }
    
        // Rapid Fire upgrade'i varsa ateş hızı ARTAR (daha hızlı)
        if (WeaponUpgradeManager.Instance != null && 
            WeaponUpgradeManager.Instance.hasRapidFire)
        {
            fireRate *= WeaponUpgradeManager.Instance.powerShotFireRateMultiplier;
        }
    
        return fireRate;
    }
    
    void ShowOrbitalTarget()
    {
        // Mouse pozisyonunda kırmızı halka göster
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
    
        // Debug için geçici çizgi (prefab yoksa)
        Debug.DrawLine(new Vector3(mousePos.x - 0.5f, mousePos.y, 0), 
            new Vector3(mousePos.x + 0.5f, mousePos.y, 0), 
            Color.red);
        Debug.DrawLine(new Vector3(mousePos.x, mousePos.y - 0.5f, 0), 
            new Vector3(mousePos.x, mousePos.y + 0.5f, 0), 
            Color.red);
    }

    // Trajectory setup
    void SetupTrajectory()
    {
        // LineRenderer'ı kaldır, nokta sistemi kullanacağız
        trajectoryDots = new GameObject[trajectoryDotCount];
    
        for (int i = 0; i < trajectoryDotCount; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.SetParent(transform);
            Destroy(dot.GetComponent<Collider>()); // Collider gereksiz
        
            Renderer renderer = dot.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = Color.cyan;
        
            dot.SetActive(false);
            trajectoryDots[i] = dot;
        }
    }

    // Trajectory güncelle (noktalı)
    void UpdateTrajectory()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
    
        // SİLAHIN MERKEZİNDEN başla (FirePoint değil!)
        Vector3 startPos = transform.position;
        Vector3 direction = (mousePos - startPos).normalized;
    
        for (int i = 0; i < trajectoryDotCount; i++)
        {
            // Nokta pozisyonu
            Vector3 dotPos = startPos + direction * i * trajectoryDotSpacing;
            trajectoryDots[i].transform.position = dotPos;
        
            // Nokta boyutu (baştan sona küçülsün)
            float t = (float)i / trajectoryDotCount;
            float size = Mathf.Lerp(startDotSize, endDotSize, t);
            trajectoryDots[i].transform.localScale = Vector3.one * size;
        
            // Aktif et
            trajectoryDots[i].SetActive(true);
        }
    }

   // Birincil ateş
   void FirePrimary()
   {
       Vector3 startPos = transform.position;
       Vector3 direction;
    
       // AUTO-TARGET varsa en yakın düşmana nişan al
       if (WeaponUpgradeManager.Instance != null && 
           WeaponUpgradeManager.Instance.hasAutoTarget)
       {
           // Enemy target = FindClosestEnemy(); // En yakın
           Enemy target = FindMostDangerousEnemy(); // En tehlikeli
        
           if (target != null)
           {
               // Hedefe doğru nişan al
               direction = (target.transform.position - startPos).normalized;
               Debug.Log($"🎯 Auto-Target: {target.enemyType}");
           }
           else
           {
               // Hedef yoksa mouse yönüne ateş et
               Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
               mousePos.z = 0;
               direction = (mousePos - startPos).normalized;
           }
       }
       else
       {
           // Normal mod - Mouse yönüne ateş et
           Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
           mousePos.z = 0;
           direction = (mousePos - startPos).normalized;
       }

       // Triple Shot upgrade'i varsa 3 mermi, yoksa 1 mermi
       if (WeaponUpgradeManager.Instance != null && 
           WeaponUpgradeManager.Instance.hasTripleShot)
       {
           FireTripleShot(startPos, direction);
       }
       else
       {
           FireSingleProjectile(startPos, direction);
       }

       primaryCooldownTimer = CalculateFireRate();
   }
   
   // Tekli mermi ateşle
   void FireSingleProjectile(Vector3 startPos, Vector3 direction)
   {
       // MUZZLE FLASH - YENİ!
       if (HitEffectManager.Instance != null)
       {
           Vector3 flashOffset = direction * 0.5f; // 0.5 birim ileri
           Vector3 flashPos = startPos + flashOffset;
           HitEffectManager.Instance.ShowMuzzleFlash(flashPos, direction);
       }
       
       // ATEŞ SESİ - YENİ!
       if (SoundManager.Instance != null)
       {
           // Triple shot varsa zaten FireTripleShot'ta çaldık
           bool hasTriple = WeaponUpgradeManager.Instance != null && 
                            WeaponUpgradeManager.Instance.hasTripleShot;
        
           if (!hasTriple)
           {
               SoundManager.Instance.PlayShoot();
           }
       }
       
       GameObject projectile = Instantiate(primaryProjectilePrefab, startPos, Quaternion.identity);
       PrimaryProjectile projectileScript = projectile.GetComponent<PrimaryProjectile>();

       if (projectileScript != null)
       {
           projectileScript.direction = direction;
           projectileScript.speed = CalculateFinalProjectileSpeed();
           
           projectileScript.damage = CalculateFinalPrimaryDamage();
       }
   }
   
   // Projectile speed hesapla (upgrade dahil) - YENİ FONKSIYON! ⚡✅
   float CalculateFinalProjectileSpeed()
   {
       float speed = projectileSpeed; // Base: 10
    
       // PERMANENT UPGRADE - YENİ! ⚡✅
       if (PermanentUpgradeManager.Instance != null)
       {
           float speedBonus = PermanentUpgradeManager.Instance.GetProjectileSpeedBonus();
           speed += speedBonus;
        
           Debug.Log($"⚡ Projectile Speed: {projectileSpeed} + {speedBonus} = {speed}");
       }
    
       return speed;
   }
   
   // En yakın düşmanı bul
   Enemy FindClosestEnemy()
   {
       Enemy[] enemies = FindObjectsOfType<Enemy>();
       Enemy closest = null;
       float closestDistance = autoTargetRange; // Maksimum menzil
    
       Vector3 weaponPos = transform.position;
    
       foreach (Enemy enemy in enemies)
       {
           // Düşman yok edilmiş mi kontrol et
           if (enemy == null) continue;
        
           float distance = Vector3.Distance(weaponPos, enemy.transform.position);
        
           // Menzil içinde ve en yakınsa
           if (distance < closestDistance)
           {
               closestDistance = distance;
               closest = enemy;
           }
       }
    
       return closest;
   }
   
   // Üçlü mermi ateşle (orta + sağ + sol)
   void FireTripleShot(Vector3 startPos, Vector3 direction)
   {
       float angleOffset = 10f; // Yan mermilerin açısı (derece)
        
       // TRIPLE SHOT SESİ - YENİ! (Sadece bir kez çal)
       if (SoundManager.Instance != null)
       {
           SoundManager.Instance.PlayTripleShoot();
       }
       
       // 1. ORTA MERMİ (düz)
       FireSingleProjectile(startPos, direction);
    
       // 2. SAĞ MERMİ (saat yönünde dönük)
       Vector3 rightDirection = RotateVector(direction, -angleOffset);
       FireSingleProjectile(startPos, rightDirection);
    
       // 3. SOL MERMİ (saat yönünün tersine dönük)
       Vector3 leftDirection = RotateVector(direction, angleOffset);
       FireSingleProjectile(startPos, leftDirection);
    
       Debug.Log("🔫 Triple Shot ateşlendi!");
   }
   
   // Bir vektörü belirli açı kadar döndür
   Vector3 RotateVector(Vector3 vector, float angleDegrees)
   {
       float angleRadians = angleDegrees * Mathf.Deg2Rad;
    
       float cos = Mathf.Cos(angleRadians);
       float sin = Mathf.Sin(angleRadians);
    
       float newX = vector.x * cos - vector.y * sin;
       float newY = vector.x * sin + vector.y * cos;
    
       return new Vector3(newX, newY, 0).normalized;
   }
   
   // Damage hesaplama - TEK KONTROL NOKTASI!
   float CalculateFinalDamage()
   {
       float finalDamage = basePrimaryDamage;
    
       // Upgrade Manager'dan multiplier al
       if (UpgradeManager.Instance != null)
       {
           finalDamage *= UpgradeManager.Instance.damageMultiplier;
       }
    
       return finalDamage;
   }
    
    // Trajectory'yi gizle
    void HideTrajectory()
    {
        if (trajectoryDots != null)
        {
            foreach (GameObject dot in trajectoryDots)
            {
                if (dot != null)
                {
                    dot.SetActive(false);
                }
            }
        }
    }
    
    // İkincil ateş (Ulti)
    // Ulti ateş et (Orbital Burst)
    void FireSecondary()
    {
        Debug.Log("⚡ ORBITAL BURST ULTİ!");
    
        // Merkez pozisyon (oyuncu)
        Vector3 centerPos = transform.position;
    
        // 360° daire etrafında eşit açılarda mermi fırlat
        float angleStep = 360f / ultiProjectileCount;
    
        for (int i = 0; i < ultiProjectileCount; i++)
        {
            // Açıyı hesapla
            float angle = i * angleStep;
        
            // Yönü hesapla (radyan cinsinden)
            float rad = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
        
            // Projectile oluştur
            GameObject projectile = Instantiate(ultiProjectilePrefab, centerPos, Quaternion.identity);
            UltiProjectile projectileScript = projectile.GetComponent<UltiProjectile>();
        
            if (projectileScript != null)
            {
                projectileScript.direction = direction;
                projectileScript.speed = 15f;
                projectileScript.damage = 100;
            }
        
            // Muzzle flash efekti
            if (HitEffectManager.Instance != null)
            {
                HitEffectManager.Instance.ShowMuzzleFlash(centerPos, direction, new Color(1f, 0.7f, 0f));
            }
        }
    
        // Ses efekti
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUlti(); // Veya özel ses
        }
    
        // Cooldown başlat
        ultiCooldownTimer = CalculateFinalUltiCooldown();
        isUltiReady = false;
        shakeTimer = 0f; // Titreşimi durdur
    
        Debug.Log($"⚡ {ultiProjectileCount} mermi 360° saçıldı!");
    }
    
    // Silah görselini güncelle
    void UpdateWeaponVisuals()
    {
        if (weaponRenderer == null) return;
    
        if (isUltiReady)
        {
            // ULTİ HAZIR - Turuncu
            weaponRenderer.color = readyColor;
        
            // Titreşim SADECE 1 SANİYE - YENİ!
            if (shakeTimer > 0)
            {
                float shakeX = Random.Range(-shakeAmount, shakeAmount);
                float shakeY = Random.Range(-shakeAmount, shakeAmount);
                transform.localPosition = originalWeaponPos + new Vector3(shakeX, shakeY, 0);
            }
            else
            {
                // 1 saniye bitti, artık sabit
                transform.localPosition = originalWeaponPos;
            }
        }
        else
        {
            // Normal durum - Gri
            weaponRenderer.color = normalColor;
            transform.localPosition = originalWeaponPos;
        }
    }
    
    // Ulti cooldown'ını sıfırla (level değişiminde)
    public void ResetUltiCooldown()
    {
        // YENİ SİSTEM! ✅
        ultiCooldownTimer = ultiCooldown;
        isUltiReady = false;
        shakeTimer = 0f;
    
        // Silah rengini normale çevir
        UpdateWeaponVisuals();
    
        Debug.Log("⚡ Ulti cooldown sıfırlandı!");
    }
}