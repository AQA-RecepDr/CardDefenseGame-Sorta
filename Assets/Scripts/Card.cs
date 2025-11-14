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
    
    //private TextMesh cooldownText; // Zone'da cooldown göstergesi
   //private GameObject cooldownTextObj;

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
        //AddNeonBorder();
        //CreateCooldownCircle();
        //CreateHotkeyText();
    }
    void Update()
    {
        // Cooldown sayacı
        if (isPlaceOnCooldown)
        {
            placeCooldownTimer -= Time.deltaTime;
            
            if (placeCooldownTimer <= 0)
            {
                isPlaceOnCooldown = false;
                UpdateVisual();
            }
        }
    }
    
    
    
    public void SetColor(CardColor newColor)
    {
        cardColor = newColor;
        UpdateVisual();
    }
    
    void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        Color color;
    
        switch (cardColor)
        { 
            case CardColor.Red:
                ColorUtility.TryParseHtmlString("#A0153E", out color);
                spriteRenderer.color = color;
                break;
            case CardColor.Blue:
                ColorUtility.TryParseHtmlString("#3E92CC", out color);
                spriteRenderer.color = color;
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