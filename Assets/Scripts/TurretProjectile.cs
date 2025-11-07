using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    public Transform target; // Hedef düşman
    public float speed = 6f;
    public int damage = 1;
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Kırmızı renk
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        
        AddNeonTrail();
    }
    
    void AddNeonTrail()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
    
        // Trail material
        trail.material = new Material(Shader.Find("Sprites/Default"));
    
        // Renk (projektil tipine göre değiştir!)
        Color trailColor = Color.red;
        
    
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
    
    // YENİ FONKSİYON
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
        Debug.Log($"💥 TurretProjectile damage set: {damage}");
    }
    
    // Damage getter (gerekirse)
    public int GetDamage()
    {
        return damage;
    }

    
    void Update()
    {
        // Hedef yoksa yok ol
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Hedefe doğru hareket et
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Hedefe çarptı mı?
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.5f)
        {
            HitTarget();
        }
        
        // Ekrandan çıktıysa yok ol
        if (transform.position.x > 12f || transform.position.y > 6f || transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void HitTarget()
    {
        Debug.Log($"💥 TurretProjectile HitTarget çağrıldı! damage değişkeni: {damage}"); // YENİ!
        // Düşmana turret hasarı ver (isTurret = true)
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"💥 Enemy.TakeDamage çağrılacak! Damage: {damage}, isTurret: true"); // YENİ!
            enemy.TakeDamage(damage, true); // Turret hasarı olduğunu belirt!
            ShowImpactRing(enemy.transform.position, new Color(1f, 0.5f, 0f));
        }
    
        Destroy(gameObject);
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