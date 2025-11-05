using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    
    [Header("Coin Sistemi")]
    public int currentSessionCoins = 0; // Bu oyunda kazanılan
    public int totalCoins = 0; // Toplam (PlayerPrefs'den)
    
    [Header("Kazanç Oranları")]
    public int coinsPerKill = 10; // Her düşman 10 coin
    
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
        // Toplam coin'i yükle
        LoadCoins();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoins(currentSessionCoins);
        }
    }
    
    // Coin kazan (düşman öldürünce)
    public void AddCoins(int amount)
    {
        currentSessionCoins += amount;
        
        Debug.Log($"+{amount} coin! Toplam bu oyunda: {currentSessionCoins}");
        
        // UI güncelle (varsa)
        UpdateCoinUI();
    }
    
    // Oyun bitince coin'leri kaydet
    public void SaveSessionCoins()
    {
        totalCoins += currentSessionCoins;
        
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();
        
        Debug.Log($" Coin kaydedildi! Bu oyun: {currentSessionCoins}, Toplam: {totalCoins}");
    }
    
    // Coin'leri yükle
    void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        Debug.Log($"💰 Toplam coin yüklendi: {totalCoins}");
    }
    
    // Coin harca (upgrade için)
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.Save();
            
            Debug.Log($"💸 {amount} coin harcandı! Kalan: {totalCoins}");
            
            return true;
        }
        
        Debug.LogWarning($" Yeterli coin yok! Gerekli: {amount}, Var: {totalCoins}");
        return false;
    }
    
    // UI güncelle
    void UpdateCoinUI()
    {
        if (UIManager.Instance != null)
        {
            // UIManager'a coin text ekleyeceğiz
            UIManager.Instance.UpdateCoins(currentSessionCoins);
        }
    }
    
    // Bu oyunda kazanılan coin sayısı
    public int GetSessionCoins()
    {
        return currentSessionCoins;
    }
    
    // Toplam coin sayısı
    public int GetTotalCoins()
    {
        return totalCoins;
    }
}