using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableCard : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private Vector3 originalPosition;

    void Start()
    {
        mainCamera = Camera.main;
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        // Oyun bittiyse sürükleme yapma
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;
        
        // Karta tıklandığında
        isDragging = true;
        
        // Mouse pozisyonu ile kart arasındaki farkı hesapla
        Vector3 mousePos = GetMouseWorldPosition();
        offset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        // Sürüklerken kartı mouse'u takip ettir
        if (isDragging)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            transform.position = mousePos + offset;
        }
    }

    void OnMouseUp()
    {
        // Bırakıldığında
        isDragging = false;
    
        // En yakın zone'u bul
        Zone closestZone = FindClosestZone();
    
        if (closestZone != null)
        {
            // Zone'e yerleştirmeyi dene
            Card card = GetComponent<Card>();
            bool placed = closestZone.TryPlaceCard(card);
        
            if (placed)
            {
                // Eski yerine dön
                transform.position = originalPosition;
                
                Debug.Log($"🎴 Kart Zone {closestZone.zoneIndex}'a yerleştirildi!");
                return;
            }
        }
    
        // Yerleştirilemediyse eski yerine dön
        transform.position = originalPosition;
        Debug.Log("❌ Kart yerleştirilemedi!");
    }

    // Mouse pozisyonunu dünya koordinatlarına çevir
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Kamera uzaklığı
        return mainCamera.ScreenToWorldPoint(mousePos);
    }
    
    Zone FindClosestZone()
    {
        Zone[] allZones = FindObjectsOfType<Zone>();
        Zone closest = null;
        float closestDistance = float.MaxValue;
        float maxPlaceDistance = 3f; // Maksimum yerleştirme mesafesi
    
        foreach (Zone zone in allZones)
        {
            // Zone'un card slot pozisyonuna uzaklık
            Vector3 targetPos = zone.cardSlot != null ? zone.cardSlot.position : zone.transform.position;
            float distance = Vector3.Distance(transform.position, targetPos);
        
            // Yeterince yakınsa ve en yakınsa
            if (distance < maxPlaceDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closest = zone;
            }
        }
    
        if (closest != null)
        {
            Debug.Log($"🎯 En yakın zone: {closest.zoneIndex}, Mesafe: {closestDistance:F2}");
        }
        else
        {
            Debug.Log($"⚠️ Yakın zone yok! (Max mesafe: {maxPlaceDistance})");
        }
    
        return closest;
    }
    
}