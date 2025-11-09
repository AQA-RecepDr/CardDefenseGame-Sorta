using System.Collections;
using UnityEngine;

public class ZoneHighlightEffect : MonoBehaviour
{
    [Header("Zone Shape")]
    public ZoneShape shape = ZoneShape.Triangle;
    public enum ZoneShape { Triangle, Rectangle }
    
    [Header("Border Settings")]
    public Color defaultColor = Color.white; // Varsayılan beyaz
    public float borderWidth = 0.1f;
    public Vector2 zoneSize = new Vector2(3f, 4f);
    
    [Header("Fade Animation")]
    public float fadeInSpeed = 3f; // Hızlı fade-in
    public float fadeOutSpeed = 2f; // Daha yavaş fade-out
    
    [Header("Animation")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseMinAlpha = 0.5f;
    public float pulseMaxAlpha = 0.9f;
    
    [Header("Glow Effect")]
    public bool enableGlow = true;
    public float glowIntensity = 0.5f;
    
    [Header("Fill Effect")]
    public bool enableFill = true;
    public float fillAlpha = 0.15f; // Hafif iç dolgu
    
    private LineRenderer borderLine;
    private LineRenderer glowLine;
    private SpriteRenderer fillSprite;
    private float pulseTimer = 0f;
    
    // Fade state
    private bool isVisible = false;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    
    // Color state
    private Color currentColor;
    private Color targetColor;
    
    void Start()
    {
        currentColor = defaultColor;
        targetColor = defaultColor;
        
        CreateBorder();
        if (enableGlow) CreateGlow();
        if (enableFill) CreateFill();
        
        // Başlangıçta görünmez
        SetAlpha(0f);
    }
    
    void Update()
    {
        // Fade animasyonu
        AnimateFade();
        
        // Color transition
        AnimateColorTransition();
        
        if (isVisible && enablePulse)
        {
            AnimatePulse();
        }
    }
    
    // Dışarıdan çağrılacak - Görünürlük + Renk
    public void Show(Color color)
    {
        Debug.Log($"💡 ZoneHighlightEffect.Show() çağrıldı! Color: {color}");
        isVisible = true;
        targetAlpha = 1f;
        targetColor = color;
        Debug.Log($"💡 targetAlpha: {targetAlpha}, currentAlpha: {currentAlpha}");
    }
    
    public void Hide()
    {
        Debug.Log($"🌑 ZoneHighlightEffect.Hide() çağrıldı!");
        isVisible = false;
        targetAlpha = 0f;
        targetColor = defaultColor;
    }
    
    void AnimateFade()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha))
            return;
        
        float speed = targetAlpha > currentAlpha ? fadeInSpeed : fadeOutSpeed;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, speed * Time.deltaTime);
    }
    
    void AnimateColorTransition()
    {
        if (currentColor != targetColor)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);
            UpdateColors();
        }
    }
    
    void CreateBorder()
    {
        GameObject borderObj = new GameObject("ZoneBorder");
        borderObj.transform.SetParent(transform, false);
        borderObj.transform.localPosition = Vector3.zero;
        
        borderLine = borderObj.AddComponent<LineRenderer>();
        borderLine.material = new Material(Shader.Find("Sprites/Default"));
        borderLine.startWidth = borderWidth;
        borderLine.endWidth = borderWidth;
        borderLine.loop = true;
        borderLine.sortingOrder = 5;
        borderLine.useWorldSpace = false;
        
        // Additive blend (glow)
        borderLine.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        borderLine.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        
        SetBorderShape();
    }
    
    void SetBorderShape()
    {
        if (shape == ZoneShape.Triangle)
        {
            borderLine.positionCount = 3;
            
            float width = zoneSize.x;
            float height = zoneSize.y;
            
            borderLine.SetPosition(0, new Vector3(0, height * 0.5f, 0));
            borderLine.SetPosition(1, new Vector3(width * 0.5f, -height * 0.5f, 0));
            borderLine.SetPosition(2, new Vector3(-width * 0.5f, -height * 0.5f, 0));
        }
        else
        {
            borderLine.positionCount = 4;
            
            float w = zoneSize.x * 0.5f;
            float h = zoneSize.y * 0.5f;
            
            borderLine.SetPosition(0, new Vector3(-w, h, 0));
            borderLine.SetPosition(1, new Vector3(w, h, 0));
            borderLine.SetPosition(2, new Vector3(w, -h, 0));
            borderLine.SetPosition(3, new Vector3(-w, -h, 0));
        }
    }
    
    void CreateGlow()
    {
        GameObject glowObj = new GameObject("ZoneGlow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.localPosition = Vector3.zero;
        
        glowLine = glowObj.AddComponent<LineRenderer>();
        glowLine.material = new Material(Shader.Find("Sprites/Default"));
        glowLine.startWidth = borderWidth * 3f;
        glowLine.endWidth = borderWidth * 3f;
        glowLine.loop = true;
        glowLine.sortingOrder = 4;
        glowLine.useWorldSpace = false;
        
        glowLine.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowLine.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        
        glowLine.positionCount = borderLine.positionCount;
        for (int i = 0; i < borderLine.positionCount; i++)
        {
            glowLine.SetPosition(i, borderLine.GetPosition(i));
        }
    }
    
    void CreateFill()
    {
        GameObject fillObj = new GameObject("ZoneFill");
        fillObj.transform.SetParent(transform, false);
        fillObj.transform.localPosition = Vector3.zero;
        
        fillSprite = fillObj.AddComponent<SpriteRenderer>();
        
        // Üçgen veya kare texture oluştur
        if (shape == ZoneShape.Triangle)
        {
            // Basit beyaz sprite kullan, scale ile ayarla
            fillSprite.sprite = CreateTriangleSprite();
        }
        
        fillSprite.sortingOrder = 3; // En arkada
        fillSprite.color = new Color(1, 1, 1, 0); // Başlangıçta görünmez
    }
    
    Sprite CreateTriangleSprite()
    {
        // Basit beyaz kare sprite kullan (Unity built-in)
        // Gerçek projede üçgen texture kullanılabilir
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
    
    void AnimatePulse()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        
        float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        float pulseAlpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, pulse);
        
        UpdateColors(pulseAlpha);
    }
    
    void UpdateColors(float pulseMultiplier = 1f)
    {
        float finalAlpha = currentAlpha * pulseMultiplier;
        
        // Border
        if (borderLine != null)
        {
            Color c = currentColor;
            c.a = finalAlpha;
            borderLine.startColor = c;
            borderLine.endColor = c;
        }
        
        // Glow
        if (glowLine != null)
        {
            Color g = currentColor;
            g.a = finalAlpha * glowIntensity;
            glowLine.startColor = g;
            glowLine.endColor = g;
        }
        
        // Fill
        if (fillSprite != null && enableFill)
        {
            Color f = currentColor;
            f.a = finalAlpha * fillAlpha;
            fillSprite.color = f;
        }
    }
    
    void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        UpdateColors();
    }
}