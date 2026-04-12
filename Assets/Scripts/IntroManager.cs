using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    private GameObject introCanvas;

    void Start()
    {
        // Make sure it runs regardless of other script execution order
        CreateIntroCanvas();
        PauseGame();
    }

    void Update()
    {
        // Ensure the cursor remains unlocked while the intro screen is active
        if (introCanvas != null && introCanvas.activeSelf)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void CreateIntroCanvas()
    {
        introCanvas = new GameObject("IntroCanvas");
        Canvas canvas = introCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        introCanvas.AddComponent<GraphicRaycaster>(); // Allows UI clicks

        CanvasScaler cScaler = introCanvas.AddComponent<CanvasScaler>();
        cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cScaler.referenceResolution = new Vector2(1920, 1080);

        // Background Panel
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(introCanvas.transform, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 1f); // Dark background
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.font = font;
        titleText.color = Color.white;
        titleText.fontSize = 80;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "Political Galaxy";

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.65f);
        titleRect.sizeDelta = new Vector2(1500, 200);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Instruction Text
        GameObject instObj = new GameObject("InstructionText");
        instObj.transform.SetParent(panelObj.transform, false);
        Text instText = instObj.AddComponent<Text>();
        instText.font = font;
        instText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        instText.fontSize = 32;
        instText.alignment = TextAnchor.MiddleCenter;
        instText.text = "Explore the echoes of political polarization.\nUse W,A,S,D to move. Mouse to look around.\nNavigate close to topics to uncover their perspectives.";
        instText.lineSpacing = 1.25f;

        RectTransform instRect = instObj.GetComponent<RectTransform>();
        instRect.anchorMin = new Vector2(0.5f, 0.45f);
        instRect.anchorMax = new Vector2(0.5f, 0.45f);
        instRect.sizeDelta = new Vector2(1500, 300);
        instRect.anchoredPosition = Vector2.zero;

        // Start Button Object
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.4f, 0.8f, 1f); // Blueish button
        Button btn = btnObj.AddComponent<Button>();
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.25f);
        btnRect.anchorMax = new Vector2(0.5f, 0.25f);
        btnRect.sizeDelta = new Vector2(300, 90);
        btnRect.anchoredPosition = Vector2.zero;

        // Start Button Text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = font;
        btnText.text = "START";
        btnText.color = Color.white;
        btnText.fontSize = 35;
        btnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Hook up event
        btn.onClick.AddListener(StartGame);
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        introCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
        
        Time.timeScale = 1f;
        introCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Give control back to player
        Cursor.visible = false;
    }
}
