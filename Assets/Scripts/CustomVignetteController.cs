using UnityEngine;
using UnityEngine.UI;

public class CustomVignetteController : MonoBehaviour
{
    private Image vignetteImage;
    public float fadeSpeed = 5f;
    public float maxVignetteAlpha = 0.95f;
    public float speedThreshold = 0.5f;
    public float maxSpeedForFullVignette = 40f;

    private Vector3 lastPosition;

    void Start()
    {
        var cam = Camera.main;
        if (cam == null) return;

        GameObject canvasObj = new GameObject("Vignette_Canvas");
        canvasObj.transform.SetParent(cam.transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 0, 0.38f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.001f;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;

        GameObject imgObj = new GameObject("VignetteImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        vignetteImage = imgObj.AddComponent<Image>();

        Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(128, 128);
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float dist = Vector2.Distance(center, new Vector2(x, y));
                float alpha = Mathf.InverseLerp(70, 128, dist);
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        }
        tex.Apply();

        vignetteImage.sprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        vignetteImage.color = new Color(1, 1, 1, 0f);
        vignetteImage.raycastTarget = false;

        RectTransform rt = imgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2500, 2500); // make it large enough to cover FOV
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        lastPosition = transform.position;
    }

    void Update()
    {
        if (vignetteImage == null) return;

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float currentSpeed = distanceMoved / Time.deltaTime;
        lastPosition = transform.position;

        float targetAlpha = 0f;
        if (currentSpeed > speedThreshold)
        {
            float speedRatio = Mathf.Clamp01((currentSpeed - speedThreshold) / maxSpeedForFullVignette);
            targetAlpha = speedRatio * maxVignetteAlpha;
        }

        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        vignetteImage.color = c;
    }
}
