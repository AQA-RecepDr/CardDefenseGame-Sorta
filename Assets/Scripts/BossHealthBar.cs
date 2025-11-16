using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Boss için segmentli health bar
/// Her segment kırılabilir, neon/cyberpunk stili
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance;
    
    [Header("Bar Settings")]
    public int segmentCount = 6; // 6 segment (her biri 1000 HP)
    public int hpPerSegment = 1000; // Her segment 1000 HP
    public float segmentWidth = 120f; // Her segment genişliği
    public float segmentHeight = 30f;
    public float segmentSpacing = 5f; // Segmentler arası boşluk
    
    [Header("Colors")]
    public Color fullSegmentColor = new Color(0.9f, 0.3f, 1f); // Mor 💜
    public Color emptySegmentColor = new Color(0.3f, 0.1f, 0.4f); // Koyu mor
    public Color glowColor = new Color(1f, 0.9f, 0.3f); // Sarı glow ⚡
    
    [Header("Animation")]
    public bool enablePulse = true;
    public float pulseSpeed = 3f;
    public float pulseIntensity = 0.2f;
    
    [Header("Break Effect")]
    public float breakFlashDuration = 0.3f;
    public Color breakFlashColor = Color.white;
    public float shatterDuration = 0.6f; // Parçaların uçuş süresi
    public float shatterDistance = 200f; // Parçaların ne kadar uzağa uçacağı
    
    [Header("Screen Flash")]
    public bool enableScreenFlash = true;
    public float screenFlashDuration = 0.2f;
    
    // Internal
    private Canvas canvas;
    private GameObject barContainer;
    private List<Image> segmentImages = new List<Image>(); // Ana fill (doluluk)
    private List<Image> segmentGlows = new List<Image>();
    private List<Image> segmentBackgrounds = new List<Image>();
    private List<Image> segmentFillBars = new List<Image>(); // YENİ! Her segment'in doluluk bar'ı
    
    private int currentMaxHP = 6000;
    private int currentHP = 6000;
    private int lastBrokenSegment = -1;
    
    private float pulseTimer = 0f;
    private bool isVisible = false;
    
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
        CreateHealthBar();
        HideBar(); // Başta gizli
    }
    
    void Update()
    {
        if (!isVisible) return;
        
        // Pulse animasyonu
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            UpdatePulse();
        }
    }
    
    /// <summary>
    /// Health bar'ı oluştur
    /// </summary>
    void CreateHealthBar()
    {
        // Canvas bul veya oluştur
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas bulunamadı!");
            return;
        }
        
        // Bar container
        barContainer = new GameObject("BossHealthBarContainer");
        barContainer.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f); // Üst orta
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -20); // Üstten 20 piksel aşağı
        
        float totalWidth = (segmentWidth * segmentCount) + (segmentSpacing * (segmentCount - 1));
        containerRect.sizeDelta = new Vector2(totalWidth, segmentHeight);
        
        // Segmentleri oluştur
        for (int i = 0; i < segmentCount; i++)
        {
            CreateSegment(i);
        }
        
        Debug.Log("💜 Boss Health Bar oluşturuldu!");
    }
    
    /// <summary>
    /// Tek bir segment oluştur
    /// </summary>
    void CreateSegment(int index)
    {
        // Segment container
        GameObject segmentObj = new GameObject($"Segment_{index}");
        segmentObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform segmentRect = segmentObj.AddComponent<RectTransform>();
        segmentRect.anchorMin = new Vector2(0, 0.5f);
        segmentRect.anchorMax = new Vector2(0, 0.5f);
        segmentRect.pivot = new Vector2(0, 0.5f);
        
        float xPos = index * (segmentWidth + segmentSpacing);
        segmentRect.anchoredPosition = new Vector2(xPos, 0);
        segmentRect.sizeDelta = new Vector2(segmentWidth, segmentHeight);
        
        // KATMAN SIRALAMASI ÖNEMLİ! Alttaki önce oluşturulmalı! 🎨
        
        // 1) BACKGROUND (en altta - koyu arka plan)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(segmentObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = emptySegmentColor;
        segmentBackgrounds.Add(bgImage);
        
        // 2) FILL BAR (ortada - doluluk gösterir) 💧
        GameObject fillBarObj = new GameObject("FillBar");
        fillBarObj.transform.SetParent(segmentObj.transform, false);
        
        RectTransform fillBarRect = fillBarObj.AddComponent<RectTransform>();
        fillBarRect.anchorMin = new Vector2(0, 0);
        fillBarRect.anchorMax = new Vector2(1, 1);
        fillBarRect.sizeDelta = Vector2.zero;
        fillBarRect.anchoredPosition = Vector2.zero;
        
        Image fillBarImage = fillBarObj.AddComponent<Image>();
        fillBarImage.color = fullSegmentColor;
        fillBarImage.type = Image.Type.Filled; // Filled image!
        fillBarImage.fillMethod = Image.FillMethod.Horizontal;
        fillBarImage.fillOrigin = (int)Image.OriginHorizontal.Right; // SAĞDAN BAŞLA! 🎯
        fillBarImage.fillAmount = 1f; // Başta dolu
        segmentFillBars.Add(fillBarImage);
        
        // Segmentler listesi - fill bar kullanıyoruz
        segmentImages.Add(fillBarImage);
        
        // Outline ekle (kenar çizgisi)
        Outline outline = fillBarObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.5f);
        outline.effectDistance = new Vector2(2, -2);
        
        // 3) GLOW (en üstte - ışıldama efekti) ✨
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(segmentObj.transform, false);
        
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.sizeDelta = new Vector2(4, 4); // Biraz büyük
        glowRect.anchoredPosition = Vector2.zero;
        
        Image glowImage = glowObj.AddComponent<Image>();
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
        
        // GLOW RAYCAST KAPALI - Fill bar görünsün! 🎯
        glowImage.raycastTarget = false;
        
        segmentGlows.Add(glowImage);
    }
    
    /// <summary>
    /// Pulse animasyonu güncelle
    /// </summary>
    void UpdatePulse()
    {
        float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
        float brightness = 1f + pulse;
        
        // Sadece dolu segmentlerde pulse
        int filledSegments = Mathf.CeilToInt((float)currentHP / hpPerSegment);
        
        for (int i = 0; i < segmentFillBars.Count; i++)
        {
            if (i < filledSegments && segmentFillBars[i].enabled)
            {
                // Dolu segment - pulse yap
                Color pulseColor = fullSegmentColor * brightness;
                segmentFillBars[i].color = pulseColor; // Fill bar pulse!
                
                // Glow'u da pulse yap
                Color glowPulse = glowColor * brightness;
                glowPulse.a = 0.3f + (pulse * 0.2f);
                segmentGlows[i].color = glowPulse;
            }
        }
    }
    
    /// <summary>
    /// HP güncelle
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        currentHP = currentHealth;
        currentMaxHP = maxHealth;
        
        // Kaç segment dolu olmalı?
        int filledSegments = Mathf.CeilToInt((float)currentHP / hpPerSegment);
        
        // Segment kırıldı mı kontrol et
        if (filledSegments < lastBrokenSegment || lastBrokenSegment == -1)
        {
            lastBrokenSegment = filledSegments;
            
            // Kırılan segment varsa flash efekti
            if (filledSegments < segmentCount)
            {
                StartCoroutine(SegmentBreakEffect(filledSegments));
            }
        }
        
        // Segmentleri güncelle
        UpdateSegments(filledSegments);
        
        // Boss öldüyse bar'ı gizle
        if (currentHP <= 0)
        {
            HideBar();
        }
    }
    
    /// <summary>
    /// Segmentleri güncelle (doluluk oranlarıyla birlikte!)
    /// </summary>
    void UpdateSegments(int filledCount)
    {
        for (int i = 0; i < segmentFillBars.Count; i++)
        {
            if (i < filledCount - 1)
            {
                // Tamamen dolu segment (son segment değil)
                segmentFillBars[i].enabled = true;
                segmentGlows[i].enabled = true;
                segmentFillBars[i].fillAmount = 1f; // Tamamen dolu
            }
            else if (i == filledCount - 1)
            {
                // SON SEGMENT - Kısmi dolu olabilir! 💧
                segmentFillBars[i].enabled = true;
                segmentGlows[i].enabled = true;
                
                // Bu segmentteki HP'yi hesapla
                int hpInThisSegment = currentHP - (i * hpPerSegment);
                float fillRatio = Mathf.Clamp01((float)hpInThisSegment / hpPerSegment);
                
                segmentFillBars[i].fillAmount = fillRatio; // Kısmi doluluk!
                
                // Debug - sadece aktif segmentte
                if (Time.frameCount % 30 == 0) // Her 30 frame'de bir log
                {
                    Debug.Log($"💜 Segment {i}: {hpInThisSegment}/{hpPerSegment} HP, Fill: {fillRatio:F2}");
                }
            }
            else
            {
                // Boş segment (kırılmış)
                segmentFillBars[i].enabled = false;
                segmentGlows[i].enabled = false;
            }
        }
    }
    
    /// <summary>
    /// EPIC Segment kırılma efekti! 💥
    /// </summary>
    System.Collections.IEnumerator SegmentBreakEffect(int brokenIndex)
    {
        if (brokenIndex >= segmentImages.Count) yield break;
        
        // SCREEN FLASH - Ekran beyaza dönsün! ⚡
        if (enableScreenFlash)
        {
            StartCoroutine(ScreenFlashEffect());
        }
        
        // GÜÇLÜ CAMERA SHAKE! 📷
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.3f, 0.25f); // Daha güçlü!
        }
        
        // SOUND - Boss hit!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBossHit();
        }
        
        // SEGMENT SHATTER - Parçalara ayrılsın! 💥
        StartCoroutine(ShatterSegment(brokenIndex));
        
        Debug.Log($"💥 Boss segment KIRI L D I! Kalan: {brokenIndex}");
        
        yield return null;
    }
    
    /// <summary>
    /// Segment parçalara ayrılıp uçar! 💥
    /// </summary>
    System.Collections.IEnumerator ShatterSegment(int index)
    {
        if (index >= segmentCount) yield break;
        
        // Orijinal segment objesi
        Transform segmentTransform = barContainer.transform.GetChild(index);
        Image segmentFillBar = segmentFillBars[index];
        Image segmentGlow = segmentGlows[index];
        
        // Orijinal pozisyon ve renk
        Vector3 originalPos = segmentTransform.localPosition;
        Color originalColor = segmentFillBar.color;
        
        // İKİ PARÇA OLUŞTUR - Sol ve Sağ! 
        GameObject leftPiece = CreateShatterPiece(segmentTransform, true);
        GameObject rightPiece = CreateShatterPiece(segmentTransform, false);
        
        // PARTİCLE EXPLOSION! ✨
        CreateBreakParticles(segmentTransform.position);
        
        // Orijinal segment'i gizle
        segmentFillBar.enabled = false;
        segmentGlow.enabled = false;
        
        // Segment'in world pozisyonunu al
        RectTransform segmentRect = segmentTransform.GetComponent<RectTransform>();
        Vector2 segmentScreenPos = RectTransformUtility.WorldToScreenPoint(null, segmentRect.position);
        
        // Ekran merkezini hesapla
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
        // Merkeze doğru yön vektörü
        Vector2 toCenterDirection = (screenCenter - segmentScreenPos).normalized;
        
        // Parçaları uçur - EKRAN MERKEZİNE DOĞRU! 🎯
        float elapsed = 0f;
        
        while (elapsed < shatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shatterDuration;
            
            // Ease out cubic
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            
            if (leftPiece != null)
            {
                // Sol parça - merkeze doğru ama hafif sola kaymış
                RectTransform leftRect = leftPiece.GetComponent<RectTransform>();
                Vector2 leftOffset = new Vector2(-50f, 0f); // 50px sola offset
                Vector2 targetPos = (toCenterDirection * shatterDistance) + leftOffset;
                leftRect.anchoredPosition = Vector2.Lerp(Vector2.zero, targetPos, ease);
                leftRect.localScale = Vector3.one * (1f + ease * 0.5f); // Büyüsün
                leftRect.localRotation = Quaternion.Euler(0, 0, -360f * ease); // Dönsün
                
                // Fade out
                Image leftImg = leftPiece.GetComponent<Image>();
                if (leftImg != null)
                {
                    Color c = leftImg.color;
                    c.a = 1f - ease;
                    leftImg.color = c;
                }
            }
            
            if (rightPiece != null)
            {
                // Sağ parça - merkeze doğru ama hafif sağa kaymış
                RectTransform rightRect = rightPiece.GetComponent<RectTransform>();
                Vector2 rightOffset = new Vector2(50f, 0f); // 50px sağa offset
                Vector2 targetPos = (toCenterDirection * shatterDistance) + rightOffset;
                rightRect.anchoredPosition = Vector2.Lerp(Vector2.zero, targetPos, ease);
                rightRect.localScale = Vector3.one * (1f + ease * 0.5f); // Büyüsün
                rightRect.localRotation = Quaternion.Euler(0, 0, 360f * ease); // Dönsün
                
                // Fade out
                Image rightImg = rightPiece.GetComponent<Image>();
                if (rightImg != null)
                {
                    Color c = rightImg.color;
                    c.a = 1f - ease;
                    rightImg.color = c;
                }
            }
            
            yield return null;
        }
        
        // Parçaları yok et
        if (leftPiece != null) Destroy(leftPiece);
        if (rightPiece != null) Destroy(rightPiece);
    }
    
    /// <summary>
    /// Kırık parça oluştur (sol veya sağ yarı)
    /// </summary>
    GameObject CreateShatterPiece(Transform parent, bool isLeft)
    {
        GameObject piece = new GameObject(isLeft ? "LeftPiece" : "RightPiece");
        piece.transform.SetParent(parent, false);
        
        RectTransform pieceRect = piece.AddComponent<RectTransform>();
        pieceRect.anchorMin = Vector2.zero;
        pieceRect.anchorMax = Vector2.one;
        pieceRect.sizeDelta = new Vector2(0, 0);
        pieceRect.anchoredPosition = Vector2.zero;
        
        // Sol yarı mı sağ yarı mı?
        if (isLeft)
        {
            pieceRect.pivot = new Vector2(1f, 0.5f); // Sağ kenardan pivot
            pieceRect.anchorMax = new Vector2(0.5f, 1f); // Sol yarı
        }
        else
        {
            pieceRect.pivot = new Vector2(0f, 0.5f); // Sol kenardan pivot
            pieceRect.anchorMin = new Vector2(0.5f, 0f); // Sağ yarı
        }
        
        // Image ekle
        Image pieceImage = piece.AddComponent<Image>();
        pieceImage.color = fullSegmentColor;
        
        return piece;
    }
    
    /// <summary>
    /// Particle patlaması! ✨
    /// </summary>
    void CreateBreakParticles(Vector3 worldPosition)
    {
        GameObject particleObj = new GameObject("BreakParticles");
        particleObj.transform.position = worldPosition;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(10f, 30f);
        main.startColor = new ParticleSystem.MinMaxGradient(fullSegmentColor, glowColor);
        main.maxParticles = 30;
        main.duration = 0.3f;
        main.loop = false;
        
        // Emission
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30) // Anında 30 parçacık!
        });
        
        // Shape - daire
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        
        // Velocity - dışa doğru
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.radial = 10f;
        
        // Color over lifetime (fade out)
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;
        
        // Renderer
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Canvas'a ekle (UI particle)
        particleObj.transform.SetParent(canvas.transform, true);
        
        // Play ve yok et
        particles.Play();
        Destroy(particleObj, 1f);
    }
    
    /// <summary>
    /// Ekran flash efekti - Beyaz çakma! ⚡
    /// </summary>
    System.Collections.IEnumerator ScreenFlashEffect()
    {
        // Full screen beyaz panel oluştur
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(canvas.transform, false);
        
        RectTransform flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashRect.anchoredPosition = Vector2.zero;
        
        Image flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0.8f); // Beyaz, yarı transparan
        
        // En üstte görünsün
        Canvas.ForceUpdateCanvases();
        flashObj.transform.SetAsLastSibling();
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < screenFlashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.8f, 0f, elapsed / screenFlashDuration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        
        // Yok et
        Destroy(flashObj);
    }
    
    /// <summary>
    /// Bar'ı göster
    /// </summary>
    public void ShowBar()
    {
        if (barContainer != null)
        {
            barContainer.SetActive(true);
            isVisible = true;
            Debug.Log("💜 Boss Health Bar gösterildi!");
        }
    }
    
    /// <summary>
    /// Bar'ı gizle
    /// </summary>
    public void HideBar()
    {
        if (barContainer != null)
        {
            barContainer.SetActive(false);
            isVisible = false;
            Debug.Log("💜 Boss Health Bar gizlendi!");
        }
    }
    
    /// <summary>
    /// Bar'ı sıfırla (yeni boss için)
    /// </summary>
    public void ResetBar(int maxHP)
    {
        currentMaxHP = maxHP;
        currentHP = maxHP;
        lastBrokenSegment = segmentCount; // Tüm segmentler dolu
        
        // Tüm segmentleri göster ve doldur
        UpdateSegments(segmentCount);
        
        // Fill bar'ları sıfırla
        for (int i = 0; i < segmentFillBars.Count; i++)
        {
            segmentFillBars[i].fillAmount = 1f; // Tamamen dolu
        }
        
        ShowBar();
    }
}