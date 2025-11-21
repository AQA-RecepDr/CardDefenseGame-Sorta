using UnityEngine;
using System.Collections;

public class FinalBossController : MonoBehaviour
{
    [Header("Zone Positions")]
    public Vector2 topZonePos = new Vector2(0f, 6f);
    public Vector2 rightZonePos = new Vector2(6f, 0f);
    public Vector2 bottomZonePos = new Vector2(0f, -6f);
    public Vector2 leftZonePos = new Vector2(-6f, 0f);
    private Vector2[] zonePositions;
    private int currentZoneIndex = 0;
    
    [Header("Enemy Spawning")]
    public float enemySpawnInterval = 2f; // Her 2 saniyede düşman spawn
    private float enemySpawnTimer = 0f;
    public int minionsPerSpawn = 2; // Her seferde 2 kırmızı düşman
    
    [Header("Boss Movement")]
    public float zoneSwitchInterval = 4f; // 3-5 saniye arası
    private float zoneSwitchTimer = 0f;
    
    private Enemy bossEnemy;
    private bool isBossActive = false;

    void Start()
    {
        bossEnemy = GetComponent<Enemy>();
        
        // Zone pozisyonlarını hazırla
        zonePositions = new Vector2[]
        {
            topZonePos,
            rightZonePos,
            bottomZonePos,
            leftZonePos
        };
        
        // Başlangıç
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.3f);
        StartBoss();
    }

    void StartBoss()
    {
        isBossActive = true;
        
        // İlk zone'a teleport
        currentZoneIndex = Random.Range(0, 4);
        transform.position = zonePositions[currentZoneIndex];
        
        // Boss health bar göster
        if (BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.ShowBar();
            BossHealthBar.Instance.ResetBar(bossEnemy.maxHealth);
        }
        
        // İlk zone switch interval'i random yap
        zoneSwitchInterval = Random.Range(3f, 5f);
        
        Debug.Log("💀 Final Boss başladı!");
    }

    void Update()
    {
        if (!isBossActive) return;
        
        // Enemy spawn
        enemySpawnTimer += Time.deltaTime;
        if (enemySpawnTimer >= enemySpawnInterval)
        {
            SpawnRedEnemies();
            enemySpawnTimer = 0f;
        }
        
        // Zone switching
        zoneSwitchTimer += Time.deltaTime;
        if (zoneSwitchTimer >= zoneSwitchInterval)
        {
            SwitchZone();
            zoneSwitchTimer = 0f;
            // Random interval (3-5 saniye)
            zoneSwitchInterval = Random.Range(3f, 5f);
        }
    }

    void SwitchZone()
    {
        // Farklı bir zone seç
        int newZoneIndex = currentZoneIndex;
        while (newZoneIndex == currentZoneIndex)
        {
            newZoneIndex = Random.Range(0, 4);
        }
        
        currentZoneIndex = newZoneIndex;
        
        // Teleport efekti
        Vector2 newPos = zonePositions[currentZoneIndex];
        transform.position = newPos;
        
        Debug.Log($"💀 Boss zone değiştirdi! → Zone {currentZoneIndex}, Position: {newPos}");
        
        // Teleport particle efekti
        if (HitEffectManager.Instance != null)
        {
            HitEffectManager.Instance.ShowHitEffect(transform.position, Color.red);
        }
    }

    void SpawnRedEnemies()
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            for (int i = 0; i < minionsPerSpawn; i++)
            {
                spawner.SpawnSpecificEnemy(Enemy.EnemyType.Red);
            }
        }
    }
    
    // Boss öldüğünde çağrılır
    public void StopBoss()
    {
        isBossActive = false; // Update() artık çalışmayacak
        
        Debug.Log("💀 Final Boss durduruldu! Artık hareket etmiyor.");
    }
    
}