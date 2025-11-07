using UnityEngine;

public class PlayerCrystal : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color glowColor = new Color(0f, 1f, 1f); // Cyan
    public float outlineThickness = 0.15f; // Kalınlık
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;
    
    private GameObject outlineObj;
    private SpriteRenderer outlineSprite;
    private SpriteRenderer mainSprite;
    private float baseThickness;
    
    void Start()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        
        if (mainSprite == null || mainSprite.sprite == null)
        {
            Debug.LogError("❌ Sprite bulunamadı!");
            return;
        }
        
        CreateOutline();
        baseThickness = outlineThickness;
    }
    
    void Update()
    {
        // Pulse animasyonu
        if (outlineObj != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            float currentThickness = baseThickness + pulse;
            
            // Outline'ı büyüt/küçült
            float scale = 1f + (currentThickness * 2f);
            outlineObj.transform.localScale = Vector3.one * scale;
        }
    }
    
    void CreateOutline()
    {
        // Outline objesi oluştur
        outlineObj = new GameObject("PlayerOutline");
        outlineObj.transform.SetParent(transform, false);
        outlineObj.transform.localPosition = Vector3.zero;
        
        // Sprite renderer ekle
        outlineSprite = outlineObj.AddComponent<SpriteRenderer>();
        outlineSprite.sprite = mainSprite.sprite; // Aynı sprite!
        outlineSprite.color = glowColor;
        outlineSprite.sortingOrder = mainSprite.sortingOrder - 1; // Arkada
        
        // Biraz büyük
        float scale = 1f + (outlineThickness * 2f);
        outlineObj.transform.localScale = Vector3.one * scale;
        
        // Material (glow için)
        outlineSprite.material = new Material(Shader.Find("Sprites/Default"));
        
        Debug.Log("✨ Player outline oluşturuldu!");
    }
}