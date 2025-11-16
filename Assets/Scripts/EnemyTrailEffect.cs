using UnityEngine;

/// <summary>
/// Düşmanlar için neon trail effect
/// Hareket ederken arkalarında parlak iz bırakır
/// </summary>
public class EnemyTrailEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    public Color trailColor = Color.white;
    public float trailDuration = 0.3f; // İz ne kadar kalır
    public float trailStartWidth = 0.3f;
    public float trailEndWidth = 0.05f;
    
    [Header("Glow")]
    public float glowIntensity = 1.5f;
    public bool useAdditiveBlend = true; // Additive = daha parlak
    
    [Header("Quality")]
    public int cornerVertices = 5;
    public int capVertices = 5;
    
    // Internal
    private TrailRenderer trailRenderer;
    
    void Start()
    {
        CreateTrail();
    }
    
    /// <summary>
    /// Trail renderer oluştur
    /// </summary>
    void CreateTrail()
    {
        // TrailRenderer ekle
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        
        // Material oluştur
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        
        if (useAdditiveBlend)
        {
            // Additive blend - daha parlak, neon gibi
            trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
        else
        {
            // Normal blend
            trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        
        trailRenderer.material = trailMat;
        
        // Renk ayarları (gradient)
        Color startColor = trailColor * glowIntensity;
        Color endColor = trailColor * glowIntensity;
        endColor.a = 0f; // Fade out
        
        trailRenderer.startColor = startColor;
        trailRenderer.endColor = endColor;
        
        // Boyut ayarları
        trailRenderer.startWidth = trailStartWidth;
        trailRenderer.endWidth = trailEndWidth;
        
        // Süre
        trailRenderer.time = trailDuration;
        
        // Quality
        trailRenderer.numCornerVertices = cornerVertices;
        trailRenderer.numCapVertices = capVertices;
        trailRenderer.minVertexDistance = 0.05f; // Minimum nokta mesafesi
        
        // Render ayarları
        trailRenderer.sortingLayerName = "Default";
        trailRenderer.sortingOrder = -1; // Düşmanın arkasında
        
        // Texture mode (smooth gradient)
        trailRenderer.textureMode = LineTextureMode.Stretch;
        
        Debug.Log($"💨 Trail effect oluşturuldu: {trailColor}");
    }
    
    /// <summary>
    /// Trail rengini değiştir
    /// </summary>
    public void SetTrailColor(Color color)
    {
        trailColor = color;
        
        if (trailRenderer != null)
        {
            Color startColor = trailColor * glowIntensity;
            Color endColor = trailColor * glowIntensity;
            endColor.a = 0f;
            
            trailRenderer.startColor = startColor;
            trailRenderer.endColor = endColor;
        }
    }
    
    /// <summary>
    /// Trail'i aktif/pasif yap
    /// </summary>
    public void SetTrailEnabled(bool enabled)
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = enabled;
        }
    }
    
    /// <summary>
    /// Trail'i temizle (anında yok et)
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    
    /// <summary>
    /// Trail süresini değiştir
    /// </summary>
    public void SetTrailDuration(float duration)
    {
        trailDuration = duration;
        
        if (trailRenderer != null)
        {
            trailRenderer.time = duration;
        }
    }
}