using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class RewardScreen : MonoBehaviour
{
    public static RewardScreen Instance;
    
    [Header("UI Referansları")]
    public GameObject rewardPanel;
    public TextMeshProUGUI titleText;
    public GameObject[] upgradeButtons; // 3 buton
    
    private List<UpgradeData> currentOffers;
    
    // YENİ - Kart seçimi için
    public enum RewardType
    {
        Upgrade,  // Pasif upgrade
        Card,     // Kart ekleme
        Weapon    // Silah geliştirme (ileride)
    }

    private RewardType currentRewardType;
    
    
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
        HideRewardScreen();
    }
  
    // Kart seçim ekranını göster
    public void ShowCardSelection()
    {
        Debug.Log("Kart seçim ekranı gösteriliyor...");
        
        currentRewardType = RewardType.Card;
    
        rewardPanel.SetActive(true);
        titleText.text = "KART SEÇ";
        
        Time.timeScale = 0f;
    
        // Rastgele 3 kart al
        List<Card.CardColor> availableCards = GetAvailableCards();
        List<Card.CardColor> offeredCards = GetRandomCards(availableCards, 3);
    
        // Butonları güncelle
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < offeredCards.Count)
            {
                SetupCardButton(upgradeButtons[i], offeredCards[i], i);
                upgradeButtons[i].SetActive(true);
            }
            else
            {
                upgradeButtons[i].SetActive(false);
            }
        }
    
        // Oyunu durdur
        Time.timeScale = 0f;
    }
    
    // Silah upgrade seçim ekranını göster
    public void ShowWeaponUpgradeSelection()
    {
        Debug.Log("⚔️ Silah upgrade seçim ekranı gösteriliyor...");
        
        currentRewardType = RewardType.Weapon;
    
        rewardPanel.SetActive(true);
        titleText.text = "SİLAH GELİŞTİRMESİ SEÇ";
        
        // OYUNU DURDUR - YENİ! ✅
        Time.timeScale = 0f;
    
        // Rastgele 3 silah upgrade'i al
        List<WeaponUpgradeManager.WeaponUpgradeType> availableUpgrades = GetAvailableWeaponUpgrades();
        List<WeaponUpgradeManager.WeaponUpgradeType> offeredUpgrades = GetRandomWeaponUpgrades(availableUpgrades, 3);
    
        // Butonları güncelle
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < offeredUpgrades.Count)
            {
                SetupWeaponUpgradeButton(upgradeButtons[i], offeredUpgrades[i], i);
                upgradeButtons[i].SetActive(true);
            }
            else
            {
                upgradeButtons[i].SetActive(false);
            }
        }
    
        Time.timeScale = 0f;
    }
    
    // Upgrade seçim ekranını göster
    public void ShowUpgradeSelection()
        {
            Debug.Log("📋 Upgrade seçim ekranı gösteriliyor...");
 
            currentRewardType = RewardType.Upgrade;
            
            rewardPanel.SetActive(true);
            titleText.text = "GELİŞTİRME SEÇ";
            
            // OYUNU DURDUR - YENİ! ✅
            Time.timeScale = 0f;
            
            // Rastgele 3 upgrade al
            currentOffers = UpgradeManager.Instance.GetRandomUpgrades(3);
            
            // Butonları güncelle
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (i < currentOffers.Count)
                {
                    SetupUpgradeButton(upgradeButtons[i], currentOffers[i], i);
                    upgradeButtons[i].SetActive(true);
                }
                else
                {
                    upgradeButtons[i].SetActive(false);
                }
            }
            
            // Oyunu durdur - DÜZELTME: Yorum satırını kaldırdık!
            Time.timeScale = 0f;
        }

    // Kullanılabilir kartları al (henüz elde olmayan)
    List<Card.CardColor> GetAvailableCards()
    {
        // CardManager'dan mevcut kartları al
        List<Card.CardColor> currentCards = new List<Card.CardColor>();
    
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            foreach (Card card in cardManager.handCards)
            {
                if (card != null)
                {
                    currentCards.Add(card.cardColor);
                }
            }
        }
        
        Debug.Log($"📋 Eldeki kartlar: {currentCards.Count}");
        foreach (var c in currentCards)
        {
            Debug.Log($"  - {c}");
        }
    
        // Tüm kartlar
        List<Card.CardColor> allCards = new List<Card.CardColor>
        {
            Card.CardColor.Red,
            Card.CardColor.Blue,
            Card.CardColor.Green,
            Card.CardColor.Yellow,
            //Card.CardColor.Purple,
            //Card.CardColor.Orange
        };
    
        // Henüz elde olmayan kartlar
        List<Card.CardColor> availableCards = new List<Card.CardColor>();
    
        foreach (Card.CardColor cardColor in allCards)
        {
            if (!currentCards.Contains(cardColor))
            {
                availableCards.Add(cardColor);
            }
        }
        
        Debug.Log($"📦 Kullanılabilir kartlar: {availableCards.Count}");
        foreach (var c in availableCards)
        {
            Debug.Log($"  + {c}");
        }
    
        // Eğer tüm kartlar varsa (4 kart limiti), yine de tüm kartları sun
        if (availableCards.Count == 0)
        {
            Debug.Log("⚠️ Tüm kartlar elde! Değiştirme modu aktif.");
            return allCards;
        }
    
        return availableCards;
    }
    
    // Rastgele N kart seç
    List<Card.CardColor> GetRandomCards(List<Card.CardColor> cardList, int count)
    {
        List<Card.CardColor> available = new List<Card.CardColor>(cardList);
        List<Card.CardColor> selected = new List<Card.CardColor>();
    
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            selected.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
    
        return selected;
    }
    
    // Kullanılabilir silah upgrade'lerini al
