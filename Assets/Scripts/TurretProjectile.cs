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
        }
    
        Destroy(gameObject);
    }
}