using System.Collections;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 1; // Her coin 1 değerinde
    public float lifetime = 15f; // 15 saniye sonra kaybolur
    
    [Header("Visual Effects")]
    public bool showTrailWhenPulled = true;
    private TrailRenderer trail;
    private ParticleSystem sparkles;
    
    [Header("Movement")]
    public float fallSpeed = 0.5f; // Yavaşça aşağı düşer
    public bool isBeingPulled = false; // Vakum tarafından çekiliyor mu?
    public float pullSpeed = 10f; // Çekilme hızı
    
    private Vector3 targetPosition; // Vakum hedefi (player)
    private SpriteRenderer spriteRenderer;
    private float spawnTime;
    
    [Header("Auto Collect")]
    public float autoCollectDistance = 1.5f; // Bu mesafeden yakınsa otomatik topla
    private Transform playerTransform;
    
    private Vector3 velocity = Vector3.zero; // Coin'in mevcut hızı
    public float momentumDecay = 0.92f; // Her frame hızı azalt (0.92 = %8 yavaşlama)
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnTime = Time.time;
        
        // Player'ı bul
        GameObject player = GameObject.Find("WeaponCenter");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
    
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player bulunamadı!");
        }
        
        // Başlangıç hareketi (saçılma)
        StartCoroutine(InitialScatter());
        
        CreateTrailEffect();
        CreateSparkleEffect();
    }
    
    void Update()
{
    // Lifetime kontrolü
    if (Time.time - spawnTime > lifetime)
    {
        Destroy(gameObject);
        return;
    }
    
    // Coin rotasyonu (sürekli dönsün) - YENİ! 🔄
    transform.Rotate(0, 0, 180f * Time.deltaTime);
    
    // MANYETİK ÇEKİM
    if (playerTransform != null && !isBeingPulled)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Çok yakınsa otomatik topla
        if (distanceToPlayer < 0.5f)
        {
            CollectCoin();
            return;
        }
        
        // Yakınsa manyetik çekim
        if (distanceToPlayer < autoCollectDistance)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float magnetForce = (autoCollectDistance - distanceToPlayer) / autoCollectDistance;
            
            // SMOOTH LERP - YENİ! 🎯
            float smoothSpeed = Mathf.Lerp(2f, 8f, magnetForce); // Yaklaştıkça hızlanır
            transform.position += direction * smoothSpeed * Time.deltaTime;
            
            // Trail aktif et
            if (trail != null && !trail.emitting)
            {
                trail.emitting = true;
            }
            
            // Sparkle aktif et
            if (sparkles != null)
            {
                var emission = sparkles.emission;
                emission.enabled = true;
                emission.rateOverTime = 20;
            }
        }
    }
    
    if (isBeingPulled)
    {
        // VAKUM ÇEKİMİ - HIZLANARAK! 🌪️
        float currentDistance = Vector3.Distance(transform.position, targetPosition);
        float speedMultiplier = Mathf.Lerp(1f, 3f, 1f - (currentDistance / 10f)); // Yaklaştıkça hızlanır
        
        // ÖNCEKİ POZİSYONU SAKLA
        Vector3 oldPosition = transform.position;
        
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            pullSpeed * speedMultiplier * Time.deltaTime
        );
        
        // VELOCITY HESAPLA!
        velocity = (transform.position - oldPosition) / Time.deltaTime;
        
        // Trail aktif
        if (trail != null) trail.emitting = true;
        
        // Sparkle aktif
        if (sparkles != null)
        {
            var emission = sparkles.emission;
            emission.enabled = true;
            emission.rateOverTime = 30; // Daha fazla particle
        }
        
        // SCALE ANİMASYONU - Yaklaştıkça büyür! 📈
        float scaleMultiplier = Mathf.Lerp(0.8f, 1.3f, 1f - (currentDistance / 10f));
        transform.localScale = Vector3.one * 0.3f * scaleMultiplier;
        
        // Hedefe ulaştı mı?
        if (currentDistance < 0.3f)
        {
            CollectCoin();
        }
    }
    else
    {
        // MOMENTUM VAR MI? 👇
        if (velocity.magnitude > 0.1f) // Hala momentum varsa
        {
            // Momentum'u uygula
            transform.position += velocity * Time.deltaTime;
        
            // Her frame momentum'u azalt (sürtünme)
            velocity *= momentumDecay;
        
            // Trail aktif tut (momentum varken)
            if (trail != null) trail.emitting = true;
        }
        else
        {
            // Momentum bitti, normal düşme
            velocity = Vector3.zero;
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        
            // Trail kapalı
            if (trail != null) trail.emitting = false;
        }
        
        // Sparkle kapalı
        if (sparkles != null)
        {
            var emission = sparkles.emission;
            emission.enabled = false;
        }
    }
    
    // Ekrandan çıktıysa yok et
    if (Mathf.Abs(transform.position.x) > 20f || transform.position.y < -15f)
    {
        Destroy(gameObject);
    }
}
    
    void CreateTrailEffect()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
    
        // Sarı - altın trail
        Color trailColor = new Color(1f, 0.9f, 0f); // Altın sarısı
        trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
    
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        trail.time = 0.3f;
        trail.sortingOrder = 4;
        trail.numCornerVertices = 5;
        trail.numCapVertices = 5;
    
        // Başlangıçta kapalı
        trail.emitting = false;
    
        // Glow (additive blend)
        trail.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trail.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    }

    void CreateSparkleEffect()
    {
        // Mini particle system
        GameObject sparkleObj = new GameObject("CoinSparkles");
        sparkleObj.transform.SetParent(transform, false);
        sparkleObj.transform.localPosition = Vector3.zero;
    
        sparkles = sparkleObj.AddComponent<ParticleSystem>();
        var main = sparkles.main;
        main.startSize = 0.1f;
        main.startSpeed = 1f;
        main.startLifetime = 0.5f;
        main.maxParticles = 10;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
    
        var emission = sparkles.emission;
        emission.rateOverTime = 0; // Normalde kapalı
        emission.enabled = false;
    
        var shape = sparkles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
    
        // Sarı particle
        var colorOverLifetime = sparkles.colorOverLifetime;
        colorOverLifetime.enabled = true;
    
        ParticleSystem.MainModule particleMain = sparkles.main;
        particleMain.startColor = Color.yellow;
    
        // Renderer
        ParticleSystemRenderer psRenderer = sparkles.GetComponent<ParticleSystemRenderer>();
        psRenderer.sortingOrder = 6;
        psRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }
    
    // Başlangıç saçılma animasyonu
    IEnumerator InitialScatter()
    {
        Vector3 startPos = transform.position;
        
        // Rastgele yön
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(1f, 2f);
        Vector3 targetPos = startPos + new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Yukarı fırlat, sonra düşür (arc motion)
            float heightBonus = Mathf.Sin(t * Mathf.PI) * 1f;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += heightBonus;
            
            transform.position = currentPos;
            yield return null;
        }
    }
    
    // Vakum tarafından çek
    public void PullTowards(Vector3 target)
    {
        isBeingPulled = true;
        targetPosition = target;
    }
    
    // Vakumu durdur
    public void StopPull()
    {
        isBeingPulled = false;
    }
    
    // Coin topla
    void CollectCoin()
    {
        // BURST EFEKTİ - YENİ! 💥
        if (HitEffectManager.Instance != null)
        {
            // Merkez patlama
            HitEffectManager.Instance.ShowHitEffect(transform.position, Color.yellow);
        
            // 6 yönlü particle burst (yıldız şekli)
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                float angleRad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * 0.3f;
            
                HitEffectManager.Instance.ShowHitEffect(
                    transform.position + offset, 
                    new Color(1f, 0.9f, 0f, 0.7f)
                );
            }
        }
    
        // SCREEN SHAKE - YENİ! 📳
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.05f, 0.02f); // Hafif shake
        }
    
        // Coin manager'a bildir
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(coinValue);
        }
    
        // Ses efekti
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoinCollect();
        }
    
        Destroy(gameObject);
    }
}