using UnityEngine;

/// <summary>
/// Neon stili zone highlight efekti
/// Mouse hover ve kart sürükleme için
/// </summary>
public class NeonZoneHighlight : MonoBehaviour
{
    [Header("Zone Shape")]
    public bool isTriangle = true; // true = üçgen, false = kare
    public Vector2 size = new Vector2(3f, 4f); // Zone boyutu
    
    [Header("Triangle Adjustment (sadece üçgen için)")]
    public float triangleTopOffset = 0f; // Üst nokta offset (yukarı/aşağı)
    public float triangleBaseWidth = 1f; // Alt kenar genişlik çarpanı
    public float triangleHeight = 1f; // Yükseklik çarpanı
    public bool flipVertically = false; // Üçgeni dikey olarak ters çevir
    
    [Header("Rotation")]
    public float rotationAngle = 0f; // Z ekseninde dönüş açısı (derece)
    
    [Header("Neon Effect Settings")]
    public float lineWidth = 0.15f; // Çizgi kalınlığı
    public float glowWidth = 0.35f; // Glow kalınlığı
    
    [Header("Colors")]
    public Color idleColor = new Color(0.3f, 0.3f, 0.5f, 0.3f); // Hover yok → hafif gri-mavi
    public Color hoverColor = new Color(1f, 1f, 1f, 0.8f); // Hover → parlak beyaz
    
    [Header("Animation")]
    public float transitionSpeed = 8f; // Renk geçiş hızı
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.2f; // Pulse ne kadar güçlü
    
    [Header("Fill")]
    public bool showFill = true;
    public float fillAlpha = 0.15f; // İç dolgu alpha değeri
    
    // Internal
    private LineRenderer borderLine;
    private LineRenderer glowLine;
    private SpriteRenderer fillRenderer;
    
    private Color currentColor;
    private Color targetColor;
    private float pulseTimer = 0f;
    
    private bool isHighlighted = false;
    
    void Awake()
    {
        CreateHighlightVisuals();
        currentColor = idleColor;
        targetColor = idleColor;
    }
    
    // Inspector'da değer değiştiğinde otomatik güncelle (Editor'da)
    void OnValidate()
    {
        if (Application.isPlaying && borderLine != null)
        {
            RefreshShape();
            RefreshLineWidths();
        }
    }
    
    /// <summary>
    /// Shape ve rotation'ı yeniden uygula
    /// </summary>
    public void RefreshShape()
    {
        SetShape();
        
        // Rotation'ı güncelle
        if (borderLine != null)
            borderLine.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        if (glowLine != null)
            glowLine.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        if (fillRenderer != null)
            fillRenderer.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
    }
    
    void RefreshLineWidths()
    {
        if (borderLine != null)
        {
            borderLine.startWidth = lineWidth;
            borderLine.endWidth = lineWidth;
        }
        
        if (glowLine != null)
        {
            glowLine.startWidth = glowWidth;
            glowLine.endWidth = glowWidth;
        }
    }
    
    void Update()
    {
        // Line width değişikliklerini kontrol et
        if (borderLine != null && (borderLine.startWidth != lineWidth))
        {
            RefreshLineWidths();
        }
        
        // Renk transition
        if (currentColor != targetColor)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        }
        