List<WeaponUpgradeManager.WeaponUpgradeType> GetAvailableWeaponUpgrades()
{
    List<WeaponUpgradeManager.WeaponUpgradeType> available = new List<WeaponUpgradeManager.WeaponUpgradeType>();
    
    if (WeaponUpgradeManager.Instance != null)
    {
        // Henüz alınmamış upgrade'leri ekle
        if (!WeaponUpgradeManager.Instance.hasRapidFire)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.RapidFire);
            
        if (!WeaponUpgradeManager.Instance.hasTripleShot)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.TripleShot);
            
        if (!WeaponUpgradeManager.Instance.hasSpreadShot)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.SpreadShot);
            
        if (!WeaponUpgradeManager.Instance.hasPierceShot)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.PierceShot);
            
        if (!WeaponUpgradeManager.Instance.hasAutoTarget)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.AutoTarget);
            
        if (!WeaponUpgradeManager.Instance.hasPowerShot)
            available.Add(WeaponUpgradeManager.WeaponUpgradeType.PowerShot);
    }
    
    // Eğer hepsi alınmışsa, tümünü tekrar sun (stacking için)
    if (available.Count == 0)
    {
        available.Add(WeaponUpgradeManager.WeaponUpgradeType.RapidFire);
        available.Add(WeaponUpgradeManager.WeaponUpgradeType.PowerShot);
    }
    
    return available;
}

// Rastgele silah upgrade'i seç
List<WeaponUpgradeManager.WeaponUpgradeType> GetRandomWeaponUpgrades(
    List<WeaponUpgradeManager.WeaponUpgradeType> upgradeList, int count)
{
    List<WeaponUpgradeManager.WeaponUpgradeType> available = 
        new List<WeaponUpgradeManager.WeaponUpgradeType>(upgradeList);
    List<WeaponUpgradeManager.WeaponUpgradeType> selected = 
        new List<WeaponUpgradeManager.WeaponUpgradeType>();
    
    for (int i = 0; i < count && available.Count > 0; i++)
    {
        int randomIndex = Random.Range(0, available.Count);
        selected.Add(available[randomIndex]);
        available.RemoveAt(randomIndex);
    }
    
    return selected;
}

// Silah upgrade butonunu ayarla
void SetupWeaponUpgradeButton(GameObject buttonObj, 
    WeaponUpgradeManager.WeaponUpgradeType upgradeType, int index)
{
    TextMeshProUGUI nameText = buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
    TextMeshProUGUI descText = buttonObj.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
    
    if (nameText == null || descText == null)
    {
        Debug.LogError("❌ Text componentleri bulunamadı!");
        return;
    }
    
    string upgradeName = GetWeaponUpgradeName(upgradeType);
    string upgradeDesc = GetWeaponUpgradeDescription(upgradeType);
    
    nameText.text = upgradeName;
    descText.text = upgradeDesc;
    
    Button button = buttonObj.GetComponent<Button>();
    if (button != null)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            OnWeaponUpgradeSelected(upgradeType);
        });
    }
}



