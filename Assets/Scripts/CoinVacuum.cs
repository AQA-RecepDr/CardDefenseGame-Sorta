using UnityEngine;

public class CoinVacuum : MonoBehaviour
{
    public static CoinVacuum Instance;
    
    [Header("Vacuum Settings")]
    public float vacuumRange = 8f; // Çok uzak menzil (tüm ekran)
    public float vacuumWidth = 2f; // Dar çekim alanı
    public bool isVacuumActive = false;
    
    [Header("Visual Feedback")]
    public GameObject vacuumBeamPrefab; // Opsiyonel: beam göstergesi
    private LineRenderer vacuumBeam;
    private ParticleSystem vacuumParticles;
    
    
    [Header("Visual")]
    public LineRenderer vacuumLine; // Vakum görsel göstergesi (opsiyonel)
    
    private Camera mainCamera;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        CreateVacuumVisuals();
    }
    
    void Update()
    {
        // SAÄž TIK BASILI TUTARKEN - Vakum aktif
        if (Input.GetMouseButton(1))
        {
            isVacuumActive = true;
            ActivateVacuum();
            
            // PULSE EFEKTİ - KONİ YAŞIYOR GİBİ! ✨
            if (vacuumBeam != null && vacuumBeam.enabled)
            {
                float pulse = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f; // 0-1 arası
                float alpha = Mathf.Lerp(0.1f, 0.2f, pulse); // Hafif pulse
        
                Color c = vacuumBeam.startColor;
                c.a = alpha;
                vacuumBeam.startColor = c;
            }
        }
        else
        {
            isVacuumActive = false;
            DeactivateVacuum();
        }
    }
    
    void CreateVacuumVisuals()
    {
        // Vakum koni efekti
        GameObject beamObj = new GameObject("VacuumCone");
        beamObj.transform.SetParent(transform, false);

        vacuumBeam = beamObj.AddComponent<LineRenderer>();
        vacuumBeam.material = new Material(Shader.Find("Sprites/Default"));
    
        // KONİ AYARLARI - Başta dar, sonda geniş
        vacuumBeam.startWidth = 0.2f;  // Karakterden dar başla
        vacuumBeam.endWidth = 9.0f;    // Dışa doğru genişle
    
        vacuumBeam.positionCount = 2;
        vacuumBeam.sortingOrder = -5; // Arkada dursun (karakterin altında)

        // TRANSPARAN SARI - Hafif görünsün
        Color coneColor = new Color(1f, 0.9f, 0.3f, 0.08f); // Çok transparan
        vacuumBeam.startColor = coneColor; // Başta hafif görünür
    
        Color endColor = coneColor;
        endColor.a = 0f; // Sonda tamamen transparan
        vacuumBeam.endColor = endColor;

        // Yumuşak blend
        vacuumBeam.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        vacuumBeam.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        // Başlangıçta kapalı
        vacuumBeam.enabled = false;

        Debug.Log("✨ Vakum koni efekti oluşturuldu!");
    }
    
    void ActivateVacuum()
    {
        // Mouse pozisyonu - ÖNCE TANIMLA! 🎯
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
    
        // Player pozisyonu (merkez)
        Vector3 playerPos = transform.position;
    
        // Yön
        Vector3 direction = (mousePos - playerPos).normalized;
    
        // BEAM GÖSTERGESİ - YENİ! ⚡
        if (vacuumBeam != null)
        {
            vacuumBeam.enabled = true;
        
            // Beam çiz
            vacuumBeam.SetPosition(0, playerPos);
            vacuumBeam.SetPosition(1, playerPos + direction * vacuumRange);
        }
    
        // Tüm coinleri bul
        CoinPickup[] allCoins = FindObjectsOfType<CoinPickup>();
    
        foreach (CoinPickup coin in allCoins)
        {
            // Coin vakum alanında mı?
            if (IsCoinInVacuumCone(coin.transform.position, playerPos, direction))
            {
                coin.PullTowards(playerPos);
            }
            else
            {
                coin.StopPull();
            }
        }
    
        // Vakum sesi (sürekli çalarken)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayVacuumLoop();
        }
        
    }
    
    void DeactivateVacuum()
    {
        if (vacuumBeam != null)
        {
            vacuumBeam.enabled = false;
        }
        
        // Tüm coinlerin çekimini durdur
        CoinPickup[] allCoins = FindObjectsOfType<CoinPickup>();
        
        foreach (CoinPickup coin in allCoins)
        {
            coin.StopPull();
        }
    }
    
    // Coin vakum konisi içinde mi?
    bool IsCoinInVacuumCone(Vector3 coinPos, Vector3 playerPos, Vector3 direction)
    {
        // Coin ile player arası vektör
        Vector3 toCoin = coinPos - playerPos;
        float distance = toCoin.magnitude;
        
        // Menzil kontrolü
        if (distance > vacuumRange)
            return false;
        
        // Yön kontrolü (coin mouse yönünde mi?)
        float angle = Vector3.Angle(direction, toCoin);
        
        // Dar koni (örnek: 15 derece)
        float coneAngle = 15f;
        
        return angle < coneAngle;
    }
    
    // Debug çizimi (Test için)
    void OnDrawGizmos()
    {
        if (!isVacuumActive || mainCamera == null) return;
        
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector3 direction = (mousePos - transform.position).normalized;
        
        // Vakum yönünü çiz
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + direction * vacuumRange);
    }
}