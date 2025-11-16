using UnityEngine;

/// <summary>
/// Düşmanlar için geometrik şekil renderer
/// Her düşman tipi farklı şekil
/// </summary>
public class EnemyShapeRenderer : MonoBehaviour
{
    public enum ShapeType
    {
        Triangle,   // Üçgen - White (hızlı)
        Square,     // Kare - Black (tank)
        Pentagon,   // Beşgen - Yellow (charge)
        Hexagon,    // Altıgen - Orange (minion)
        Diamond,    // Elmas - Blue (slow)
        Star,       // Yıldız - Red (boss)
        Circle      // Daire - Default/fallback
    }
    
    [Header("Shape Settings")]
    public ShapeType shapeType = ShapeType.Circle;
    public float size = 0.5f; // Şekil boyutu
    public Color shapeColor = Color.white;
    
    [Header("Neon Glow")]
    public bool enableGlow = true;
    public float glowIntensity = 1.5f;
    public float glowSize = 0.15f; // Glow kalınlığı
    
    [Header("Gradient")]
    public bool enableGradient = true;
    public float gradientStrength = 0.4f; // Merkez ne kadar koyu (0-1)
    
    [Header("Animation")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.1f;
    
    // Internal
    private MeshFilter shapeMeshFilter;
    private MeshRenderer shapeMeshRenderer;
    private MeshFilter glowMeshFilter;
    private MeshRenderer glowMeshRenderer;
    
    private Material shapeMaterial;
    private Material glowMaterial;
    
    private float pulseTimer = 0f;
    
    void Start()
    {
        CreateShape();
    }
    
    void Update()
    {
        // Pulse animasyonu
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
            float brightness = 1f + pulse;
            
            // Rengi güncelle
            if (shapeMaterial != null)
            {
                Color pulseColor = shapeColor * brightness;
                shapeMaterial.color = pulseColor;
            }
            
            if (glowMaterial != null && enableGlow)
            {
                Color glowColor = shapeColor * glowIntensity * brightness;
                glowColor.a = shapeColor.a * 0.6f;
                glowMaterial.color = glowColor;
            }
        }
    }
    
    /// <summary>
    /// Şekli oluştur
    /// </summary>
    void CreateShape()
    {
        // 1) ANA ŞEKİL
        GameObject shapeObj = new GameObject("Shape");
        shapeObj.transform.SetParent(transform);
        shapeObj.transform.localPosition = Vector3.zero;
        
        shapeMeshFilter = shapeObj.AddComponent<MeshFilter>();
        shapeMeshRenderer = shapeObj.AddComponent<MeshRenderer>();
        
        // Material
        shapeMaterial = new Material(Shader.Find("Sprites/Default"));
        shapeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shapeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shapeMeshRenderer.material = shapeMaterial;
        shapeMeshRenderer.sortingLayerName = "Default";
        shapeMeshRenderer.sortingOrder = 0;
        
        // Mesh oluştur
        shapeMeshFilter.mesh = CreateShapeMesh();
        
        // 2) GLOW (varsa)
        if (enableGlow)
        {
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(transform);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localScale = Vector3.one * (1f + glowSize);
            
            glowMeshFilter = glowObj.AddComponent<MeshFilter>();
            glowMeshRenderer = glowObj.AddComponent<MeshRenderer>();
            
            // Glow material (additive)
            glowMaterial = new Material(Shader.Find("Sprites/Default"));
            glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
            glowMeshRenderer.material = glowMaterial;
            glowMeshRenderer.sortingLayerName = "Default";
            glowMeshRenderer.sortingOrder = -1;
            
            // Glow mesh (aynı şekil, biraz büyük)
            glowMeshFilter.mesh = CreateShapeMesh();
        }
        
        // İlk rengi ayarla
        UpdateColors();
    }
    
    /// <summary>
    /// Şekil mesh'i oluştur
    /// </summary>
    Mesh CreateShapeMesh()
    {
        Mesh mesh = new Mesh();
        
        switch (shapeType)
        {
            case ShapeType.Triangle:
                mesh = CreateTriangle();
                break;
            case ShapeType.Square:
                mesh = CreateSquare();
                break;
            case ShapeType.Pentagon:
                mesh = CreatePolygon(5);
                break;
            case ShapeType.Hexagon:
                mesh = CreatePolygon(6);
                break;
            case ShapeType.Diamond:
                mesh = CreateDiamond();
                break;
            case ShapeType.Star:
                mesh = CreateStar();
                break;
            case ShapeType.Circle:
                mesh = CreateCircle();
                break;
        }
        
        return mesh;
    }
    
