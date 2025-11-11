using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Kart Ayarları")]
    public GameObject cardPrefab;
    public Transform handTransform;
    
    [Header("Kart Dizilimi - Yelpaze")]
    public bool useFanLayout = true; // Yelpaze modu aktif mi?
    public float fanSpread = 15f; // Yelpaze açısı (derece)
    public float fanRadius = 2f; // Yelpaze yarıçapı
    public float cardYOffset = -3f; // Kartların Y pozisyonu
    
    [Header("Sabit El - Dinamik Yüklenecek")]
    public List<Card> handCards = new List<Card>();
    private Card.CardColor[] availableCards; // Level'e göre yüklenecek
    
    
    // 4 farklı renk kart oluştur (her renkten 1)
    void CreateHand()
    {
        Debug.Log($"🎴 CreateHand çağrıldı! availableCards: {availableCards?.Length}");
        if (availableCards == null || availableCards.Length == 0)
        {
            Debug.LogWarning("⚠️ Kullanılabilir kart yok!");
            return;
        }
    
        for (int i = 0; i < availableCards.Length; i++)
        {
            CreateCard(availableCards[i], i);
        }
    }
    
    // Yeni kart ekle (elde 4'ten az kart varsa)
    public void AddNewCard(Card.CardColor cardColor)
    {
        Debug.Log($"🎴 Yeni kart ekleniyor: {cardColor}");
    
        // NULL CHECK - YENİ!
        if (cardPrefab == null)
        {
            Debug.LogError("❌ cardPrefab NULL! Unity Editor'de CardManager'a Card Prefab atayın!");
            return;
        }
    
        if (handTransform == null)
        {
            Debug.LogError("❌ handTransform NULL! Unity Editor'de CardManager'a Hand Transform atayın!");
            return;
        }
    
        // 4 kart limiti kontrolü
        if (handCards.Count >= 4)
        {
            Debug.LogWarning("⚠️ Kart limiti dolu! Kart eklenemedi.");
            return;
        }
    
        int index = handCards.Count; // Yeni kartın indeksi
        CreateCard(cardColor, index);
    
        // Kartları yeniden hizala
        RealignCards();
    }

    // Mevcut bir kartı değiştir
    public void ReplaceCard(int index, Card.CardColor newCardColor)
    {
        Debug.Log($"🔄 Kart değiştiriliyor: Index {index} → {newCardColor}");
    
        if (index < 0 || index >= handCards.Count)
        {
            Debug.LogError("❌ Geçersiz kart indeksi!");
            return;
        }
    
        // Eski kartı yok et
        if (handCards[index] != null)
        {
            Destroy(handCards[index].gameObject);
        }
    
        // Yeni kart oluştur
        GameObject cardObj = Instantiate(cardPrefab, handTransform);
        Card card = cardObj.GetComponent<Card>();
    
        if (card != null)
        {
            card.SetColor(newCardColor);
            handCards[index] = card; // Listeye ekle
        
            // Pozisyonu ayarla
            UpdateCardPosition(index);
        }
    }
    
    // Kartları yeniden hizala (pozisyonları güncelle)
    void RealignCards()
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i] != null)
            {
                UpdateCardPosition(i);
            
                // Hover effect'e pozisyon güncellemesini bildir
                CardHoverEffect hoverEffect = handCards[i].GetComponent<CardHoverEffect>();
                if (hoverEffect != null)
                {
                    hoverEffect.UpdateOriginalTransform();
                }
            }
        }
    }
    
    void Awake()
    {
        // Hand Transform yoksa otomatik bul
        if (handTransform == null)
        {
            // Canvas altında "Hand" isminde obje ara
            GameObject handObj = GameObject.Find("Hand");
        
            if (handObj != null)
            {
                handTransform = handObj.transform;
                Debug.Log("✅ Hand Transform otomatik bulundu!");
            }
            else
            {
                Debug.LogError("❌ 'Hand' objesi bulunamadı! Lütfen oluşturun.");
            }
        }
    }
    
    void CreateCard(Card.CardColor color, int index)
    {
        Debug.Log($"🔧 CreateCard başladı - Color: {color}, Index: {index}");
    
        GameObject cardObj = Instantiate(cardPrefab, handTransform);
        Card card = cardObj.GetComponent<Card>();
    
        if (card == null)
        {
            Debug.LogError("Card component bulunamadı!");
            return;
        }
    
        card.SetColor(color);
        
        // HOTKEY EKLE - YENİ! 
        CardHotkey hotkey = cardObj.AddComponent<CardHotkey>();
        hotkey.hotkeyNumber = index + 1; // 1, 2, 3, 4
    
        // YAN YANA DİZİLİM - YENİ!
        int totalCards = availableCards.Length;
        float spacing = 3f; // Daha geniş aralık (1.0 → 1.5)
        float totalWidth = (totalCards - 1) * spacing;
        float startX = -totalWidth / 2f;
        float x = startX + (index * spacing);
    
        // Y pozisyonu sabit (altta)
        cardObj.transform.localPosition = new Vector3(x, 0, 0);
        
       // Z pozisyonu ayarla (üst üste gelmesin)
        Vector3 pos = cardObj.transform.localPosition;
        pos.z = 0; // Hepsi aynı z'de
        cardObj.transform.localPosition = pos;
    
        Debug.Log($"🎴 Kart oluşturuldu! Pos: {cardObj.transform.localPosition}");
    
        handCards.Add(card);
    }
    
    // Tek bir kartın pozisyonunu ayarla
    void UpdateCardPosition(int index)
    {
        if (index < 0 || index >= handCards.Count) return;
        if (handCards[index] == null) return;
    
        int totalCards = handCards.Count;
    
        if (useFanLayout)
        {
            // YELPAZE MODU
            CalculateFanPosition(index, totalCards);
        }
        else
        {
            // DÜZ DİZİLİM (eski sistem)
            CalculateStraightPosition(index, totalCards);
        }
    }
    
    // Yelpaze (fan) pozisyon hesaplama
    void CalculateFanPosition(int index, int totalCards)
    {
        Card card = handCards[index];
        if (card == null) return;
    
        // Orta nokta
        float centerIndex = (totalCards - 1) / 2f;
    
        // Bu kartın orta noktaya göre uzaklığı (-2, -1, 0, 1, 2 gibi)
        float offset = index - centerIndex;
    
        // Dönüş açısı (derece)
        float angle = offset * fanSpread;
    
        // X ve Y pozisyonunu hesapla (yay üzerinde)
        float angleRad = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleRad) * fanRadius;
        float y = -Mathf.Cos(angleRad) * fanRadius + cardYOffset;
    
        // Pozisyonu ayarla
        card.transform.localPosition = new Vector3(x, y, 0);
    
        // Kartı döndür (yelpaze efekti için)
        card.transform.localRotation = Quaternion.Euler(0, 0, -angle);
    
        Debug.Log($"🎴 Kart {index}: Açı={angle}°, Pos=({x:F2}, {y:F2})");
    }
    
    // Düz dizilim (eski sistem)
    void CalculateStraightPosition(int index, int totalCards)
    {
        Card card = handCards[index];
        if (card == null) return;
    
        float spacing = 3f;
        float totalWidth = (totalCards - 1) * spacing;
        float startX = -totalWidth / 2f;
        float x = startX + (index * spacing);
    
        card.transform.localPosition = new Vector3(x, cardYOffset, 0);
        card.transform.localRotation = Quaternion.identity; // Dönüş yok
    }
    
    // LevelManager'dan çağrılacak
    public void SetAvailableCards(Card.CardColor[] cards)
    {
        Debug.Log($"🎴 SetAvailableCards çağrıldı! Kart sayısı: {cards.Length}");
        availableCards = cards;
    
        // Eski kartları temizle
        ClearHand();
    
        // Yeni kartları oluştur
        CreateHand();
    }
    
    // Eski kartları temizle
    void ClearHand()
    {
        foreach (Card card in handCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        handCards.Clear();
    }
    
    
    
}