// Silah upgrade ismi
string GetWeaponUpgradeName(WeaponUpgradeManager.WeaponUpgradeType upgradeType)
{
    switch (upgradeType)
    {
        case WeaponUpgradeManager.WeaponUpgradeType.RapidFire:
            return " Hızlı Ateş";
        case WeaponUpgradeManager.WeaponUpgradeType.TripleShot:
            return " Üçlü Atış";
        case WeaponUpgradeManager.WeaponUpgradeType.SpreadShot:
            return " Yayılma Atışı";
        case WeaponUpgradeManager.WeaponUpgradeType.PierceShot:
            return " Delici Atış";
        case WeaponUpgradeManager.WeaponUpgradeType.AutoTarget:
            return " Otomatik Nişan";
        case WeaponUpgradeManager.WeaponUpgradeType.PowerShot:
            return " Güçlü Atış";
        default:
            return "Silah Upgrade";
    }
}

// Silah upgrade açıklaması
string GetWeaponUpgradeDescription(WeaponUpgradeManager.WeaponUpgradeType upgradeType)
{
    switch (upgradeType)
    {
        case WeaponUpgradeManager.WeaponUpgradeType.RapidFire:
            return "Ateş hızı %50 artar.";
        case WeaponUpgradeManager.WeaponUpgradeType.TripleShot:
            return "Aynı anda 3 mermi ateşler.";
        case WeaponUpgradeManager.WeaponUpgradeType.SpreadShot:
            return "5 mermi yelpaze şeklinde ateşler.";
        case WeaponUpgradeManager.WeaponUpgradeType.PierceShot:
            return "Mermiler düşmanları deler geçer.";
        case WeaponUpgradeManager.WeaponUpgradeType.AutoTarget:
            return "En yakın düşmana otomatik nişan alır.";
        case WeaponUpgradeManager.WeaponUpgradeType.PowerShot:
            return "Mermi hasarı %50 artar.";
        default:
            return "";
    }
}

// Silah upgrade seçildi
void OnWeaponUpgradeSelected(WeaponUpgradeManager.WeaponUpgradeType upgradeType)
{
    Debug.Log($"⚔️ Silah upgrade seçildi: {upgradeType}");
    
    // Upgrade'i uygula
    if (WeaponUpgradeManager.Instance != null)
    {
        WeaponUpgradeManager.Instance.ApplyWeaponUpgrade(upgradeType);
    }
    
    // Ekranı kapat ve oyuna devam
    HideRewardScreen();
    ContinueGame();
}
    
    // Kart butonunu ayarla
    void SetupCardButton(GameObject buttonObj, Card.CardColor cardColor, int index)
    {
        Debug.Log($"🔧 Kart butonu ayarlanıyor: {cardColor}");
    
        // Buton içindeki text'leri bul
        TextMeshProUGUI nameText = buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = buttonObj.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
    
        if (nameText == null || descText == null)
        {
            Debug.LogError("❌ Text componentleri bulunamadı!");
            return;
        }
    
        // Kart bilgilerini al
        string cardName = GetCardName(cardColor);
        string cardDesc = GetCardDescription(cardColor);
    
        nameText.text = cardName;
        descText.text = cardDesc;
    
        // Buton click event'ini ayarla
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                OnCardSelected(cardColor);
            });
        }
    
        Debug.Log($"✅ Kart butonu hazır: {cardName}");
    }
    
    // Kart ismini al
    string GetCardName(Card.CardColor cardColor)
    {
        switch (cardColor)
        {
            case Card.CardColor.Red:
                return " Turret Kartı";
            case Card.CardColor.Blue:
                return " Yavaşlatma Kartı";
            case Card.CardColor.Green:
                return " Can Kartı";
            case Card.CardColor.Yellow:
                return "Şimşek Kartı";
            case Card.CardColor.Purple:
                return "Zehir Kartı";
            case Card.CardColor.Orange:
                return " Zayıflık Kartı";
            default:
                return "Kart";
        }
    }

