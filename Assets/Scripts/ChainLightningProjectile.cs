using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningProjectile : MonoBehaviour
{
    [Header("Chain Lightning Ayarları")]
    public int damage = 40;
    public float speed = 30f;
    public int maxChains = 3; // Maksimum 3 düşmana sekecek
    public float chainRange = 5f; // Seken menzil
    
    private int currentChain = 0;
    private Transform currentTarget;
    private List<int> hitEnemyIDs = new List<int>(); // Vurduğu düşmanlar
    private int sourceZoneIndex; // Hangi zone'dan geldi
    
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Parlak sarı renk
        spriteRenderer.color = new Color(1f, 1f, 0.2f);
        
        // Trail ekle (elektrik izi)
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.3f;
        trailRenderer.startWidth = 0.3f;
        trailRenderer.endWidth = 0.05f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = Color.yellow;
        trailRenderer.endColor = new Color(1f, 1f, 0f, 0f);
    }

    void AddNeonTrail()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
    
        // Trail material
        trail.material = new Material(Shader.Find("Sprites/Default"));
    
        // Renk (projektil tipine göre değiştir!)
        Color trailColor = Color.white; 
    
       trail.startColor = trailColor;
        trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f); // Fade out
    
        // Boyut
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
    
        // Süre (ne kadar iz kalacak)
        trail.time = 0.3f; // 0.3 saniye
    
        // Render ayarları
        trail.sortingOrder = -1; // Projektilden arkada
        trail.numCornerVertices = 5;
        trail.numCapVertices = 5;
    
        // Glow için (Additive blend)
        trail.material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
        trail.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trail.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
    }
    
    private bool isHitting = false; // YENİ! - Vuruş yapılıyor mu?

    void Update()
    {
        // Hedef yoksa yok ol
        if (currentTarget == null)
        {
            Destroy(gameObject);
            return;
        }
    
        // VURUŞ YAPILIYORSA HAREKET ETME - YENİ!
        if (isHitting) return;
    
        // Hedefe doğru hareket et
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    
        // Hedefe çarptı mı?
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance < 0.5f)
        {
            isHitting = true; // Vuruş yapılıyor, Update durur
            HitTarget();
        }
    
        // Ekrandan çıktıysa yok ol
        if (Mathf.Abs(transform.position.x) > 15f || Mathf.Abs(transform.position.y) > 10f)
        {
            Destroy(gameObject);
        }
    }
    
    // İlk hedefi ayarla (Zone'dan çağrılacak)
    public void Initialize(Transform target, int zoneIndex)
    {
        currentTarget = target;
        sourceZoneIndex = zoneIndex;
        currentChain = 0;
    }
    
    // Hedefe çarptı
    void HitTarget()
    {
        Debug.Log($"🔍 HitTarget ÇAĞRILDI! isHitting: {isHitting}, currentChain: {currentChain}");
        
        Enemy enemy = currentTarget.GetComponent<Enemy>();
    
        if (enemy != null)
        {
            int enemyID = enemy.GetInstanceID();
            
            Debug.Log($"🎯 Enemy bulundu: {enemy.name}, ID: {enemy.GetInstanceID()}");
        
            // Hasar ver
            enemy.TakePlayerDamage(damage);
            ShowImpactRing(enemy.transform.position, Color.white); 
            // Bu düşmanı listeye ekle
            hitEnemyIDs.Add(enemyID);
        
            Debug.Log($"⚡ Chain Lightning vuruş! Chain: {currentChain + 1}/{maxChains}, Hedef: {enemy.name}, Hasar: {damage}");
        
            // Hit efekti
            if (HitEffectManager.Instance != null)
            {
                HitEffectManager.Instance.ShowHitEffect(currentTarget.position, Color.yellow);
            }
        
            // Ses
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayHit();
            }
        
            // Chain artır
            currentChain++;
        
            // Daha fazla zincir var mı?
            if (currentChain < maxChains)
            {
                // Sonraki hedefe sek
                Transform nextTarget = FindNextTarget();
            
                if (nextTarget != null)
                {
                    currentTarget = nextTarget;
                    isHitting = false;
                    Debug.Log($"🔁 Sekme! Yeni hedef: {nextTarget.name}");
                    return; // Devam et, yok olma
                }
                else
                {
                    Debug.Log("❌ Sekme hedefi bulunamadı!");
                }
            }
            else
            {
                Debug.Log("✅ Max chain'e ulaşıldı!");
            }
            
        }
        else
        {
            Debug.LogError("❌ Enemy component bulunamadı!");
        }
    
        // Chain bitti veya hedef yok - yok ol
        Debug.Log("💥 Projectile yok ediliyor...");
        Destroy(gameObject);
    }
    
    // Sonraki hedefi bul
    Transform FindNextTarget()
{
    Enemy[] enemies = FindObjectsOfType<Enemy>();
    Transform closest = null;
    float closestDistance = chainRange;
    
    // Mevcut hedefin ID'sini al
    int currentTargetID = currentTarget != null ? currentTarget.GetComponent<Enemy>().GetInstanceID() : -1;
    
    Debug.Log($"🔍 FindNextTarget - Zone: {sourceZoneIndex}, Mevcut hedef ID: {currentTargetID}, Toplam düşman: {enemies.Length}");
    
    // Önce yeni düşman ara
    foreach (Enemy enemy in enemies)
    {
        // Aynı zone'da mı?
        if (enemy.zoneIndex != sourceZoneIndex)
            continue;
        
        int enemyID = enemy.GetInstanceID();
        
        // Mevcut hedef DEĞİLSE ve daha önce vurmadıysak
        if (enemyID != currentTargetID && !hitEnemyIDs.Contains(enemyID))
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
                Debug.Log($"✅ Yeni hedef bulundu: {enemy.name}, Mesafe: {distance:F2}");
            }
        }
    }
    
    // Yeni hedef bulunamadıysa, eski hedefe geri sek (TEK SEFER!)
    if (closest == null)
    {
        // Aynı zone'daki düşmanları kontrol et
        foreach (Enemy enemy in enemies)
        {
            if (enemy.zoneIndex != sourceZoneIndex)
                continue;
            
            int enemyID = enemy.GetInstanceID();
            
            // Mevcut hedef DEĞİLSE (eski bir hedefe geri sekebilir)
            if (enemyID != currentTargetID)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy.transform;
                    Debug.Log($"🔁 Geri sekme: {enemy.name}");
                }
            }
        }
    }
    
    if (closest == null)
    {
        Debug.Log("❌ Hiç hedef bulunamadı!");
    }
    
    return closest;
}
  
    // Impact ring efekti
    void ShowImpactRing(Vector3 position, Color ringColor)
    {
        GameObject ringObj = new GameObject("ImpactRing");
        ringObj.transform.position = position;
    
        ImpactRing impactRing = ringObj.AddComponent<ImpactRing>();
        impactRing.ringColor = ringColor;
        impactRing.duration = 0.4f;
        impactRing.startRadius = 0.2f;
        impactRing.endRadius = 1.2f;
    }
}