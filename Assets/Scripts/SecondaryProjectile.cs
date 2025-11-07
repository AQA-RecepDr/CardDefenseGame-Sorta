using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryProjectile : MonoBehaviour
{
    public Vector3 direction;
    public float speed = 15f; // Daha hızlı!
    public int damage = 100;
    
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private List<int> hitEnemies = new List<int>(); // Vurduğu düşmanların ID'leri

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    
        // Orbital Strike rengi - PARLAK TURUNCU/SARI
        spriteRenderer.color = new Color(1f, 0.8f, 0f); // Altın sarısı
    
        // Trail ekle (gökyüzünden düşerken iz bırakır)
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }
    
        trailRenderer.time = 0.5f;
        trailRenderer.startWidth = 0.5f; // Daha kalın iz
        trailRenderer.endWidth = 0.2f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(1f, 0.8f, 0f); // Altın
        trailRenderer.endColor = new Color(1f, 0.3f, 0f, 0f); // Turuncu fade
    }

    void Update()
    {
        // Sabit hızda ileri git
        transform.position += direction * speed * Time.deltaTime;

        AddNeonTrail();
        
        // PIERCING çarpışma kontrolü
        CheckEnemyCollision();
        
        // Ekrandan çıktıysa yok ol
        if (transform.position.x > 12f || transform.position.x < -12f || 
            transform.position.y > 6f || transform.position.y < -6f)
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
        Color trailColor = Color.yellow;
        
    
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
            // Bu düşmana daha önce vurduk mu? (Piercing için önemli!)
            if (hitEnemies.Contains(enemy.GetInstanceID()))
                continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distance < 0.6f)
            {
                // GÜÇLÜ HASAR VER!
                enemy.TakePlayerDamage(damage); // 100 hasar
                ShowImpactRing(enemy.transform.position, new Color(1f, 0f, 1f));
                /* Sarı düşmansa charge ver
                if (enemy.enemyType == Enemy.EnemyType.Yellow)
                {
                    Player player = FindObjectOfType<Player>();
                    if (player != null)
                    {
                        player.AddCharge();
                    }
                }
                */
                
                // Bu düşmanı listeye ekle (bir daha vurmasın)
                hitEnemies.Add(enemy.GetInstanceID());
                
                Debug.Log($"⚡ ULTİ VURUŞ! {enemy.enemyType}'a {damage} hasar!");
                
                // PIERCING - Mermi YOK OLMASIN, devam etsin!
                // return; // Bu satırı koymuyoruz!
            }
        }
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