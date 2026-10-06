using UnityEngine;
using UnityEngine.UI;

public class HelmetHUD : MonoBehaviour
{
    void Start()
    {
        // HUD'i WorldSpace (3D obje) olarak kurguluyoruz. 
        // ScreenSpace VR'da hatalar (siyah kare) yapabiliyor.
        GameObject hudObj = new GameObject("HelmetHUD_Canvas");
        hudObj.transform.SetParent(this.transform, false);
        
        // Gzden 35 santim ileriye koyuyoruz
        hudObj.transform.localPosition = new Vector3(0, 0, 0.35f); 
        hudObj.transform.localRotation = Quaternion.identity;
        hudObj.transform.localScale = Vector3.one * 0.0007f; 

        Canvas canvas = hudObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Lazerlerin kaska arpip taklmamas iin raycaster' kapatarz
        var raycaster = hudObj.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false; 

        RectTransform canvasRT = hudObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(800, 800);

        // Kask ereveleri (Kenarlardan 200 birim kalnlk)
        CreateBorder(hudObj.transform, "Top", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -250), new Vector2(0, 0));
        CreateBorder(hudObj.transform, "Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 250));
        CreateBorder(hudObj.transform, "Left", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(250, 0));
        CreateBorder(hudObj.transform, "Right", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-250, 0), new Vector2(0, 0));
    }

    void CreateBorder(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject border = new GameObject("Border_" + name);
        border.transform.SetParent(parent, false);
        
        Image img = border.AddComponent<Image>();
        img.color = new Color(0.01f, 0.02f, 0.05f, 0.98f); // Koyu estetik bir kask rengi
        
        RectTransform rt = border.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
