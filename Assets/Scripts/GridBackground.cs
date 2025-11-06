using UnityEngine;

public class GridBackground : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridSpacing = 1f;
    public Color gridColor = new Color(0f, 0.5f, 1f, 0.1f); // Mavi neon, şeffaf
    public float lineWidth = 0.02f;
    
    void Start()
    {
        DrawGrid();
    }
    
    void DrawGrid()
    {
        // Dikey çizgiler
        for (int x = -gridWidth; x <= gridWidth; x++)
        {
            CreateLine(
                new Vector3(x * gridSpacing, -gridHeight * gridSpacing, 0),
                new Vector3(x * gridSpacing, gridHeight * gridSpacing, 0)
            );
        }
        
        // Yatay çizgiler
        for (int y = -gridHeight; y <= gridHeight; y++)
        {
            CreateLine(
                new Vector3(-gridWidth * gridSpacing, y * gridSpacing, 0),
                new Vector3(gridWidth * gridSpacing, y * gridSpacing, 0)
            );
        }
    }
    
    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = transform;
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingOrder = -10; // En arkada
    }
}