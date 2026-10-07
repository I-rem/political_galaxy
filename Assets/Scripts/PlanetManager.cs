using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance;
    public DataLoader dataLoader;

    private float minTweetCount = float.MaxValue;
    private float maxTweetCount = 0f;
    private float minPlanetScale = 20f;
    private float maxPlanetScale = 250f;
    private float minOrbitScale = 80f; 
    private float maxOrbitScale = 250f;

    // Checklist
    private Dictionary<string, Text> checklistTexts = new Dictionary<string, Text>();
    private int visitedCount = 0;
    private GameObject gotoPortalTextObj;
    private Text bridgeChecklistText;
    private GameObject bridgeChecklistItemObj;

    // Portal
    private List<GameObject> allPolarizingObjects = new List<GameObject>();
    private bool portalSpawned = false;

    void Awake() 
    { 
        Instance = this; 
        
        // Musk (X) Algoritma Değişimi yöneticisini otomatik ekle
        if (GetComponent<MuskTransitionManager>() == null)
            gameObject.AddComponent<MuskTransitionManager>();

        // Sahnedeki (Kullanıcının eklediği) eski tip EventSystem'leri otomatik düzelt
        var oldModules = Resources.FindObjectsOfTypeAll<UnityEngine.EventSystems.StandaloneInputModule>();
        foreach (var module in oldModules)
        {
            if (module.gameObject.scene.IsValid()) // Sadece sahnedekiler
            {
                GameObject go = module.gameObject;
                DestroyImmediate(module);
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        StartCoroutine(DisableLeftLaserRoutine());
    }

    System.Collections.IEnumerator DisableLeftLaserRoutine()
    {
        yield return new WaitForSeconds(1f); // XR Rig'in tam yüklenmesini bekle
        var allRays = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        foreach (var ray in allRays)
        {
            if (ray.gameObject.name.ToLower().Contains("left"))
            {
                ray.enabled = false;
                var visual = ray.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
                if (visual != null) visual.enabled = false;
            }
        }
    }

    void Start()
    {
        // Eski versiyonlardan kalma WallPanel objelerini temizle
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
            if (obj.name == "WallPanel" || obj.name == "PortalWall") 
                Destroy(obj);

        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 6000f;
        }

        // Kokpit ve Rüzgar sistemini sahneye otomatik ekle (eğer yoksa)
        if (FindObjectOfType<GravityWindEffect>() == null)
        {
            GameObject windObj = new GameObject("GravityWindEffect_Auto");
            windObj.AddComponent<GravityWindEffect>();
        }

        dataLoader.LoadData();
        CalculateBounds();
        GenerateStars();
        GeneratePlanets();
        CreateChecklistUI();
    }


#if UNITY_EDITOR
    [ContextMenu("Spawn Mission Window into Scene (Click Me)")]
    public void SpawnMissionWindowIntoScene()
    {
        // Sahneyi kirletmemek için varsa önce eskisini siliyoruz
        GameObject existing = GameObject.Find("ChecklistCanvas");
        if (existing != null) DestroyImmediate(existing);

        // UI'yi oluşturalım
        CreateChecklistUI_Internal(false);
        
        GameObject newCanvas = GameObject.Find("ChecklistCanvas");
        if (newCanvas != null)
        {
            // Wrist'ten çıkarıp root'a alalım ki kolayca editleyebilsin
            newCanvas.transform.SetParent(null);
            newCanvas.transform.position = new Vector3(0, 1.5f, 2f);
            newCanvas.transform.localScale = Vector3.one * 0.002f;
            Debug.Log("Görev penceresi sahneye eklendi! Hiyerarşiden 'ChecklistCanvas' objesini bulup düzenleyebilirsiniz.");
        }
    }
#endif

    void CreateChecklistUI()
    {
        // Eğer sahnede hazır bir ChecklistCanvas varsa onu kullan!
        GameObject existingCanvas = GameObject.Find("ChecklistCanvas");
        if (existingCanvas != null)
        {
            // Zaten sahnede (kullanıcı editlemiş), sadece VR eline takalım
            Transform wristTransform = CreateDummyVRWrist();
            if (wristTransform != null)
            {
                existingCanvas.transform.SetParent(wristTransform, false);
                existingCanvas.transform.localPosition = Vector3.zero;
                existingCanvas.transform.localRotation = Quaternion.identity;
                // Boyutu BURADA DA zorla küçültelim ki devasa kalmasın (iPad gibi olmasın, Telefon gibi olsun = 0.0002f)
                existingCanvas.transform.localScale = Vector3.one * 0.00035f;
            }

            // Dinamik objeleri bulup referanslayalım ki hata vermesin
            Transform bgTransform = existingCanvas.transform.Find("PhoneFrame/ChecklistBG");
            if (bgTransform == null) bgTransform = existingCanvas.transform.Find("ChecklistBG"); // Eski yapıdaysa

            if (bgTransform != null)
            {
                GameObject bgObj = bgTransform.gameObject;
                Transform portalObj = bgObj.transform.Find("PortalText");
                if (portalObj != null) gotoPortalTextObj = portalObj.gameObject;

                Transform bridgeObj = bgObj.transform.Find("BridgeItem");
                if (bridgeObj != null) 
                {
                    bridgeChecklistItemObj = bridgeObj.gameObject;
                    bridgeChecklistText = bridgeChecklistItemObj.GetComponent<UnityEngine.UI.Text>();
                }

                // Eski checklistText'leri temizle (varsa)
                foreach (Transform child in bgObj.transform)
                {
                    if (child.name == "ChecklistText") Destroy(child.gameObject);
                }

                // Listeyi Doldur
                Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");

                foreach (string category in dataLoader.PolarizingViews.Keys)
                {
                    if (category == "Bridge Planet") continue;

                    GameObject catObj = new GameObject("ChecklistText");
                    catObj.transform.SetParent(bgObj.transform, false);
                    catObj.transform.SetSiblingIndex(bgObj.transform.childCount - 3); 

                    UnityEngine.UI.Text catText = catObj.AddComponent<UnityEngine.UI.Text>();
                    catText.font     = f;
                    catText.text     = category + " ( )";
                    catText.color    = new Color(0.8f, 0.8f, 0.85f, 1f);
                    catText.fontSize = 26; // Reduced to fit 8 items
                    checklistTexts.Add(category, catText);

                    UnityEngine.UI.ContentSizeFitter tcsf = catObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    tcsf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                }
            }

            // Start butonuna tetik ekleyelim
            GameObject startBtnObj = GameObject.Find("StartButton");
            if (startBtnObj != null)
            {
                UnityEngine.UI.Button startBtn = startBtnObj.GetComponent<UnityEngine.UI.Button>();
                if (startBtn != null)
                {
                    startBtn.onClick.RemoveAllListeners();
                    startBtn.onClick.AddListener(() => {
                        Destroy(startBtnObj);
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        IntroManager[] intros = Resources.FindObjectsOfTypeAll<IntroManager>();
                        foreach(var intro in intros) if (intro.gameObject.scene.IsValid()) intro.StartGame();
                        VRFlightController[] vrFlights = Resources.FindObjectsOfTypeAll<VRFlightController>();
                        foreach (var vrf in vrFlights) if (vrf.gameObject.scene.IsValid()) vrf.isGameStarted = true;
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
                    });
                }
            }

            return; // Kodu bitir, baştan yaratma!
        }

        CreateChecklistUI_Internal(true);
    }

    void CreateChecklistUI_Internal(bool attachToWrist)
    {
        // 1. EVENT SYSTEM KONTROLÜ (VR'da UI'a tıklayabilmek için şart)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ChecklistCanvas ana obje
        GameObject canvasObj = new GameObject("ChecklistCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        
        // 1. VR için WorldSpace'e geçiriyoruz
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        
        // Beyaz kanvas sınırını kaldırmak için RectTransform boyutunu sıfırlayalım
        RectTransform cRect = canvasObj.GetComponent<RectTransform>();
        cRect.sizeDelta = Vector2.zero;

        // 2. Siyah Silüet VR Kolu / Bileği (Kamera altında simüle edelim)
        if (attachToWrist)
        {
            Transform wristTransform = CreateDummyVRWrist();
            if (wristTransform != null)
            {
                canvasObj.transform.SetParent(wristTransform, false);
                canvasObj.transform.localScale = Vector3.one * 0.00035f; // Gerçek telefon boyutunda (14-15 cm)
                canvasObj.transform.localPosition = Vector3.zero; 
                canvasObj.transform.localRotation = Quaternion.identity; 
            }
        }

        // TELEFON ÇERÇEVESİ
        GameObject phoneObj = new GameObject("PhoneFrame");
        phoneObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image phoneImg = phoneObj.AddComponent<UnityEngine.UI.Image>();
        
        // Önce Sprite olarak yüklemeyi dene (Unity sprite olarak çevirmişse)
        Sprite phoneSprite = Resources.Load<Sprite>("UI/phone");
        if (phoneSprite != null)
        {
            phoneImg.sprite = phoneSprite;
        }
        else 
        {
            // Texture2D olarak yüklemeyi dene
            Texture2D phoneTex = Resources.Load<Texture2D>("UI/phone");
            if (phoneTex != null)
            {
                phoneImg.sprite = Sprite.Create(phoneTex, new Rect(0, 0, phoneTex.width, phoneTex.height), new Vector2(0.5f, 0.5f));
            }
            else 
            {
                phoneImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Siyah çerçeve (Fallback)
            }
        }
        phoneImg.type = UnityEngine.UI.Image.Type.Simple;
        phoneImg.preserveAspect = true;
        
        RectTransform phoneRect = phoneObj.GetComponent<RectTransform>();
        phoneRect.anchorMin = new Vector2(0.5f, 0.5f);
        phoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        phoneRect.sizeDelta = new Vector2(380, 750); // Telefonun dış boyutları

        // GÖREV LİSTESİ ARKA PLANI (Telefonun içine oturacak)
        GameObject bgObj = new GameObject("ChecklistBG");
        bgObj.transform.SetParent(phoneObj.transform, false);
        UnityEngine.UI.Image bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.15f, 0.25f, 0.9f); // Hologram mavimsi

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(340, 0); // Genişliği sabitle ki yazılar taşmasın, alt satıra geçsin

        UnityEngine.UI.VerticalLayoutGroup vlg = bgObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 8; // Yazılar alt satıra geçince birbirine girmemesi için boşluğu artırdık
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperCenter;

        UnityEngine.UI.ContentSizeFitter csf = bgObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained; // Genişliği biz veriyoruz
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // --- START BUTONU (Lazerle Tıklama Sorunlarına Karşı Tetik Uyarısına Çevrildi) ---
        GameObject startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(bgObj.transform, false);
        Image startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0.8f, 0.2f, 0.1f, 0.9f); // Kırmızımsı uyarı rengi
        
        Button startBtn = startBtnObj.AddComponent<Button>();
        // Beyaz "seçim" (selection) dikdörtgenlerini engellemek için navigasyonu kapatıyoruz!
        UnityEngine.UI.Navigation nav = new UnityEngine.UI.Navigation();
        nav.mode = UnityEngine.UI.Navigation.Mode.None;
        startBtn.navigation = nav;
        
        LayoutElement le = startBtnObj.AddComponent<LayoutElement>();
        le.minHeight = 35f;
        
        GameObject startBtnTextObj = new GameObject("Text");
        startBtnTextObj.transform.SetParent(startBtnObj.transform, false);
        Text startBtnText = startBtnTextObj.AddComponent<Text>();
        startBtnText.font = f;
        startBtnText.text = "<b>BAŞLAMAK İÇİN SAĞ TETİĞİ ÇEKİN</b>";
        startBtnText.color = Color.white;
        startBtnText.fontSize = 26; // Telefonda okunsun diye büyütüldü
        startBtnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform startBtnTextRect = startBtnTextObj.GetComponent<RectTransform>();
        startBtnTextRect.anchorMin = Vector2.zero; startBtnTextRect.anchorMax = Vector2.one;
        startBtnTextRect.sizeDelta = Vector2.zero; startBtnTextRect.anchoredPosition = Vector2.zero;
        // ----------------------------------------------

        // Başlık
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(bgObj.transform, false);
        UnityEngine.UI.Text titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleText.font = f;
        titleText.text      = "<b><size=28>Güneş Sistemlerini Keşfet</size></b>";
        titleText.color      = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        UnityEngine.UI.ContentSizeFitter tsf = titleObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        tsf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // Separator line
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(bgObj.transform, false);
        UnityEngine.UI.Image lineImg = lineObj.AddComponent<UnityEngine.UI.Image>();
        lineImg.color = new Color(1, 1, 1, 0.3f);
        UnityEngine.UI.LayoutElement lineLe = lineObj.AddComponent<UnityEngine.UI.LayoutElement>();
        lineLe.minHeight = 4;

        // Populate checklist (skip Bridge Planet — it has its own dedicated row)
        foreach (string category in dataLoader.PolarizingViews.Keys)
        {
            if (category == "Bridge Planet") continue;

            GameObject catObj = new GameObject("ChecklistText");
            catObj.transform.SetParent(bgObj.transform, false);
            UnityEngine.UI.Text catText = catObj.AddComponent<UnityEngine.UI.Text>();
            catText.font     = f;
            catText.text     = category + " ( )";
            catText.color    = new Color(0.8f, 0.8f, 0.85f, 1f);
            catText.fontSize = 20; // Reduced to fit 8 items
            checklistTexts.Add(category, catText);

            UnityEngine.UI.ContentSizeFitter tcsf = catObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            tcsf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        }

        // Bridge Item
        bridgeChecklistItemObj = new GameObject("BridgeItem");
        bridgeChecklistItemObj.transform.SetParent(bgObj.transform, false);
        bridgeChecklistText = bridgeChecklistItemObj.AddComponent<UnityEngine.UI.Text>();
        bridgeChecklistText.font = f;
        bridgeChecklistText.text = "Köprü Gezegeni Bul ( )";
        bridgeChecklistText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Başlangıçta pasif gri
        bridgeChecklistText.fontSize = 23; // Font daha da küçültüldü
        UnityEngine.UI.ContentSizeFitter bcsf = bridgeChecklistItemObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        bcsf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        // Portal Text
        gotoPortalTextObj = new GameObject("PortalText");
        gotoPortalTextObj.transform.SetParent(bgObj.transform, false);
        UnityEngine.UI.Text portalText = gotoPortalTextObj.AddComponent<UnityEngine.UI.Text>();
        portalText.font = f;
        portalText.text = "<b><color=#00ff00>PORTAL AÇILDI!\nÇekirdeğe ilerle.</color></b>";
        portalText.color = Color.white;
        portalText.fontSize = 34;
        portalText.alignment = TextAnchor.MiddleCenter;
        gotoPortalTextObj.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        gotoPortalTextObj.SetActive(false);

        bridgeChecklistItemObj.SetActive(false);
    }

    void MakeCLText(GameObject parent, Font f, string content, int size)
    {
        GameObject obj = new GameObject("CLText");
        obj.transform.SetParent(parent.transform, false);
        Text t = obj.AddComponent<Text>();
        t.font = f; t.text = content; t.fontSize = size;
        t.color = Color.white; t.alignment = TextAnchor.UpperLeft;
        obj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void MarkPlanetVisited(string categoryName)
    {
        if (checklistTexts.ContainsKey(categoryName) && !checklistTexts[categoryName].text.Contains("(O)"))
        {
            checklistTexts[categoryName].text = "<color=#00ccff><b>" + categoryName + "</b> (O)</color>";
            visitedCount++;
            
            if (visitedCount >= checklistTexts.Count)
            {
                if (gotoPortalTextObj != null) gotoPortalTextObj.SetActive(true);
                if (!portalSpawned) { portalSpawned = true; SpawnPortalAndBridgePlanet(); }
            }
        }
    }

    public void ShowBridgeChecklist()
    {
        if (bridgeChecklistItemObj != null) bridgeChecklistItemObj.SetActive(true);
    }

    public void MarkBridgeVisited()
    {
        if (bridgeChecklistText != null && !bridgeChecklistText.text.Contains("(O)"))
            bridgeChecklistText.text = "<color=#00ccff><b>Köprüyü keşfet</b> (O)</color>";
    }

    // ─────────────────────────────────────────────
    // TELEFON (BİLEKLİK) İÇİN BİLGİ EKRANI SİSTEMİ
    // ─────────────────────────────────────────────
    public void ShowPlanetInfoOnPhone(string title, string explanation, string keywords, string mass)
    {
        GameObject canvas = GameObject.Find("ChecklistCanvas");
        if (canvas == null) return;
        Transform phoneFrame = canvas.transform.Find("PhoneFrame");
        if (phoneFrame == null) return;

        Transform clBg = phoneFrame.Find("ChecklistBG");
        Transform inBg = phoneFrame.Find("InfoBG");

        if (inBg == null)
        {
            GameObject infoObj = new GameObject("InfoBG");
            infoObj.transform.SetParent(phoneFrame, false);
            Image ibgImg = infoObj.AddComponent<Image>();
            ibgImg.color = new Color(0.05f, 0.15f, 0.25f, 0.9f);
            
            RectTransform iRect = infoObj.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.5f, 0.5f);
            iRect.anchorMax = new Vector2(0.5f, 0.5f);
            iRect.sizeDelta = new Vector2(340, 720); // Telefonun içi boyutu

            GameObject iTextObj = new GameObject("InfoText");
            iTextObj.transform.SetParent(infoObj.transform, false);
            Text t = iTextObj.AddComponent<Text>();
            
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.font = f;
            t.fontSize = 20;
            t.color = Color.white;
            t.alignment = TextAnchor.UpperLeft;
            t.supportRichText = true;
            
            RectTransform itRect = iTextObj.GetComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero; itRect.anchorMax = Vector2.one;
            itRect.offsetMin = new Vector2(20, 20); itRect.offsetMax = new Vector2(-20, -20);
            
            inBg = infoObj.transform;
        }

        if (clBg != null) clBg.gameObject.SetActive(false);
        inBg.gameObject.SetActive(true);

        Text infoText = inBg.Find("InfoText").GetComponent<Text>();
        infoText.text = $"<b><size=28><color=#ffffff>{title}</color></size></b>\n\n" +
                        $"<b><color=#00ccff>Açıklama:</color></b>\n{explanation}\n\n" +
                        $"<b><color=#00ccff>Anahtar Kelimeler:</color></b>\n{keywords}\n\n" +
                        $"<b><color=#00ccff>Gravity Mass:</color></b> {mass} Tweets\n\n" +
                        $"<i><color=#aaaaaa>(Fly away to exit)</color></i>";
    }

    public void HidePlanetInfoOnPhone()
    {
        GameObject canvas = GameObject.Find("ChecklistCanvas");
        if (canvas != null) 
        {
            Transform phoneFrame = canvas.transform.Find("PhoneFrame");
            if (phoneFrame != null)
            {
                Transform clBg = phoneFrame.Find("ChecklistBG");
                Transform inBg = phoneFrame.Find("InfoBG");
                if (inBg != null) inBg.gameObject.SetActive(false);
                if (clBg != null) clBg.gameObject.SetActive(true);
            }
        }
    }

    // ─────────────────────────────────────────────
    // VR SIMULATED ARM
    // ─────────────────────────────────────────────
    Transform CreateDummyVRWrist()
    {
        // 1. GERÇEK VR KUMANDASI KONTROLÜ
        // Eğer sahnede XR Origin varsa ve "Left Controller" bulunursa, saati direkt gerçek elinize takar.
        GameObject leftController = GameObject.Find("Left Controller");
        if (leftController != null)
        {
            Transform vrWristPoint = leftController.transform.Find("VRWristPoint");
            if (vrWristPoint == null)
            {
                GameObject offsetObj = new GameObject("VRWristPoint");
                offsetObj.transform.SetParent(leftController.transform, false);
                // Bilek noktası: Kumandanın 15 cm gerisi (kolda) ve biraz üstü
                offsetObj.transform.localPosition = new Vector3(0.0f, 0.05f, -0.1f);
                // Kola saat takmışsınız gibi açısı ayarlandı
                offsetObj.transform.localRotation = Quaternion.Euler(35f, 180f, 180f);
                vrWristPoint = offsetObj.transform;
            }
            return vrWristPoint; // Sanal kol oluşturmadan direkt gerçek eli döndür
        }

        // 2. PC SİMÜLASYONU (VR Kumandası bulunamadıysa)
        if (Camera.main == null) return null;

        // Kol objesi (siyah silüet)
        GameObject armObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        armObj.name = "Simulated_LeftArm";
        var renderer = armObj.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = Color.black;
        }
        armObj.transform.SetParent(Camera.main.transform, false);
        
        // PC'de okunabilmesi için kolu ekrana çok daha yakın bir pozisyona aldık
        armObj.transform.localPosition = new Vector3(-0.35f, -0.3f, 0.6f);
        // Kolu kameraya doğru uzatılmış gibi döndür
        armObj.transform.localRotation = Quaternion.Euler(75f, 10f, 25f);
        // Yakınlaştığı için kolun boyutunu da daralttık
        armObj.transform.localScale = new Vector3(0.12f, 0.4f, 0.1f); 
        
        // Çarpışmaları engelle
        Destroy(armObj.GetComponent<Collider>());
        
        // Siyah silüet materyali (Işıksız)
        Material blackMat = new Material(Shader.Find("Unlit/Color"));
        blackMat.color = new Color(0.01f, 0.01f, 0.01f, 1f); // Tam siyah silüet
        armObj.GetComponent<Renderer>().material = blackMat;

        // Bilek noktası (UI'ın takılacağı yer)
        GameObject wristObj = new GameObject("WristPoint");
        wristObj.transform.SetParent(armObj.transform, false);
        wristObj.transform.localPosition = new Vector3(0f, 0.8f, 0f); // Kapsülün üst ucu (Bilek)
        // Bileği bize bakacak şekilde düzelt
        wristObj.transform.localRotation = Quaternion.identity;

        return wristObj.transform;
    }

    // ─────────────────────────────────────────────
    // PORTAL + BRIDGE
    // ─────────────────────────────────────────────
    void SpawnPortalAndBridgePlanet()
    {
        Vector3 portalPos = new Vector3(0f, 0f, 1300f);
        float ringRadius = 130f;

        // Portal Halkası Partikülleri (görünmez duvar alanında)
        GameObject portalObj = new GameObject("Portal");
        portalObj.transform.position = portalPos;

        ParticleSystem ringPS = portalObj.AddComponent<ParticleSystem>();
        var rm = ringPS.main;
        rm.maxParticles = 4000;
        rm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        rm.startSpeed = new ParticleSystem.MinMaxCurve(0f, 3f);
        rm.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        rm.startColor = new Color(0.5f, 0.9f, 1f, 1f);
        rm.simulationSpace = ParticleSystemSimulationSpace.World;

        var rem = ringPS.emission;
        rem.rateOverTime = 1500f;

        var rs = ringPS.shape;
        rs.shapeType = ParticleSystemShapeType.Circle;
        rs.radius = ringRadius;
        rs.radiusThickness = 0.03f;
        rs.arc = 360f;
        rs.arcMode = ParticleSystemShapeMultiModeValue.Loop;

        portalObj.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        ringPS.Play();

        // İç dolgu aura
        GameObject innerObj = new GameObject("InnerGlow");
        innerObj.transform.SetParent(portalObj.transform, false);
        ParticleSystem iPS = innerObj.AddComponent<ParticleSystem>();
        var im = iPS.main;
        im.maxParticles = 1500; im.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 2f);
        im.startSpeed = 0f; im.startSize = new ParticleSystem.MinMaxCurve(4f, 10f);
        im.startColor = new Color(0.2f, 0.6f, 1f, 0.1f);
        im.simulationSpace = ParticleSystemSimulationSpace.World;
        var iEm = iPS.emission; iEm.rateOverTime = 400f;
        var iS = iPS.shape; iS.shapeType = ParticleSystemShapeType.Circle;
        iS.radius = ringRadius; iS.radiusThickness = 1f;
        innerObj.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        iPS.Play();

        // Portal trigger collider'a artık gerek yok — PortalGate.Update() ile algılanıyor
        // SphereCollider kaldırıldı

        // Bridge gezegeni — başta gizli, portal geçilince PortalGate aktif eder
        GameObject bridgePlanet = SpawnBridgePlanet();

        PortalGate gate = portalObj.AddComponent<PortalGate>();
        gate.PolarizingPlanets = new List<GameObject>(allPolarizingObjects);
        gate.BridgePlanet = bridgePlanet;
    }

    GameObject SpawnBridgePlanet()
    {
        // Portalın (z=1300) hemen arkasında görünsün
        Vector3 bridgePos = new Vector3(0f, 0f, 1900f);
        float bScale = 180f;

        GameObject bridgeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bridgeObj.name = "Planet_TheBridge";
        bridgeObj.transform.position = bridgePos;
        bridgeObj.transform.localScale = Vector3.one * bScale;
        bridgeObj.GetComponent<Renderer>().enabled = false;

        // Siyah Nokta Bulutu (Beyaz arka planda kontrastlı görünsün)
        GameObject cloudObj = new GameObject("BridgeCloud");
        cloudObj.transform.SetParent(bridgeObj.transform, false);
        cloudObj.transform.localPosition = Vector3.zero;
        cloudObj.transform.localScale = Vector3.one * (1f / bScale);

        ParticleSystem ps = cloudObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.maxParticles = 6000;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        // Siyah nokta bulutu
        Color darkColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        main.startColor = darkColor;

        var em = ps.emission;
        em.rateOverTime = 1500f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = bScale / 2f;
        shape.radiusThickness = 0.35f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 22f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.6f;
        noise.octaveCount = 3;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(darkColor, 0f), new GradientColorKey(darkColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        cloudObj.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        ps.Play();

        // Orbit trigger
        float orbitRadius = 350f;
        SphereCollider orbitCol = bridgeObj.AddComponent<SphereCollider>();
        orbitCol.isTrigger = true;
        orbitCol.radius = orbitRadius / bScale;

        // Siyah Keywords (beyaz arka planda okunur)
        List<string> bridgeKeywords = new List<string> {
            "unity", "dialogue", "empathy", "bridge", "common ground",
            "consensus", "understanding", "cooperation", "tolerance", "solidarity",
            "inclusion", "reconciliation", "peace", "harmony", "coexistence",
            "community", "compassion", "respect", "equity", "justice",
            "integration", "moderation", "reform", "progress", "balance"
        };

        for (int k = 0; k < 150; k++)
        {
            string word = bridgeKeywords[Random.Range(0, bridgeKeywords.Count)];
            GameObject wordObj = new GameObject("BridgeKeyword_" + word);
            // BridgeObj'a parent → SetActive ile otomatik gizlenir/gösterilir
            wordObj.transform.SetParent(bridgeObj.transform, false);
            wordObj.transform.position = bridgePos + Random.onUnitSphere * orbitRadius;

            OrbitingKeyword ok = wordObj.AddComponent<OrbitingKeyword>();
            ok.centerPoint = bridgeObj.transform;
            ok.orbitSpeed = Random.Range(2f, 6f);
            // Biraz daha büyük font — bridge dünyasında daha iyi okunur
            ok.SetupText(word, new Color(0.05f, 0.05f, 0.15f, 0.9f), fontSize: 12, characterSize: 0.04f);
        }

        bridgeObj.AddComponent<BridgePlanetInfo>();

        // ORACLE MAKİNESİNİ EKLE (Tweet Prediction Sistemi)
        bridgeObj.AddComponent<OracleMachine>();

        // --- DEV BEYAZ ARKA PLAN (SİLÜETİ GİZLEMEK İÇİN) ---
        // Portal geçildiğinde eski arka planları ve silüetleri gizlemek için 
        // Bridge gezegeninin etrafını saran devasa bir ters-yüzeyli küre oluşturuyoruz.
        GameObject whiteBg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        whiteBg.name = "BridgeWhiteBackground";
        whiteBg.transform.SetParent(bridgeObj.transform, false);
        whiteBg.transform.localPosition = Vector3.zero;
        whiteBg.transform.localScale = Vector3.one * 30f; // Gezegenin 30 katı büyüklüğünde (dev oda)
        Destroy(whiteBg.GetComponent<Collider>()); // Çarpışmayı kaldır
        
        // Yüzeyleri (Normalleri) ters çevir ki içeriden beyaz görünsün
        Mesh mesh = whiteBg.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i + 0];
            triangles[i + 0] = triangles[i + 1];
            triangles[i + 1] = temp;
        }
        mesh.triangles = triangles;
        
        Material wMat = new Material(Shader.Find("Unlit/Color"));
        wMat.color = Color.white;
        whiteBg.GetComponent<Renderer>().material = wMat;
        // ----------------------------------------------------

        // Bridge gezegeni başta gizli
        bridgeObj.SetActive(false);
        return bridgeObj;
    }

    // ─────────────────────────────────────────────
    // YILDIZLAR (sadece polarizing taraf)
    // ─────────────────────────────────────────────
    void GenerateStars()
    {
        GameObject starfield = new GameObject("Starfield");
        starfield.transform.position = Vector3.zero;
        ParticleSystem ps = starfield.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.maxParticles = 5000;
        main.startLifetime = Mathf.Infinity;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor = new Color(1f, 1f, 1f, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 2500) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1000f;
        shape.radiusThickness = 1f;

        // --- YENİ EKLENEN KISIM: Force Field (Güç Alanı) Etkileşimi ---
        var externalForces = ps.externalForces;
        externalForces.enabled = false;
        externalForces.multiplier = 1f;

        starfield.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        
        // Oyuncuya (Kameraya) Force Field Ekle
        if (Camera.main != null)
        {
            ParticleSystemForceField ff = Camera.main.gameObject.GetComponent<ParticleSystemForceField>();
            if (ff == null)
            {
                ff = Camera.main.gameObject.AddComponent<ParticleSystemForceField>();
            }
            ff.shape = ParticleSystemForceFieldShape.Sphere;
            ff.endRange = 300f; // Uzay hızı yüksek olduğu için etki alanını 300 birime çıkardık
            ff.gravity = -5000f; // Gemi çok hızlı geçeceği için parçacıklara verilecek anlık kuvveti devasa yaptık
            ff.drag = 1.5f; // Daha doğal süzülmeleri için sürtünmeyi düşürdük
        }
        // --------------------------------------------------------------

        // Starfield nesnesini polarizing objelere ekle → bridge'e geçince gizlensin
        allPolarizingObjects.Add(starfield);
    }

    void CalculateBounds()
    {
        foreach (var view in dataLoader.PolarizingViews.Values)
        {
            // Exclude the Bridge Planet from sizing calculations — it's not a polarizing planet
            if (view.CategoryName == "Bridge Planet") continue;
            if (view.TweetCount < minTweetCount) minTweetCount = view.TweetCount;
            if (view.TweetCount > maxTweetCount) maxTweetCount = view.TweetCount;
        }
        if (Mathf.Approximately(minTweetCount, maxTweetCount)) maxTweetCount += 1f;
    }

    // ─────────────────────────────────────────────
    // THEMATIC SOLAR SYSTEMS
    // ─────────────────────────────────────────────

    // Defines one thematic solar system (a Sun + which planet categories orbit it)
    private struct SolarSystem
    {
        public string Title;          // Sun label shown in 3D space
        public Vector3 SunPosition;   // World position of the sun
        public Color SunColor;        // Glow colour of the sun
        public string[] PlanetKeys;   // DataLoader category names that orbit this sun
        public float SunScale;        // Radius of the sun sphere
    }

    void GeneratePlanets()
    {
        // ── Define all 4 thematic solar systems ─────────────────────────
        SolarSystem[] systems = new SolarSystem[]
        {
            new SolarSystem {
                Title       = "Racism & Identity",
                SunPosition = new Vector3(-400f, 300f, 650f),
                SunColor    = new Color(0.9f, 0.45f, 0.1f, 1f),   // burnt orange
                SunScale    = 60f,
                PlanetKeys  = new[] { "Ethnonationalism", "Identitarian Left" }
            },
            new SolarSystem {
                Title       = "Sexism & Gender",
                SunPosition = new Vector3(400f, 300f, 650f),
                SunColor    = new Color(0.85f, 0.15f, 0.55f, 1f),  // hot pink
                SunScale    = 55f,
                PlanetKeys  = new[] { "Gender Essentialism Extremism" }
            },
            new SolarSystem {
                Title       = "Economy & Establishment",
                SunPosition = new Vector3(0f, -400f, 650f),
                SunColor    = new Color(0.2f, 0.8f, 0.25f, 1f),   // money green
                SunScale    = 70f,
                PlanetKeys  = new[] { "Progressive Left", "Libertarian Right", "Hyper-Partisan Populism" }
            },
            new SolarSystem {
                Title       = "Dogma & The Apocalypse",
                SunPosition = new Vector3(0f, 700f, 650f),
                SunColor    = new Color(0.5f, 0.1f, 0.9f, 1f),    // deep violet
                SunScale    = 65f,
                PlanetKeys  = new[] { "Religious Extremism", "Eco-Authoritarianism" }
            }
        };

        // Colour palette — each planet key gets a unique colour
        Dictionary<string, Color> planetColors = new Dictionary<string, Color>
        {
            { "Ethnonationalism",            new Color(0.95f, 0.55f, 0.1f,  1f) },
            { "Identitarian Left",           new Color(0.15f, 0.75f, 0.9f,  1f) },
            { "Gender Essentialism Extremism",new Color(0.9f,  0.2f,  0.55f, 1f) },
            { "Progressive Left",            new Color(0.1f,  0.85f, 0.45f, 1f) },
            { "Libertarian Right",           new Color(0.95f, 0.85f, 0.1f,  1f) },
            { "Hyper-Partisan Populism",     new Color(0.95f, 0.25f, 0.1f,  1f) },
            { "Religious Extremism",         new Color(0.75f, 0.35f, 0.95f, 1f) },
            { "Eco-Authoritarianism",        new Color(0.2f,  0.9f,  0.3f,  1f) }
        };

        foreach (SolarSystem sys in systems)
        {
            // ── Build the Sun ──────────────────────────────────────────
            GameObject sunObj = BuildSun(sys.Title, sys.SunPosition, sys.SunColor, sys.SunScale);
            allPolarizingObjects.Add(sunObj);

            // ── Place planets that belong to this system ───────────────
            int slotCount  = sys.PlanetKeys.Length;
            float baseOrbit = 180f + slotCount * 40f; // orbit ring radius from sun

            for (int slot = 0; slot < slotCount; slot++)
            {
                string key = sys.PlanetKeys[slot];
                if (!dataLoader.PolarizingViews.ContainsKey(key)) continue;
                PoliticalViewData viewData = dataLoader.PolarizingViews[key];

                // Spread planets evenly around the sun on the XY plane
                float angle   = (slot / (float)slotCount) * 360f * Mathf.Deg2Rad;
                float orbitR  = baseOrbit + slot * 30f; // stagger slightly
                Vector3 offset = new Vector3(Mathf.Cos(angle) * orbitR,
                                             Mathf.Sin(angle) * orbitR * 0.6f,
                                             Random.Range(-40f, 40f));
                Vector3 position = sys.SunPosition + offset;

                // ── Planet size from tweet count ─────────────────────
                float normalizedScale  = Mathf.InverseLerp(minTweetCount, maxTweetCount, viewData.TweetCount);
                float exponentialScale = Mathf.Pow(normalizedScale, 1.5f);
                float targetScale      = Mathf.Lerp(minPlanetScale, maxPlanetScale, exponentialScale);
                float planetScale      = targetScale * 0.6f;

                Color pColor = planetColors.ContainsKey(key)
                    ? planetColors[key]
                    : new Color(0.9f, 0.1f, 0.1f, 1f);

                // ── Spawn the planet ─────────────────────────────────
                GameObject planetObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planetObj.name = "Planet_" + viewData.CategoryName;
                planetObj.transform.position = position;
                planetObj.transform.localScale = Vector3.one * planetScale;
                planetObj.GetComponent<Renderer>().enabled = false;

                // Particle cloud
                GameObject cloudObj = new GameObject("Cloud");
                cloudObj.transform.SetParent(planetObj.transform, false);
                cloudObj.transform.localScale = Vector3.one * (1f / planetScale);

                ParticleSystem ps   = cloudObj.AddComponent<ParticleSystem>();
                var main            = ps.main;
                main.maxParticles   = 6000;
                main.startLifetime  = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
                main.startSpeed     = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
                main.startSize      = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;  // must follow the orbiting planet
                main.scalingMode    = ParticleSystemScalingMode.Hierarchy;
                main.startColor     = pColor;

                var em              = ps.emission;
                em.rateOverTime     = 1500f;

                var shape           = ps.shape;
                shape.shapeType     = ParticleSystemShapeType.Sphere;
                shape.radius        = planetScale / 2f;
                shape.radiusThickness = 0.35f;

                var noise           = ps.noise;
                noise.enabled       = true;
                noise.strength      = 25f;
                noise.frequency     = 0.3f;
                noise.scrollSpeed   = 0.8f;
                noise.octaveCount   = 3;

                var col             = ps.colorOverLifetime;
                col.enabled         = true;
                Gradient grad       = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(pColor, 0f), new GradientColorKey(pColor, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
                );
                col.color           = grad;

                cloudObj.GetComponent<ParticleSystemRenderer>().material =
                    new Material(Shader.Find("Sprites/Default"));
                ps.Play();

                // Gravity + info
                PlanetGravity pg    = planetObj.AddComponent<PlanetGravity>();
                pg.ViewData         = viewData;
                pg.PlanetColor      = pColor;
                pg.PlanetPS         = ps;
                pg.PlanetRenderer   = planetObj.GetComponent<Renderer>();
                pg.gravityForce     = 70f;

                // Orbit the Sun of this system
                PlanetOrbit orbit   = planetObj.AddComponent<PlanetOrbit>();
                orbit.centerPoint   = sys.SunPosition;
                orbit.orbitSpeed    = Random.Range(10f, 18f);  // degrees/sec — full orbit in ~20-36 s
                orbit.orbitAxis     = new Vector3(
                    Random.Range(-0.12f, 0.12f), 1f, Random.Range(-0.08f, 0.08f)
                ).normalized;
                pg.PlanetOrbit      = orbit;

                // Gravity trigger radius
                float gravOrbitR    = Mathf.Lerp(minOrbitScale, maxOrbitScale, exponentialScale);
                SphereCollider sc   = planetObj.AddComponent<SphereCollider>();
                sc.isTrigger        = true;
                sc.radius           = gravOrbitR / planetScale;

                // Orbiting keywords
                for (int k = 0; k < 200; k++)
                {
                    if (viewData.Keywords.Count == 0) continue;
                    string word = viewData.Keywords[Random.Range(0, viewData.Keywords.Count)];

                    GameObject wordObj = new GameObject("Keyword_" + word);
                    wordObj.transform.SetParent(planetObj.transform, false);   // follow the planet!
                    wordObj.transform.localPosition = Random.onUnitSphere * (gravOrbitR / planetScale);
                    wordObj.transform.localScale = Vector3.one * (1f / planetScale); // cancel parent scale

                    OrbitingKeyword ok = wordObj.AddComponent<OrbitingKeyword>();
                    ok.centerPoint   = planetObj.transform;
                    ok.orbitSpeed    = Random.Range(2f, 8f);
                    ok.SetupText(word, pColor * 0.9f + Color.white * 0.1f);
                    pg.OrbitingKeywords.Add(ok);
                    // wordObj is a child of planetObj — no need to track it separately
                }

                allPolarizingObjects.Add(planetObj);
            }
        }
    }

    // ─────────────────────────────────────────────
    // SUN BUILDER
    // ─────────────────────────────────────────────
    GameObject BuildSun(string title, Vector3 position, Color color, float scale)
    {
        GameObject sunRoot = new GameObject("Sun_" + title);
        sunRoot.transform.position = position;

        // Particle glow core
        ParticleSystem ps   = sunRoot.AddComponent<ParticleSystem>();
        var main            = ps.main;
        main.maxParticles   = 3000;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(1.0f, 2.5f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0f, 2f);
        main.startSize      = new ParticleSystem.MinMaxCurve(scale * 0.04f, scale * 0.12f);
        main.startColor     = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em              = ps.emission;
        em.rateOverTime     = 800f;

        var shape           = ps.shape;
        shape.shapeType     = ParticleSystemShapeType.Sphere;
        shape.radius        = scale * 0.5f;
        shape.radiusThickness = 0.4f;

        var noise           = ps.noise;
        noise.enabled       = true;
        noise.strength      = scale * 0.08f;
        noise.frequency     = 0.5f;
        noise.scrollSpeed   = 0.4f;

        var col             = ps.colorOverLifetime;
        col.enabled         = true;
        Gradient g          = new Gradient();
        Color bright        = color * 1.4f; bright.a = 1f;
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(bright, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.25f), new GradientAlphaKey(0.9f, 0.75f), new GradientAlphaKey(0f, 1f) }
        );
        col.color           = g;
        sunRoot.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        ps.Play();

        // Outer halo ring
        GameObject halo     = new GameObject("SunHalo");
        halo.transform.SetParent(sunRoot.transform, false);
        ParticleSystem hps  = halo.AddComponent<ParticleSystem>();
        var hm              = hps.main;
        hm.maxParticles     = 600;
        hm.startLifetime    = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        hm.startSpeed       = new ParticleSystem.MinMaxCurve(0f, 1f);
        hm.startSize        = new ParticleSystem.MinMaxCurve(scale * 0.08f, scale * 0.18f);
        Color haloColor     = color; haloColor.a = 0.3f;
        hm.startColor       = haloColor;
        hm.simulationSpace  = ParticleSystemSimulationSpace.World;
        var hem             = hps.emission;
        hem.rateOverTime    = 200f;
        var hs              = hps.shape;
        hs.shapeType        = ParticleSystemShapeType.Circle;
        hs.radius           = scale * 0.8f;
        hs.radiusThickness  = 0.08f;
        halo.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        hps.Play();

        // Floating title label above the sun
        GameObject labelObj = new GameObject("SunLabel");
        labelObj.transform.SetParent(sunRoot.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, scale * 0.9f, 0f);
        TextMesh tm         = labelObj.AddComponent<TextMesh>();
        tm.text             = title;
        tm.color            = color * 1.5f; tm.color = new Color(tm.color.r, tm.color.g, tm.color.b, 1f);
        tm.fontSize         = 400;
        tm.characterSize    = 0.1f;
        tm.anchor           = TextAnchor.MiddleCenter;
        tm.fontStyle        = FontStyle.Bold;
        Font f              = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f    = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tm.font             = f;
        tm.GetComponent<Renderer>().material = tm.font.material;

        // Billboard the label toward camera every frame
        labelObj.AddComponent<SunLabelBillboard>();

        return sunRoot;
    }
}
