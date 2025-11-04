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
}