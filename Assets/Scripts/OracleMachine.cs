using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OracleMachine : MonoBehaviour
{
    private GameObject oracleCanvas;
    private Text oracleText;
    private GameObject inputZone;
    private ParticleSystem resultPS;

    private string baseTweet = "The opposing side is completely [___].";

    void Start()
    {
        CreateOracleUI();
        CreateInputZone();
        SpawnKeywords();
    }

    void CreateOracleUI()
    {
        oracleCanvas = new GameObject("OracleCanvas");
        oracleCanvas.transform.SetParent(transform, false);
        Canvas canvas = oracleCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        oracleCanvas.transform.localPosition = new Vector3(0, 0, 15f); // Merkezden biraz uzakta
        oracleCanvas.transform.localRotation = Quaternion.identity;
        oracleCanvas.transform.localScale = Vector3.one * 0.1f;

        GameObject panel = new GameObject("OraclePanel");
        panel.transform.SetParent(oracleCanvas.transform, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.2f, 0.1f, 0.9f); // Matrix yeşili gibi
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 500);

        GameObject textObj = new GameObject("OracleText");
        textObj.transform.SetParent(panel.transform, false);
        oracleText = textObj.AddComponent<Text>();
        
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        oracleText.font = f;
        oracleText.fontSize = 40;
        oracleText.color = Color.white;
        oracleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20); textRect.offsetMax = new Vector2(-20, -20);

        UpdateOracleDisplay("WAITING FOR INPUT", Color.white);
    }

    void CreateInputZone()
    {
        inputZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        inputZone.name = "OracleInputZone";
        inputZone.transform.SetParent(transform, false);
        inputZone.transform.localPosition = new Vector3(0, -10f, 10f); // Ekranın altında
        inputZone.transform.localScale = new Vector3(15f, 2f, 15f);

        // Şeffaf materyal
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = new Color(0.2f, 0.8f, 0.2f, 0.3f); // Yarı saydam yeşil platform
        inputZone.GetComponent<Renderer>().material = mat;

        BoxCollider col = inputZone.GetComponent<BoxCollider>();
        col.isTrigger = true;

        // Trigger detektör scripti ekle
        OracleInputDetector det = inputZone.AddComponent<OracleInputDetector>();
        det.machine = this;

        // Başarı/Hata Partikülü
        GameObject psObj = new GameObject("ResultPS");
        psObj.transform.SetParent(inputZone.transform, false);
        resultPS = psObj.AddComponent<ParticleSystem>();
        var main = resultPS.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startSpeed = 20f;
        main.startSize = 2f;
        main.startLifetime = 1f;
        var em = resultPS.emission;
        em.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0, 100) });
        var shape = resultPS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        psObj.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));
    }

    void SpawnKeywords()
    {
        // Kutunun etrafında havada asılı duran kelimeler
        string[] words = { "corrupt", "misunderstood", "evil", "human" };
        bool[] isPolarizing = { true, false, true, false };

        for (int i = 0; i < words.Length; i++)
        {
            GameObject wordObj = new GameObject("GrabWord_" + words[i]);
            wordObj.transform.SetParent(transform, false);
            // Kelimeleri ekranın sağında ve solunda havada dağıt
            float sign = (i % 2 == 0) ? 1f : -1f;
            wordObj.transform.localPosition = new Vector3(sign * 15f, -5f + (i * 2f), 5f);

            // VR'da tutulabilmesi için fiziksel hacim
            BoxCollider col = wordObj.AddComponent<BoxCollider>();
            col.size = new Vector3(8f, 2f, 1f);

            Rigidbody rb = wordObj.AddComponent<Rigidbody>();
            rb.useGravity = false; // Uzayda süzülecek
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;

            // Yazı UI
            GameObject textCanvas = new GameObject("TextCanvas");
            textCanvas.transform.SetParent(wordObj.transform, false);
            Canvas c = textCanvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            textCanvas.transform.localScale = Vector3.one * 0.1f;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textCanvas.transform, false);
            Text t = textObj.AddComponent<Text>();
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.font = f;
            t.text = words[i];
            t.fontSize = 40;
            t.color = Color.black;
            t.alignment = TextAnchor.MiddleCenter;

            // Arka plan (Beyaz kart)
            Image bg = textCanvas.AddComponent<Image>();
            bg.color = Color.white;
            RectTransform bgRect = textCanvas.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(80, 20);

            // XR Interaction Toolkit: Grab özelliği ekle (Reflection ile, versiyon hatası vermemesi için)
            // Ya da XR toolkit namespace'i direk kullanalım
            try {
                var grabType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
                if (grabType == null) grabType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
                
                if (grabType != null) {
                    wordObj.AddComponent(grabType);
                }
            } catch { }

            // Kelimenin kutuplaştırıcı olup olmadığını tutan bileşen
            OracleWord ow = wordObj.AddComponent<OracleWord>();
            ow.Word = words[i];
            ow.IsPolarizing = isPolarizing[i];
        }
    }

    public void ProcessWord(OracleWord ow, GameObject wordObj)
    {
        StartCoroutine(CalculatePrediction(ow, wordObj));
    }

    IEnumerator CalculatePrediction(OracleWord ow, GameObject wordObj)
    {
        UpdateOracleDisplay("ANALYZING...", Color.yellow);
        
        // Kelimeyi makinenin içine çek ve yok et
        Destroy(wordObj);

        yield return new WaitForSeconds(1.5f);

        if (ow.IsPolarizing)
        {
            UpdateOracleDisplay($"Input: '{ow.Word}'\n\nPREDICTION: 98% POLARIZING\n\n<color=#ff0000>RESULT: ECHO CHAMBER REINFORCED</color>", Color.red);
            var main = resultPS.main; main.startColor = Color.red;
            resultPS.Play();
            
            // Paneli kırmızıya boya
            oracleCanvas.GetComponentInChildren<Image>().color = new Color(0.3f, 0.05f, 0.05f, 0.9f);
        }
        else
        {
            UpdateOracleDisplay($"Input: '{ow.Word}'\n\nPREDICTION: 92% BRIDGING\n\n<color=#00ffff>RESULT: CONSENSUS ACHIEVED</color>", Color.cyan);
            var main = resultPS.main; main.startColor = Color.cyan;
            resultPS.Play();

            // Paneli maviye boya
            oracleCanvas.GetComponentInChildren<Image>().color = new Color(0.05f, 0.2f, 0.3f, 0.9f);
        }

        yield return new WaitForSeconds(5f);

        UpdateOracleDisplay("WAITING FOR NEXT INPUT", Color.white);
        oracleCanvas.GetComponentInChildren<Image>().color = new Color(0.05f, 0.2f, 0.1f, 0.9f);
    }

    void UpdateOracleDisplay(string status, Color color)
    {
        oracleText.text = $"<b>PREDICTION ENGINE v1.0</b>\n\n" +
                          $"Tweet: \"{baseTweet}\"\n\n" +
                          $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{status}</color>";
    }
}

public class OracleWord : MonoBehaviour
{
    public string Word;
    public bool IsPolarizing;
}

public class OracleInputDetector : MonoBehaviour
{
    public OracleMachine machine;

    void OnTriggerEnter(Collider other)
    {
        OracleWord ow = other.GetComponent<OracleWord>();
        if (ow != null)
        {
            machine.ProcessWord(ow, other.gameObject);
        }
    }
}