// Kart açıklamasını al
    string GetCardDescription(Card.CardColor cardColor)
    {
        switch (cardColor)
        {
            case Card.CardColor.Red:
                return "Lane'e turret yerleştirir. Düşmanlara ateş eder.";
            case Card.CardColor.Blue:
                return "Lane'deki düşmanları %50 yavaşlatır.";
            case Card.CardColor.Green:
                return "Bu lane'den düşman öldürdükçe +1 can kazanırsın.";
            case Card.CardColor.Yellow:
                return "Boss'a elektrik saldırısı yapar.";
            case Card.CardColor.Purple:
                return "Lane'e zehir döker. Her saniye 5 hasar verir.";
            case Card.CardColor.Orange:
                return "Lane'deki düşmanlar %50 fazla hasar alır.";
            default:
                return "";
        }
    }
    
    // Kart seçildi
    void OnCardSelected(Card.CardColor selectedCard)
    {
        Debug.Log($"🎴 Kart seçildi: {selectedCard}");
    
        // CardManager'a kartı ekle
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            // Eğer 4 kart varsa, değiştirme moduna gir
            if (cardManager.handCards.Count >= 4)
            {
                // TODO: İleride kart değiştirme UI'ı eklenecek
                // Şimdilik ilk kartı değiştir
                Debug.Log("⚠️ Kart limiti dolu! İlk kart değiştiriliyor.");
                cardManager.ReplaceCard(0, selectedCard);
            }
            else
            {
                // Yeni kart ekle
                cardManager.AddNewCard(selectedCard);
            }
        }
    
        // Ekranı kapat ve oyuna devam
        HideRewardScreen();
        ContinueGame();
    }
    
    
    
    
    // Upgrade butonunu ayarla
    void SetupUpgradeButton(GameObject buttonObj, UpgradeData upgrade, int index)
    {
        Debug.Log($"🔧 Buton {index} ayarlanıyor: {upgrade.upgradeName}");
        // Buton içindeki text'leri bul
        TextMeshProUGUI nameText = buttonObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = buttonObj.transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        
        if (nameText == null)
        {
            Debug.LogError($"NameText bulunamadı! Button: {buttonObj.name}");
            return;
        }
    
        if (descText == null)
        {
            Debug.LogError($"DescText bulunamadı! Button: {buttonObj.name}");
            return;
        }
        
        nameText.text = upgrade.upgradeName;
        descText.text = upgrade.description;
        
        // Buton click event'ini ayarla
        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($" Button component bulunamadı! {buttonObj.name}");
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Debug.Log($"🖱️ Butona tıklandı! Index: {index}");
            OnUpgradeSelected(index);
        });
        
        Debug.Log($"✅ Buton {index} hazır!");
    }
    
    void OnUpgradeSelected(int index)
    {
        Debug.Log($"📌 OnUpgradeSelected çağrıldı! Index: {index}");

        // Reward tipine göre farklı işlem yap
        if (currentRewardType == RewardType.Upgrade)
        {
            if (index >= 0 && index < currentOffers.Count)
            {
                UpgradeData selectedUpgrade = currentOffers[index];
        
                Debug.Log($"✅ Seçilen upgrade: {selectedUpgrade.upgradeName}");
        
                // Upgrade'i uygula
                UpgradeManager.Instance.ApplyUpgrade(selectedUpgrade);
        
                // Ekranı kapat ve oyuna devam
                HideRewardScreen();
                ContinueGame();
            }
            else
            {
                Debug.LogError($"❌ Geçersiz index! {index} / {currentOffers.Count}");
            }
        }
    }
    
    // Reward ekranını gizle
    void HideRewardScreen()
    {
        rewardPanel.SetActive(false);
    }
    void ContinueGame()
    {
        Debug.Log("✅ Reward seçildi, oyun devam ediyor!");
        
        // OYUNU DEVAM ETTİR - YENİ! ✅
        Time.timeScale = 1f;
    
        // Reward ekranını kapat
        HideRewardScreen();
    
        // LEVELMANAGER'A DEVAM ET - YENİ! ✅
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ContinueToNextLevel();
        }
        else
        {
            Debug.LogWarning("⚠️ LevelManager bulunamadı!");
        }
    } 
}
