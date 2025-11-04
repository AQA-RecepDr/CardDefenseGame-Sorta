using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;
    
    [Header("Prefab")]
    public GameObject damageTextPrefab;
    
    [Header("Canvas")]
    public Canvas canvas; // UI Canvas referansı
    
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

    public void ShowDamage(int damage, Vector3 worldPosition, Color color)
    {
        Debug.Log($"🎯 ShowDamage çağrıldı! Damage: {damage}, Position: {worldPosition}");
    
        if (damageTextPrefab == null)
        {
            Debug.LogError("❌ damageTextPrefab NULL!");
            return;
        }
    
        if (canvas == null)
        {
            Debug.LogError("❌ canvas NULL!");
            return;
        }
    
        // Damage object'i oluştur
        GameObject damageObj = Instantiate(damageTextPrefab, canvas.transform);
    
        // RectTransform'u al ve pozisyonu ayarla
        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // World position'ı Canvas'a göre local position'a çevir
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Camera.main.WorldToScreenPoint(worldPosition),
                canvas.worldCamera,
                out canvasPosition
            );
        
            rectTransform.anchoredPosition = canvasPosition;
            
        }
        // Damage component'ini al
        DamageText damageText = damageObj.GetComponent<DamageText>();
        if (damageText != null)
        {
            // Kritik mi kontrol et (altın renk = kritik)
            bool isCritical = IsCriticalColor(color);
            
            // Tek seferde hem renk hem damage set et!
            damageText.Initialize(damage, color);
            
            // KRİTİK İSE BÜYÜK YAĞP - YENİ! ✅
            if (isCritical)
            {
                rectTransform.localScale = Vector3.one * 1.5f; // %50 daha büyük!
            }
        }
        
    }
    
    // Kritik renk mi? (Parlak kırmızı)
    bool IsCriticalColor(Color color)
    {
        // Parlak kırmızı (R:1, G:0.1, B:0.1)
        return Mathf.Approximately(color.r, 1f) && 
               Mathf.Approximately(color.g, 0.1f) && 
               Mathf.Approximately(color.b, 0.1f);
    }
}