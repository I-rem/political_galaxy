using UnityEngine;
using UnityEngine.UI;

public class HelmetHUD : MonoBehaviour
{
    void Start()
    {
        // Kask UI'n kamera nnde oluturalm
        GameObject hudObj = new GameObject("HelmetHUD_Canvas");
        hudObj.transform.SetParent(this.transform, false);

        Canvas canvas = hudObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GetComponent<Camera>();
        canvas.planeDistance = 0.5f; // Tam yzn nnde
        canvas.sortingOrder = 10;

        // Vinyet (Kararma) ve Referans erevesi
        GameObject vignetteObj = new GameObject("Vignette");
        vignetteObj.transform.SetParent(hudObj.transform, false);
        Image img = vignetteObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);

        // Gradient veya sprite yoksa, dumduz frame izelim (Ortas bos kalacak)
        // Bunun yerine, 4 adet siyah ereve yapalm
        CreateBorder(hudObj.transform, "Top", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -0.9f), new Vector2(0, 0));
        CreateBorder(hudObj.transform, "Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 0.9f));
        CreateBorder(hudObj.transform, "Left", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(0.9f, 0));
        CreateBorder(hudObj.transform, "Right", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-0.9f, 0), new Vector2(0, 0));
    }

    void CreateBorder(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject border = new GameObject("Border_" + name);
        border.transform.SetParent(parent, false);
        Image img = border.AddComponent<Image>();
        
        // Mide bulantsn engellemek iin statik bir uzay kask i yzeyi rengi
        img.color = new Color(0.02f, 0.05f, 0.1f, 0.9f); 

        RectTransform rt = border.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        
        // Ekran kenarlarndan dar doru kalnlk
        // Bylece dnya her zaman sabit bir "Kokpit" erevesinden grnr
        if (name == "Top") { rt.offsetMin = new Vector2(0, -70); rt.offsetMax = new Vector2(0, 0); }
        if (name == "Bottom") { rt.offsetMin = new Vector2(0, 0); rt.offsetMax = new Vector2(0, 70); }
        if (name == "Left") { rt.offsetMin = new Vector2(0, 0); rt.offsetMax = new Vector2(70, 0); }
        if (name == "Right") { rt.offsetMin = new Vector2(-70, 0); rt.offsetMax = new Vector2(0, 0); }
    }
}
