using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum ZoneDirection
    {
        Top,    // Üst
        Right,  // Sağ
        Bottom, // Alt
        Left    // Sol
    }
    
    [Header("Bölge Ayarları")]
    public ZoneDirection direction;
    public int zoneIndex; // 0, 1, 2, 3
    
    [Header("Lightning - Chain")]
    public GameObject chainLightningProjectilePrefab; // Yeni prefab
    public float lightningFireRate = 3f; // 3 saniyede bir (eskiden 2s)
    
    [Header("Kart Sistemi")]
    public Card placedCard; // Bu bölgeye yerleştirilmiş kart
    public Transform cardSlot; // Kartın duracağı pozisyon
    
    [Header("Highlight")]
    public bool isHighlighted = false;
    public GameObject highlightVisual;
    private SpriteRenderer zoneRenderer;
    private Color normalColor;
    public Color highlightColor = new Color(1f, 1f, 0.5f, 0.3f); // Sarı, yarı saydam
    
    // Buff sistemi (Lane'den kopyalandı)
    public bool hasSlowBuff = false;
    public float slowMultiplier = 0.5f;
    public bool hasHealBuff = false;
    public bool hasTurretBuff = false;
    public float turretFireRate = 1f;
    private float turretTimer = 0f;
    public GameObject turretProjectilePrefab;
    public bool hasLightningBuff = false;
    private float lightningTimer = 0f;
    public GameObject lightningProjectilePrefab;
    
    // Kart süresi
    public float cardDuration = 30f;
    private float cardTimer = 0f;
    private bool hasActiveCard = false;
    
    
    void Start()
    {
        // Zone renderer'ı al (eğer varsa)
        zoneRenderer = GetComponent<SpriteRenderer>();
    
        if (zoneRenderer != null)
        {
            normalColor = zoneRenderer.color;
        }
        
        // Highlight visual'ı gizle
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(false);
        }
    }
    
    // Zone'u highlight et
    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
    
        // Highlight visual kullan (varsa)
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(highlight);
        }
        // Yoksa sprite renderer'ı değiştir
        else if (zoneRenderer != null)
        {
            zoneRenderer.color = highlight ? highlightColor : normalColor;
        }
    }
    
    void Update()
    {
        // Turret buff aktifse
        if (hasTurretBuff)
        {
            TurretUpdate();
        }
        
        // Lightning buff aktifse
        if (hasLightningBuff)
        {
            LightningUpdate();
        }
        
        // Kart süresi kontrolü
        if (hasActiveCard && placedCard != null)
        {
            cardTimer -= Time.deltaTime;
            UpdateCardFade();
            
            if (cardTimer <= 0)
            {
                ExpireCard();
            }
        }
    }
    
    // Zone'u temizle (wave bitince)
    public void ClearZone()
    {
        if (placedCard != null)
        {
            Debug.Log($"🧹 Zone {zoneIndex} temizleniyor - Kart: {placedCard.cardColor}");
        
            // Kartı yok et
            Destroy(placedCard.gameObject);
        
            // Referansı temizle
            placedCard = null;
        
            // Buff'ı kaldır
            RemoveBuff();
        
            // Timer'ı sıfırla
            hasActiveCard = false;
            cardTimer = 0f;
        }
    }
    
 // Kart yerleştir
    public bool TryPlaceCard(Card card)
    {
        if (card.isPlaceOnCooldown)
            return false;

        if (placedCard != null)
            return false;
        
        // ORİJİNAL KARTI COOLDOWN'A SOK - ÖNCE! ✅
        card.StartPlaceCooldown();

        GameObject cardCopy = Instantiate(card.gameObject);
        Card cardCopyScript = cardCopy.GetComponent<Card>();

        // Kartı bölgenin card slot pozisyonuna yerleştir
        if (cardSlot != null)
        {
            cardCopy.transform.position = cardSlot.position;
        }
        else
        {
            cardCopy.transform.position = transform.position;
        }
        
        cardCopy.transform.localScale = card.transform.localScale;

        //DraggableCard draggable = cardCopy.GetComponent<DraggableCard>();
        //if (draggable != null)
        //{
        //    Destroy(draggable);
        //}

        placedCard = cardCopyScript;
        ApplyCardBuff(cardCopyScript.cardColor);
        
        hasActiveCard = true;
        cardTimer = cardDuration;
        
        // Ses
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCardPlace();
        }
        
        // KART İSMİ GÖSTERGESİ EKLE - YENİ! (Opsiyonel)
        CreateCardLabel(cardCopyScript.cardColor);

        return true;
    }
    
    // Kart label'ı oluştur
    void CreateCardLabel(Card.CardColor color)
    {
        // TextMeshPro ile kart ismi göster
        GameObject labelObj = new GameObject("CardLabel");
        labelObj.transform.SetParent(placedCard.transform);
        labelObj.transform.localPosition = new Vector3(0, 0.7f, 0);
    
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = GetCardName(color);
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    
        // Karakteri kameraya baktır
        textMesh.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
    }

