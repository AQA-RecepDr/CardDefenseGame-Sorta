using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour
{
    public enum CardColor
    {
            Red,      // Turret
            Blue,     // Yavaşlatma
            Green,    // Can kazanma
            Yellow,   // Boss'a elektrik
            Purple,   // Toxic (Zehir) - YENİ!
            Orange    // Debuff (Zayıflık) - YENİ!
    }
    
    private TextMesh cooldownText; // Zone'da cooldown göstergesi
    private GameObject cooldownTextObj;

    public CardColor cardColor;
    private SpriteRenderer spriteRenderer;
    
    // Cooldown sistemi - Sadece lane yerleştirme için
    public float placeCooldownTime = 3f;
    private float placeCooldownTimer = 0f;
    public bool isPlaceOnCooldown = false;
    private LineRenderer cooldownCircle;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateVisual();
        AddNeonBorder();
        CreateCooldownCircle();
        CreateHotkeyText();
    }
    void CreateHotkeyText()
    {
        int hotkeyNum = transform.GetSiblingIndex() + 1;
    
        GameObject textObj = new GameObject("HotkeyText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, -0f, -0.1f); // AŞAĞI TAŞINDI! (0.35 → -0.5)
        textObj.transform.localScale = Vector3.one * 1.2f;
    
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = hotkeyNum.ToString();
        textMesh.fontSize = 65;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.fontStyle = FontStyle.Bold;
    
        MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "Default";
            meshRenderer.sortingOrder = 10;
        }
    
        Debug.Log($"🔢 Hotkey oluşturuldu: {hotkeyNum}");
    }
    void Update()
    {
        // Cooldown sayacı
        if (isPlaceOnCooldown)
        {
            placeCooldownTimer -= Time.deltaTime;
            
            // COOLDOWN TEXT GÜNCELLE - YENİ! ⏰
            if (cooldownText != null)
            {
                int timeLeft = Mathf.CeilToInt(placeCooldownTimer);
                cooldownText.text = timeLeft.ToString();
            
                // Renk değişimi (kırmızı → sarı → yeşil)
                if (timeLeft > 15)
                {
                    cooldownText.color = Color.red; // Uzun süre
                }
                else if (timeLeft > 5)
                {
                    cooldownText.color = Color.yellow; // Orta
                }
                else
                {
                    cooldownText.color = Color.green; // Az kaldı!
                }
            }
            
            // Cooldown circle güncelle
            UpdateCooldownCircle();
            
            if (placeCooldownTimer <= 0)
            {
                isPlaceOnCooldown = false;
                UpdateVisual();
            }
            if (cooldownCircle != null)
            {
                cooldownCircle.positionCount = 0;
            }
        }
    }
    
    void CreateCooldownCircle()
    {
        GameObject circleObj = new GameObject("CooldownCircle");
        circleObj.transform.SetParent(transform, false);
        circleObj.transform.localPosition = Vector3.zero;
    
        cooldownCircle = circleObj.AddComponent<LineRenderer>();
        cooldownCircle.material = new Material(Shader.Find("Sprites/Default"));
        cooldownCircle.startWidth = 0.1f; // Daha kalın (0.06 → 0.1)
        cooldownCircle.endWidth = 0.1f;
        cooldownCircle.positionCount = 0;
        cooldownCircle.loop = false;
        cooldownCircle.sortingOrder = 15; // Daha üstte (2 → 15)
        cooldownCircle.useWorldSpace = false;
    
        // PARLAK KIRMIZI (bloom için!)
        cooldownCircle.startColor = new Color(1f, 0f, 0f, 1f);
        cooldownCircle.endColor = new Color(1f, 0f, 0f, 1f);
    
        Debug.Log("⭕ Cooldown circle oluşturuldu!");
    }
    
    
    void AddNeonBorder()
    {
        // Null check
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }
    
        // Ayrı border objesi oluştur
        GameObject borderObj = new GameObject("CardBorder");
        borderObj.transform.SetParent(transform, false);
        borderObj.transform.localPosition = Vector3.zero;
    
        LineRenderer border = borderObj.AddComponent<LineRenderer>();
        border.material = new Material(Shader.Find("Sprites/Default"));
        border.startWidth = 0.08f;
        border.endWidth = 0.08f;
        border.positionCount = 5;
        border.loop = true;
        border.sortingOrder = 1;
        border.useWorldSpace = false;
    
        // Renk
        Color borderColor = spriteRenderer.color;
        borderColor.a = 1f;
        border.startColor = borderColor;
        border.endColor = borderColor;
    
        // Şekil
        float width = 0.6f;
        float height = 0.9f;
    
        border.SetPosition(0, new Vector3(-width/2, -height/2, 0));
        border.SetPosition(1, new Vector3(width/2, -height/2, 0));
        border.SetPosition(2, new Vector3(width/2, height/2, 0));
        border.SetPosition(3, new Vector3(-width/2, height/2, 0));
        border.SetPosition(4, new Vector3(-width/2, -height/2, 0));
    
        Debug.Log("✅ Border child objesi oluşturuldu!");
    }
    
    public void SetColor(CardColor newColor)
    {
        cardColor = newColor;
        UpdateVisual();
    }
    
    // Zone'da cooldown text oluştur
    public void CreateCooldownText()
    {
        // Zaten varsa yok et
        if (cooldownTextObj != null)
        {
            Destroy(cooldownTextObj);
        }
    
        cooldownTextObj = new GameObject("CooldownText");
        cooldownTextObj.transform.SetParent(transform, false);
        cooldownTextObj.transform.localPosition = new Vector3(0, 0, -0.1f); // Kartın ortasında
        cooldownTextObj.transform.localScale = Vector3.one * 2.0f; // Büyük
    
        cooldownText = cooldownTextObj.AddComponent<TextMesh>();
        cooldownText.text = Mathf.CeilToInt(placeCooldownTimer).ToString();
        cooldownText.fontSize = 100;
        cooldownText.characterSize = 0.1f;
        cooldownText.anchor = TextAnchor.MiddleCenter;
        cooldownText.alignment = TextAlignment.Center;
        cooldownText.color = Color.white;
        cooldownText.fontStyle = FontStyle.Bold;
    
        // Renderer ayarları
        MeshRenderer meshRenderer = cooldownTextObj.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "Default";
            meshRenderer.sortingOrder = 20; // En üstte
        }
    
        Debug.Log($"⏰ Cooldown text oluşturuldu: {placeCooldownTimer}s");
    }

