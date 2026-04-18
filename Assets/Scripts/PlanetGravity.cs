using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlanetGravity : MonoBehaviour
{
    public PoliticalViewData ViewData;
    public Color PlanetColor;
    public ParticleSystem PlanetPS;
    public Renderer PlanetRenderer;
    public List<OrbitingKeyword> OrbitingKeywords = new List<OrbitingKeyword>();
    
    public float gravityForce = 156f; // 2x artırıldı (78 -> 156)
    
    private GameObject infoCanvas;
    private Text infoText;
    private bool isPlayerAtCore = false;
    private bool hasBeenRead = false;

    private string GetExplanationForCategory(string category)
    {
        string catLower = category.ToLower();
        if (catLower.Contains("religious"))
            return "Religious Extremism is the advocacy of radical religious ideologies that reject moderate interpretations and often call for the total restructuring of society according to strict, fundamentalist religious laws.";
        if (catLower.Contains("populism"))
            return "A political approach that claims to support \"the ordinary people\" against a \"corrupt elite.\" It simplifies complex issues into a moral struggle between the virtuous public and a dishonest establishment.";
        if (catLower.Contains("gender"))
            return "Gender Essentialism Extremism is a term used to describe a radical adherence to the belief that men and women have fixed, innate, and unchangeable biological natures that dictate their roles, behaviors, and social status.";
        if (catLower.Contains("ethno"))
            return "Ethnonationalism is a form of nationalism where the nation is defined specifically by a shared ethnic identity rather than shared political principles or citizenship.";
        if (catLower.Contains("eco"))
            return "Eco-authoritarianism is a political concept that suggests democratic systems are too slow or inefficient to handle the climate crisis, proposing instead that an authoritarian government must impose strict environmental regulations to ensure human survival.";
        
        return "A deep dive view mapping specific polarized perspectives inside social systems.";
    }

    private Vector3 lastPosition;
    private Vector3 positionDelta;

    void Start()
    {
        lastPosition = transform.position;
        CreateCanvasWithFixedText();
    }

    void CreateCanvasWithFixedText()
    {
        infoCanvas = new GameObject("InfoCanvas_" + ViewData.CategoryName);
        infoCanvas.SetActive(false);

        Canvas canvas = infoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler cScaler = infoCanvas.AddComponent<CanvasScaler>();
        cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cScaler.referenceResolution = new Vector2(1920, 1080);

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
        infoText = textObj.AddComponent<Text>();
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if(font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        infoText.font = font;
        infoText.color = Color.white;
        infoText.fontSize = 28;
        infoText.lineSpacing = 1.3f;
        infoText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = infoText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.05f);
        textRect.anchorMax = new Vector2(0.95f, 0.95f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        List<string> uniqueKeywords = new List<string>();
        foreach(string kw in ViewData.Keywords) {
            if(!uniqueKeywords.Contains(kw)) uniqueKeywords.Add(kw);
            if(uniqueKeywords.Count >= 20) break;
        }

        string explanationText = GetExplanationForCategory(ViewData.CategoryName);

        string description = $"<b><size=48><color=#ffffff>{ViewData.CategoryName}</color></size></b>\n\n" +
                             $"<b><color=#ffffff>Explanation:</color></b>\n" +
                             $"<color=#ffffff>{explanationText}</color>\n\n" +
                             $"<b><color=#ffffff>Keywords:</color></b>\n" +
                             $"<color=#ffffff>{string.Join(", ", uniqueKeywords)}</color>\n\n" +
                             $"<b><color=#ffffff>Gravity Mass (Tweets):</color></b> <color=#ffffff>{ViewData.TweetCount}</color>\n\n" +
                             $"<i><color=#aaaaaa>(Click / Press E or ESC / Move away to exit)</color></i>";

        infoText.text = description;
    }

    void Update()
    {
        if (isPlayerAtCore && Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.eKey.wasPressedThisFrame ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                ClosePanelAndResume();
            }
        }
    }

    void FixedUpdate()
    {
        positionDelta = transform.position - lastPosition;
        lastPosition = transform.position;
    }

    private void ClosePanelAndResume()
    {
        infoCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        ChangeToReadState();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            float surfaceDistance = (transform.localScale.x / 2f) + 36f; 

            if (distance > surfaceDistance)
            {
                Vector3 direction = (transform.position - other.transform.position).normalized;
                CharacterController cc = other.GetComponent<CharacterController>();
                if (cc != null)
                {
                    float pullStrength = gravityForce * (1f - (distance / (GetComponent<SphereCollider>().radius * transform.localScale.x)));
                    pullStrength = Mathf.Max(pullStrength, gravityForce * 0.3f);
                    pullStrength = Mathf.Clamp(pullStrength, 5f, gravityForce);
                    
                    // Match the planet's movement in space so we don't get left behind
                    cc.Move((direction * pullStrength * Time.deltaTime) + positionDelta);
                }
                
                if (isPlayerAtCore && distance > surfaceDistance + 25f)
                {
                    ClosePanelAndResume();
                    isPlayerAtCore = false;
                }
            }
            else
            {
                if (!isPlayerAtCore)
                {
                    isPlayerAtCore = true;
                    // Artık basit açılış mekanizması
                    infoCanvas.SetActive(true);
                    
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayOrbitEntry();
                    }

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    SpaceFPSController fps = other.GetComponent<SpaceFPSController>();
                    if (fps != null)
                    {
                        fps.StopMovement();
                    }
                }
                
                // If they are locked at the core surface, they still need to move WITH the planet
                CharacterController surfaceCc = other.GetComponent<CharacterController>();
                if (surfaceCc != null)
                {
                    surfaceCc.Move(positionDelta);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isPlayerAtCore)
            {
                ClosePanelAndResume();
            }
            isPlayerAtCore = false;
        }
    }

    private void ChangeToReadState()
    {
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            // Sağ üstteki SİYAH KUTULU TABLOYA HABER VER: TİK atılsın
            if (PlanetManager.Instance != null)
            {
                PlanetManager.Instance.MarkPlanetVisited(ViewData.CategoryName);
            }
            ChangePlanetColorToBlue();
        }
    }

    private void ChangePlanetColorToBlue()
    {
        if (PlanetPS != null)
        {
            // Keyword rengiyle aynı tona eşitlendi: (0.4, 0.7, 1)
            Color blueColor = new Color(0.4f, 0.7f, 1f, 0.9f); 
            var main = PlanetPS.main;
            main.startColor = blueColor;

            var col = PlanetPS.colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(
               new GradientColorKey[] { new GradientColorKey(blueColor, 0f), new GradientColorKey(blueColor, 1f) },
               new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;
            
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[PlanetPS.particleCount];
            int count = PlanetPS.GetParticles(particles);
            for(int i = 0; i < count; i++) {
                particles[i].startColor = blueColor;
            }
            PlanetPS.SetParticles(particles, count);
        }

        if (OrbitingKeywords != null)
        {
            Color blueTextColor = new Color(0.4f, 0.7f, 1f, 0.9f);
            foreach(var kw in OrbitingKeywords)
            {
                if(kw != null) kw.ChangeColor(blueTextColor);
            }
        }
    }
}
