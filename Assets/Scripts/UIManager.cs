using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    
    [Header("Oyun Bitiş Ekranları")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI winCoinText;
    public TextMeshProUGUI loseCoinText; 
    
    public static UIManager Instance; // Singleton (her yerden erişilebilir)
    
    [Header("UI Referansları")]
    public TextMeshProUGUI playerHealthText;
    //public TextMeshProUGUI chargeText;
    //public TextMeshProUGUI dragonHealthText;
    
    [Header("Countdown")]
    public TextMeshProUGUI countdownText;
    
    [Header("Wave UI")] // YENİ!
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI waveAnnouncementText; // YENİ!

    void Awake()
    {
        // Singleton pattern
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
        // WAVE UI'YI BAŞTA GİZLE - YENİ! ✅
        HideWaveUI();
    }

// Wave UI'yi göster
    public void ShowWaveUI()
    {
        if (waveText != null)
            waveText.gameObject.SetActive(true);
    
        if (enemyCountText != null)
            enemyCountText.gameObject.SetActive(true);
    }

// Wave UI'yi gizle
    public void HideWaveUI()
    {
        if (waveText != null)
            waveText.gameObject.SetActive(false);
    
        if (enemyCountText != null)
            enemyCountText.gameObject.SetActive(false);
    }
    

    IEnumerator WaveAnnouncementCoroutine(int waveNumber)
    {
        waveAnnouncementText.gameObject.SetActive(true);
        waveAnnouncementText.text = $"WAVE {waveNumber}\nHAZIR OL!";
    
        // 2 saniye göster
        yield return new WaitForSeconds(2f);
    
        waveAnnouncementText.gameObject.SetActive(false);
        Debug.Log($"✅ Wave announcement kapandı!");
    }
    
    // Wave numarasını güncelle
    public void UpdateWaveNumber(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + waveNumber;
        }
    }

// Düşman sayacını güncelle
    public void UpdateEnemyCount(int killed, int total, int remaining)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Öldürülen: {killed}/{total}\nKalan: {remaining}";
        }
    }

    // Oyuncu canını güncelle
    public void UpdatePlayerHealth(int current, int max)
    {
        playerHealthText.text = "Can: " + current + "/" + max;
    }
    
    // Kazanma ekranını göster
    public void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            
            // COİN SAYISINI GÖSTER - YENİ! 💰✅
            if (winCoinText != null && CoinManager.Instance != null)
            {
                int coinsEarned = CoinManager.Instance.GetSessionCoins();
                winCoinText.text = $"🎉 {coinsEarned} COİN KAZANDIN!\n\nKalıcı Geliştirmeler İçin Tıkla!";
            }
        }
    }
    

// Kaybetme ekranını göster
    public void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
            
            // COİN SAYISINI GÖSTER - YENİ! 💰✅
            if (loseCoinText != null && CoinManager.Instance != null)
            {
                int coinsEarned = CoinManager.Instance.GetSessionCoins();
                loseCoinText.text = $"💰 {coinsEarned} COİN KAZANDIN!\n\nKalıcı Geliştirmeler İçin Tıkla!";
            }
        }
    }
    
    public void ShowCountdown(int seconds)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = seconds.ToString();
        }
    }

    public void HideCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
}