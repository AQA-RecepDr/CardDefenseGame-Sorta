using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryProjectile : MonoBehaviour
{
    public Vector3 direction;
    public float speed = 10f;
    public float damage;
    
    private List<int> hitEnemies = new List<int>(); // Vurduğu düşmanların ID'leri ← YENİ!
    
    private SpriteRenderer spriteRenderer;
    private bool hasHit = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        AddNeonTrail();

        // PROJEKTİL GLOW - YENİ! ✨
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Rengi parlat (bloom için)
            Color glowColor = Color.cyan; // Projektil tipine göre değiştir
            sr.color = glowColor;

            // Parlak material (opsiyonel)
            sr.material = new Material(Shader.Find("Sprites/Default"));

            // Pierce upgrade'i varsa farklı renk
            if (WeaponUpgradeManager.Instance != null &&
                WeaponUpgradeManager.Instance.hasPierceShot)
            {
                spriteRenderer.color = new Color(0f, 1f, 1f); // Parlak cyan (pierce göstergesi)
            }
            else
            {
                spriteRenderer.color = Color.cyan; // Normal cyan
            }
        }
    }

    void Update()
    {
        
        if (hasHit) return;
        
        // Sabit hızda ileri git
        transform.position += direction * speed * Time.deltaTime;
    
        // Çarpışma kontrolü
        CheckEnemyCollision();
    
        // Boss kontrolü - Boss'un arkasına geçtiyse yok ol
        //CheckBossCollision();
    
        // Ekrandan çıktıysa yok ol (güvenlik)
        if (transform.position.x > 15f || transform.position.x < -15f || 
            transform.position.y > 8f || transform.position.y < -8f)
        {
            Destroy(gameObject);
        }
    }
    
    void AddNeonTrail()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
    
        // Trail material
        trail.material = new Material(Shader.Find("Sprites/Default"));
    
        // Renk (projektil tipine göre değiştir!)
        Color trailColor = Color.cyan; // Örnek: Primary için cyan
    
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
    
    void CheckEnemyCollision()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (hitEnemies.Contains(enemy.GetInstanceID()))
                continue;
        
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
    
            if (distance < 0.5f)
            {
                // KRİTİK KONTROLÜ - YENİ! ✅
                bool isCritical = CheckCritical();
                int finalDamage = CalculateDamage(isCritical);
                
                enemy.TakePlayerDamage(finalDamage);
                
                // IMPACT RING - YENİ! 💥
                ShowImpactRing(enemy.transform.position, Color.cyan);
                
                // KRİTİK FEEDBACK - YENİ! ✅
                if (isCritical)
                {
                    ShowCriticalFeedback(enemy.transform.position, finalDamage);
                }
            
                hitEnemies.Add(enemy.GetInstanceID());
            
                // Pierce upgrade kontrolü
                bool hasPierce = WeaponUpgradeManager.Instance != null && 
                                 WeaponUpgradeManager.Instance.hasPierceShot;
            
                // Maksimum pierce sayısı (3 düşman)
                int maxPierceCount = WeaponUpgradeManager.Instance != null ? 
                    WeaponUpgradeManager.Instance.pierceShotMaxTargets : 3;
            
                // Pierce yoksa VEYA maksimum pierce sayısına ulaştıysa yok ol
                if (!hasPierce || hitEnemies.Count >= maxPierceCount)
                {
                    Destroy(gameObject);
                    return;
                }
            
                Debug.Log($"⚔️ Pierce! {hitEnemies.Count}/{maxPierceCount}");
            }
        }
    }
    
    // Kritik olup olmadığını kontrol et
    bool CheckCritical()
    {
        float criticalChance = 0.1f; // %10 base chance
    
        // Permanent upgrade varsa ekle (sonra ekleyeceğiz)
         if (PermanentUpgradeManager.Instance != null)
         {
             criticalChance += PermanentUpgradeManager.Instance.GetCriticalChance() / 100f;
         }
    
        float roll = Random.value; // 0.0 - 1.0
    
        bool isCritical = roll <= criticalChance;
    
        if (isCritical)
        {
            Debug.Log($"💥 KRİTİK VURUŞ! (Roll: {roll:F2} <= {criticalChance:F2})");
        }
    
        return isCritical;
    }
    
    // Hasarı hesapla (kritik dahil)
    int CalculateDamage(bool isCritical)
    {
        int baseDamage = (int)damage;
    
        if (isCritical)
        {
            float criticalMultiplier = 1.3f; // %30 fazla (%130 toplam)
        
            // Permanent upgrade varsa ekle (sonra ekleyeceğiz)
             if (PermanentUpgradeManager.Instance != null)
             {
                 criticalMultiplier = 1f + (PermanentUpgradeManager.Instance.GetCriticalDamage() / 100f);
             }
        
            int criticalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
        
            Debug.Log($"💥 Kritik hasar: {baseDamage} → {criticalDamage} ({criticalMultiplier}x)");
        
            return criticalDamage;
        }
    
        return baseDamage;
    }
    
    // Kritik feedback göster
    void ShowCriticalFeedback(Vector3 position, int damage)
    {
        // Büyük altın renkli damage text
        if (DamageTextManager.Instance != null)
        {
            Vector3 textPosition = position + Vector3.up * 0.5f;
            Color criticalColor = new Color(1f, 0.1f, 0.1f); // Altın sarısı
            DamageTextManager.Instance.ShowDamage(damage, textPosition, criticalColor);
        }
    
        // Özel efekt (altın patlama)
        if (HitEffectManager.Instance != null)
        {
            Color criticalEffectColor = new Color(1f, 0.2f, 0.2f, 0.8f);
            HitEffectManager.Instance.ShowHitEffect(position, criticalEffectColor);
        }
    
        // Ekstra screen shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.12f, 0.05f); // Biraz daha güçlü
        }
    
        // Kritik ses efekti (varsa)
        if (SoundManager.Instance != null)
        {
            // SoundManager.Instance.PlayCriticalHit();
        }
    }
    
    // Impact ring efekti - YENİ VERSİYON! 💍
    void ShowImpactRing(Vector3 position, Color ringColor)
    {
        // Ring objesi oluştur
        GameObject ringObj = new GameObject("ImpactRing");
        ringObj.transform.position = position;
    
        // ImpactRing component ekle (kendi animasyonunu yapacak!)
        ImpactRing impactRing = ringObj.AddComponent<ImpactRing>();
        impactRing.ringColor = ringColor;
        impactRing.duration = 0.4f;
        impactRing.startRadius = 0.2f;
        impactRing.endRadius = 1.2f;
    
        Debug.Log($"💍 Impact Ring oluşturuldu: {position}");
    }
}
