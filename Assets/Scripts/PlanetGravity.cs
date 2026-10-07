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

    public string GetExplanationForCategory(string category)
    {
        string catLower = category.ToLower();
        if (catLower.Contains("religious"))
            return "Dini Aşırıcılık, ılımlı yorumları reddeden ve genellikle toplumun katı köktendinci yasalara göre tamamen yeniden yapılandırılmasını savunan radikal dini ideolojilerdir.";
        if (catLower.Contains("populism") || catLower.Contains("maga"))
            return "'Yozlaşmış elitlere' karşı 'sıradan halkı' desteklediğini iddia eden siyasi bir yaklaşım. Karmaşık sorunları, erdemli halk ile dürüst olmayan müesses nizam arasındaki ahlaki bir mücadeleye indirger.";
        if (catLower.Contains("gender"))
            return "Aşırı Cinsiyet Özcülüğü, erkeklerin ve kadınların rollerini, davranışlarını ve sosyal statülerini belirleyen sabit, doğuştan gelen ve değişmez biyolojik doğaları olduğu inancına radikal bir şekilde bağlılığı tanımlamak için kullanılan bir terimdir.";
        if (catLower.Contains("ethno"))
            return "Etnomilliyetçilik, ulusun ortak siyasi ilkeler veya vatandaşlıktan ziyade doğrudan ortak bir etnik kimlikle tanımlandığı bir milliyetçilik biçimidir.";
        if (catLower.Contains("eco"))
            return "Eko-Otoriteryenizm, demokratik sistemlerin iklim krizini ele almak için çok yavaş veya verimsiz olduğunu öne süren ve bunun yerine insanlığın hayatta kalmasını sağlamak için otoriter bir hükümetin katı çevre düzenlemeleri dayatması gerektiğini savunan siyasi bir kavramdır.";
        if (catLower.Contains("progressive"))
            return "İlerici Sol, eşitsizliği gidermek için ekonomik sistemlerin yapısal dönüşümünü savunur. Temel görüşleri arasında evrensel sağlık hizmetleri, aşırı servetin vergilendirilmesi, öğrenci borçlarının iptal edilmesi, işçi hakları ve güçlü iklim yasaları yer alır.";
        if (catLower.Contains("libertarian"))
            return "Özgürlükçü Sağ ideoloji, bireysel özgürlüğün en yüce siyasi değer olduğunu savunur. Vergiyi zorbalık olarak görür, düzenlemeye tabi olmayan serbest piyasaları savunur ve hükümet müdahalesini - sosyal veya ekonomik olsun - doğası gereği zorba olarak değerlendirir.";
        if (catLower.Contains("identitarian"))
            return "Kimlikçi Sol, siyaseti öncelikle ırk, sömürgecilik ve sistemik baskı merceğinden çerçeveler. Beyazlığın yapı sökümünü, tazminatları, polisin kaldırılmasını ve siyasi söylemlerde tarihsel olarak marjinalleştirilmiş sesleri merkeze almayı savunur.";

        return "Sosyal sistemler içindeki belirli kutuplaşmış perspektiflerin haritasını çıkaran derinlemesine bir görünüm.";
    }


    public static string GetTranslatedCategoryName(string category)
    {
        string lower = category.ToLower();
        if (lower.Contains("religious")) return "Dini Aşırıcılık";
        if (lower.Contains("maga") || lower.Contains("populism")) return "MAGA / Popülizm";
        if (lower.Contains("gender")) return "Cinsiyet Özcülüğü";
        if (lower.Contains("ethno")) return "Etnomilliyetçilik";
        if (lower.Contains("eco")) return "Eko-Otoriteryenizm";
        if (lower.Contains("progressive")) return "İlerici Sol";
        if (lower.Contains("libertarian")) return "Özgürlükçü Sağ";
        if (lower.Contains("identitarian")) return "Kimlikçi Sol";
        if (lower.Contains("bridge")) return "Köprü Gezegeni";
        return category;
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
                                GetTranslatedCategoryName(ViewData.CategoryName),
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
