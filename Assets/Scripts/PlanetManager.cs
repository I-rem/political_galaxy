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
    private Dictionary<string, GameObject> themeCenters = new Dictionary<string, GameObject>();

    private GameObject GetOrCreateThemeCenter(string themeName, Vector3 position)
    {
        if (themeCenters.ContainsKey(themeName))
            return themeCenters[themeName];

        GameObject themeObj = new GameObject("Theme_" + themeName);
        themeObj.transform.position = position;
        
        GameObject textObj = new GameObject("ThemeText_" + themeName);
        textObj.transform.position = position;
        textObj.transform.SetParent(themeObj.transform);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = themeName;
        tm.color = new Color(1f, 1f, 1f, 0.5f);
        tm.fontSize = 120;
        tm.characterSize = 0.5f;
        tm.anchor = TextAnchor.MiddleCenter;
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tm.font = font;
        tm.GetComponent<Renderer>().material = tm.font.material;

        textObj.AddComponent<BillboardText>();

        ThemeRotator rotator = themeObj.AddComponent<ThemeRotator>();
        rotator.rotationSpeed = Random.Range(15f, 30f) * (Random.value > 0.5f ? 1f : -1f);

        themeCenters.Add(themeName, themeObj);
        allPolarizingObjects.Add(themeObj);
        
        return themeObj;
    }

    void Awake() { Instance = this; }

    void Start()
    {
        // Eski versiyonlardan kalma WallPanel objelerini temizle
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
            if (obj.name == "WallPanel" || obj.name == "PortalWall") 
                Destroy(obj);

        if (Camera.main != null)
        {
            Camera.main.farClipPlane = 6000f;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }

        dataLoader.LoadData();
        CalculateBounds();
        GenerateStars();
        GeneratePlanets();
        CreateChecklistUI();
    }


    // ─────────────────────────────────────────────
    // CHECKLİST UI
    // ─────────────────────────────────────────────
    void CreateChecklistUI()
    {
        GameObject canvasObj = new GameObject("ChecklistCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9;
        canvasObj.AddComponent<CanvasScaler>();

        GameObject bgObj = new GameObject("ChecklistBG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        bgObj.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f, 0.9f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(1f, 1f);
        bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.pivot = new Vector2(1f, 1f);
        bgRect.anchoredPosition = new Vector2(-20f, -20f);

        VerticalLayoutGroup vlg = bgObj.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 5;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter csf = bgObj.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Başlık
        MakeCLText(bgObj, f, "<b><color=#ffffff>Discover the planets</color></b>\n", 10);

        // Her gezegen satırı
        foreach (var category in dataLoader.PolarizingViews.Keys)
        {
            GameObject itemObj = new GameObject("Item_" + category);
            itemObj.transform.SetParent(bgObj.transform, false);
            Text itemText = itemObj.AddComponent<Text>();
            itemText.font = f;
            itemText.text = category + " ( )";
            itemText.color = new Color(0.6f, 0.6f, 0.6f);
            itemText.fontSize = 7;
            itemText.alignment = TextAnchor.MiddleLeft;
            itemObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            checklistTexts[category] = itemText;
        }

        // → GO TO PORTAL
        gotoPortalTextObj = new GameObject("PortalText");
        gotoPortalTextObj.transform.SetParent(bgObj.transform, false);
        Text pText = gotoPortalTextObj.AddComponent<Text>();
        pText.font = f;
        pText.text = "\n<b><color=#ffffff>-> GO TO PORTAL</color></b>";
        pText.fontSize = 9;
        pText.alignment = TextAnchor.MiddleCenter;
        gotoPortalTextObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        gotoPortalTextObj.SetActive(false);

        // Explore the bridge (başta gizli)
        bridgeChecklistItemObj = new GameObject("BridgeItem");
        bridgeChecklistItemObj.transform.SetParent(bgObj.transform, false);
        bridgeChecklistText = bridgeChecklistItemObj.AddComponent<Text>();
        bridgeChecklistText.font = f;
        bridgeChecklistText.text = "Explore the bridge ( )";
        bridgeChecklistText.color = new Color(0.6f, 0.6f, 0.6f);
        bridgeChecklistText.fontSize = 7;
        bridgeChecklistText.alignment = TextAnchor.MiddleLeft;
        bridgeChecklistItemObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
            bridgeChecklistText.text = "<color=#00ccff><b>Explore the bridge</b> (O)</color>";
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

        starfield.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
        
        // Starfield nesnesini polarizing objelere ekle → bridge'e geçince gizlensin
        allPolarizingObjects.Add(starfield);
    }

    void CalculateBounds()
    {
        foreach (var view in dataLoader.PolarizingViews.Values)
        {
            if (view.TweetCount < minTweetCount) minTweetCount = view.TweetCount;
            if (view.TweetCount > maxTweetCount) maxTweetCount = view.TweetCount;
        }
        if (Mathf.Approximately(minTweetCount, maxTweetCount)) maxTweetCount += 1f;
    }

    // ─────────────────────────────────────────────
    // POLARİZİNG GEZEGENLER
    // ─────────────────────────────────────────────
    private Color GetPlanetColor(string category)
    {
        string catLower = category.ToLower();
        if (catLower.Contains("religious")) return new Color(0.8f, 0.1f, 0.8f, 1f); // Purple
        if (catLower.Contains("populism")) return new Color(0.9f, 0.6f, 0.1f, 1f); // Orange
        if (catLower.Contains("gender")) return new Color(0.1f, 0.8f, 0.6f, 1f); // Teal
        if (catLower.Contains("ethno")) return new Color(0.9f, 0.1f, 0.1f, 1f); // Red
        if (catLower.Contains("eco")) return new Color(0.2f, 0.9f, 0.2f, 1f); // Green
        
        // New categories
        if (catLower.Contains("progressive")) return new Color(0.1f, 0.4f, 0.9f, 1f); // Deep Blue
        if (catLower.Contains("identitarian")) return new Color(0.9f, 0.3f, 0.6f, 1f); // Pink/Magenta
        if (catLower.Contains("libertarian")) return new Color(0.9f, 0.9f, 0.1f, 1f); // Yellow
            
        Random.InitState(category.GetHashCode());
        return new Color(Random.Range(0.2f, 0.9f), Random.Range(0.2f, 0.9f), Random.Range(0.2f, 0.9f), 1f);
    }

    void GeneratePlanets()
    {
        int index = 0;

        foreach (var kvp in dataLoader.PolarizingViews)
        {
            PoliticalViewData viewData = kvp.Value;

            string catLower = viewData.CategoryName.ToLower();
            string themeName = "Unknown Theme";
            Vector3 themePosition = Vector3.zero;

            if (catLower.Contains("identitarian") || catLower.Contains("ethno") || catLower.Contains("alt-right") || catLower.Contains("social justice"))
            {
                themeName = "Racism & Identity";
                themePosition = new Vector3(-400f, 300f, 650f);
            }
            else if (catLower.Contains("gender"))
            {
                themeName = "Sexism & Gender";
                themePosition = new Vector3(400f, 300f, 650f);
            }
            else if (catLower.Contains("progressive") || catLower.Contains("libertarian") || catLower.Contains("populism") || catLower.Contains("hyper") || catLower.Contains("free market") || catLower.Contains("democratic socialism"))
            {
                themeName = "The Economy & Establishment";
                themePosition = new Vector3(0f, -400f, 650f);
            }
            else if (catLower.Contains("eco") || catLower.Contains("religious"))
            {
                themeName = "Dogma & The Apocalypse";
                themePosition = new Vector3(0f, 700f, 650f);
            }
            else 
            {
                themeName = "Fringe Outliers";
                themePosition = new Vector3(800f, 0f, 650f);
            }

            GameObject themeCenter = GetOrCreateThemeCenter(themeName, themePosition);

            GameObject planetObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planetObj.name = "Planet_" + viewData.CategoryName;
            
            planetObj.transform.SetParent(themeCenter.transform, false);

            float orbitDistance = Random.Range(200f, 400f);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            planetObj.transform.localPosition = new Vector3(Mathf.Cos(angle) * orbitDistance, Random.Range(-50f, 50f), Mathf.Sin(angle) * orbitDistance);

            // Get absolute position to use for keyword spawning later below
            Vector3 position = planetObj.transform.position;

            float normalizedScale = Mathf.InverseLerp(minTweetCount, maxTweetCount, viewData.TweetCount);
            float exponentialScale = Mathf.Pow(normalizedScale, 1.5f);
            float planetScale = Mathf.Lerp(minPlanetScale, maxPlanetScale, exponentialScale);
            planetObj.transform.localScale = Vector3.one * planetScale;
            planetObj.GetComponent<Renderer>().enabled = false;

            // Nokta Bulutu
            GameObject cloudObj = new GameObject("Cloud");
            cloudObj.transform.SetParent(planetObj.transform, false);
            cloudObj.transform.localScale = Vector3.one * (1f / planetScale);

            ParticleSystem ps = cloudObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 6000;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            Color pColor = GetPlanetColor(viewData.CategoryName);
            main.startColor = pColor;

            var em = ps.emission;
            em.rateOverTime = 1500f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = planetScale / 2f;
            shape.radiusThickness = 0.35f;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 25f;
            noise.frequency = 0.3f;
            noise.scrollSpeed = 0.8f;
            noise.octaveCount = 3;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(pColor, 0f), new GradientColorKey(pColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            cloudObj.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            ps.Play();

            PlanetGravity pg = planetObj.AddComponent<PlanetGravity>();
            pg.ViewData = viewData;
            pg.PlanetColor = pColor;
            pg.PlanetPS = ps;
            pg.PlanetRenderer = planetObj.GetComponent<Renderer>();

            // 3D Audio Setup
            float orbitRadius = Mathf.Lerp(minOrbitScale, maxOrbitScale, exponentialScale);

            AudioSource source = planetObj.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.minDistance = 100f;
            source.maxDistance = orbitRadius * 2.5f; // Ensures it fades out past orbit
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.loop = true;
            source.playOnAwake = true;
            source.volume = 0.8f;
            
            // Mass inversely proportional to pitch. Heavier = deeper hum
            source.pitch = Mathf.Lerp(1.5f, 0.4f, normalizedScale);
            
            if (AudioManager.Instance != null)
            {
                if (AudioManager.Instance.orbitEntryClip != null)
                {
                    source.clip = AudioManager.Instance.orbitEntryClip;
                    source.Play();
                }
            }

            SphereCollider orbitCollider = planetObj.AddComponent<SphereCollider>();
            orbitCollider.isTrigger = true;
            orbitCollider.radius = orbitRadius / planetScale;

            for (int k = 0; k < 200; k++)
            {
                if (viewData.Keywords.Count == 0) continue;
                string word = viewData.Keywords[Random.Range(0, viewData.Keywords.Count)];

                GameObject wordObj = new GameObject("Keyword_" + word);
                wordObj.transform.position = position + Random.onUnitSphere * orbitRadius;
                
                // Parent the keywords to the ThemeCenter so they move in the solar system,
                // without inheriting the large scale of the actual planetObj.
                wordObj.transform.SetParent(themeCenter.transform, true);

                int count = viewData.KeywordFrequencies != null && viewData.KeywordFrequencies.ContainsKey(word) ? viewData.KeywordFrequencies[word] : 1;
                
                // Map count (e.g. 1 to 20) to a 0-1 range
                float t = Mathf.Clamp01((count - 1f) / 19f);
                
                // Color mapping: To make it physically "glow" in Unity, the RGB values must exceed 1.0 (HDR). 
                // We multiply the peak common words by 5x to trigger Post-Processing Bloom!
                float hdrMultiplier = 1f + (t * 4f); 
                Color glowingColor = new Color(pColor.r * hdrMultiplier, pColor.g * hdrMultiplier, pColor.b * hdrMultiplier, 0.6f + t * 0.4f);

                OrbitingKeyword ok = wordObj.AddComponent<OrbitingKeyword>();
                ok.centerPoint = planetObj.transform;
                ok.orbitSpeed = Random.Range(15f, 40f);
                
                // Size mapping: Most words are 0.15f, highest are 0.6f. Font size stays static.
                float cSize = Mathf.Lerp(0.15f, 0.6f, t);
                
                ok.SetupText(word, glowingColor, 90, cSize);
                pg.OrbitingKeywords.Add(ok);

                // Keyword → Portal listesine ekle (bridge geçişinde gizlenecek)
                allPolarizingObjects.Add(wordObj);
            }

            allPolarizingObjects.Add(planetObj);
            index++;
        }
    }
}
