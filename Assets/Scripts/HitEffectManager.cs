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
    /*public void ShowHitEffect(Vector3 position, Color color)
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
    }*/
    
    public void ShowHitEffect(Vector3 position, Color color)
    {
        // Particle oluştur
        GameObject particleObj = new GameObject("HitEffect");
        particleObj.transform.position = position;
    
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
    
        // NEON AYARLARI - YENİ! ✨
        main.startColor = color;
        main.startSize = 0.3f;
        main.startSpeed = 3f;
        main.startLifetime = 0.5f;
        main.maxParticles = 20;
    
        // Glow için
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });
    
        // Renk fade
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
    
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
    
        // Boyut küçülme
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
    
        // Renderer ayarları (BLOOM için önemli!)
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 10;
    
        // Additive blend (glow)
        renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    
        // Particle'ı başlat
        ps.Play();
    
        // Otomatik yok et
        Destroy(particleObj, main.startLifetime.constantMax);
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