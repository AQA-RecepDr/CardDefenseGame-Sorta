using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    
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
    
        transform.localPosition = originalPosition;
        isShaking = false;
    }
}