using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KeywordUIManager : MonoBehaviour
{
    public static KeywordUIManager Instance;

    private GameObject canvasObj;
    private GameObject bgObj;
    private Text titleText;
    private Transform tweetContainer;
    private List<GameObject> tweetTextObjs = new List<GameObject>();
    private Button closeBtnComp;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        CreateUI();
        canvasObj.SetActive(false);
    }

    private bool prevTrig = false;

    void Update()
    {
        if (canvasObj != null && canvasObj.activeSelf)
        {
            bool wantsToClose = false;
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                wantsToClose = true;
            }

            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                var rh = new List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, rh);
                if (rh.Count > 0)
                {
                    if (rh[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool trigger))
                    {
                         if (trigger && !prevTrig) wantsToClose = true;
                         prevTrig = trigger;
                    }
                    if (rh[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool pb) && pb) wantsToClose = true;
                }
            }

            if (wantsToClose) CloseUI();
        }
    }

    void CreateUI()
    {
        // Canvas Setup (Overlay on top of everything)
        canvasObj = new GameObject("KeywordUICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
            {
                canvasObj.transform.SetParent(Camera.main.transform, false);
                canvasObj.transform.localPosition = new Vector3(0, 0, 2f); // Center
                canvasObj.transform.localRotation = Quaternion.identity;
                canvasObj.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            }
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Disable initially so it doesn't block rays
        canvasObj.SetActive(false);

        // Dark Overlay background
        GameObject overlayObj = new GameObject("DarkOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        // Add invisible button to overlay to close when clicking outside
        Button overlayBtn = overlayObj.AddComponent<Button>();
        overlayBtn.onClick.AddListener(CloseUI);

        // Main Panel (Window)
        bgObj = new GameObject("KeywordPanel");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(1300, 950);

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(bgObj.transform, false);
        titleText = titleObj.AddComponent<Text>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.font = font;
        titleText.fontSize = 48;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(-40, 50); // -40 for left/right padding

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(bgObj.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        closeBtnComp = closeBtnObj.AddComponent<Button>();
        closeBtnComp.onClick.AddListener(CloseUI);
        
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        closeRect.sizeDelta = new Vector2(40, 40);

        GameObject closeTextObj = new GameObject("X");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeTxt = closeTextObj.AddComponent<Text>();
        closeTxt.font = font;
        closeTxt.text = "X";
        closeTxt.fontSize = 24;
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform cTxtRect = closeTextObj.GetComponent<RectTransform>();
        cTxtRect.anchorMin = Vector2.zero; cTxtRect.anchorMax = Vector2.one; 
        cTxtRect.sizeDelta = Vector2.zero;

        // Scrollable Area Setup
        GameObject scrollObj = new GameObject("Scroll View");
        scrollObj.transform.SetParent(bgObj.transform, false);
        RectTransform scrollRectTransform = scrollObj.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.offsetMin = new Vector2(20, 20); // Left, Bottom padding
        scrollRectTransform.offsetMax = new Vector2(-20, -80); // Right, Top padding (leave space for title)

        ScrollRect scrollRectComp = scrollObj.AddComponent<ScrollRect>();
        scrollRectComp.horizontal = false;
        scrollRectComp.vertical = true;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.pivot = new Vector2(0, 1);
        Image viewportImg = viewportObj.AddComponent<Image>();
        viewportImg.color = new Color(1f, 1f, 1f, 1f); // White solid color for safe masking
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRectComp.viewport = viewportRect;

        // Content (Tweet Container)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0); // Height will be driven by ContentSizeFitter

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 15;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRectComp.content = contentRect;
        tweetContainer = contentObj.transform;

        // Pre-create 10 text objects
        for (int i = 0; i < 10; i++)
        {
            GameObject tweetObj = new GameObject("TweetItem" + i);
            tweetObj.transform.SetParent(tweetContainer, false);
            
            // Add background to tweet item
            Image itemBg = tweetObj.AddComponent<Image>();
            itemBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Add padding to the layout of the text inside the item
            VerticalLayoutGroup itemVlg = tweetObj.AddComponent<VerticalLayoutGroup>();
            itemVlg.padding = new RectOffset(15, 15, 15, 15);
            itemVlg.childAlignment = TextAnchor.UpperLeft;
            itemVlg.childControlHeight = true;
            itemVlg.childControlWidth = true;
            itemVlg.childForceExpandHeight = false;
            
            // The actual Text component is a child
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(tweetObj.transform, false);
            Text t = tObj.AddComponent<Text>();
            t.font = font;
            t.fontSize = 28;
            t.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            t.supportRichText = true;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            
            tObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            tweetTextObjs.Add(tObj);
            tweetObj.SetActive(false); // Hide until used
        }
    }

    public void ShowKeywordTweets(string keyword, List<string> allTweets, int totalFrequency)
    {
        List<string> validTweets = new List<string>();
        if (allTweets != null)
        {
            foreach (string t in allTweets)
            {
                if (!string.IsNullOrWhiteSpace(t))
                {
                    validTweets.Add(t);
                }
            }
        }

        if (validTweets.Count == 0)
        {
            string[] names = { "Alex", "Jordan", "Taylor", "Sam", "Casey", "Riley", "Morgan", "Avery", "Parker", "Quinn" };
            string[] openers = { "Just saw the news.", "Can't believe this.", "This is wild.", "Thoughts on this?", "Hot take:", "Finally someone said it.", "I'm exhausted." };
            string[] cores = { $"Honestly, the discourse around <color=#ffaaaa>{keyword}</color> is missing the point.", $"If you look closely at <color=#ffaaaa>{keyword}</color>, you'll see why people are mad.", $"Nobody is addressing <color=#ffaaaa>{keyword}</color> seriously.", $"Why does <color=#ffaaaa>{keyword}</color> keep breaking the timeline?", $"The implications of <color=#ffaaaa>{keyword}</color> are massive for our future.", $"Can we take a moment to discuss <color=#ffaaaa>{keyword}</color> seriously without bias?", $"<color=#ffaaaa>{keyword}</color> is literally trending right now.", $"I've completely changed my mind on <color=#ffaaaa>{keyword}</color>." };
            
            for (int k = 0; k < 12; k++)
            {
                string user = names[Random.Range(0, names.Length)] + Random.Range(10, 9999);
                string post = openers[Random.Range(0, openers.Length)] + " " + cores[Random.Range(0, cores.Length)];
                validTweets.Add($"<b><color=#7bb3ff>@{user}</color></b>  <color=#777777><i>• Oct {Random.Range(1, 31)}, 2020</i></color>\n{post}");
            }
        }

        titleText.text = "Keyword: <b><color=#00ccff>" + keyword + "</color></b> (Used " + totalFrequency + " times)";

        // Select 10 random logic
        List<string> selectedTweets = new List<string>();

        if (validTweets.Count <= 10)
        {
            selectedTweets.AddRange(validTweets);
        }
        else
        {
            // Pick 10 random without replacement
            List<string> unpicked = new List<string>(validTweets);
            for (int i = 0; i < 10; i++)
            {
                int r = Random.Range(0, unpicked.Count);
                selectedTweets.Add(unpicked[r]);
                unpicked.RemoveAt(r);
            }
        }

        // Format all real tweets with authentic usernames and dates to adhere to UI layout 
        string[] realNames = { "TruthSeeker", "PoltX", "VoiceOfReason", "EagleEye", "CitizenWatch", "EchoBurst", "AmericanVoter", "LibertyFirst" };
        for (int i = 0; i < selectedTweets.Count; i++)
        {
            if (!selectedTweets[i].Contains("<color=#7bb3ff>@"))
            {
                string rUser = realNames[Random.Range(0, realNames.Length)] + Random.Range(10, 9999);
                selectedTweets[i] = $"<b><color=#7bb3ff>@{rUser}</color></b>  <color=#777777><i>• Oct {Random.Range(1, 31)}, 2020</i></color>\n{selectedTweets[i]}";
            }
        }

        // Reorder Activation and UI population safely!
        // First activate canvas so layout components properly register dirty states
        canvasObj.SetActive(true);

        // Populate texts
        for (int i = 0; i < tweetTextObjs.Count; i++)
        {
            GameObject container = tweetTextObjs[i].transform.parent.gameObject;
            if (i < selectedTweets.Count)
            {
                container.SetActive(true);
                Text t = tweetTextObjs[i].GetComponent<Text>();
                t.text = selectedTweets[i];
            }
            else
            {
                container.SetActive(false);
            }
        }

        // Force layout rebuild in this exact frame before timeScale=0 
        Canvas.ForceUpdateCanvases();
        
        ScrollRect sr = tweetContainer.parent.parent.GetComponent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;

        // Finally pause the physics & movement systems and free the cursor
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseUI()
    {
        canvasObj.SetActive(false);
        Time.timeScale = 1f; // Unpause
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
