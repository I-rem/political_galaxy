using UnityEngine;

public class SkyboxBlender : MonoBehaviour
{
    [Tooltip("Bir tam nefes/geçiş döngüsü kaç saniye sürecek?")]
    public float cycleDuration = 10f;
    
    private Material skyboxMat;

    void Start()
    {
        // Geçerli skybox materyalini RenderSettings'den al
        skyboxMat = RenderSettings.skybox;
        
        if (skyboxMat == null || !skyboxMat.HasProperty("_Blend"))
        {
            Debug.LogWarning("SkyboxBlender: RenderSettings.skybox materyali bulunamadı veya '_Blend' özelliğine sahip değil! Lütfen Skybox/PanoramicBlend shader'ını kullanan bir materyal atayın.");
            enabled = false;
        }
    }

    void Update()
    {
        if (skyboxMat != null)
        {
            // Zaman içinde 0 ile 1 arasında yumuşak bir gidiş geliş (ping-pong) oluşturur
            float rawBlend = Mathf.PingPong(Time.time / cycleDuration, 1f);
            
            // Daha doğal, nefes alır gibi yumuşak bir geçiş için SmoothStep kullanıyoruz
            float smoothBlend = Mathf.SmoothStep(0f, 1f, rawBlend);
            
            // Shader'daki _Blend parametresini güncelliyoruz
            skyboxMat.SetFloat("_Blend", smoothBlend);
            
            // DİĞER SHADER'LAR İÇİN (Örn: Kask Yansıması) global olarak da ayarlıyoruz
            Shader.SetGlobalFloat("_GlobalSkyboxBlend", smoothBlend);
        }
    }
}
