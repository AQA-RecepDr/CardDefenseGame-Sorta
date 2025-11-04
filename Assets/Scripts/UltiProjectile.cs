using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltiProjectile : MonoBehaviour
{
    [Header("Ulti Ayarları")]
    public Vector3 direction;
    public float speed = 15f;
    public int damage = 100;
    public float lifetime = 3f; // 3 saniye sonra yok ol
    
    private List<int> hitEnemyIDs = new List<int>(); // Piercing için
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Parlak turuncu-sarı
        spriteRenderer.color = new Color(1f, 0.7f, 0f);
        
        // Trail ekle (uzun iz)
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.5f;
        trailRenderer.startWidth = 0.4f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(1f, 0.7f, 0f);
        trailRenderer.endColor = new Color(1f, 0.3f, 0f, 0f);
        
        // Lifetime sonra yok ol
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Sabit hızda ilerle
        transform.position += direction * speed * Time.deltaTime;
        
        // Piercing çarpışma kontrolü
        CheckEnemyCollision();
        
        // Ekrandan çok uzaklaştıysa yok ol
        if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 15f)
        {
            Destroy(gameObject);
        }
    }

    void CheckEnemyCollision()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        
        foreach (Enemy enemy in enemies)
        {
            // Bu düşmana daha önce vurduk mu?
            if (hitEnemyIDs.Contains(enemy.GetInstanceID()))
                continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance < 0.5f)
            {
                // GÜÇLÜ HASAR VER! 💥
                enemy.TakePlayerDamage(damage);
                
                // Bu düşmanı listeye ekle
                hitEnemyIDs.Add(enemy.GetInstanceID());
                
                // Hit efekti
                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.ShowHitEffect(enemy.transform.position, new Color(1f, 0.7f, 0f));
                }
                
                Debug.Log($"⚡ ULTI VURUŞ! {enemy.name} → {damage} hasar (Piercing)");
                
                // PIERCING - Mermi YOK OLMASIN, devam etsin! ✅
            }
        }
    }
}
