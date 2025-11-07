using UnityEngine;
using UnityEngine.UI;

public class CardHotkey : MonoBehaviour
{
    public int hotkeyNumber = 1; // 1, 2, 3, 4
    private Text hotkeyText;
    
    void Start()
    {
        CreateHotkeyText();
    }
    
    void CreateHotkeyText()
    {
        // Canvas'ı bul
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas bulunamadı!");
            return;
        }
        
        // Text objesi oluştur
        GameObject textObj = new GameObject("HotkeyText");
        textObj.transform.SetParent(transform, false);
        
        hotkeyText = textObj.AddComponent<Text>();
        hotkeyText.text = hotkeyNumber.ToString();
        hotkeyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hotkeyText.fontSize = 32;
        hotkeyText.fontStyle = FontStyle.Bold;
        hotkeyText.alignment = TextAnchor.MiddleCenter;
        hotkeyText.color = Color.white;
        
        // Glow efekti için outline (opsiyonel)
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        // Shadow ekle
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(3, -3);
        
        // Pozisyon (kartın üst ortasında)
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 30); // Üstte
        rectTransform.sizeDelta = new Vector2(100, 50);
    }
}