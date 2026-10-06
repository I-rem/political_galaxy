using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TweetUIManager : MonoBehaviour
{
    public static TweetUIManager Instance;
    private GameObject sidebarPanel;
    private Text keywordTitleText;
    private Transform contentTransform;
    private Font defaultFont;

    void Awake()
    {
        Instance = this;
        InitializeUI();
    }

    void InitializeUI()
    {
        GameObject canvasObj = new GameObject("TweetUICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        // VR için WorldSpace
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        // VR lazer etkileşimi
        canvasObj.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();

        CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        sidebarPanel = new GameObject("SidebarPanel");
        sidebarPanel.transform.SetParent(canvasObj.transform, false);
        Image bg = sidebarPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);
        
        RectTransform rt = sidebarPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(450, 0);
        rt.anchoredPosition = Vector2.zero;

        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(sidebarPanel.transform, false);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(CloseSidebar);

        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1);
        closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.sizeDelta = new Vector2(40, 40);
        closeRt.anchoredPosition = new Vector2(-10, -10);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.font = defaultFont;
        closeText.text = "X";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        closeText.fontSize = 24;
        RectTransform ctRt = closeTextObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero; ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero; ctRt.anchoredPosition = Vector2.zero;

        GameObject titleObj = new GameObject("KeywordTitle");
        titleObj.transform.SetParent(sidebarPanel.transform, false);
        keywordTitleText = titleObj.AddComponent<Text>();
        keywordTitleText.font = defaultFont;
        keywordTitleText.fontSize = 32;
        keywordTitleText.color = new Color(0.4f, 0.7f, 1f, 1f);
        keywordTitleText.alignment = TextAnchor.UpperLeft;
        
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 1);
        titleRt.sizeDelta = new Vector2(-70, 50);
        titleRt.anchoredPosition = new Vector2(20, -15);

        GameObject scrollViewObj = new GameObject("Scroll View");
        scrollViewObj.transform.SetParent(sidebarPanel.transform, false);
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollViewObj.AddComponent<Image>().color = new Color(0,0,0,0.2f);

        RectTransform svRt = scrollViewObj.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0);
        svRt.anchorMax = new Vector2(1, 1);
        svRt.pivot = new Vector2(0.5f, 0.5f);
        svRt.offsetMin = new Vector2(10, 10);
        svRt.offsetMax = new Vector2(-10, -70);

        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        viewportObj.AddComponent<Image>().color = Color.clear;
        viewportObj.AddComponent<Mask>().showMaskGraphic = false;
        RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero; vpRt.anchoredPosition = Vector2.zero;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        contentTransform = contentObj.transform;
        
        RectTransform cRt = contentObj.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1);
        cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(0, 1);
        cRt.sizeDelta = new Vector2(0, 0);
        cRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 15;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRt;
        scrollRect.content = cRt;

        sidebarPanel.SetActive(false);
    }

    public void ShowKeywordTweets(string keyword, List<string> tweets)
    {
        // Güçlü bir kontrol: Nesne yok edilmişse veya native tarafı uçmuşsa InitializeUI çağır
        bool needsInit = false;
        if (sidebarPanel == null || contentTransform == null)
        {
            needsInit = true;
        }
        else
        {
            try 
            { 
                // Native C++ nesnesinin yaşayıp yaşamadığını test et
                int testCount = contentTransform.childCount; 
                string testName = sidebarPanel.name;
            } 
            catch (System.Exception) 
            { 
                needsInit = true; 
            }
        }

        if (needsInit)
        {
            InitializeUI();
        }

        sidebarPanel.SetActive(true);
        keywordTitleText.text = "#" + keyword.ToUpper();

        // VR'da okunabilmesi için kullanıcının hemen önüne konumlandır
        if (Camera.main != null)
        {
            GameObject canvasObj = sidebarPanel.transform.parent.gameObject;
            canvasObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            canvasObj.transform.rotation = Camera.main.transform.rotation;
            canvasObj.transform.localScale = Vector3.one * 0.002f;
        }

        // Eski objeleri temizle
        if (contentTransform != null)
        {
            for (int i = contentTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(contentTransform.GetChild(i).gameObject);
            }
        }

        List<string> selectedTweets = new List<string>();
        if (tweets.Count <= 10)
        {
            selectedTweets.AddRange(tweets);
        }
        else
        {
            List<string> copy = new List<string>(tweets);
            for(int i = 0; i < 10; i++)
            {
                int r = Random.Range(0, copy.Count);
                selectedTweets.Add(copy[r]);
                copy.RemoveAt(r);
            }
        }

        foreach (string t in selectedTweets)
        {
            CreateTweetElement(t);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
    }

    void CreateTweetElement(string text)
    {
        GameObject item = new GameObject("TweetItem");
        item.transform.SetParent(contentTransform, false);
        
        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.15f, 0.22f, 1f);

        VerticalLayoutGroup vlg = item.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(item.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.font = defaultFont;
        t.color = new Color(0.9f, 0.9f, 0.95f, 1f);
        t.fontSize = 16;
        t.lineSpacing = 1.2f;
        t.text = text;

        ContentSizeFitter tCsf = textObj.AddComponent<ContentSizeFitter>();
        tCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ContentSizeFitter iCsf = item.AddComponent<ContentSizeFitter>();
        iCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void CloseSidebar()
    {
        sidebarPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayUIClick();
    }
    
    public bool IsActive()
    {
        return sidebarPanel != null && sidebarPanel.activeSelf;
    }
}
