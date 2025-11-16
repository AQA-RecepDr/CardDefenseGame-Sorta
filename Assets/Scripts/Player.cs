using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public int maxHealth = 20;
    public int currentHealth;
    private SpriteRenderer spriteRenderer;
    
    [Header("Circular Health Bar - YENİ!")]
    public CircularHealthBar circularHealthBar;

    void Start()
    {
        // PERMANENT UPGRADE - MAX HEALTH BONUS EKLE!
        int baseMaxHealth = maxHealth; // Inspector'dan gelen değer (20)
    
        if (PermanentUpgradeManager.Instance != null)
        {
            float healthBonus = PermanentUpgradeManager.Instance.GetMaxHealthBonus();
            maxHealth = baseMaxHealth + (int)healthBonus;
        
            Debug.Log($"Max Health: {baseMaxHealth} + {healthBonus} = {maxHealth}");
        }
    
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    
        // Circular health bar'ı güncelle - YENİ! 🎯
        if (circularHealthBar != null)
        {
            circularHealthBar.SetHealth(currentHealth, maxHealth);
        }
    }

  // Oyuncu hasar alsın (düşman geçtiğinde)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Hasar aldığında kırmızı yanıp sönsün
        StartCoroutine(DamageFlash());
        
        // SCREEN SHAKE EKLE - YENİ!
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.1f); // Orta şiddette - acı verici!
        }
        
        // PLAYER HURT SESİ - YENİ!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerHurt();
        }
        
        Debug.Log("Oyuncu hasar aldı! Can: " + currentHealth);
        
        // Circular health bar'ı güncelle - YENİ! 🎯
        if (circularHealthBar != null)
        {
            circularHealthBar.SetHealth(currentHealth, maxHealth);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        Debug.Log("Oyuncu öldü! Oyun kaybedildi!");
    
        // GameManager'a haber ver
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }
    }
    
    // Can kazan
    public void Heal(int amount)
    {
        currentHealth += amount;
    
        // Max canı geçmesin
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    
        // Circular health bar'ı güncelle - YENİ! 🎯
        if (circularHealthBar != null)
        {
            circularHealthBar.SetHealth(currentHealth, maxHealth);
        }
    
        Debug.Log("Can kazanıldı! Can: " + currentHealth);
    }
}