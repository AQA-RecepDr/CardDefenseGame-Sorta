using UnityEngine;

/// <summary>
/// Düşman şeklinin içinde dönen animasyonlu çekirdek
/// </summary>
public class EnemyAnimatedCore : MonoBehaviour
{
    public enum CoreType
    {
        RotatingShape,    // Dönen şekil (üçgen, kare vs)
        PulsingOrb,       // Büyüyüp küçülen orb
        SpinningRing,     // Dönen halka
        RotatingCross     // Dönen artı işareti
    }
    
    [Header("Core Settings")]
    public CoreType coreType = CoreType.RotatingShape;
    public EnemyShapeRenderer.ShapeType coreShape = EnemyShapeRenderer.ShapeType.Triangle;
    public float coreSize = 0.2f; // Çekirdek boyutu (ana şeklin %40'ı)
    public Color coreColor = Color.white;
    
    [Header("Animation")]
    public float rotationSpeed = 90f; // Derece/saniye
    public bool enablePulse = true;
    public float pulseSpeed = 3f;
    public float pulseAmount = 0.15f; // Size değişimi
    
    [Header("Glow")]
    public float glowIntensity = 1.5f;
    
    // Internal
    private MeshFilter coreMeshFilter;
    private MeshRenderer coreMeshRenderer;
    private Material coreMaterial;
    
    private float currentRotation = 0f;
    private float pulseTimer = 0f;
    
    void Start()
    {
        CreateCore();
    }
    
    void Update()
    {
        // Rotation
        currentRotation += rotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0, 0, currentRotation);
        
        // Pulse
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmount;
            float scale = 1f + pulse;
            transform.localScale = Vector3.one * scale;
            
