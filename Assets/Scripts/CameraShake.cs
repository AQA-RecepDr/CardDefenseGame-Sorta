using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    
    // MOUSE FOLLOW - YENİ! 🎯
    [Header("Mouse Follow Settings")]
    public bool enableMouseFollow = true;
    public float mouseFollowStrength = 0.5f; // Ne kadar takip etsin (0.3-1.0 arası)
    public float mouseFollowSpeed = 5f; // Ne kadar yumuşak (3-10 arası)
    public float mouseFollowDeadzone = 0.1f; // Çok küçük hareketleri yok say
    
    private Camera mainCamera;
    private Vector3 mouseFollowOffset;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        originalPosition = transform.localPosition;
        mainCamera = GetComponent<Camera>();
    }
    
    void Update()
    {
        // MOUSE FOLLOW - YENİ! 🎯
        if (enableMouseFollow && !isShaking)
        {
            UpdateMouseFollow();
        }
    }

    void UpdateMouseFollow()
    {
        if (mainCamera == null) return;
    
        // Mouse pozisyonu (ekran koordinatları)
        Vector3 mouseScreenPos = Input.mousePosition;
    
        // Ekran merkezi
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
    
        // Mouse'un merkeze göre uzaklığı (normalize edilmiş: -1 ile 1 arası)
        Vector3 mouseOffset = mouseScreenPos - screenCenter;
        mouseOffset.x /= Screen.width;
        mouseOffset.y /= Screen.height;
        mouseOffset.z = 0;
    
        // Deadzone kontrolü (çok küçük hareketleri yok say)
        if (mouseOffset.magnitude < mouseFollowDeadzone)
        {
            mouseOffset = Vector3.zero;
        }
    
        // Offset hesapla (küçük bir kaydırma)
        Vector3 targetOffset = mouseOffset * mouseFollowStrength;
    
        // Smooth lerp ile hareket et
        mouseFollowOffset = Vector3.Lerp(
            mouseFollowOffset, 
            targetOffset, 
            Time.deltaTime * mouseFollowSpeed
        );
    
        // Kamerayı kaydır
        transform.localPosition = originalPosition + mouseFollowOffset;
    }
    
    // Ekranı sarsmak için dışarıdan çağrılır
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }
    
    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            // Oyun duraklatılmışsa pozisyonu düzelt ve bekle
            if (Time.timeScale == 0f)
            {
                transform.localPosition = originalPosition;
                yield return null; // Bekle ama coroutine'i bitirme
                continue; // Loop'un başına dön
            }
        
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
        
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
        
            elapsed += Time.unscaledDeltaTime; // Unscaled time kullan
            yield return null;
        }
    
        transform.localPosition = originalPosition + mouseFollowOffset;
        isShaking = false;
    }
}