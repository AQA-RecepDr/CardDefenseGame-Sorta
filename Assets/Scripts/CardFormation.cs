using System.Collections;
using UnityEngine;

public class CardFormation : MonoBehaviour
{
    [Header("Formation Settings")]
    public Sprite customSprite; // ← YENİ! Inspector'dan atanacak
    public Vector2 diamondSize = new Vector2(1.5f, 2f); // Genişlik x Yükseklik
    public Color formationColor = Color.red;
    public float glowIntensity = 1.5f;
    
    [Header("Animation")]
    public float flashDuration = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Material glowMaterial;
    
    void Start()
    {
        CreateDiamondVisual();
        PlaySpawnAnimation();
    }
    
    void CreateDiamondVisual()
    {
        // SpriteRenderer ekle
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        // CUSTOM SPRITE KULLAN! ✨
        if (customSprite != null)
        {
            spriteRenderer.sprite = customSprite; // ← Kendi sprite'ın!
        }
        else
        {
            spriteRenderer.sprite = CreateSimpleSprite(); // Fallback
        }
    
        spriteRenderer.color = formationColor;
        
        // Glow material
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        spriteRenderer.material = glowMaterial;
        
        // Sorting order (karakterin arkasında)
        spriteRenderer.sortingOrder = 1;
        
        // Diamond shape için scale ayarla
        transform.localScale = new Vector3(diamondSize.x, diamondSize.y, 1f);
    }
    
    Sprite CreateSimpleSprite()
    {
        // 128x128 piksel texture oluştur
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
    
        // Tüm pikselleri beyaz yap
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
    
        // Sprite oluştur (100 PPU = unity standart)
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
    
    void PlaySpawnAnimation()
    {
        StartCoroutine(SpawnFlash());
    }
    
    IEnumerator SpawnFlash()
    {
        // Flash efekti - çok parlak başla
        Color flashColor = formationColor * 3f; // Süper parlak
        flashColor.a = 1f;
        spriteRenderer.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        // Normal renge dön
        spriteRenderer.color = formationColor * glowIntensity;
    }
    
    public void PlayDespawnAnimation()
    {
        StartCoroutine(DespawnFlash());
    }
    
    IEnumerator DespawnFlash()
    {
        // Flash
        Color flashColor = formationColor * 3f;
        flashColor.a = 1f;
        spriteRenderer.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        // Fade out
        float elapsed = 0f;
        float fadeDuration = 0.2f;
        Color startColor = spriteRenderer.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);
            Color c = startColor;
            c.a = alpha;
            spriteRenderer.color = c;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}