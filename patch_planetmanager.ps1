$content = Get-Content Assets\Scripts\PlanetManager.cs -Raw

$oldBlock = @"
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
"@

$newBlock = @"
        // Floating title label above the sun
        GameObject labelObj = new GameObject("SunLabel");
        labelObj.transform.SetParent(sunRoot.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, scale * 1.2f, 0f);
        
        Canvas c = labelObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1000, 250);
        labelObj.transform.localScale = Vector3.one * 0.05f;

        GameObject bg = new GameObject("Bg");
        bg.transform.SetParent(labelObj.transform, false);
        UnityEngine.UI.Image img = bg.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0.65f); // Koyu arkaplan
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.sizeDelta = Vector2.zero;

        GameObject txtObj = new GameObject("Txt");
        txtObj.transform.SetParent(labelObj.transform, false);
        UnityEngine.UI.Text txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = title;
        txt.color = color * 1.5f; txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f);
        txt.fontSize = 120;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.font = f;
        RectTransform txtrt = txtObj.GetComponent<RectTransform>();
        txtrt.anchorMin = Vector2.zero; txtrt.anchorMax = Vector2.one;
        txtrt.sizeDelta = Vector2.zero;
"@

$content = $content.Replace($oldBlock, $newBlock)
Set-Content Assets\Scripts\PlanetManager.cs -Value $content
