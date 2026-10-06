using UnityEngine;

public class BridgePlanetInfo : MonoBehaviour
{
    public float gravityForce = 60f;

    private bool isPlayerNear = false;
    private bool hasBeenRead = false;
    private ParticleSystem cloudPS;

    void Awake()
    {
        // Cloud particle system'i erken bul
        cloudPS = GetComponentInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        // Bu obje her SetActive(true) olduğunda particle'ları yeniden başlat
        if (cloudPS != null)
        {
            cloudPS.Clear();
            cloudPS.Play();
        }
    }

    void Update()
    {
        VRFlightController vrFlight = FindObjectOfType<VRFlightController>();
        Transform playerTransform = vrFlight != null ? vrFlight.transform : null;
        if (playerTransform == null && Camera.main != null) playerTransform = Camera.main.transform;

        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            float pullRadius = 350f;
            SphereCollider[] colliders = GetComponents<SphereCollider>();
            foreach(var c in colliders) {
                if (c.isTrigger) pullRadius = c.radius * transform.localScale.x;
            }

            float surfaceDistance = (transform.localScale.x / 2f) + 36f;

            if (distance < pullRadius)
            {
                if (distance > surfaceDistance)
                {
                    // Çekiliyor
                    Vector3 direction = (transform.position - playerTransform.position).normalized;
                    float pull = Mathf.Clamp(gravityForce * 0.8f, 10f, gravityForce * 1.5f);

                    if (vrFlight != null)
                    {
                        vrFlight.transform.position += direction * pull * Time.deltaTime;
                    }
                    else
                    {
                        playerTransform.position += direction * pull * Time.deltaTime;
                    }

                    GravityWindEffect windEffect = FindObjectOfType<GravityWindEffect>();
                    if (windEffect != null) 
                    {
                        float intensity = pull / (gravityForce * 1.5f);
                        windEffect.SetPullStrength(intensity, Color.black, transform.position); // Beyaz boşlukta siyah rüzgar
                    }

                    if (isPlayerNear && distance > surfaceDistance + 25f)
                    {
                        ClosePanel();
                        isPlayerNear = false;
                    }
                }
                else
                {
                    // Çekirdeğe ulaştı
                    if (!isPlayerNear)
                    {
                        isPlayerNear = true;
                        
                        GravityWindEffect windEffect = FindObjectOfType<GravityWindEffect>();
                        if (windEffect != null) windEffect.StopWind();

                        if (PlanetManager.Instance != null)
                        {
                            string desc = "Congratulations!\n\nYou've witnessed the five main polarizing concepts and learned what you shouldn't be.\n\nNow you need to see what you need to unite.\nThe bridge between divided worlds is built with empathy, dialogue, and understanding.";
                            PlanetManager.Instance.ShowPlanetInfoOnPhone(
                                "The Bridge",
                                desc,
                                "empathy, dialogue, understanding, peace, harmony",
                                "Infinite"
                            );
                        }

                        MarkAsRead();
                    }
                }
            }
            else
            {
                // Kapsam dışı
                if (isPlayerNear)
                {
                    ClosePanel();
                    isPlayerNear = false;
                }
                GravityWindEffect windEffect = FindObjectOfType<GravityWindEffect>();
                if (windEffect != null) windEffect.StopWind();
            }
        }
    }

    private void ClosePanel()
    {
        if (PlanetManager.Instance != null) PlanetManager.Instance.HidePlanetInfoOnPhone();
    }

    private void MarkAsRead()
    {
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            if (PlanetManager.Instance != null)
                PlanetManager.Instance.MarkBridgeVisited();
        }
    }
}
