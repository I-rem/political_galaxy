using UnityEngine;

public class GravityWindEffect : MonoBehaviour
{
    public static GravityWindEffect Instance;

    private ParticleSystem windPS;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule main;

    private float targetEmissionRate = 0f;
    private float currentEmissionRate = 0f;
    
    private GameObject psObj;
    private Vector3 currentPlanetPos;

    System.Collections.IEnumerator Start()
    {
        if (Instance == null)
        {
            Instance = this;
            CreateWindParticleSystem();
            yield return new WaitForEndOfFrame();
        }
        else
        {
            Destroy(this);
        }
    }

    private void CreateWindParticleSystem()
    {
        // Create a child object for the particles
        psObj = new GameObject("WindEffectParticles");
        psObj.transform.SetParent(this.transform);
        // Position it right at the camera
        psObj.transform.localPosition = Vector3.zero;

        windPS = psObj.AddComponent<ParticleSystem>();
        
        // Sistemi ayarlamadan önce durdur ki "duration" ayarı hata vermesin
        windPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Main Module Setup
        main = windPS.main;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = 120f; // Extremely fast
        main.startSize = 0.2f; // Thin lines
        main.startColor = new Color(1f, 1f, 1f, 0.4f); // Semi-transparent white
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Don't turn with camera
        main.playOnAwake = true;

        // Emission Module Setup
        emission = windPS.emission;
        emission.rateOverTime = 0f; // Start with no wind

        // Shape Module Setup
        var shape = windPS.shape;
        shape.shapeType = ParticleSystemShapeType.ConeVolume;
        shape.radius = 5f;
        shape.angle = 10f;
        shape.length = 40f; // 40 birim uzunluğunda bir hacim
        shape.position = new Vector3(0, 0, -10f); // Kameranın 10 birim gerisinden başlar, 30 birim önüne kadar uzanır

        // Renderer Setup for Speed Lines (Stretched Billboard)
        ParticleSystemRenderer renderer = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 8f; // Stretch them to look like lines
        renderer.velocityScale = 0.1f;
        
        // Use default particle material
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        
        // Yeniden başlat
        windPS.Play();
    }

    void Update()
    {
        // 1. Rüzgar sistemini her karede oyuncunun GERÇEK kafasına sabitle
        Transform followTarget = null;
        VRFlightController vrFlight = FindObjectOfType<VRFlightController>();
        if (vrFlight != null)
        {
            Camera c = vrFlight.GetComponentInChildren<Camera>();
            if (c != null) followTarget = c.transform;
            else followTarget = vrFlight.transform;
        }
        else if (Camera.main != null)
        {
            followTarget = Camera.main.transform;
        }
        
        if (followTarget != null)
        {
            transform.position = followTarget.position;
        }

        // If we just entered gravity, snap emission rate instantly and burst particles
        if (currentEmissionRate <= 0.1f && targetEmissionRate > 0.1f)
        {
            currentEmissionRate = targetEmissionRate;
            windPS.Emit(20); // Instant burst to fill the screen immediately
        }
        else
        {
            // Faster lerp for quicker reaction to distance
            currentEmissionRate = Mathf.Lerp(currentEmissionRate, targetEmissionRate, Time.deltaTime * 15f);
        }
        
        emission.rateOverTime = currentEmissionRate;

        // Rotate the particle system to aim at the planet so particles stream towards it
        if (currentEmissionRate > 0.1f && currentPlanetPos != Vector3.zero)
        {
            Vector3 dir = (currentPlanetPos - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                psObj.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    public void SetPullStrength(float intensity, Color planetColor, Vector3 planetPos)
    {
        currentPlanetPos = planetPos;

        if (intensity <= 0.1f)
        {
            targetEmissionRate = 0f;
            return;
        }

        // Map intensity to particle emission rate (Azaltıldı)
        targetEmissionRate = Mathf.Clamp(intensity * 0.5f, 10f, 80f);

        // Adjust speed slightly based on intensity
        main.startSpeed = Mathf.Clamp(intensity * 1.5f, 60f, 150f);
        main.startSize = 0.15f; // Kalın çizgiler inceltildi

        // Rengi tamamen opak (1.0 alpha) yap, VR'da uçup gitmesin
        Color windColor = Color.Lerp(Color.white, planetColor, 0.4f);
        windColor.a = 1f; 
        main.startColor = windColor;
    }

    public void StopWind()
    {
        targetEmissionRate = 0f;
    }
}
