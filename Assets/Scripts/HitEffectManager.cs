using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance;
    
    [Header("Hit Efektleri")]
    public GameObject hitParticlePrefab;
    
    [Header("Muzzle Flash")] // YENİ!
    public GameObject muzzleFlashPrefab;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Vuruş efekti göster
    public void ShowHitEffect(Vector3 position, Color color)
    {
        if (hitParticlePrefab != null)
        {
            GameObject effect = Instantiate(hitParticlePrefab, position, Quaternion.identity);
            
            // Rengi ayarla
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
            
            // 2 saniye sonra yok et
            Destroy(effect, 2f);
        }
    }
    
    // Hızlı kullanım - varsayılan renk
    public void ShowHitEffect(Vector3 position)
    {
        ShowHitEffect(position, new Color(1f, 0.6f, 0f)); // Turuncu
    }
    
    // Muzzle flash göster (varsayılan renk)
    public void ShowMuzzleFlash(Vector3 position, Vector3 direction)
    {
        ShowMuzzleFlash(position, direction, Color.white); // Varsayılan beyaz
    }

// Renkli muzzle flash
    public void ShowMuzzleFlash(Vector3 position, Vector3 direction, Color flashColor)
    {
        if (muzzleFlashPrefab != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
            GameObject flash = Instantiate(muzzleFlashPrefab, position, rotation);
        
            // Renk ayarla
            ParticleSystem ps = flash.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = flashColor; // flashColor kullan (color değil!)
            }
        
            Destroy(flash, 0.5f);
        }
    }
}