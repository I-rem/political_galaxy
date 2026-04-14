using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
        }

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
        bg.color = new Color(0.02f, 0.05f, 0.08f, 0.9f); // Sleek semi-transparent dark space UI

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
        titleText.fontSize = 90;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.supportRichText = true;
        titleText.text = "<b><color=#7bb3ff>Political</color> Galaxy</b>";

        UnityEngine.UI.Outline titleOutline = titleObj.AddComponent<UnityEngine.UI.Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.5f);
        titleOutline.effectDistance = new Vector2(2, -2);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.sizeDelta = new Vector2(1500, 200);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Mission Text
        GameObject missionObj = new GameObject("MissionText");
        missionObj.transform.SetParent(panelObj.transform, false);
        Text missionText = missionObj.AddComponent<Text>();
        missionText.font = font;
        missionText.color = new Color(0.9f, 0.9f, 0.95f, 1f);
        missionText.fontSize = 32;
        missionText.alignment = TextAnchor.UpperCenter;
        missionText.supportRichText = true;
        missionText.text = "This interactive experience explores how algorithms create <b><color=#ff7b7b>echo chambers</color></b> and drive societal polarization.\n\n" + 
                           "Each celestial body in this space embodies a specific extreme ideology,\npulling you into its ideological gravity well. " +
                           "Navigate carefully to uncover these perspectives.";
        missionText.lineSpacing = 1.3f;

        RectTransform missionRect = missionObj.GetComponent<RectTransform>();
        missionRect.anchorMin = new Vector2(0.5f, 0.55f);
        missionRect.anchorMax = new Vector2(0.5f, 0.55f);
        missionRect.sizeDelta = new Vector2(1600, 300);
        missionRect.anchoredPosition = Vector2.zero;

        // Controls Text
        GameObject controlsObj = new GameObject("ControlsText");
        controlsObj.transform.SetParent(panelObj.transform, false);
        Text controlsText = controlsObj.AddComponent<Text>();
        controlsText.font = font;
        controlsText.color = new Color(0.7f, 0.8f, 0.9f, 1f); // slightly blueish gray
        controlsText.fontSize = 28;
        controlsText.alignment = TextAnchor.MiddleCenter;
        controlsText.supportRichText = true;
        controlsText.text = "<b>[ W A S D ]</b> Move Ship   |   <b>[ Mouse ]</b> Look Around   |   <b>[ Shift ]</b> Boost Speed   |   <b>[ E ] / [ Esc ]</b> Exit Orbit";
        
        RectTransform controlsRect = controlsObj.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0.35f);
        controlsRect.anchorMax = new Vector2(0.5f, 0.35f);
        controlsRect.sizeDelta = new Vector2(1500, 100);
        controlsRect.anchoredPosition = Vector2.zero;

        // Start Button Object
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.1f, 0.25f, 0.45f, 0.9f); // Sleek darker blue
        UnityEngine.UI.Outline btnOutline = btnObj.AddComponent<UnityEngine.UI.Outline>();
        btnOutline.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f); // glowing outline
        btnOutline.effectDistance = new Vector2(1, -1);
        
        Button btn = btnObj.AddComponent<Button>();
        
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.15f);
        btnRect.anchorMax = new Vector2(0.5f, 0.15f);
        btnRect.sizeDelta = new Vector2(350, 80);
        btnRect.anchoredPosition = Vector2.zero;

        // Start Button Text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = font;
        btnText.text = "<b>COMMENCE JOURNEY</b>";
        btnText.color = new Color(0.9f, 0.95f, 1f, 1f);
        btnText.fontSize = 28;
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
