using UnityEngine;

public class PlayerCrystal : MonoBehaviour
{
    [Header("Crystal Settings")]
    public int sides = 6; // Hexagon
    public float radius = 0.8f;
    public Color glowColor = new Color(0f, 1f, 1f); // Cyan
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    
    private LineRenderer lineRenderer;
    private float baseRadius;
    
    void Start()
    {
        baseRadius = radius;
        CreateCrystal();
    }
    
    void Update()
    {
        // Pulse animasyonu
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        radius = baseRadius + pulse;
        UpdateCrystal();
    }
    
    void CreateCrystal()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = glowColor;
        lineRenderer.endColor = glowColor;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = sides + 1;
        lineRenderer.loop = true;
        lineRenderer.sortingOrder = 10;
        
        UpdateCrystal();
    }
    
    void UpdateCrystal()
    {
        if (lineRenderer == null) return;
        
        for (int i = 0; i < sides + 1; i++)
        {
            float angle = i * 360f / sides * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );
            lineRenderer.SetPosition(i, pos);
        }
    }
}