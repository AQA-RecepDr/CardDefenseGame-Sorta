using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Oyun Durumu")]
    public bool isGameOver = false;
    
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Oyunu kazan
    public void WinGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f; // Oyunu durdur
        
        Debug.Log("KAZANDIN!");
        
        // COİN'LERİ KAYDET - YENİ! 💰✅
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveSessionCoins();
        }
        
        // UI'yı göster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen();
        }
    }
    
    // Oyunu kaybet
    public void LoseGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Time.timeScale = 0f; // Oyunu durdur
        
        Debug.Log("KAYBETTİN!");
        
        // COİN'LERİ KAYDET - YENİ! 💰✅
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveSessionCoins();
        }
        
        // UI'yı göster
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLoseScreen();
        }
    }
    
    // Oyunu yeniden başlat
    public void RestartGame()
    {
        Time.timeScale = 1f; // Zamanı normal yap
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Ana menüye dön
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene"); // Scene ismini kontrol et!
    }
    
    // Oyundan çık
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyundan çıkıldı!");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
    
    // Permanent Upgrade ekranını aç
    public void OpenPermanentUpgrades()
    {
        if (PermanentUpgradeUI.Instance != null)
        {
            PermanentUpgradeUI.Instance.OpenPanel();
        }
    }
}