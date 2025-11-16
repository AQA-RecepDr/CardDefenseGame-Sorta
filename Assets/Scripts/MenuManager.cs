using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menü Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    
    void Start()
    {
        // Ana menüyü aç, options'ı kapat
        ShowMainMenu();
    }
    
    // Ana menüyü göster
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }
    
    // Oyunu başlat
    public void StartGame()
    {
        Debug.Log("🎮 Oyun başlıyor!");
        
        mainMenuPanel.SetActive(false);
        
        // GAMEMANAGER'A HABER VER! 🎮
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        // Oyun objelerini hazırla
        EnableGameObjects();
        
        // LevelManager'ı başlat (countdown ile)
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.enabled = true;
            levelManager.LoadLevel(0); // İlk level (countdown başlar)
            Debug.Log("✅ LevelManager başlatıldı!");
        }
        else
        {
            Debug.LogWarning("⚠️ LevelManager bulunamadı!");
        }
        
    }
    
   // Options menüsünü göster
    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }
    
    // Oyundan çık
    public void QuitGame()
    {
        Debug.Log("👋 Oyundan çıkılıyor!");
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
    void EnableGameObjects()
    {
        // Kartları ayarla - SADECE KIRMIZI! ✅
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager != null)
        {
            // SADECE KIRMIZI KART İLE BAŞLA! ✅
            Card.CardColor[] startingCards = new Card.CardColor[]
            {
                Card.CardColor.Red  // Sadece turret!
            };
        
            cardManager.SetAvailableCards(startingCards);
            Debug.Log("🎴 Oyun başlangıç kartı: Kırmızı (Turret)");
        }
    
        // Wave UI'yi göster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWaveUI();
        }
    
        Debug.Log("✅ Oyun objeleri hazır!");
    }
}