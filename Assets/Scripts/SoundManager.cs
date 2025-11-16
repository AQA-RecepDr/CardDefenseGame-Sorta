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
    
    [Header("Coin Sounds")]
    public AudioClip coinCollectSound; // Coin toplarken
    public AudioClip vacuumSound; // Vakum sesi (loop
    
    // COIN PITCH SİSTEMİ
    private int coinCollectCount = 0; // 1 saniye içinde toplanan coin
    private float coinCollectTimer = 0f; // Timer
    private float coinCollectResetTime = 1f; // 1 saniye sonra sıfırla
    
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
    
    void Update()
    {
        // Coin pitch timer
        if (coinCollectCount > 0)
        {
            coinCollectTimer += Time.deltaTime;
            
            if (coinCollectTimer >= coinCollectResetTime)
            {
                // Sıfırla
                coinCollectCount = 0;
                coinCollectTimer = 0f;
                Debug.Log("Coin pitch sıfırlandı");
            }
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
    // Ses çal (pitch shifting ile)
    public void PlaySoundWithPitch(AudioClip clip, float volumeScale = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        if (clip != null && sfxSource != null)
        {
            // Random pitch değeri
            float randomPitch = Random.Range(pitchMin, pitchMax);
            
            // Geçici AudioSource oluştur (pitch için)
            GameObject tempGO = new GameObject("TempAudio");
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            
            tempSource.clip = clip;
            tempSource.volume = sfxVolume * volumeScale;
            tempSource.pitch = randomPitch;
            tempSource.Play();
            
            // Ses bitince yok et
            Destroy(tempGO, clip.length / randomPitch);
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
    
    // Coin sesleri
    //public void PlayCoinCollect() => PlaySound(coinCollectSound, 0.4f);
    
    public void PlayCoinCollect()
    {
        if (coinCollectSound == null || sfxSource == null) return;
        
        // Coin sayısını artır
        coinCollectCount++;
        coinCollectTimer = 0f; // Timer'ı sıfırla
        
        // Pitch hesapla (1 coin = 1.0, 10 coin = 1.9)
        // Linear interpolation: 1.0 → 1.9 arası
        float targetPitch = Mathf.Lerp(1.0f, 1.9f, (coinCollectCount - 1) / 9f);
        targetPitch = Mathf.Clamp(targetPitch, 1.0f, 1.9f); // 10'dan fazla olursa 1.9'da kal
        
        // Geçici AudioSource oluştur
        GameObject tempGO = new GameObject("CoinAudio");
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        
        tempSource.clip = coinCollectSound;
        tempSource.volume = sfxVolume * 0.4f;
        tempSource.pitch = targetPitch;
        tempSource.Play();
        
        // Ses bitince yok et
        Destroy(tempGO, coinCollectSound.length / targetPitch);
        
        Debug.Log($"🪙 Coin #{coinCollectCount} - Pitch: {targetPitch:F2}");
    }
    public void PlayVacuumLoop() => PlaySound(vacuumSound, 0.3f);
    
    // Hızlı erişim fonksiyonları
    // Hızlı erişim fonksiyonları - PITCH SHIFTING İLE! 🎵
    public void PlayShoot() => PlaySoundWithPitch(shootSound, 1f, 0.85f, 1.15f); //
    public void PlayTripleShoot() => PlaySoundWithPitch(tripleShootSound, 1f, 0.9f, 1.1f);
    public void PlayUlti() => PlaySound(ultiSound, 1.2f); // Ulti pitch shifting olmasın (her zaman epic!)
    public void PlayTurretShoot() => PlaySoundWithPitch(turretShootSound, 0.5f, 0.85f, 1.15f); // Daha fazla varyasyon
    
    public void PlayHit() => PlaySoundWithPitch(hitSound, 0.6f, 0.9f, 1.1f);
    public void PlayPierceHit() => PlaySoundWithPitch(pierceHitSound, 0.7f, 0.9f, 1.1f);
    public void PlayEnemyDeath() => PlaySoundWithPitch(enemyDeathSound, 1f, 0.95f, 1.05f);
    public void PlayBossHit() => PlaySound(bossHitSound, 0.8f); // Boss pitch shifting olmasın (her zaman güçlü!)
    
    public void PlayChargeFire() => PlaySound(chargeFireSound, 1.0f);
    public void PlayPlayerHurt() => PlaySound(playerHurtSound);
    public void PlayChargeFull() => PlaySound(chargeFullSound);
    public void PlayCardPlace() => PlaySound(cardPlaceSound, 0.4f);
    
    public void PlayButtonHover() => PlaySound(buttonHoverSound, 0.3f);
    public void PlayButtonClick() => PlaySound(buttonClickSound, 0.5f);
    public void PlayLevelUp() => PlaySound(levelUpSound);
}