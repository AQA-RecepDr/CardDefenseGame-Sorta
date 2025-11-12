using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum ZoneDirection
    {
        Top,    // Ust
        Right,  // Sag
        Bottom, // Alt
        Left    // Sol
    }
    
    [Header("Bolge Ayarlari")]
    public ZoneDirection direction;
    public int zoneIndex; // 0, 1, 2, 3
    
    [Header("Lightning - Chain")]
    public GameObject chainLightningProjectilePrefab; // Yeni prefab
    public float lightningFireRate = 3f; // 3 saniyede bir (eskiden 2s)
    
    [Header("Kart Sistemi")]
    public Card placedCard; // Bu bolgeye yerlestirilmis kart
    public Transform cardSlot; // Kartin duracagi pozisyon
    
    [Header("Card Formation - Visual")]
    public GameObject cardFormationPrefab; // Prefab (opsiyonel)
    private GameObject activeFormation; // Spawn edilmiş formation
    
    [Header("Formation Position")]
    public float formationDistance = 0.8f; // Karaktere mesafe (küçült = yakınlaş)
    public Vector2 formationSize = new Vector2(1.5f, 2f); // Diamond boyutu
    public Sprite formationSprite;
    
    [Header("Highlight")]
    public bool isHighlighted = false;
    public GameObject highlightVisual;
    private SpriteRenderer zoneRenderer;
    private Color normalColor;
    public Color highlightColor = new Color(1f, 1f, 0.5f, 0.3f); // Sari, yari saydam
    
    // Buff sistemi (Lane'den kopyalandi)
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
    
    // Kart suresi
    public float cardDuration = 30f;
    private float cardTimer = 0f;
    private bool hasActiveCard = false;
    
    // Neon Highlight referansi
    private NeonZoneHighlight neonHighlight;
    
    
    void Start()
    {
        // Zone renderer'i al (eger varsa)
        zoneRenderer = GetComponent<SpriteRenderer>();
    
        if (zoneRenderer != null)
        {
            normalColor = zoneRenderer.color;
        }
        
        // Highlight visual'i gizle
        if (highlightVisual != null)
        {
            highlightVisual.SetActive(false);
        }
        
        // NeonZoneHighlight component'ini bul
        neonHighlight = GetComponent<NeonZoneHighlight>();
    }
    
    // Zone'u highlight et (renk ile)
    public void Highlight(bool highlight, Color? color = null)
    {
        isHighlighted = highlight;
        
        // Highlight visual kullan (ZoneHighlightEffect)
        if (highlightVisual != null)
        {
            ZoneHighlightEffect effect = highlightVisual.GetComponent<ZoneHighlightEffect>();

            if (effect != null)
            {
                if (highlight)
                {
                    Color highlightColor = color ?? Color.white;
                    effect.Show(highlightColor);
                }
                else
                {
                    effect.Hide();
                }
            }
            else
            {
                // Eski sistem (fallback)
                highlightVisual.SetActive(highlight);
            }
        }
        
        // Yoksa sprite renderer'i degistir
        if (zoneRenderer != null && highlightVisual == null)
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
        
        // Kart suresi kontrolu
        if (hasActiveCard && placedCard != null)
        {
            cardTimer -= Time.deltaTime;
            UpdateCardFade();
            
            // KART AKTIFKEN HIGHLIGHT'I KORU!
            UpdateCardHighlight();
            
            // FORMATION'U GÜNCELLE
            UpdateFormationPosition();
            
            if (cardTimer <= 0)
            {
                ExpireCard();
            }
        }
    }
    
    /// <summary>
    /// Kart aktifken highlight'i guncelle (kart rengiyle)
    /// </summary>
    void UpdateCardHighlight()
    {
        if (neonHighlight != null && placedCard != null)
        {
            Color cardColor = GetCardColorForHighlight(placedCard.cardColor);
            neonHighlight.SetHighlight(true, cardColor);
        }
    }
    
    /// <summary>
    /// Kart rengini highlight icin Color'a cevir
    /// </summary>
    Color GetCardColorForHighlight(Card.CardColor cardColor)
    {
        switch (cardColor)
        {
            case Card.CardColor.Red:
                return new Color(1f, 0.2f, 0.2f, 0.8f); // Kirmizi
            case Card.CardColor.Blue:
                return new Color(0.2f, 0.5f, 1f, 0.8f); // Mavi
            case Card.CardColor.Green:
                return new Color(0.2f, 1f, 0.4f, 0.8f); // Yesil
            case Card.CardColor.Yellow:
                return new Color(1f, 1f, 0.2f, 0.8f); // Sari
            case Card.CardColor.Purple:
                return new Color(0.8f, 0.2f, 1f, 0.8f); // Mor
            case Card.CardColor.Orange:
                return new Color(1f, 0.6f, 0f, 0.8f); // Turuncu
            default:
                return Color.white;
        }
    }
    
    // Zone'u temizle (wave bitince)
    public void ClearZone()
    {
        if (placedCard != null)
        {
            Debug.Log($"Zone {zoneIndex} temizleniyor - Kart: {placedCard.cardColor}");
        
            // Karti yok et
            Destroy(placedCard.gameObject);
        
            // Referansi temizle
            placedCard = null;
        
            // Buff'i kaldir
            RemoveBuff();
        
            // Timer'i sifirla
            hasActiveCard = false;
            cardTimer = 0f;
            
            // HIGHLIGHT'I KAPAT!
            if (neonHighlight != null)
            {
                neonHighlight.ResetToIdle();
            }
            
            DestroyFormation();
        }
    }
    
 // Kart yerlestir
    public bool TryPlaceCard(Card card)
    {
        if (card.isPlaceOnCooldown)
            return false;

        if (placedCard != null)
            return false;
        
        // ORIGINAL KARTI COOLDOWN'A SOK - ONCE!
        card.StartPlaceCooldown();

        GameObject cardCopy = Instantiate(card.gameObject);
        Card cardCopyScript = cardCopy.GetComponent<Card>();

        // Karti bolgenin card slot pozisyonuna yerlestir
        if (cardSlot != null)
        {
            cardCopy.transform.position = cardSlot.position;
        }
        else
        {
            cardCopy.transform.position = transform.position;
        }
        
        cardCopy.transform.localScale = card.transform.localScale;

        placedCard = cardCopyScript;
        ApplyCardBuff(cardCopyScript.cardColor);
        
        // KARTI GİZLE - YENİ! 👻
        SpriteRenderer cardRenderer = cardCopy.GetComponent<SpriteRenderer>();
        if (cardRenderer != null)
        {
            cardRenderer.enabled = false; // Kartı görünmez yap
        }
        
        hasActiveCard = true;
        cardTimer = cardDuration;
        
        // Ses
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCardPlace();
        }
        
        // KART ISMI GOSTERGESI EKLE - YENI! (Opsiyonel)
        //CreateCardLabel(cardCopyScript.cardColor);
        
        CreateCardFormation(cardCopyScript.cardColor);
        
        // HIGHLIGHT'I AKTIF ET - KART RENGIYLE!
        if (neonHighlight != null)
        {
            Color cardColor = GetCardColorForHighlight(cardCopyScript.cardColor);
            neonHighlight.SetHighlight(true, cardColor);
            Debug.Log($"Zone {zoneIndex} - Kart yerlestirildi, highlight aktif: {cardCopyScript.cardColor}");
        }

        return true;
    }
    
    // Kart label'i olustur
    void CreateCardLabel(Card.CardColor color)
    {
        // TextMeshPro ile kart ismi goster
        GameObject labelObj = new GameObject("CardLabel");
        labelObj.transform.SetParent(placedCard.transform);
        labelObj.transform.localPosition = new Vector3(0, 0.7f, 0);
    
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = GetCardName(color);
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    
        // Karakteri kameraya baktir
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
                Debug.Log("Zone " + zoneIndex + " - Yesil buff aktif!");
                break;
            case Card.CardColor.Red:
                hasTurretBuff = true;
                turretTimer = 0f;
                Debug.Log("Zone " + zoneIndex + " - Kirmizi buff aktif!");
                break;
            case Card.CardColor.Yellow:
                hasLightningBuff = true;
                lightningTimer = 0f;
                Debug.Log("Zone " + zoneIndex + " - Sari buff aktif!");
                break;
        }
    }
    
    // Buff'i kaldir
    public void RemoveBuff()
    {
        hasSlowBuff = false;
        hasHealBuff = false;
        hasTurretBuff = false;
        hasLightningBuff = false;
    }
    
    // Karti kaldir
    public void RemoveCard()
    {
        placedCard = null;
        hasActiveCard = false;
        cardTimer = 0f;
        RemoveBuff();
        
        // HIGHLIGHT'I KAPAT!
        if (neonHighlight != null)
        {
            neonHighlight.ResetToIdle();
        }
        DestroyFormation();
    }
    
    // Kart suresi doldu
    void ExpireCard()
    {
        if (placedCard != null)
        {
            Debug.Log($"Zone {zoneIndex} - Kart suresi doldu!");
            Destroy(placedCard.gameObject);
            RemoveCard();
            DestroyFormation();
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
    
    // Turret guncelleme
    void TurretUpdate()
    {
        turretTimer += Time.deltaTime;
        
        if (turretTimer >= turretFireRate)
        {
            turretTimer = 0f;
            TurretFire();
        }
    }
    
    // Turret ates et
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
    
                // TURRET HASARINI HESAPLA
                int turretDamage = CalculateTurretDamage();
                projectileScript.damage = turretDamage;
    
                Debug.Log($"Turret ates etti! Hasar: {turretDamage}");
            }
        }
    }
    
    // Turret hasarini hesapla (upgrade dahil)
    int CalculateTurretDamage()
    {
        int baseDamage = 25; // Base turret hasari
    
        // Upgrade var mi kontrol et
        if (UpgradeManager.Instance != null)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * UpgradeManager.Instance.turretDamageMultiplier);
        }
    
        return baseDamage;
    }
    
    // Bu bolgedeki en yakin dusmani bul
    Enemy FindClosestEnemyInZone()
    {
        if (placedCard == null) return null;
        
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Enemy enemy in enemies)
        {
            // Ayni bolgede mi?
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
    
    // Lightning guncelleme
    void LightningUpdate()
    {
        lightningTimer += Time.deltaTime;
        
        if (lightningTimer >= lightningFireRate)
        {
            lightningTimer = 0f;
            LightningFire();
        }
    }
    
    // Lightning ates et (en uzaktaki dusmana)
    // Lightning ates et (Chain Lightning)
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
        
            // CHAIN LIGHTNING PROJECTILE
            GameObject projectile = Instantiate(chainLightningProjectilePrefab, lightningPos, Quaternion.identity);
            ChainLightningProjectile projectileScript = projectile.GetComponent<ChainLightningProjectile>();
        
            if (projectileScript != null)
            {
                projectileScript.Initialize(target.transform, zoneIndex);
            }
        
            Debug.Log($"Chain Lightning basladi! (Zone {zoneIndex}) -> {target.gameObject.name}");
        }
    }

