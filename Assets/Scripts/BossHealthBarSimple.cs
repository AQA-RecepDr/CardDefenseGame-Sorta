using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BASİT Boss Health Bar - Garantili çalışır!
/// Tek bar, doluluk oranı gösterir
/// </summary>
public class SimpleBossHealthBar : MonoBehaviour
{
    public static SimpleBossHealthBar Instance;
    
    [Header("Bar Settings")]
    public float barWidth = 800f;
    public float barHeight = 40f;
    
    [Header("Colors")]
    public Color fullColor = new Color(0.9f, 0.3f, 1f); // Mor 💜
    public Color emptyColor = new Color(0.3f, 0.1f, 0.4f); // Koyu mor
    public Color glowColor = new Color(1f, 0.9f, 0.3f); // Sarı glow ⚡
    
    [Header("Animation")]
    public bool enablePulse = true;
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.2f;
    
    [Header("Break Effect")]
    public bool enableScreenFlash = true;
    public float screenFlashDuration = 0.2f;
    public float breakThreshold = 1000f; // Her 1000 HP'de break efekti
    
    // Internal
    private Canvas canvas;
    private GameObject barContainer;
    private Image backgroundImage;
    private Image fillImage;
    private Image glowImage;
    private Text hpText;
    
    private int currentMaxHP = 6000;
    private int currentHP = 6000;
    private int lastBreakHP = 6000;
    
    private float pulseTimer = 0f;
    private bool isVisible = false;
    
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
        CreateHealthBar();
        HideBar();
    }
    
    void Update()
    {
        if (!isVisible) return;
        
        // Pulse animasyonu
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            UpdatePulse();
        }
    }
    
    void CreateHealthBar()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas bulunamadı!");
            return;
        }
        
        // Container
        barContainer = new GameObject("SimpleBossHealthBar");
        barContainer.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -20);
        containerRect.sizeDelta = new Vector2(barWidth, barHeight);
        
        // 1) BACKGROUND (koyu arka plan)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = emptyColor;
        
        // 2) FILL (doluluk - RectTransform ile width değişecek!) 💧
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1); // Sol tarafa anchor
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(barWidth, 0); // Başta tam genişlik
        
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullColor;
        
        // Outline
        Outline outline = fillObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        outline.effectDistance = new Vector2(2, -2);
        
        // 3) GLOW (ortada)
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.sizeDelta = new Vector2(4, 4);
        glowRect.anchoredPosition = Vector2.zero;
        
        glowImage = glowObj.AddComponent<Image>();
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
        glowImage.raycastTarget = false;
        
        // 4) HP TEXT (bar'ın ALTINDA - ayrı container) 💬
        GameObject textObj = new GameObject("HPText");
        textObj.transform.SetParent(barContainer.transform.parent, false); // Container'ın parent'ına ekle!
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0, -70); // Bar'ın 10px altında (bar 40px + 20px üstten + 10px aralık)
        textRect.sizeDelta = new Vector2(barWidth, 40); // Aynı genişlik
        
        hpText = textObj.AddComponent<Text>();
        hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpText.fontSize = 28;
        hpText.fontStyle = FontStyle.Bold;
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
        hpText.text = "6000 / 6000";
        hpText.raycastTarget = false;
        
        // Shadow (daha okunaklı)
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = Color.black;
        textShadow.effectDistance = new Vector2(3, -3);
        
        // Outline ekle (ekstra contrast)
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, -2);
        
        Debug.Log("💜 Simple Boss Health Bar oluşturuldu!");
    }
    
    void UpdatePulse()
    {
        float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
        float brightness = 1f + pulse;
        
        Color pulseColor = fullColor * brightness;
        fillImage.color = pulseColor;
        
        Color glowPulse = glowColor * brightness;
        glowPulse.a = 0.3f + (pulse * 0.2f);
        glowImage.color = glowPulse;
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        currentHP = currentHealth;
        currentMaxHP = maxHealth;
        
        // Fill ratio hesapla
        float fillRatio = Mathf.Clamp01((float)currentHP / currentMaxHP);
        
        // Bar genişliğini güncelle! 💧
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(barWidth * fillRatio, 0);
        
        // HP text güncelle
        hpText.text = $"{currentHP} / {currentMaxHP}";
        
        // Her 1000 HP kayıpda break efekti
        if (currentHP < lastBreakHP - breakThreshold)
        {
            lastBreakHP = currentHP - (currentHP % (int)breakThreshold);
            TriggerBreakEffect();
        }
        
        // Boss öldü mü?
        if (currentHP <= 0)
        {
            HideBar();
        }
        
        // Debug - her 60 frame'de bir
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"💜 Boss HP: {currentHP}/{currentMaxHP} ({fillRatio:P0})");
        }
    }
    
    void TriggerBreakEffect()
    {
        // Screen flash
        if (enableScreenFlash)
        {
            StartCoroutine(ScreenFlashEffect());
        }
        
        // Camera shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.25f, 0.2f);
        }
        
        // Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossHit();
        }
        
        Debug.Log($"💥 Boss milestone reached! {currentHP} HP kaldı!");
    }
    
    System.Collections.IEnumerator ScreenFlashEffect()
    {
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(canvas.transform, false);
        
        RectTransform flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashRect.anchoredPosition = Vector2.zero;
        
        Image flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0.6f);
        
        flashObj.transform.SetAsLastSibling();
        
        float elapsed = 0f;
        while (elapsed < screenFlashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.6f, 0f, elapsed / screenFlashDuration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        
        Destroy(flashObj);
    }
    
    public void ShowBar()
    {
        if (barContainer != null)
        {
            barContainer.SetActive(true);
            isVisible = true;
            Debug.Log("💜 Simple Boss Health Bar gösterildi!");
        }
    }
    
    public void HideBar()
    {
        if (barContainer != null)
        {
            barContainer.SetActive(false);
            isVisible = false;
            Debug.Log("💜 Simple Boss Health Bar gizlendi!");
        }
    }
    
    public void ResetBar(int maxHP)
    {
        currentMaxHP = maxHP;
        currentHP = maxHP;
        lastBreakHP = maxHP;
        
        // Bar'ı tam doldur
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(barWidth, 0);
        
        hpText.text = $"{currentHP} / {currentMaxHP}";
        
        ShowBar();
    }
}