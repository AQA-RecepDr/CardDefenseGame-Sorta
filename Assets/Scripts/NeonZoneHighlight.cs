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
    public float fillAlpha = 0f; // İç dolgu alpha değeri
    public Color fillColor = Color.white; // Fill rengi
    public bool reverseGradient = false;
    
    // Internal
    private LineRenderer borderLine;
    private LineRenderer glowLine;
    private MeshRenderer fillMeshRenderer; //  SpriteRenderer → MeshRenderer
    private MeshFilter fillMeshFilter; //
    //second pulse
    private MeshRenderer secondaryFillRenderer;
    private MeshFilter secondaryFillFilter;
    private float secondaryPulseTimer = 0f;
    
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
        
        // Mesh'i yeniden oluştur - YENİ!
        if (fillMeshFilter != null)
        {
            CreateFillMesh();
        }
        
        // İkinci mesh'i de refresh - YENİ!
        if (secondaryFillFilter != null)
        {
            CreateSecondaryFillMesh();
        }
        
        // Rotation'ı güncelle
        if (borderLine != null)
            borderLine.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        if (glowLine != null)
            glowLine.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        if (fillMeshRenderer != null) // DEĞİŞTİ!
            fillMeshRenderer.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
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
            
            // İKİNCİ PULSE! 💫 (Daha hızlı, offset'li)
            secondaryPulseTimer += Time.deltaTime * (pulseSpeed * 2f); // 2x hızlı
            float secondaryPulse = Mathf.Sin(secondaryPulseTimer + 1.5f) * pulseIntensity * 0.6f; // Offset + daha hafif
            ApplySecondaryPulse(secondaryPulse);
        }
        else
        {
            ApplyColors(0f);
            ApplySecondaryPulse(0f);
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
    /// Fill rengini ve alpha'yı ayarla - YENİ!
    /// </summary>
    public void SetFillColor(Color color, float alpha)
    {
        fillColor = color;
        fillAlpha = alpha;
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
        SetupLineRenderer(borderLine, lineWidth, -1);
        
        // 2) GLOW LINE (glow efekti)
        GameObject glowObj = new GameObject("NeonGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        
        glowLine = glowObj.AddComponent<LineRenderer>();
        SetupLineRenderer(glowLine, glowWidth, -2);
        
        // 3) FILL (iç dolgu) - MESH BASED! 🎨
        if (showFill)
        {
            GameObject fillObj = new GameObject("NeonFill");
            fillObj.transform.SetParent(transform);
            fillObj.transform.localPosition = Vector3.zero;
            fillObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
            
            fillMeshFilter = fillObj.AddComponent<MeshFilter>();
            fillMeshRenderer = fillObj.AddComponent<MeshRenderer>();
            
            // Material oluştur
            Material fillMat = new Material(Shader.Find("Sprites/Default"));
            fillMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fillMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fillMat.EnableKeyword("_VERTEX_COLORS"); // Vertex color'ları aktif et
            fillMeshRenderer.material = fillMat;
            fillMeshRenderer.sortingLayerName = "Default";
            fillMeshRenderer.sortingOrder = -3;
            
            // Mesh'i oluştur
            CreateFillMesh();
            
            // İKİNCİ FILL KATMANI! 💫
            GameObject secondaryFillObj = new GameObject("SecondaryFill");
            secondaryFillObj.transform.SetParent(transform);
            secondaryFillObj.transform.localPosition = Vector3.zero;
            secondaryFillObj.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
            
            secondaryFillFilter = secondaryFillObj.AddComponent<MeshFilter>();
            secondaryFillRenderer = secondaryFillObj.AddComponent<MeshRenderer>();
            
            // Material oluştur (aynı material)
            Material secondaryMat = new Material(Shader.Find("Sprites/Default"));
            secondaryMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            secondaryMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            secondaryMat.EnableKeyword("_VERTEX_COLORS");
            secondaryFillRenderer.material = secondaryMat;
            secondaryFillRenderer.sortingLayerName = "Default";
            secondaryFillRenderer.sortingOrder = -4; // Ana fill'in altında
            
            // Mesh'i oluştur (aynı mesh)
            CreateSecondaryFillMesh();
            
            // Mesh'i oluştur
            CreateFillMesh();
        }
        
        // Shape'i ayarla
        SetShape();
    }
    
    /// <summary>
    /// Fill için mesh oluştur (zone shape'ine uygun) - YENİ!
    /// </summary>
    void CreateFillMesh()
    {
        if (fillMeshFilter == null) return;
        
        Mesh mesh = new Mesh();
        
        if (isTriangle)
        {
            // Üçgen mesh
            float w = size.x * triangleBaseWidth;
            float h = size.y * triangleHeight;
            
            float topY = h * 0.5f + triangleTopOffset;
            float bottomY = -h * 0.5f;
            
            if (flipVertically)
            {
                topY = -h * 0.5f - triangleTopOffset;
                bottomY = h * 0.5f;
            }
            
            // Vertices (3 nokta)
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, topY, 0),              // Üst
                new Vector3(w * 0.5f, bottomY, 0),    // Sağ alt
                new Vector3(-w * 0.5f, bottomY, 0)    // Sol alt
            };
            
            // Triangles (1 üçgen = 3 index)
            mesh.triangles = new int[] { 0, 1, 2 };
            
            // UV mapping
            mesh.uv = new Vector2[]
            {
                new Vector2(0.5f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f)
            };
            // GRADIENT! 🎨 Merkeze yakın renkli, uzağa doğru transparan
            // Zone yönüne göre hangi nokta uzak, hangisi yakın?
            float nearAlpha = 0.3f;  // Merkeze yakın = tam renkli
            float farAlpha = 0f;   // Uzak = transparan
            
            // Üst nokta uzaksa (Top zone gibi), alt kenarlar yakın
            // flipVertically kontrolü ile düzgün gradient
            Color nearColor = Color.white;
            nearColor.a = nearAlpha;
            Color farColor = Color.white;
            farColor.a = farAlpha;
            
            // reverseGradient kontrolü - Zone Top için ters olacak
            bool shouldReverse = reverseGradient ? !flipVertically : flipVertically;
            
            if (!shouldReverse)
            {
                // Normal: Üst uzak, alt yakın
                mesh.colors = new Color[]
                {
                    farColor,   // Üst nokta → transparan
                    nearColor,  // Sağ alt → renkli
                    nearColor   // Sol alt → renkli
                };
            }
            else
            {
                // Ters: Alt uzak, üst yakın
                mesh.colors = new Color[]
                {
                    nearColor,  // Üst nokta → renkli
                    farColor,   // Sağ alt → transparan
                    farColor    // Sol alt → transparan
                };
            }
            
        }
        else
        {
            // Kare mesh
            float w = size.x * 0.5f;
            float h = size.y * 0.5f;
            
            mesh.vertices = new Vector3[]
            {
                new Vector3(-w, h, 0),   // Sol üst
                new Vector3(w, h, 0),    // Sağ üst
                new Vector3(w, -h, 0),   // Sağ alt
                new Vector3(-w, -h, 0)   // Sol alt
            };
            
            // Triangles (2 üçgen = 6 index)
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };
            // GRADIENT! 🎨 - Kare için basit yukarı→aşağı gradient
            Color topColor = Color.white;
            topColor.a = 0f; // Üst → transparan
            Color bottomColor = Color.white;
            bottomColor.a = 1f; // Alt → renkli
            
            mesh.colors = new Color[]
            {
                topColor,    // Sol üst
                topColor,    // Sağ üst
                bottomColor, // Sağ alt
                bottomColor  // Sol alt
            };
        }
        
        mesh.RecalculateNormals();
        fillMeshFilter.mesh = mesh;
        
        // İkinci katman için de aynı mesh'i kopyala
        if (secondaryFillFilter != null)
        {
            secondaryFillFilter.mesh = mesh;
        }
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
        lr.sortingLayerName = "Default";
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
        Color baseColor = currentColor;
        float brightness = 1f + pulseOffset;
        
        // Border (ince çizgi)
        if (borderLine != null)
        {
            Color borderColor = baseColor * brightness;
            borderColor.a = baseColor.a;
            borderLine.startColor = borderColor;
            borderLine.endColor = borderColor;
        }
        
        // Glow (kalın çizgi)
        if (glowLine != null)
        {
            Color glowColor = baseColor * brightness * 0.6f;
            glowColor.a = baseColor.a * 0.5f;
            glowLine.startColor = glowColor;
            glowLine.endColor = glowColor;
        }
        
        // Fill (iç dolgu) - MESH BASED! 🎨
        if (fillMeshRenderer != null && showFill)
        {
            Color fillCol = fillColor;
            fillCol.a = fillAlpha * brightness;
            fillMeshRenderer.material.color = fillCol;
        }
    }
    
    /// <summary>
    /// İkinci fill mesh'i oluştur (ana mesh'in kopyası)
    /// </summary>
    void CreateSecondaryFillMesh()
    {
        if (secondaryFillFilter == null || fillMeshFilter == null) return;
        
        // Ana mesh'i kopyala
        secondaryFillFilter.mesh = fillMeshFilter.mesh;
    }
    
    /// <summary>
    /// İkinci pulse katmanına renk uygula
    /// </summary>
    void ApplySecondaryPulse(float pulseOffset)
    {
        if (secondaryFillRenderer != null && showFill)
        {
            float brightness = 1f + pulseOffset;
            Color fillCol = fillColor;
            fillCol.a = fillAlpha * 0.4f * brightness; // Ana fill'in %40'ı kadar alpha
            secondaryFillRenderer.material.color = fillCol;
        }
    }
    
    /// <summary>
    /// Idle durumuna dön
    /// </summary>
    public void ResetToIdle()
    {
        SetHighlight(false);
        
        // Fill'i gizle - YENİ!
        fillAlpha = 0f;
        fillColor = Color.white;
    }
}