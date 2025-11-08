using UnityEngine;

public class GridBackground : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridSpacing = 1f;
    public Color gridColor = new Color(0f, 0.5f, 1f, 0.1f); // Mavi neon, şeffaf
    public float lineWidth = 0.02f;
    public int lineSegments = 50; // Daha fazla segment = daha smooth dalga
    
    [Header("Animation Settings - YENİ! 🌊")]
    public bool enableAnimation = true;
    public float waveSpeed = 1f; // Dalga hızı
    public float waveAmplitude = 0.12f; // Dalga genliği (ne kadar hareket)
    public float waveFrequency = 1.5f; // Dalga frekansı
    public float randomMovementAmount = 0.04f; // Random hareket miktarı
    
    private LineRenderer[] verticalLines;
    private LineRenderer[] horizontalLines;
    private float[] verticalOffsets; // Her çizgi için random offset
    private float[] horizontalOffsets;
    
    void Start()
    {
        DrawGrid();
    }
    
    void Update()
    {
        if (enableAnimation)
        {
            AnimateGrid();
        }
    }
    
    void DrawGrid()
    {
        int verticalCount = (gridWidth * 2) + 1;
        int horizontalCount = (gridHeight * 2) + 1;
        
        verticalLines = new LineRenderer[verticalCount];
        verticalOffsets = new float[verticalCount];
        horizontalLines = new LineRenderer[horizontalCount];
        horizontalOffsets = new float[horizontalCount];
        
        int vIndex = 0;
        int hIndex = 0;
        
        // Dikey çizgiler
        for (int x = -gridWidth; x <= gridWidth; x++)
        {
            float xPos = x * gridSpacing;
            Vector3 start = new Vector3(xPos, -gridHeight * gridSpacing, 0);
            Vector3 end = new Vector3(xPos, gridHeight * gridSpacing, 0);
            
            verticalLines[vIndex] = CreateAnimatedLine(start, end, true);
            verticalOffsets[vIndex] = Random.Range(0f, 100f); // Random başlangıç
            vIndex++;
        }
        
        // Yatay çizgiler
        for (int y = -gridHeight; y <= gridHeight; y++)
        {
            float yPos = y * gridSpacing;
            Vector3 start = new Vector3(-gridWidth * gridSpacing, yPos, 0);
            Vector3 end = new Vector3(gridWidth * gridSpacing, yPos, 0);
            
            horizontalLines[hIndex] = CreateAnimatedLine(start, end, false);
            horizontalOffsets[hIndex] = Random.Range(0f, 100f);
            hIndex++;
        }
        
        Debug.Log($"✨ Animated Grid oluşturuldu! {verticalCount} dikey + {horizontalCount} yatay çizgi");
    }
    
    LineRenderer CreateAnimatedLine(Vector3 start, Vector3 end, bool isVertical)
    {
        GameObject lineObj = new GameObject(isVertical ? "VerticalGridLine" : "HorizontalGridLine");
        lineObj.transform.parent = transform;
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = lineSegments; // ÇOK SEGMENTLÄ° - YENÄ°!
        lr.sortingOrder = -10;
        lr.useWorldSpace = true;
        
        // Additive blend (glow)
        lr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        
        // Ä°lk pozisyonlarÄ± ayarla (dÃ¼z Ã§izgi)
        for (int i = 0; i < lineSegments; i++)
        {
            float t = (float)i / (lineSegments - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);
            lr.SetPosition(i, pos);
        }
        
        return lr;
    }
    
    void AnimateGrid()
    {
        float time = Time.time;
        
        // DÄ°KEY Ã‡Ä°ZGÄ°LERÄ° ANÄŽMATE ET 🌊
        for (int i = 0; i < verticalLines.Length; i++)
        {
            if (verticalLines[i] == null) continue;
            
            LineRenderer lr = verticalLines[i];
            
            for (int j = 0; j < lineSegments; j++)
            {
                float t = (float)j / (lineSegments - 1);
                
                // Orijinal pozisyon
                Vector3 basePos = lr.GetPosition(j);
                float baseX = (-gridWidth + i) * gridSpacing; // X sabit
                float baseY = Mathf.Lerp(-gridHeight * gridSpacing, gridHeight * gridSpacing, t);
                
                // DALGA HAREKETÄ° (X ekseninde) 🌊
                float wave = Mathf.Sin((baseY * waveFrequency) + (time * waveSpeed) + verticalOffsets[i]) * waveAmplitude;
                
                // RANDOM MÄŽNÄŽ HAREKET (Perlin Noise) 🎲
                float randomX = Mathf.PerlinNoise(
                    (baseY * 0.3f) + (time * 0.15f) + verticalOffsets[i],
                    verticalOffsets[i]
                ) * randomMovementAmount;
                
                float randomY = Mathf.PerlinNoise(
                    verticalOffsets[i],
                    (baseY * 0.3f) + (time * 0.1f)
                ) * randomMovementAmount * 0.5f; // Y'de daha az hareket
                
                // Yeni pozisyon
                Vector3 newPos = new Vector3(
                    baseX + wave + randomX,
                    baseY + randomY,
                    0
                );
                
                lr.SetPosition(j, newPos);
            }
        }
        
        // YATAY Ã‡Ä°ZGÄ°LERÄ° ANÄŽMATE ET 🌊
        for (int i = 0; i < horizontalLines.Length; i++)
        {
            if (horizontalLines[i] == null) continue;
            
            LineRenderer lr = horizontalLines[i];
            
            for (int j = 0; j < lineSegments; j++)
            {
                float t = (float)j / (lineSegments - 1);
                
                // Orijinal pozisyon
                float baseX = Mathf.Lerp(-gridWidth * gridSpacing, gridWidth * gridSpacing, t);
                float baseY = (-gridHeight + i) * gridSpacing; // Y sabit
                
                // DALGA HAREKETÄ° (Y ekseninde) 🌊
                float wave = Mathf.Sin((baseX * waveFrequency) + (time * waveSpeed) + horizontalOffsets[i]) * waveAmplitude;
                
                // RANDOM MÄŽNÄŽ HAREKET 🎲
                float randomY = Mathf.PerlinNoise(
                    (baseX * 0.3f) + (time * 0.15f) + horizontalOffsets[i],
                    horizontalOffsets[i]
                ) * randomMovementAmount;
                
                float randomX = Mathf.PerlinNoise(
                    horizontalOffsets[i],
                    (baseX * 0.3f) + (time * 0.1f)
                ) * randomMovementAmount * 0.5f; // X'te daha az hareket
                
                // Yeni pozisyon
                Vector3 newPos = new Vector3(
                    baseX + randomX,
                    baseY + wave + randomY,
                    0
                );
                
                lr.SetPosition(j, newPos);
            }
        }
    }
}