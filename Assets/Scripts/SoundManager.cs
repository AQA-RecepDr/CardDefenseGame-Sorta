using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource sfxSource; // Efekt sesleri için
    public AudioSource musicSource; // Müzik için (ileride)
    
    [Header("Weapon Sounds")]
    public AudioClip shootSound;
    public AudioClip tripleShootSound;
    public AudioClip ultiSound;
    public AudioClip turretShootSound;
    
    [Header("Hit Sounds")]
    public AudioClip hitSound;
    public AudioClip pierceHitSound;
    public AudioClip enemyDeathSound;
    public AudioClip bossHitSound;
    
    [Header("Player Sounds")]
    public AudioClip playerHurtSound;
    public AudioClip chargeFullSound;
    public AudioClip cardPlaceSound;
    public AudioClip chargeFireSound;
    
    [Header("UI Sounds")]
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip levelUpSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arası kalıcı
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Volume ayarlarını uygula
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
    
    // Ses çal (tek seferlik)
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    // Ses çal (volume ayarlı)
    public void PlaySound(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }
    
    // Hızlı erişim fonksiyonları
    public void PlayShoot() => PlaySound(shootSound);
    public void PlayTripleShoot() => PlaySound(tripleShootSound);
    public void PlayUlti() => PlaySound(ultiSound, 1.2f); // Daha yüksek
    public void PlayTurretShoot() => PlaySound(turretShootSound, 0.5f); // Daha alçak
    
    public void PlayHit() => PlaySound(hitSound, 0.6f);
    public void PlayPierceHit() => PlaySound(pierceHitSound, 0.7f);
    public void PlayEnemyDeath() => PlaySound(enemyDeathSound);
    public void PlayBossHit() => PlaySound(bossHitSound, 0.8f);
    
    public void PlayChargeFire() => PlaySound(chargeFireSound, 1.0f);
    public void PlayPlayerHurt() => PlaySound(playerHurtSound);
    public void PlayChargeFull() => PlaySound(chargeFullSound);
    public void PlayCardPlace() => PlaySound(cardPlaceSound, 0.4f);
    
    public void PlayButtonHover() => PlaySound(buttonHoverSound, 0.3f);
    public void PlayButtonClick() => PlaySound(buttonClickSound, 0.5f);
    public void PlayLevelUp() => PlaySound(levelUpSound);
}