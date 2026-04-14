using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BridgePlanetInfo : MonoBehaviour
{
    public float gravityForce = 60f;

    private GameObject infoCanvas;
    private bool isPlayerNear = false;
    private bool hasBeenRead = false;
    private ParticleSystem cloudPS;

    void Awake()
    {
        // Cloud particle system'i erken bul
        cloudPS = GetComponentInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        // Bu obje her SetActive(true) olduğunda particle'ları yeniden başlat
        if (cloudPS != null)
        {
            cloudPS.Clear();
            cloudPS.Play();
        }
    }

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        infoCanvas = new GameObject("BridgeInfoCanvas");
        // İlk frame'de beyaz dikdörtgen çıkmasın — anında kapat
        infoCanvas.SetActive(false);

        Canvas canvas = infoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler cs = infoCanvas.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(infoCanvas.transform, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.98f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.15f);
        panelRect.anchorMax = new Vector2(0.85f, 0.85f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panelObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.font = font;
        t.color = Color.white;
        t.fontSize = 28;
        t.lineSpacing = 1.5f;
        t.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = t.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.05f);
        textRect.anchorMax = new Vector2(0.95f, 0.95f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        t.text =
            "<b><size=48><color=#ffffff>The Bridge</color></size></b>\n\n" +
            "<color=#ffffff>Congratulations!\n\n" +
            "You've witnessed the five main polarizing concepts\n" +
            "and learned what you shouldn't be.\n\n" +
            "Now you need to see what you need to unite.\n\n" +
            "The bridge between divided worlds is built\n" +
            "with empathy, dialogue, and understanding.</color>\n\n" +
            "<i><color=#aaaaaa>(Click / Press E or ESC to exit)</color></i>";

        // Canvas başta kapatıldı — tekrar SetActive çıkarma
    }

    void Update()
    {
        if (isPlayerNear && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                ClosePanel();
                MarkAsRead();
            }
        }
    }

    private void ClosePanel()
    {
        if (infoCanvas != null) infoCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void MarkAsRead()
    {
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            if (PlanetManager.Instance != null)
                PlanetManager.Instance.MarkBridgeVisited();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        float distance = Vector3.Distance(transform.position, other.transform.position);
        float surfaceDistance = (transform.localScale.x / 2f) + 36f;

        if (distance > surfaceDistance)
        {
            Vector3 direction = (transform.position - other.transform.position).normalized;
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                float pull = Mathf.Clamp(gravityForce * 0.6f, 5f, gravityForce);
                cc.Move(direction * pull * Time.deltaTime);
            }
            if (isPlayerNear) { ClosePanel(); isPlayerNear = false; }
        }
        else
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                if (infoCanvas != null) infoCanvas.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerNear) { ClosePanel(); MarkAsRead(); }
        isPlayerNear = false;
    }
}
