using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Karakterin altında küçük ulti cooldown bar'ı
/// World Space Canvas kullanır - garantili çalışır!
/// </summary>
public class UltiBarUI : MonoBehaviour
{
    public static UltiBarUI Instance;
    
    [Header("Bar Settings")]
    public float barWidth = 1.2f; // World space (1.2 birim)
    public float barHeight = 0.12f; // World space
    public Vector3 offset = new Vector3(0, -0.8f, 0); // Karakterin altında
    
    [Header("Colors")]
    public Color emptyColor = new Color(0.2f, 0.2f, 0.3f, 0.8f); // Koyu
    public Color fillingColor = new Color(1f, 0.9f, 0.3f, 1f); // Sarı (dolurken)
    public Color readyColor = new Color(0f, 1f, 0.3f, 1f); // Yeşil (hazır)
    
    [Header("Animation")]
    public bool enablePulse = true;
    public float pulseSpeed = 5f;
    
    // Internal
    private Canvas worldCanvas;
    private GameObject barContainer;
    private Image backgroundImage;
    private Image fillImage;
    private Transform playerTransform;
    
    private float pulseTimer = 0f;
    private bool wasReady = false;
    
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
        // Player'ı bul
        GameObject player = GameObject.Find("WeaponCenter");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ Player bulunamadı!");
        }
        
        CreateUltiBar();
    }
    
    void Update()
    {
        if (barContainer == null || playerTransform == null) return;
        
        // Bar'ı player'ın altında tut
        barContainer.transform.position = playerTransform.position + offset;
        
        // Her zaman kameraya baksın
        if (Camera.main != null)
        {
            barContainer.transform.rotation = Camera.main.transform.rotation;
        }
        
        // Pulse animasyonu (hazırsa)
        if (enablePulse && WeaponManager.Instance != null && WeaponManager.Instance.isUltiReady)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            UpdatePulse();
        }
    }
    
    void CreateUltiBar()
    {
        // WORLD SPACE CANVAS OLUŞTUR! 🌍
        GameObject canvasObj = new GameObject("UltiBar_WorldCanvas");
        canvasObj.transform.SetParent(transform);
        
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace; // ✅ World Space!
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        // Canvas RectTransform ayarla
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
        canvasRect.localScale = Vector3.one * 0.01f; // Küçük (world scale)
        
        // Bar container
        barContainer = new GameObject("UltiBarContainer");
        barContainer.transform.SetParent(canvasObj.transform, false);
        
        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        // 1) BACKGROUND (koyu)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = emptyColor;
        
        // 2) FILL (doluluk)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(0, 0); // Başta boş
        
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillingColor;
        
        // Outline (kenar çizgisi)
        Outline outline = fillObj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
        
        Debug.Log("⚡ Ulti Bar UI oluşturuldu (World Space)!");
    }
    
    void UpdatePulse()
    {
        float pulse = Mathf.Sin(pulseTimer) * 0.3f + 0.7f; // 0.7-1.0 arası
        
        Color pulseColor = readyColor * pulse;
        pulseColor.a = 1f;
        fillImage.color = pulseColor;
    }
    
    /// <summary>
    /// Ulti doluluk oranını güncelle (0-1 arası)
    /// </summary>
    public void UpdateUltiFill(float fillRatio, bool isReady)
    {
        if (fillImage == null) return;
        
        // Bar genişliğini güncelle
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(barWidth * fillRatio, 0);
        
        // Renk güncelle
        if (isReady)
        {
            // Hazır - Yeşil (pulse ile)
            if (!wasReady)
            {
                wasReady = true;
                pulseTimer = 0f; // Pulse'u sıfırla
            }
            // UpdatePulse() rengi güncelleyecek
        }
        else
        {
            // Dolmakta - Sarı
            wasReady = false;
            fillImage.color = fillingColor;
        }
    }
    
    /// <summary>
    /// Bar'ı göster/gizle
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(visible);
        }
    }
}