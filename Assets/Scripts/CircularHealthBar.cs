using UnityEngine;

/// <summary>
/// Karakterin etrafında dairesel health bar
/// Neon/cyberpunk stili
/// </summary>
public class CircularHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float radius = 1.2f; // Çemberin yarıçapı
    public float lineWidth = 0.15f; // Çizgi kalınlığı
    public float glowWidth = 0.25f; // Glow kalınlığı
    public int segments = 60; // Çember pürüzsüzlüğü (daha fazla = daha smooth)
    
    [Header("Colors")]
    public Color fullHealthColor = new Color(0.2f, 1f, 0.4f); // Yeşil
    public Color midHealthColor = new Color(1f, 0.9f, 0.2f); // Sarı
    public Color lowHealthColor = new Color(1f, 0.2f, 0.2f); // Kırmızı
    
    [Header("Animation")]
    public float updateSpeed = 5f; // Bar dolum hızı
    public bool enablePulse = true;
    public float pulseSpeed = 4f;
    public float pulseIntensity = 0.3f;
    
    [Header("Critical Health Pulse - %10 Altı!")]
    public float criticalPulseSpeed = 8f; // Çok hızlı pulse
    public float criticalPulseIntensity = 0.8f; // Çok güçlü
    public float criticalRadiusPulse = 0.15f; // Bar büyüyüp küçülsün
    
    [Header("Damage Flash")]
    public float damageFlashDuration = 0.2f;
    public Color damageFlashColor = Color.red;
    
    // Internal
    private LineRenderer healthLine;
    private LineRenderer glowLine;
    private LineRenderer backLine; // Arka plan (boş kısım)
    
    private float currentFillAmount = 1f; // 0-1 arası
    private float targetFillAmount = 1f;
    
    private bool isDamageFlashing = false;
    private float damageFlashTimer = 0f;
    private float pulseTimer = 0f;
    
    void Start()
    {
        CreateHealthBar();
    }
    
    void Update()
    {
        // Fill amount smooth transition
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * updateSpeed);
        }
        else
        {
            currentFillAmount = targetFillAmount;
        }
        
        // Damage flash timer
        if (isDamageFlashing)
        {
            damageFlashTimer -= Time.deltaTime;
            if (damageFlashTimer <= 0f)
            {
                isDamageFlashing = false;
            }
        }
        
        // Pulse efekti - KRİTİK CANDA ÇOK HIZLI! 🚨
        if (enablePulse && currentFillAmount < 0.2f)
        {
            // %10 altı - KRİTİK! Çok hızlı pulse
            pulseTimer += Time.deltaTime * criticalPulseSpeed;
        }
        else if (enablePulse && currentFillAmount < 0.4f)
        {
            // %30 altı - Hafif pulse
            pulseTimer += Time.deltaTime * pulseSpeed;
        }
        else
        {
            pulseTimer = 0f;
        }
        
        // Çemberi güncelle
        UpdateCircle();
    }
    
    /// <summary>
    /// Health bar'ı oluştur
    /// </summary>
    void CreateHealthBar()
    {
        // 1) BACK LINE (arka plan - boş kısım)
        GameObject backObj = new GameObject("HealthBack");
        backObj.transform.SetParent(transform);
        backObj.transform.localPosition = Vector3.zero;
        
        backLine = backObj.AddComponent<LineRenderer>();
        SetupLineRenderer(backLine, lineWidth, -2);
        backLine.startColor = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Koyu gri
        backLine.endColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        
        // 2) GLOW LINE (glow efekti)
        GameObject glowObj = new GameObject("HealthGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        
        glowLine = glowObj.AddComponent<LineRenderer>();
        SetupLineRenderer(glowLine, glowWidth, -1);
        
        // 3) HEALTH LINE (ana çizgi)
        GameObject healthObj = new GameObject("HealthLine");
        healthObj.transform.SetParent(transform);
        healthObj.transform.localPosition = Vector3.zero;
        
        healthLine = healthObj.AddComponent<LineRenderer>();
        SetupLineRenderer(healthLine, lineWidth, 0);
        
        // İlk çemberi çiz
        UpdateCircle();
    }
    
    /// <summary>
    /// LineRenderer ayarla
    /// </summary>
    void SetupLineRenderer(LineRenderer lr, float width, int sortingOrder)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = false; // Loop yok, manuel kapatacağız
        lr.useWorldSpace = false;
        lr.sortingLayerName = "Default";
        lr.sortingOrder = sortingOrder;
        
        // Additive blend (glow için)
        lr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    }
    
    /// <summary>
    /// Çemberi güncelle (fill amount'a göre)
    /// </summary>
    void UpdateCircle()
    {
        // Full circle (arka plan)
        DrawCircle(backLine, 1f);
        
        // Health circle (doluluk oranına göre)
        DrawCircle(healthLine, currentFillAmount);
        DrawCircle(glowLine, currentFillAmount);
        
        // Renk güncelle
        UpdateColors();
    }
    
    /// <summary>
    /// Çember çiz
    /// </summary>
    void DrawCircle(LineRenderer lr, float fillAmount)
    {
        // Kaç segment çizeceğiz
        int visibleSegments = Mathf.CeilToInt(segments * fillAmount);
        
        // En az 2 nokta olmalı
        if (visibleSegments < 2)
        {
            lr.positionCount = 0;
            return;
        }
        
        lr.positionCount = visibleSegments + 1;
        
        // KRİTİK CANDA RADIUS PULSE! 🚨
        float currentRadius = radius;
        if (currentFillAmount < 0.2f)
        {
            float radiusPulse = Mathf.Sin(pulseTimer) * criticalRadiusPulse;
            currentRadius += radiusPulse; // Bar büyüyüp küçülür
        }
        
        // Çemberi çiz (üstten başla, saat yönünün tersine)
        float angleStep = (360f * fillAmount) / segments;
        float startAngle = 90f; // Üstten başla
        
        for (int i = 0; i <= visibleSegments; i++)
        {
            float angle = startAngle - (angleStep * i);
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 pos = new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );
            
            lr.SetPosition(i, pos);
        }
    }
    
    /// <summary>
    /// Renkleri güncelle (can oranına göre)
    /// </summary>
    void UpdateColors()
    {
        Color healthColor;
        
        // Damage flash aktifse
        if (isDamageFlashing)
        {
            healthColor = damageFlashColor;
        }
        else
        {
            // Can oranına göre renk gradient'i
            if (currentFillAmount > 0.5f)
            {
                // Yeşil → Sarı (1.0 → 0.5)
                float t = (currentFillAmount - 0.5f) / 0.5f;
                healthColor = Color.Lerp(midHealthColor, fullHealthColor, t);
            }
            else
            {
                // Sarı → Kırmızı (0.5 → 0.0)
                float t = currentFillAmount / 0.5f;
                healthColor = Color.Lerp(lowHealthColor, midHealthColor, t);
            }
        }
        
        // Pulse efekti - KRİTİK CANDA ÇOK GÜÇLÜ! 🚨
        if (enablePulse && currentFillAmount < 0.2f)
        {
            // %10 altı - KRİTİK PULSE!
            float pulse = Mathf.Sin(pulseTimer) * criticalPulseIntensity;
            float brightness = 1f + pulse;
            healthColor *= brightness;
        }
        else if (enablePulse && currentFillAmount < 0.4f)
        {
            // %30 altı - Hafif pulse
            float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
            float brightness = 1f + pulse;
            healthColor *= brightness;
        }
        
        // Renkleri uygula
        healthLine.startColor = healthColor;
        healthLine.endColor = healthColor;
        
        Color glowColor = healthColor * 0.6f;
        glowColor.a = healthColor.a * 0.5f;
        glowLine.startColor = glowColor;
        glowLine.endColor = glowColor;
    }
    
    /// <summary>
    /// Can değerini güncelle (dışarıdan çağrılacak)
    /// </summary>
    public void SetHealth(float current, float max)
    {
        float newFill = Mathf.Clamp01(current / max);
        
        // Can azaldıysa damage flash
        if (newFill < targetFillAmount)
        {
            TriggerDamageFlash();
        }
        
        targetFillAmount = newFill;
    }
    
    /// <summary>
    /// Hasar flash efekti
    /// </summary>
    void TriggerDamageFlash()
    {
        isDamageFlashing = true;
        damageFlashTimer = damageFlashDuration;
        
        // Camera shake (varsa)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.08f, 0.1f);
        }
    }
    
    /// <summary>
    /// Bar'ı gizle/göster
    /// </summary>
    public void SetVisible(bool visible)
    {
        healthLine.enabled = visible;
        glowLine.enabled = visible;
        backLine.enabled = visible;
    }
}