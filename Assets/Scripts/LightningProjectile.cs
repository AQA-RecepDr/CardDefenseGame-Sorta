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

    void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakePlayerDamage(damage);
            Debug.Log($"⚡ Lightning vuruş! Hasar: {damage}");
        }
    
        Destroy(gameObject);
    }
}