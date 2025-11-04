using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardProjectile : MonoBehaviour
{
    public Card.CardColor cardColor;
    public Vector3 direction; // YENİ - Herhangi bir yöne gidebilir
    public float speed = 8f;
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetVisualColor();
    }

    void Update()
    {
        // Debug - İlk 2 saniye yönü yazdır
        if (Time.time < 2f)
        {
            Debug.Log("CardProjectile Direction: " + direction + " | Position: " + transform.position);
        }
        
        // Belirlenen yöne doğru hareket et
        transform.position += direction * speed * Time.deltaTime;
        
        // Çarpışma kontrolü
        CheckEnemyCollision();
        
        // Ekranın dışına çıktıysa yok ol
        if (transform.position.x > 12f || transform.position.x < -12f || 
            transform.position.y > 6f || transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void SetVisualColor()
    {
        switch (cardColor)
        {
            case Card.CardColor.Red:
                spriteRenderer.color = new Color(0.93f, 0.27f, 0.27f);
                break;
            case Card.CardColor.Blue:
                spriteRenderer.color = new Color(0.23f, 0.51f, 0.96f);
                break;
            case Card.CardColor.Green:
                spriteRenderer.color = new Color(0.13f, 0.77f, 0.37f);
                break;
            case Card.CardColor.Yellow:
                spriteRenderer.color = new Color(0.92f, 0.70f, 0.03f);
                break;
        }
    }

    void CheckEnemyCollision()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
    
        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
        
            // Çarptı mı?
            if (distance < 0.5f)
            {
                
                // Hasar ver (oyuncu ateşi = 1 hasar)
                enemy.TakeDamage(1, false);
            
                // Projectile'ı yok et
                Destroy(gameObject);
                return;
            }
        }
    }
}