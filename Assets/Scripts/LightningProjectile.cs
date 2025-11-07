using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningProjectile : MonoBehaviour
{
    public Transform target; // Hedef ejderha
    public float speed = 10f; // Elektrik hızlı!
    public int damage = 1;
    
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Sarı renk
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

    void Update()
    {
        // Hedef yoksa yok ol
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        AddNeonTrail();
        
        // Hedefe doğru hareket et
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Hedefe çarptı mı?
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 1f)
        {
            HitTarget();
        }
        
        // Ekrandan çıktıysa yok ol
        if (transform.position.x > 12f)
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

    void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakePlayerDamage(damage);
            ShowImpactRing(enemy.transform.position, new Color(0.5f, 0.5f, 1f));
            Debug.Log($"⚡ Lightning vuruş! Hasar: {damage}");
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