// Bu bolgedeki en uzaktaki dusmani bul
    Enemy FindFarthestEnemyInZone()
    {
        if (placedCard == null) return null;
    
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy farthest = null;
        float farthestDistance = 0f;
    
        Vector3 center = Vector3.zero; // Merkez
    
        foreach (Enemy enemy in enemies)
        {
            // Ayni bolgede mi?
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
    
    // Card formation oluştur (visual diamond)
void CreateCardFormation(Card.CardColor cardColor)
{
    // Formation objesi oluştur
    GameObject formationObj = new GameObject("CardFormation");
    formationObj.transform.SetParent(transform);
    
    // Pozisyon: Karaktere yakın
    Vector3 formationPos = GetFormationPosition();
    formationObj.transform.position = formationPos;
    
    // CardFormation component ekle
    CardFormation formation = formationObj.AddComponent<CardFormation>();
    
    // Renk ayarla
    formation.formationColor = GetCardColorForFormation(cardColor);
    
    formation.diamondSize = formationSize;
    formation.customSprite = formationSprite;
    
    // Referansı sakla
    activeFormation = formationObj;
}

// Formation pozisyonu (karaktere yakın)
Vector3 GetFormationPosition()
{
    // Merkez (karakter) pozisyonu
    Vector3 center = Vector3.zero;
    
    // Zone yönüne göre offset
    switch (direction)
    {
        case ZoneDirection.Top:
            return center + Vector3.up * formationDistance; // ← DİNAMİK!
        case ZoneDirection.Right:
            return center + Vector3.right * formationDistance;
        case ZoneDirection.Bottom:
            return center + Vector3.down * formationDistance;
        case ZoneDirection.Left:
            return center + Vector3.left * formationDistance;
        default:
            return center;
    }
}

// Formation rotation (zone yönü)
float GetFormationRotation()
{
    switch (direction)
    {
        case ZoneDirection.Top:
            return 45f; // Yukarı bakan diamond
        case ZoneDirection.Right:
            return 135f; // Sağa bakan
        case ZoneDirection.Bottom:
            return 225f; // Aşağı bakan
        case ZoneDirection.Left:
            return 315f; // Sola bakan
        default:
            return 45f;
    }
}

    // Kart rengini formation color'a çevir
    Color GetCardColorForFormation(Card.CardColor cardColor)
    { 
        switch (cardColor)
        {
        case Card.CardColor.Red:
            return new Color(1f, 0.2f, 0.2f, 0.8f);
        case Card.CardColor.Blue:
            return new Color(0.2f, 0.5f, 1f, 0.8f);
        case Card.CardColor.Green:
            return new Color(0.2f, 1f, 0.4f, 0.8f);
        case Card.CardColor.Yellow:
            return new Color(1f, 1f, 0.2f, 0.8f);
        default:
            return Color.white;
        }
    }

    // Formation'u yok et (animasyonlu)
    void DestroyFormation()
    {
        if (activeFormation != null) 
        {
            CardFormation formation = activeFormation.GetComponent<CardFormation>();
            if (formation != null)
            {
                formation.PlayDespawnAnimation(); // Animasyonlu yok olma
            }
            else
            {
                Destroy(activeFormation); // Direkt yok ol
            }
        
            activeFormation = null;
        }
    }
    
    // Formation pozisyon/boyutunu güncelle (Inspector değişikliklerini uygula)
    void UpdateFormationPosition()
    {
        if (activeFormation != null)
        {
            // Pozisyonu güncelle
            activeFormation.transform.position = GetFormationPosition();
        
            // Boyutu güncelle
            CardFormation formation = activeFormation.GetComponent<CardFormation>();
            if (formation != null)
            {
                formation.diamondSize = formationSize;
                // Scale'i hemen uygula
                activeFormation.transform.localScale = new Vector3(formationSize.x, formationSize.y, 1f);
            }
        }
    }
    
    // Inspector'da değer değişince çağrılır
    void OnValidate()
    {
        // Play mode'dayken ve formation varsa güncelle
        if (Application.isPlaying && activeFormation != null)
        {
            UpdateFormationPosition();
        }
    }

}