// Cooldown text'i gizle/yok et
    public void HideCooldownText()
    {
        if (cooldownTextObj != null)
        {
            Destroy(cooldownTextObj);
            cooldownTextObj = null;
            cooldownText = null;
        }
    }
    
    
    void UpdateCooldownCircle()
    {
        if (cooldownCircle == null) return;
    
        // Cooldown progress (1.0 → 0.0)
        float progress = placeCooldownTimer / placeCooldownTime;
    
        // Daha az segment ama daha görünür
        int segments = Mathf.Max(3, Mathf.CeilToInt(progress * 30)); // 30 segment
        cooldownCircle.positionCount = segments + 1;
    
        if (segments <= 3)
        {
            cooldownCircle.positionCount = 0;
            return;
        }
    
        // Circle çiz
        float radius = 0.55f; // Daha büyük (0.5 → 0.55)
        float startAngle = -90f; // 12 o'clock pozisyonu
    
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = startAngle + (360f * progress * t);
            float angleRad = angle * Mathf.Deg2Rad;
        
            Vector3 pos = new Vector3(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius,
                -0.1f // Z offset (önde olsun)
            );
        
            cooldownCircle.SetPosition(i, pos);
        }
    
        // Debug
        Debug.Log($"⭕ Cooldown: {progress:P0}");
    }
    
    void UpdateVisual()
    {
        if (spriteRenderer == null) return;
    
        switch (cardColor)
        {
            case CardColor.Red:
                spriteRenderer.color = new Color(0.93f, 0.27f, 0.27f);
                break;
            case CardColor.Blue:
                spriteRenderer.color = new Color(0.23f, 0.51f, 0.96f);
                break;
            case CardColor.Green:
                spriteRenderer.color = new Color(0.13f, 0.77f, 0.37f);
                break;
            case CardColor.Yellow:
                spriteRenderer.color = new Color(0.92f, 0.70f, 0.03f);
                break;
            case CardColor.Purple:
                spriteRenderer.color = new Color(0.58f, 0.29f, 0.98f);
                break;
            case CardColor.Orange:
                spriteRenderer.color = new Color(1f, 0.65f, 0f);
                break;
        }
    
        // Cooldown fade
        if (isPlaceOnCooldown)
        {
            Color fadeColor = spriteRenderer.color;
            fadeColor.a = 0.3f;
            spriteRenderer.color = fadeColor;
        }
    }

    public void StartPlaceCooldown()
    {
        isPlaceOnCooldown = true;
        placeCooldownTimer = placeCooldownTime;
        UpdateVisual();
    }
}