// Kart ismini al
    string GetCardName(Card.CardColor color)
    {
        switch (color)
        {
            case Card.CardColor.Red:
                return "TURRET";
            case Card.CardColor.Blue:
                return "SLOW";
            case Card.CardColor.Green:
                return "HEAL";
            case Card.CardColor.Yellow:
                return "CHAIN";
            default:
                return "???";
        }
    }
    
    // Buff uygula
    void ApplyCardBuff(Card.CardColor color)
    {
        switch (color)
        {
            case Card.CardColor.Blue:
                hasSlowBuff = true;
                Debug.Log("Zone " + zoneIndex + " - Mavi buff aktif!");
                break;
            case Card.CardColor.Green:
                hasHealBuff = true;
                Debug.Log("Zone " + zoneIndex + " - Yeşil buff aktif!");
                break;
            case Card.CardColor.Red:
                hasTurretBuff = true;
                turretTimer = 0f;
                Debug.Log("Zone " + zoneIndex + " - Kırmızı buff aktif!");
                break;
            case Card.CardColor.Yellow:
                hasLightningBuff = true;
                lightningTimer = 0f;
                Debug.Log("Zone " + zoneIndex + " - Sarı buff aktif!");
                break;
        }
    }
    
    // Buff'ı kaldır
    public void RemoveBuff()
    {
        hasSlowBuff = false;
        hasHealBuff = false;
        hasTurretBuff = false;
        hasLightningBuff = false;
    }
    
    // Kartı kaldır
    public void RemoveCard()
    {
        placedCard = null;
        hasActiveCard = false;
        cardTimer = 0f;
        RemoveBuff();
    }
    
    // Kart süresi doldu
    void ExpireCard()
    {
        if (placedCard != null)
        {
            Debug.Log($"Zone {zoneIndex} - Kart süresi doldu!");
            Destroy(placedCard.gameObject);
            RemoveCard();
        }
    }
    
    // Kart fade efekti
    void UpdateCardFade()
    {
        if (placedCard == null) return;
        
        float remainingPercent = cardTimer / cardDuration;
        float alpha = Mathf.Lerp(0.2f, 1f, remainingPercent);
        
        SpriteRenderer cardRenderer = placedCard.GetComponent<SpriteRenderer>();
        if (cardRenderer != null)
        {
            Color currentColor = cardRenderer.color;
            currentColor.a = alpha;
            cardRenderer.color = currentColor;
        }
    }
    
    // Turret güncelleme
    void TurretUpdate()
    {
        turretTimer += Time.deltaTime;
        
        if (turretTimer >= turretFireRate)
        {
            turretTimer = 0f;
            TurretFire();
        }
    }
    
    // Turret ateş et
    void TurretFire()
    {
        Enemy target = FindClosestEnemyInZone();
        
        if (target != null && placedCard != null)
        {
            Vector3 turretPos = placedCard.transform.position;
            Vector3 direction = (target.transform.position - turretPos).normalized;
            
            if (HitEffectManager.Instance != null)
            {
                HitEffectManager.Instance.ShowMuzzleFlash(turretPos, direction);
            }
            
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayTurretShoot();
            }
            
            GameObject projectile = Instantiate(turretProjectilePrefab, turretPos, Quaternion.identity);
            TurretProjectile projectileScript = projectile.GetComponent<TurretProjectile>();
            
            if (projectileScript != null)
            {
                projectileScript.target = target.transform;
    
                // TURRET HASARINI HESAPLA - YENİ!
                int turretDamage = CalculateTurretDamage();
                projectileScript.damage = turretDamage;
    
                Debug.Log($"🔴 Turret ateş etti! Hasar: {turretDamage}");
            }
        }
    }
    
    // Turret hasarını hesapla (upgrade dahil)
    int CalculateTurretDamage()
    {
        int baseDamage = 25; // Base turret hasarı
    
        // Upgrade var mı kontrol et
        if (UpgradeManager.Instance != null)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * UpgradeManager.Instance.turretDamageMultiplier);
        }
    
        return baseDamage;
    }
    
    // Bu bölgedeki en yakın düşmanı bul
    Enemy FindClosestEnemyInZone()
    {
        if (placedCard == null) return null;
        
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Enemy enemy in enemies)
        {
            // Aynı bölgede mi?
            if (enemy.zoneIndex == zoneIndex)
            {
                float distance = Vector3.Distance(placedCard.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
        }
        
        return closest;
    }
    
    // Lightning güncelleme
    void LightningUpdate()
    {
        lightningTimer += Time.deltaTime;
        
        if (lightningTimer >= lightningFireRate)
        {
            lightningTimer = 0f;
            LightningFire();
        }
    }
    
    // Lightning ateş et (en uzaktaki düşmana)
    // Lightning ateş et (Chain Lightning)
    void LightningFire()
    {
        Enemy target = FindFarthestEnemyInZone();
    
        if (target != null && placedCard != null)
        {
            Vector3 lightningPos = placedCard.transform.position;
            Vector3 direction = (target.transform.position - lightningPos).normalized;
        
            // Muzzle flash
            if (HitEffectManager.Instance != null)
            {
                HitEffectManager.Instance.ShowMuzzleFlash(lightningPos, direction, Color.yellow);
            }
        
            // CHAIN LIGHTNING PROJECTILE - YENİ!
            GameObject projectile = Instantiate(chainLightningProjectilePrefab, lightningPos, Quaternion.identity);
            ChainLightningProjectile projectileScript = projectile.GetComponent<ChainLightningProjectile>();
        
            if (projectileScript != null)
            {
                projectileScript.Initialize(target.transform, zoneIndex);
            }
        
            Debug.Log($"⚡ Chain Lightning başladı! (Zone {zoneIndex}) → {target.gameObject.name}");
        }
    }

// Bu bölgedeki en uzaktaki düşmanı bul
    Enemy FindFarthestEnemyInZone()
    {
        if (placedCard == null) return null;
    
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy farthest = null;
        float farthestDistance = 0f;
    
        Vector3 center = Vector3.zero; // Merkez
    
        foreach (Enemy enemy in enemies)
        {
            // Aynı bölgede mi?
            if (enemy.zoneIndex == zoneIndex)
            {
                float distanceFromCenter = Vector3.Distance(center, enemy.transform.position);
                if (distanceFromCenter > farthestDistance)
                {
                    farthestDistance = distanceFromCenter;
                    farthest = enemy;
                }
            }
        }
    
        return farthest;
    }
    
}