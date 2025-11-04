using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHoverEffect : MonoBehaviour
{
    [Header("Hover Ayarları")]
    public float hoverScale = 1.2f; // Sprite için 1.2 iyi
    public float hoverSpeed = 10f;
    public float hoverYOffset = 0.5f; // World space - küçük değer
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 targetScale;
    private Vector3 targetPosition;
    private bool isHovering = false;
    
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        targetScale = originalScale;
        targetPosition = originalPosition;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortOrder = spriteRenderer.sortingOrder;
        }
    }

    void Update()
    {
        // Yumuşak geçiş
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverSpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * hoverSpeed);
    }

    // SPRITE için - Mouse kartın üzerine geldi
    void OnMouseEnter()
    {
        Debug.Log($"🖱️ MOUSE GİRDİ: {gameObject.name}");
        
        // Pause modunda hover yapma
        if (Time.timeScale == 0f) return;
        
        isHovering = true;
        targetScale = originalScale * hoverScale;
        targetPosition = originalPosition + new Vector3(0, hoverYOffset, 0);
        
        // En üste getir
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }
    }

    // Mouse karttan çıktı
    void OnMouseExit()
    {
        Debug.Log($"🖱️ MOUSE ÇIKTI: {gameObject.name}");

        isHovering = false;
        targetScale = originalScale;
        targetPosition = originalPosition;
        
        // Sorting order'ı eski haline getir
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortOrder;
        }
    }
    
    // Pozisyon güncellendiğinde çağrılır
    public void UpdateOriginalTransform()
    {
        if (!isHovering)
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;
            targetPosition = originalPosition;
            targetScale = originalScale;
        }
    }
}