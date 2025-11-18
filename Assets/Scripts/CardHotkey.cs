using UnityEngine;

public class CardHotkey : MonoBehaviour
{
    public int hotkeyNumber = 1; // 1, 2, 3, 4
    private TextMesh hotkeyText;
    private GameObject textObj;
    
    void Start()
    {
        // Eğer zaten hotkey text varsa, iki tane oluşmasın
        if (transform.Find("HotkeyText") != null)
        {
            Debug.Log($"⚠️ Kart {hotkeyNumber} için hotkey zaten var, yeniden oluşturulmuyor.");
            return;
        }
        
        CreateHotkeyText();
    }
    
    void CreateHotkeyText()
    {
        // TextMesh ile 3D text oluştur (world space için)
        textObj = new GameObject("HotkeyText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, 0.6f, 0); // Kartın üstünde
        textObj.transform.localScale = Vector3.one * 0.1f; // Küçült
        
        hotkeyText = textObj.AddComponent<TextMesh>();
        hotkeyText.text = hotkeyNumber.ToString();
        hotkeyText.fontSize = 50;
        hotkeyText.color = Color.white;
        hotkeyText.anchor = TextAnchor.MiddleCenter;
        hotkeyText.alignment = TextAlignment.Center;
        hotkeyText.fontStyle = FontStyle.Bold;
        
        // Renderer ayarları (görünür olması için)
        MeshRenderer renderer = textObj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 100; // En üstte görünsün
        }
        
        Debug.Log($"✅ Hotkey {hotkeyNumber} oluşturuldu! Pos: {textObj.transform.position}");
    }
    
    void OnDestroy()
    {
        // Text objesini de yok et
        if (textObj != null)
        {
            Destroy(textObj);
        }
    }
}