            // Brightness pulse
            if (coreMaterial != null)
            {
                float brightness = 1f + (pulse * 0.5f);
                Color pulseColor = coreColor * brightness * glowIntensity;
                coreMaterial.color = pulseColor;
            }
        }
    }
    
    /// <summary>
    /// Çekirdeği oluştur
    /// </summary>
    void CreateCore()
    {
        GameObject coreObj = new GameObject("Core");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        
        coreMeshFilter = coreObj.AddComponent<MeshFilter>();
        coreMeshRenderer = coreObj.AddComponent<MeshRenderer>();
        
        // Material (additive blend)
        coreMaterial = new Material(Shader.Find("Sprites/Default"));
        coreMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        coreMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        coreMeshRenderer.material = coreMaterial;
        coreMeshRenderer.sortingLayerName = "Default";
        coreMeshRenderer.sortingOrder = 1; // Şeklin üstünde
        
        // Mesh oluştur
        coreMeshFilter.mesh = CreateCoreMesh();
        
        // Rengi ayarla
        UpdateColor();
    }
    
    /// <summary>
    /// Core mesh'i oluştur
    /// </summary>
    Mesh CreateCoreMesh()
    {
        Mesh mesh = new Mesh();
        
        switch (coreType)
        {
            case CoreType.RotatingShape:
                mesh = CreateShapeMesh(coreShape);
                break;
            case CoreType.PulsingOrb:
                mesh = CreateCircleMesh();
                break;
            case CoreType.SpinningRing:
                mesh = CreateRingMesh();
                break;
            case CoreType.RotatingCross:
                mesh = CreateCrossMesh();
                break;
        }
        
        return mesh;
    }
    
    /// <summary>
    /// Şekil mesh'i (ana şekil sistemini kullan)
    /// </summary>
    Mesh CreateShapeMesh(EnemyShapeRenderer.ShapeType shape)
    {
        Mesh mesh = new Mesh();
        
        switch (shape)
        {
            case EnemyShapeRenderer.ShapeType.Triangle:
                mesh = CreateTriangle();
                break;
            case EnemyShapeRenderer.ShapeType.Square:
                mesh = CreateSquare();
                break;
            case EnemyShapeRenderer.ShapeType.Pentagon:
                mesh = CreatePolygon(5);
                break;
            case EnemyShapeRenderer.ShapeType.Hexagon:
                mesh = CreatePolygon(6);
                break;
            case EnemyShapeRenderer.ShapeType.Diamond:
                mesh = CreateDiamond();
                break;
            default:
                mesh = CreateCircleMesh();
                break;
        }
        
        return mesh;
    }
    
    /// <summary>
    /// Üçgen
    /// </summary>
    Mesh CreateTriangle()
    {
        Mesh mesh = new Mesh();
        
        float h = coreSize;
        float w = coreSize * 0.866f;
        
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, h * 0.5f, 0),
            new Vector3(w * 0.5f, -h * 0.5f, 0),
            new Vector3(-w * 0.5f, -h * 0.5f, 0)
        };
        
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Kare
    /// </summary>
    Mesh CreateSquare()
    {
        Mesh mesh = new Mesh();
        
        float s = coreSize * 0.5f;
        
        mesh.vertices = new Vector3[]
        {
            new Vector3(-s, s, 0),
            new Vector3(s, s, 0),
            new Vector3(s, -s, 0),
            new Vector3(-s, -s, 0)
        };
        
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Çokgen
    /// </summary>
    Mesh CreatePolygon(int sides)
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[sides + 1];
        int[] triangles = new int[sides * 3];
        
        vertices[0] = Vector3.zero;
        
        float angleStep = 360f / sides;
        for (int i = 0; i < sides; i++)
        {
            float angle = (angleStep * i - 90f) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * coreSize,
                Mathf.Sin(angle) * coreSize,
                0f
            );
        }
        
        for (int i = 0; i < sides; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % sides + 1;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Elmas
    /// </summary>
    Mesh CreateDiamond()
    {
        Mesh mesh = new Mesh();
        
        float s = coreSize;
        
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, s, 0),
            new Vector3(s * 0.7f, 0, 0),
            new Vector3(0, -s, 0),
            new Vector3(-s * 0.7f, 0, 0)
        };
        
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Daire
    /// </summary>
    Mesh CreateCircleMesh()
    {
        return CreatePolygon(12);
    }
    
    /// <summary>
    /// Halka (ring)
    /// </summary>
    Mesh CreateRingMesh()
    {
        Mesh mesh = new Mesh();
        
        int segments = 16;
        float outerRadius = coreSize;
        float innerRadius = coreSize * 0.6f;
        
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];
        
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = (angleStep * i) * Mathf.Deg2Rad;
            
            // Dış çember
            vertices[i * 2] = new Vector3(
                Mathf.Cos(angle) * outerRadius,
                Mathf.Sin(angle) * outerRadius,
                0f
            );
            
            // İç çember
            vertices[i * 2 + 1] = new Vector3(
                Mathf.Cos(angle) * innerRadius,
                Mathf.Sin(angle) * innerRadius,
                0f
            );
            
            // Üçgenler
            int current = i * 2;
            int next = ((i + 1) % segments) * 2;
            
            triangles[i * 6] = current;
            triangles[i * 6 + 1] = current + 1;
            triangles[i * 6 + 2] = next;
            
            triangles[i * 6 + 3] = current + 1;
            triangles[i * 6 + 4] = next + 1;
            triangles[i * 6 + 5] = next;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Artı işareti (cross)
    /// </summary>
    Mesh CreateCrossMesh()
    {
        Mesh mesh = new Mesh();
        
        float length = coreSize;
        float width = coreSize * 0.3f;
        
        // İki dikdörtgen (dikey + yatay)
        Vector3[] vertices = new Vector3[]
        {
            // Dikey çubuk
            new Vector3(-width * 0.5f, length * 0.5f, 0),
            new Vector3(width * 0.5f, length * 0.5f, 0),
            new Vector3(width * 0.5f, -length * 0.5f, 0),
            new Vector3(-width * 0.5f, -length * 0.5f, 0),
            
            // Yatay çubuk
            new Vector3(-length * 0.5f, width * 0.5f, 0),
            new Vector3(length * 0.5f, width * 0.5f, 0),
            new Vector3(length * 0.5f, -width * 0.5f, 0),
            new Vector3(-length * 0.5f, -width * 0.5f, 0)
        };
        
        int[] triangles = new int[]
        {
            // Dikey
            0, 1, 2, 0, 2, 3,
            // Yatay
            4, 5, 6, 4, 6, 7
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Rengi güncelle
    /// </summary>
    void UpdateColor()
    {
        if (coreMaterial != null)
        {
            coreMaterial.color = coreColor * glowIntensity;
        }
    }
    
    /// <summary>
    /// Core tipini değiştir
    /// </summary>
    public void SetCoreType(CoreType type, EnemyShapeRenderer.ShapeType shape = EnemyShapeRenderer.ShapeType.Triangle)
    {
        coreType = type;
        coreShape = shape;
        
        if (coreMeshFilter != null)
        {
            coreMeshFilter.mesh = CreateCoreMesh();
        }
    }
    
    /// <summary>
    /// Rengi değiştir
    /// </summary>
    public void SetColor(Color color)
    {
        coreColor = color;
        UpdateColor();
    }
}