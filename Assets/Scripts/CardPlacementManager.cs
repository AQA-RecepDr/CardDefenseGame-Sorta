using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlacementManager : MonoBehaviour
{
    public static CardPlacementManager Instance;
    
    [Header("Referanslar")]
    public CardManager cardManager;
    
    [Header("Ayarlar")]
    public LayerMask zoneLayer; // Zone'larin layer'i
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
        
        // CardManager'i otomatik bul
        if (cardManager == null)
        {
            cardManager = FindObjectOfType<CardManager>();
        }
    }
    
    void Update()
    {
        // Mouse'un altindaki zone'u bul
        Zone hoveredZone = GetZoneUnderMouse();
    
        // Suruklenen karti bul
        Card draggedCard = GetDraggedCard();
    
        // Onceki zone'un highlight'ini kapat
        if (currentHoveredZone != null && currentHoveredZone != hoveredZone)
        {
            NeonZoneHighlight prevHighlight = currentHoveredZone.GetComponent<NeonZoneHighlight>();
            if (prevHighlight != null)
            {
                prevHighlight.SetHighlight(false);
            }
        }
    
        // Yeni zone'u highlight et
        if (hoveredZone != null)
        {
            NeonZoneHighlight highlight = hoveredZone.GetComponent<NeonZoneHighlight>();
            
            if (highlight != null)
            {
                // Eger kart surukleniyorsa renk ver, yoksa beyaz
                if (draggedCard != null)
                {
                    Color cardColor = GetCardColor(draggedCard);
                    highlight.SetHighlight(true, cardColor);
                }
                else
                {
                    highlight.SetHighlight(true); // Varsayilan hover rengi
                }
            }
        
            currentHoveredZone = hoveredZone;
        
            // Hotkey kontrolu
            CheckHotkeys(hoveredZone);
        }
        else
        {
            currentHoveredZone = null;
        }
    }
    
    // Suruklenen karti bul
    Card GetDraggedCard()
    {
        DraggableCard[] allDraggable = FindObjectsOfType<DraggableCard>();
    
        foreach (DraggableCard draggable in allDraggable)
        {
            if (draggable == null) continue; // NULL CHECK
        
            // isDragging artik public, direkt kontrol et!
            if (draggable.isDragging)
            {
                return draggable.GetComponent<Card>();
            }
        }
    
        return null;
    }
    
    // Kart rengini Unity Color'a cevir
    Color GetCardColor(Card card)
    {
        switch (card.cardColor)
        {
            case Card.CardColor.Red:
                return new Color(1f, 0.2f, 0.2f);
            case Card.CardColor.Blue:
                return new Color(0.2f, 0.5f, 1f);
            case Card.CardColor.Green:
                return new Color(0.2f, 1f, 0.4f);
            case Card.CardColor.Yellow:
                return new Color(1f, 1f, 0.2f);
            case Card.CardColor.Purple:
                return new Color(0.8f, 0.2f, 1f);
            case Card.CardColor.Orange:
                return new Color(1f, 0.6f, 0f);
            default:
                return Color.white;
        }
    }

    // Mouse'un altindaki zone'u bul (aci bazli)
Zone GetZoneUnderMouse()
{
    // Mouse pozisyonu (world space)
    Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    mousePos.z = 0;
    
    // Player pozisyonu (merkez)
    Vector3 playerPos = Vector3.zero;
    
    // Mouse'un player'a gore yonunu hesapla
    Vector3 direction = mousePos - playerPos;
    
    // Aciyi hesapla (-180 ile 180 arasi)
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
    // Aciyi 0-360 araligina cevir
    if (angle < 0) angle += 360f;
    
    // Aciya gore zone belirle
    Zone targetZone = GetZoneByAngle(angle);
    
    return targetZone;
}

// Aciya gore zone'u belirle
Zone GetZoneByAngle(float angle)
{
    // 4 yon, her biri 90 derece alan kaplar
    // Right: 315 - 45 (0 etrafinda)
    // Top: 45 - 135 (90 etrafinda)
    // Left: 135 - 225 (180 etrafinda)
    // Bottom: 225 - 315 (270 etrafinda)
    
    Zone[] allZones = FindObjectsOfType<Zone>();
    
    foreach (Zone zone in allZones)
    {
        if (zone.direction == Zone.ZoneDirection.Right)
        {
            // Sag: 315 - 45 (0 merkezli)
            if (angle >= 315f || angle < 45f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Top)
        {
            // Ust: 45 - 135 (90 merkezli)
            if (angle >= 45f && angle < 135f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Left)
        {
            // Sol: 135 - 225 (180 merkezli)
            if (angle >= 135f && angle < 225f)
            {
                return zone;
            }
        }
        else if (zone.direction == Zone.ZoneDirection.Bottom)
        {
            // Alt: 225 - 315 (270 merkezli)
            if (angle >= 225f && angle < 315f)
            {
                return zone;
            }
        }
    }
    
    return null;
}
    
    // Hotkey kontrolu
    void CheckHotkeys(Zone zone)
    {
        // 1 tusu - Ilk kart
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            PlaceCardToZone(zone, 0);
        }
        // 2 tusu - Ikinci kart
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            PlaceCardToZone(zone, 1);
        }
        // 3 tusu - Ucuncu kart
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            PlaceCardToZone(zone, 2);
        }
        // 4 tusu - Dorduncu kart
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            PlaceCardToZone(zone, 3);
        }
    }
    
    // Karti zone'a yerlestir
    void PlaceCardToZone(Zone zone, int cardIndex)
    {
        // CardManager'dan karti al
        if (cardManager == null || cardManager.handCards == null)
        {
            Debug.LogWarning("CardManager veya kartlar yok!");
            return;
        }
        
        // Hotkey'i gizle veya yok et
        Transform hotkeyText = transform.Find("HotkeyText");
        if (hotkeyText != null)
        {
            Destroy(hotkeyText.gameObject);
        }

        // Kart indeksi gecerli mi?
        if (cardIndex < 0 || cardIndex >= cardManager.handCards.Count)
        {
            Debug.LogWarning($"Kart {cardIndex + 1} elde yok!");
            return;
        }
        
        Card card = cardManager.handCards[cardIndex];
        
        if (card == null)
        {
            Debug.LogWarning($"Kart {cardIndex + 1} null!");
            return;
        }
        
        // Kart cooldown'da mi?
        if (card.isPlaceOnCooldown)
        {
            Debug.LogWarning($"{card.cardColor} cooldown'da!");
            
            StartCoroutine(FlashCard(card));
            return;
        }
        
        // Zone'a yerlestirmeyi dene
        bool placed = zone.TryPlaceCard(card);
        
        if (placed)
        {
            // SES EFEKTI
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCardPlace();
            }
            Debug.Log($"✅ {card.cardColor} kart Zone {zone.zoneIndex}'a yerlestirildi! (Hotkey: {cardIndex + 1})");
        }
        else
        {
            Debug.LogWarning($"{card.cardColor} kart yerlestirilemedi!");
        }
        
        // COROUTINE EKLE
        IEnumerator FlashCard(Card card)
        {
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;
    
            Color original = sr.color;
    
            // Kirmizi flash
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }
}