using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlacementManager : MonoBehaviour
{
    public static CardPlacementManager Instance;
    
    [Header("Referanslar")]
    public CardManager cardManager;
    
    [Header("Ayarlar")]
    public LayerMask zoneLayer; // Zone'ların layer'ı
    public float raycastDistance = 100f;
    
    private Camera mainCamera;
    private Zone currentHoveredZone = null;
    
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
        
        // CardManager'ı otomatik bul
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
        }
        
    }
    
    void Update()
    {
        // Mouse'un altındaki zone'u bul
        Zone hoveredZone = GetZoneUnderMouse();
        
        // Önceki zone'u kapat
        if (currentHoveredZone != null && currentHoveredZone != hoveredZone)
        {
            currentHoveredZone.Highlight(false);
        }
        
        // Yeni zone'u highlight et
        if (hoveredZone != null)
        {
            hoveredZone.Highlight(true);
            currentHoveredZone = hoveredZone;
            
           // Hotkey kontrolü
            CheckHotkeys(hoveredZone);
        }
        else
        {
            currentHoveredZone = null;
        }
    }

    // Mouse'un altındaki zone'u bul (açı bazlı)
Zone GetZoneUnderMouse()
{
    // Mouse pozisyonu (world space)
    Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    mousePos.z = 0;
    
    // Player pozisyonu (merkez)
    Vector3 playerPos = Vector3.zero;
    
    // Mouse'un player'a göre yönünü hesapla
    Vector3 direction = mousePos - playerPos;
    
    // Açıyı hesapla (-180 ile 180 arası)
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
    // Açıyı 0-360 aralığına çevir
    if (angle < 0) angle += 360f;
    
    // Açıya göre zone belirle
    Zone targetZone = GetZoneByAngle(angle);
    
    return targetZone;
}

// Açıya göre zone'u belirle
Zone GetZoneByAngle(float angle)
{
    // 4 yön, her biri 90° alan kaplar
    // Right: 315° - 45° (0° etrafında)
    // Top: 45° - 135° (90° etrafında)
    // Left: 135° - 225° (180° etrafında)
    // Bottom: 225° - 315° (270° etrafında)
    
    Zone[] allZones = FindObjectsOfType<Zone>();
    
    foreach (Zone zone in allZones)
    {
        if (zone.direction == Zone.ZoneDirection.Right)
        {
            // Sağ: 315° - 45° (0° merkezli)
            if (angle >= 315f || angle < 45f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Top)
        {
            // Üst: 45° - 135° (90° merkezli)
            if (angle >= 45f && angle < 135f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Left)
        {
            // Sol: 135° - 225° (180° merkezli)
            if (angle >= 135f && angle < 225f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Bottom)
        {
            // Alt: 225° - 315° (270° merkezli)
            if (angle >= 225f && angle < 315f)
            {
                return zone;
            }
        }
    }
    
    return null;
}
    
    // Hotkey kontrolü
    void CheckHotkeys(Zone zone)
    {
        // 1 tuşu - İlk kart
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            PlaceCardToZone(zone, 0);
        }
        // 2 tuşu - İkinci kart
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            PlaceCardToZone(zone, 1);
        }
        // 3 tuşu - Üçüncü kart
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            PlaceCardToZone(zone, 2);
        }
        // 4 tuşu - Dördüncü kart
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            PlaceCardToZone(zone, 3);
        }
    }
    
    // Kartı zone'a yerleştir
    void PlaceCardToZone(Zone zone, int cardIndex)
    {
        // CardManager'dan kartı al
        if (cardManager == null || cardManager.handCards == null)
        {
            Debug.LogWarning(" CardManager veya kartlar yok!");
            return;
        }
        
        // Hotkey'i gizle veya yok et
        Transform hotkeyText = transform.Find("HotkeyText");
        if (hotkeyText != null)
        {
            Destroy(hotkeyText.gameObject);
        }

        // Kart indeksi geçerli mi?
        if (cardIndex < 0 || cardIndex >= cardManager.handCards.Count)
        {
            Debug.LogWarning($" Kart {cardIndex + 1} elde yok!");
            return;
        }
        
        Card card = cardManager.handCards[cardIndex];
        
        if (card == null)
        {
            Debug.LogWarning($"Kart {cardIndex + 1} null!");
            return;
        }
        
        // Kart cooldown'da mı?
        if (card.isPlaceOnCooldown)
        {
            Debug.LogWarning($" {card.cardColor} cooldown'da!");
            
            StartCoroutine(FlashCard(card));
            return;
        }
        
        // Zone'a yerleştirmeyi dene
        bool placed = zone.TryPlaceCard(card);
        
        if (placed)
        {
            // SES EFEKTİ - YENİ! ✅
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCardPlace();
            }
            Debug.Log($"✅ {card.cardColor} kart Zone {zone.zoneIndex}'a yerleştirildi! (Hotkey: {cardIndex + 1})");
        }
        else
        {
            Debug.LogWarning($" {card.cardColor} kart yerleştirilemedi!");
        }
        
        // COROUTINE EKLE - CLASS İÇİNE! ✅
        IEnumerator FlashCard(Card card)
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
    
            Color original = sr.color;
    
            // Kırmızı flash
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }
}