        // Pulse efekti (eğer highlight edilmişse)
        if (isHighlighted && enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
            ApplyColors(pulse);
        }
        else
        {
            ApplyColors(0f);
        }
    }
    
    /// <summary>
    /// Zone'u highlight et
    /// </summary>
    /// <param name="highlight">Highlight açık/kapalı</param>
    /// <param name="color">Opsiyonel: Özel renk (kart rengi gibi)</param>
    public void SetHighlight(bool highlight, Color? color = null)
    {
        isHighlighted = highlight;
        
        if (highlight)
        {
            // Eğer color verilmişse onu kullan, yoksa hoverColor
            targetColor = color ?? hoverColor;
        }
        else
        {
            // Highlight kapatıldı → idle renge dön
            targetColor = idleColor;
        }
    }
    
    /// <summary>
    /// Highlight visual'ları oluştur
    /// </summary>
    void CreateHighlightVisuals()
    {
        // 1) BORDER LINE (ana çizgi)
        GameObject borderObj = new GameObject("NeonBorder");
        borderObj.transform.SetParent(transform);
        borderObj.transform.localPosition = Vector3.zero;
        borderObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        
        borderLine = borderObj.AddComponent<LineRenderer>();
        SetupLineRenderer(borderLine, lineWidth, 10); // Sorting order 10
        
        // 2) GLOW LINE (glow efekti)
        GameObject glowObj = new GameObject("NeonGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        
        glowLine = glowObj.AddComponent<LineRenderer>();
        SetupLineRenderer(glowLine, glowWidth, 9); // Arkada
        
        // 3) FILL (iç dolgu)
        if (showFill)
        {
            GameObject fillObj = new GameObject("NeonFill");
            fillObj.transform.SetParent(transform);
            fillObj.transform.localPosition = Vector3.zero;
            fillObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
            
            fillRenderer = fillObj.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = CreateSimpleSprite();
            fillRenderer.sortingOrder = 8; // En arkada
            
            // Scale'i zone size'a göre ayarla
            if (isTriangle)
            {
                fillRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
            else
            {
                fillRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }
        
        // Shape'i ayarla
        SetShape();
    }
    
    /// <summary>
    /// LineRenderer ayarla (neon efekt için)
    /// </summary>
    void SetupLineRenderer(LineRenderer lr, float width, int sortingOrder)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = true;
        lr.useWorldSpace = false; // Local space
        lr.sortingOrder = sortingOrder;
        
        // Additive blend için (glow efekti)
        lr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    }
    
    /// <summary>
    /// Zone shape'ini ayarla (üçgen veya kare)
    /// </summary>
    void SetShape()
    {
        Vector3[] points;
        
        if (isTriangle)
        {
            // Üçgen shape - AYARLANABILIR!
            float w = size.x * triangleBaseWidth;
            float h = size.y * triangleHeight;
            
            // Flip kontrolü
            float topY = h * 0.5f + triangleTopOffset;
            float bottomY = -h * 0.5f;
            
            if (flipVertically)
            {
                // Ters çevir
                topY = -h * 0.5f - triangleTopOffset;
                bottomY = h * 0.5f;
            }
            
            points = new Vector3[]
            {
                new Vector3(0, topY, 0),           // Üst (veya alt)
                new Vector3(w * 0.5f, bottomY, 0),   // Sağ
                new Vector3(-w * 0.5f, bottomY, 0)   // Sol
            };
        }
        else
        {
            // Kare shape
            float w = size.x * 0.5f;
            float h = size.y * 0.5f;
            
            points = new Vector3[]
            {
                new Vector3(-w, h, 0),   // Sol üst
                new Vector3(w, h, 0),    // Sağ üst
                new Vector3(w, -h, 0),   // Sağ alt
                new Vector3(-w, -h, 0)   // Sol alt
            };
        }
        
        // Border line
        borderLine.positionCount = points.Length;
        borderLine.SetPositions(points);
        
        // Glow line
        glowLine.positionCount = points.Length;
        glowLine.SetPositions(points);
    }
    
    /// <summary>
    /// Renkleri uygula (pulse ekli)
    /// </summary>
    void ApplyColors(float pulseOffset)
    {
        // Pulse brightness modifier
        float brightness = 1f + pulseOffset;
        
        Color borderColor = currentColor * brightness;
        Color glowColor = currentColor * brightness * 0.6f; // Glow biraz daha soluk
        Color fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a * fillAlpha);
        
        // Border
        borderLine.startColor = borderColor;
        borderLine.endColor = borderColor;
        
        // Glow
        glowLine.startColor = glowColor;
        glowLine.endColor = glowColor;
        
        // Fill
        if (fillRenderer != null)
        {
            fillRenderer.color = fillColor;
        }
    }
    
    /// <summary>
    /// Basit beyaz sprite oluştur (fill için)
    /// </summary>
    Sprite CreateSimpleSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Idle durumuna dön
    /// </summary>
    public void ResetToIdle()
    {
        SetHighlight(false);
    }
}