    /// <summary>
    /// Üçgen oluştur
    /// </summary>
    Mesh CreateTriangle()
    {
        Mesh mesh = new Mesh();
        
        float h = size;
        float w = size * 0.866f; // sqrt(3)/2
        
        // Vertices
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, h * 0.5f, 0),        // Üst
            new Vector3(w * 0.5f, -h * 0.5f, 0),  // Sağ alt
            new Vector3(-w * 0.5f, -h * 0.5f, 0)  // Sol alt
        };
        
        // Triangles
        int[] triangles = new int[] { 0, 1, 2 };
        
        // Vertex colors (gradient için)
        Color[] colors = GetGradientColors(vertices);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Kare oluştur
    /// </summary>
    Mesh CreateSquare()
    {
        Mesh mesh = new Mesh();
        
        float s = size * 0.5f;
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-s, s, 0),   // Sol üst
            new Vector3(s, s, 0),    // Sağ üst
            new Vector3(s, -s, 0),   // Sağ alt
            new Vector3(-s, -s, 0)   // Sol alt
        };
        
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        
        Color[] colors = GetGradientColors(vertices);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Çokgen oluştur (Pentagon, Hexagon)
    /// </summary>
    Mesh CreatePolygon(int sides)
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[sides + 1]; // +1 merkez için
        int[] triangles = new int[sides * 3];
        
        // Merkez nokta
        vertices[0] = Vector3.zero;
        
        // Kenar noktaları
        float angleStep = 360f / sides;
        for (int i = 0; i < sides; i++)
        {
            float angle = (angleStep * i - 90f) * Mathf.Deg2Rad; // -90 = üstten başla
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * size,
                Mathf.Sin(angle) * size,
                0f
            );
        }
        
        // Üçgenler (merkez + iki kenar nokta)
        for (int i = 0; i < sides; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % sides + 1;
        }
        
        Color[] colors = GetGradientColors(vertices);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Elmas oluştur (döndürülmüş kare)
    /// </summary>
    Mesh CreateDiamond()
    {
        Mesh mesh = new Mesh();
        
        float s = size;
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, s, 0),     // Üst
            new Vector3(s * 0.7f, 0, 0),    // Sağ
            new Vector3(0, -s, 0),    // Alt
            new Vector3(-s * 0.7f, 0, 0)    // Sol
        };
        
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        
        Color[] colors = GetGradientColors(vertices);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Yıldız oluştur (5 köşeli)
    /// </summary>
    Mesh CreateStar()
    {
        Mesh mesh = new Mesh();
        
        int points = 5;
        Vector3[] vertices = new Vector3[points * 2 + 1]; // Dış + iç noktalar + merkez
        
        // Merkez
        vertices[0] = Vector3.zero;
        
        float angleStep = 360f / points;
        float outerRadius = size;
        float innerRadius = size * 0.4f;
        
        for (int i = 0; i < points; i++)
        {
            // Dış nokta
            float outerAngle = (angleStep * i - 90f) * Mathf.Deg2Rad;
            vertices[i * 2 + 1] = new Vector3(
                Mathf.Cos(outerAngle) * outerRadius,
                Mathf.Sin(outerAngle) * outerRadius,
                0f
            );
            
            // İç nokta (aralar)
            float innerAngle = (angleStep * (i + 0.5f) - 90f) * Mathf.Deg2Rad;
            vertices[i * 2 + 2] = new Vector3(
                Mathf.Cos(innerAngle) * innerRadius,
                Mathf.Sin(innerAngle) * innerRadius,
                0f
            );
        }
        
        // Üçgenler
        int[] triangles = new int[points * 6]; // Her nokta için 2 üçgen
        for (int i = 0; i < points; i++)
        {
            int current = i * 2 + 1;
            int next = ((i + 1) % points) * 2 + 1;
            int inner = i * 2 + 2;
            
            // Üçgen 1 (merkez - dış - iç)
            triangles[i * 6] = 0;
            triangles[i * 6 + 1] = current;
            triangles[i * 6 + 2] = inner;
            
            // Üçgen 2 (merkez - iç - sonraki dış)
            triangles[i * 6 + 3] = 0;
            triangles[i * 6 + 4] = inner;
            triangles[i * 6 + 5] = next;
        }
        
        Color[] colors = GetGradientColors(vertices);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Daire oluştur (fallback)
    /// </summary>
    Mesh CreateCircle()
    {
        return CreatePolygon(16); // 16 kenarlı çokgen = daire gibi
    }
    
    /// <summary>
    /// Gradient renkleri hesapla (merkez koyu, kenar parlak)
    /// </summary>
    Color[] GetGradientColors(Vector3[] vertices)
    {
        if (!enableGradient)
        {
            // Gradient kapalı - hepsi aynı renk
            Color[] gcolors = new Color[vertices.Length];
            for (int i = 0; i < gcolors.Length; i++)
            {
                gcolors[i] = Color.white;
            }
            return gcolors;
        }
        
        // Gradient aktif - merkez koyu, kenar parlak
        Color[] colors = new Color[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = vertices[i].magnitude / size;
            float brightness = Mathf.Lerp(1f - gradientStrength, 1f, distance);
            colors[i] = Color.white * brightness;
        }
        
        return colors;
    }
    
    /// <summary>
    /// Renkleri güncelle
    /// </summary>
    void UpdateColors()
    {
        if (shapeMaterial != null)
        {
            shapeMaterial.color = shapeColor;
        }
        
        if (glowMaterial != null && enableGlow)
        {
            Color glowColor = shapeColor * glowIntensity;
            glowColor.a = shapeColor.a * 0.6f;
            glowMaterial.color = glowColor;
        }
    }
    
    /// <summary>
    /// Şekil tipini değiştir (runtime'da)
    /// </summary>
    public void SetShapeType(ShapeType newType)
    {
        shapeType = newType;
        
        if (shapeMeshFilter != null)
        {
            shapeMeshFilter.mesh = CreateShapeMesh();
        }
        
        if (glowMeshFilter != null)
        {
            glowMeshFilter.mesh = CreateShapeMesh();
        }
    }
    
    /// <summary>
    /// Rengi değiştir (runtime'da)
    /// </summary>
    public void SetColor(Color newColor)
    {
        shapeColor = newColor;
        UpdateColors();
    }
}