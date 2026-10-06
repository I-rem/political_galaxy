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
    [HideInInspector] public PlanetOrbit PlanetOrbit;
    
    public float gravityForce = 156f; // 2x artırıldı (78 -> 156)
    
    private bool isPlayerAtCore = false;
    private bool hasBeenRead = false;

    private string GetExplanationForCategory(string category)
    {
        string catLower = category.ToLower();
        if (catLower.Contains("religious"))
            return "Religious Extremism is the advocacy of radical religious ideologies that reject moderate interpretations and often call for the total restructuring of society according to strict, fundamentalist religious laws.";
        if (catLower.Contains("populism") || catLower.Contains("maga"))
            return "A political approach that claims to support \"the ordinary people\" against a \"corrupt elite.\" It simplifies complex issues into a moral struggle between the virtuous public and a dishonest establishment.";
        if (catLower.Contains("gender"))
            return "Gender Essentialism Extremism is a term used to describe a radical adherence to the belief that men and women have fixed, innate, and unchangeable biological natures that dictate their roles, behaviors, and social status.";
        if (catLower.Contains("ethno"))
            return "Ethnonationalism is a form of nationalism where the nation is defined specifically by a shared ethnic identity rather than shared political principles or citizenship.";
        if (catLower.Contains("eco"))
            return "Eco-authoritarianism is a political concept that suggests democratic systems are too slow or inefficient to handle the climate crisis, proposing instead that an authoritarian government must impose strict environmental regulations to ensure human survival.";
        if (catLower.Contains("progressive"))
            return "The Progressive Left advocates for structural transformation of economic systems to address inequality. Core positions include universal healthcare, taxing extreme wealth, student debt cancellation, workers' rights, and robust climate legislation.";
        if (catLower.Contains("libertarian"))
            return "Libertarian Right ideology holds that individual liberty is the supreme political value. It opposes taxation as coercive, advocates for free markets without regulation, and views government intervention — social or economic — as inherently tyrannical.";
        if (catLower.Contains("identitarian"))
            return "The Identitarian Left frames politics primarily through the lens of race, colonialism, and systemic oppression. It advocates for deconstructing whiteness, reparations, police abolition, and centering historically marginalized voices in all political discourse.";

        return "A deep dive view mapping specific polarized perspectives inside social systems.";
    }

    void Start()
    {
        // Eski canvas sistemi iptal edildi, artık telefonda gösterilecek.
    }

    private bool wasPulling = false;

    void Update()
    {
        // 1. Çekirdekteyken paneli kapatma kontrolleri
        if (isPlayerAtCore)
        {
            bool triggerClose = false;
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                triggerClose = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                triggerClose = true;

            UnityEngine.XR.InputDevice rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            float triggerVal;
            if (rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerVal) && triggerVal > 0.5f)
                triggerClose = true;

            if (triggerClose)
            {
                ClosePanelAndResume();
            }
        }

        // 2. VRFlightController'ı Bularak Kesin Pozisyon Takibi
        VRFlightController vrFlight = FindObjectOfType<VRFlightController>();
        Transform playerTransform = vrFlight != null ? vrFlight.transform : null;
        
        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;

        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            float pullRadius = 300f;
            SphereCollider[] colliders = GetComponents<SphereCollider>();
            foreach(var c in colliders) {
                
            }

            float surfaceDistance = (transform.localScale.x / 2f) + 40f; 

            if (distance < pullRadius) 
            {
                wasPulling = true;
                if (distance > surfaceDistance)
                {
                    // Çekiliyor
                    Vector3 direction = (transform.position - playerTransform.position).normalized;
                    
                    float pullStrength = gravityForce * (1f - (distance / pullRadius));
                    pullStrength = Mathf.Max(pullStrength, gravityForce * 0.1f);
                    pullStrength = Mathf.Clamp(pullStrength, 10f, gravityForce * 1.15f); 

                    if (vrFlight != null)
                    {
                        vrFlight.transform.position += direction * pullStrength * Time.deltaTime;
                    }
                    else
                    {
                        playerTransform.position += direction * pullStrength * Time.deltaTime;
                    }

                    if (GravityWindEffect.Instance != null)
                    {
                        GravityWindEffect.Instance.SetPullStrength(pullStrength, PlanetColor, transform.position);
                    }
                    
                    if (isPlayerAtCore && distance > surfaceDistance + 25f)
                    {
                        ClosePanelAndResume();
                        isPlayerAtCore = false;
                    }
                }
                else
                {
                    // Çekirdeğe Ulaşıldı
                    if (!isPlayerAtCore)
                    {
                        if (GravityWindEffect.Instance != null)
                        {
                            GravityWindEffect.Instance.StopWind();
                        }

                        // Pause orbit so the planet doesn't drift away while player is reading
                        if (PlanetOrbit != null) PlanetOrbit.isPaused = true;

                        isPlayerAtCore = true;

                        List<string> uniqueKeywords = new List<string>();
                        foreach(string kw in ViewData.Keywords) {
                            if(!uniqueKeywords.Contains(kw)) uniqueKeywords.Add(kw);
                            if(uniqueKeywords.Count >= 15) break;
                        }
                        
                        if (PlanetManager.Instance != null)
                        {
                            PlanetManager.Instance.ShowPlanetInfoOnPhone(
                                ViewData.CategoryName,
                                GetExplanationForCategory(ViewData.CategoryName),
                                string.Join(", ", uniqueKeywords),
                                ViewData.TweetCount.ToString()
                            );
                        }
                        
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayOrbitEntry();
                        }

                        ChangeToReadState();
                    }
                }
            }
            else
            {
                // Menzil dışındayız
                if (isPlayerAtCore)
                {
                    ClosePanelAndResume();
                    isPlayerAtCore = false;
                    // Resume orbit when player flies away
                    if (PlanetOrbit != null) PlanetOrbit.isPaused = false;
                }
                
                // Sadece BU gezegen çekiyorken çıktıysak rüzgarı durdur (Diğer gezegenlerin rüzgarını kesmemek için)
                if (wasPulling)
                {
                    if (GravityWindEffect.Instance != null && !isPlayerAtCore)
                    {
                        GravityWindEffect.Instance.StopWind(); 
                    }
                    wasPulling = false;
                }
            }
        }
    }

    private void ClosePanelAndResume()
    {
        if (PlanetManager.Instance != null) PlanetManager.Instance.HidePlanetInfoOnPhone();
        Cursor.lockState = CursorLockMode.Locked;
        ChangeToReadState();
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
