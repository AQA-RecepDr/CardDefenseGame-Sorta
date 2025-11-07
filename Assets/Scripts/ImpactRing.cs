using System.Collections;
using UnityEngine;

public class ImpactRing : MonoBehaviour
{
    public float duration = 0.4f;
    public float startRadius = 0.2f;
    public float endRadius = 1.2f;
    public Color ringColor = Color.cyan;
    
    private LineRenderer ring;
    private float elapsed = 0f;
    private Color originalColor;
    
    void Start()
    {
        // LineRenderer setup
        ring = gameObject.AddComponent<LineRenderer>();
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = ringColor;
        ring.endColor = new Color(ringColor.r, ringColor.g, ringColor.b, 0f);
        ring.startWidth = 0.15f;
        ring.endWidth = 0.05f;
        ring.positionCount = 20;
        ring.loop = true;
        ring.sortingOrder = 5;
        ring.useWorldSpace = false;
        
        originalColor = ringColor;
        
        // İlk circle
        UpdateRing(startRadius);
    }
    
    void Update()
    {
        if (ring == null) return;
        
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        if (t >= 1f)
        {
            // Animasyon bitti, yok ol
            Destroy(gameObject);
            return;
        }
        
        // Radius büyüt
        float radius = Mathf.Lerp(startRadius, endRadius, t);
        UpdateRing(radius);
        
        // Fade out
        Color c = originalColor;
        c.a = 1f - t;
        ring.startColor = c;
        ring.endColor = new Color(c.r, c.g, c.b, 0f);
    }
    
    void UpdateRing(float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            float angle = i * 18f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            ring.SetPosition(i, pos);
        }
    }
}