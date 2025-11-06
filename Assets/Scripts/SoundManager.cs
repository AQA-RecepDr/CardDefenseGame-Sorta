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
    
    [Header("Boss Sounds - YENİ! 👾")]
    public AudioClip bossMusicLoop; // Boss müziği (loop)
    public AudioClip normalMusicLoop; // Normal oyun müziği (loop)
    public AudioClip bossSpawnSound; // Boss geldiğinde
    public AudioClip bossMinionSpawnSound; // Minion spawn
    public AudioClip bossTeleportSound; // Boss yer değiştirince
    public AudioClip bossHurtSound; // Boss hasar alınca (büyük)
    public AudioClip bossDeathSound; // Boss öldüğünde (epic!)
    
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
    
    // BOSS MÜZİK SİSTEMİ - YENİ! 👾

// Normal müziği başlat
public void PlayNormalMusic()
{
    if (musicSource != null && normalMusicLoop != null)
    {
        // Eğer boss müziği çalıyorsa fade out yap
        if (musicSource.isPlaying && musicSource.clip == bossMusicLoop)
        {
            StartCoroutine(CrossfadeMusic(normalMusicLoop, 1.5f));
        }
        else
        {
            musicSource.clip = normalMusicLoop;
            musicSource.loop = true;
            musicSource.Play();
        }
        
        Debug.Log("🎵 Normal müzik başladı");
    }
}

// Boss müziğini başlat
public void PlayBossMusic()
{
    if (musicSource != null && bossMusicLoop != null)
    {
        // Dramatic geçiş ile boss müziğine geç!
        StartCoroutine(CrossfadeMusic(bossMusicLoop, 1.0f));
        
        Debug.Log("👾 BOSS MÜZİĞİ BAŞLADI!");
    }
}

// Müzik geçişi (crossfade)
IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
{
    float startVolume = musicSource.volume;
    
    // Fade out (eski müzik)
    float elapsed = 0f;
    while (elapsed < duration / 2f)
    {
        elapsed += Time.deltaTime;
        musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
        yield return null;
    }
    
    // Müziği değiştir
    musicSource.clip = newClip;
    musicSource.loop = true;
    musicSource.Play();
    
    // Fade in (yeni müzik)
    elapsed = 0f;
    while (elapsed < duration / 2f)
    {
        elapsed += Time.deltaTime;
        musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (duration / 2f));
        yield return null;
    }
    
    musicSource.volume = musicVolume;
}

// Müziği durdur
public void StopMusic()
{
    if (musicSource != null)
    {
        musicSource.Stop();
    }
}

    // BOSS SESLER - Hızlı erişim
    public void PlayBossSpawn() => PlaySound(bossSpawnSound, 1.0f);
    public void PlayBossMinionSpawn() => PlaySound(bossMinionSpawnSound, 0.5f);
    public void PlayBossTeleport() => PlaySound(bossTeleportSound, 0.8f);
    public void PlayBossHurt() => PlaySound(bossHurtSound, 0.9f);
    public void PlayBossDeath() => PlaySound(bossDeathSound, 1.2f); // En yüksek!
    
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