using System.Collections;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("Ayarlar")]
    public float lifetime = 0.5f; // 0.5 saniye
    public float moveSpeed = 50f; // Canvas coordinate'lerinde yukarı çıkma hızı
    public float fadeSpeed = 1f; // Fade out hızı
    
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private Color startColor;
    private float timer = 0f;
    private Vector2 startPosition; // Vector3 değil Vector2!

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // Başlangıç pozisyonunu kaydet
            startPosition = rectTransform.anchoredPosition;
           // Debug.Log($"🎯 DamageText başlangıç pozisyonu: {startPosition}");
        }
    }

    // Yeni fonksiyon - hem damage hem renk aynı anda
    public void Initialize(int damage, Color color)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }
    
        if (textMesh != null)
        {
            // ÖNCE RENGİ SET ET!
            textMesh.color = color;
            startColor = color;
        
            // SONRA DAMAGE'İ SET ET!
            textMesh.text = damage.ToString();
            // KRİTİK KONTROLÜ - YENİ! ✅
            bool isCritical = IsCriticalColor(color);
            
            if (isCritical)
            {
                // Kritik için bounce animasyonu
                StartCoroutine(CriticalBounce());
            }
        
            Debug.Log($"✅ DamageText initialized: {damage}, Color: {color}");
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
    
    // Kritik bounce animasyonu
    IEnumerator CriticalBounce()
    {
        if (rectTransform == null) yield break;
    
        Vector3 originalScale = rectTransform.localScale;
    
        // Büyüme animasyonu (0.2 saniye)
        float duration = 0.2f;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
        
            // Bounce effect (elastic)
            float scale = 1f + (Mathf.Sin(t * Mathf.PI) * 0.3f);
            rectTransform.localScale = originalScale * scale;
        
            yield return null;
        }
    
        rectTransform.localScale = originalScale;
    }
    void Update()
    {
        if (textMesh == null || rectTransform == null) return;
        
        timer += Time.deltaTime;
        
        // Yukarı hareket et (canvas space'de)
        rectTransform.anchoredPosition = startPosition + Vector2.up * (moveSpeed * timer);
        
        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        
        // Süre dolunca yok ol
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    // Damage değerini set et
    public void SetDamage(int damage)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }
        
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            Debug.Log($"✅ Damage text set edildi: {damage}");
        }
    }
    
    // Rengi set et
    public void SetColor(Color color)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }
        
        if (textMesh != null)
        {
            textMesh.color = color;
            startColor = color; // startColor'u da güncelle!
        }
    }
}