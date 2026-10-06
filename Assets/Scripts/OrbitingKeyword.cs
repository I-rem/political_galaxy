using UnityEngine;
using UnityEngine.UI;

public class OrbitingKeyword : MonoBehaviour
{
    public Transform centerPoint;
    public float orbitSpeed = 20f;
    private Vector3 orbitAxis;
    private Text uiText;

    public void SetupText(string text, Color color, int fontSize = 350, float characterSize = 0.12f)
    {
        // 1. Setup Canvas
        Canvas c = gameObject.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        RectTransform rt = gameObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1000, 200);
        gameObject.transform.localScale = Vector3.one * 0.05f;

        // 2. Background
        GameObject bg = new GameObject("Bg");
        bg.transform.SetParent(gameObject.transform, false);
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.65f);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.sizeDelta = Vector2.zero;

        // 3. Text
        GameObject txtObj = new GameObject("Txt");
        txtObj.transform.SetParent(gameObject.transform, false);
        uiText = txtObj.AddComponent<Text>();
        uiText.text = text;
        uiText.color = color;
        uiText.fontSize = 100;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = TextAnchor.MiddleCenter;
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.font = font;

        RectTransform txtrt = txtObj.GetComponent<RectTransform>();
        txtrt.anchorMin = Vector2.zero; txtrt.anchorMax = Vector2.one;
        txtrt.sizeDelta = Vector2.zero;

        // Add BoxCollider for Raycast
        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(1000, 200, 1);

        orbitAxis = Random.onUnitSphere;
    }

    public void ChangeColor(Color newColor)
    {
        if (uiText != null)
        {
            uiText.color = newColor;
        }
    }

    void Update()
    {
        if (centerPoint != null)
        {
            transform.RotateAround(centerPoint.position, orbitAxis, orbitSpeed * Time.deltaTime);
            
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }
    }
}
