using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Portal sistemi: Her frame oyuncunun portal düzlemine göre hangi tarafta olduğunu kontrol eder.
/// Trigger yerine sürekli kontrol daha güvenilirdir (CharacterController ile uyumlu).
/// </summary>
public class PortalGate : MonoBehaviour
{
    public List<GameObject> PolarizingPlanets = new List<GameObject>();
    public GameObject BridgePlanet;

    private Transform playerTransform;
    private bool playerIsOnBridgeSide = false;
    private List<GameObject> hiddenBackgrounds = new List<GameObject>(); // Gizlenen silüetleri hafızada tut
    private Material originalSkybox; // Eski Skybox'ı hafızada tut

    void Update()
    {
        // Player'ı bir kez bul (VRFlightController veya Camera)
        if (playerTransform == null)
        {
            VRFlightController vrFlight = FindObjectOfType<VRFlightController>();
            if (vrFlight != null) playerTransform = vrFlight.transform;
            else if (Camera.main != null) playerTransform = Camera.main.transform;

            if (playerTransform == null) return;
        }

        // Portal düzlemine göre oyuncunun tarafını belirle
        Vector3 localPos = transform.InverseTransformPoint(playerTransform.position);
        bool nowOnBridgeSide = localPos.z > 0f;

        if (nowOnBridgeSide && !playerIsOnBridgeSide)
        {
            playerIsOnBridgeSide = true;
            SetPolarizingVisible(false);
            if (BridgePlanet != null) BridgePlanet.SetActive(true);
            if (PlanetManager.Instance != null) PlanetManager.Instance.ShowBridgeChecklist();

            // Tüm ortamı ve silüetleri GİZLEME (Agresif Yöntem)
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach(GameObject obj in allObjects)
            {
                // UI değilse ve arka plan/silüet ise kapat
                if (obj.GetComponent<UnityEngine.CanvasRenderer>() != null) continue;
                
                string n = obj.name.ToLower();
                if (n.Contains("silhouette") || n.Contains("audience") || n.Contains("bg") || n.Contains("background") || n.Contains("wall") || n.Contains("sky"))
                {
                    if (playerTransform != null && !obj.transform.IsChildOf(playerTransform))
                    {
                        hiddenBackgrounds.Add(obj); // Geri dönüldüğünde açmak için listeye kaydet
                        obj.SetActive(false);
                    }
                }
            }

            // Skybox'ı silmeden önce hafızaya al
            if (RenderSettings.skybox != null) originalSkybox = RenderSettings.skybox;
            RenderSettings.skybox = null; 

            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.white;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 600f; // Sisi beyaza boya

            // Sahnede ne kadar kamera varsa hepsini beyaza çevir
            Camera[] allCams = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCams)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.white;
            }
        }
        else if (!nowOnBridgeSide && playerIsOnBridgeSide)
        {
            playerIsOnBridgeSide = false;
            SetPolarizingVisible(true);
            if (BridgePlanet != null) BridgePlanet.SetActive(false);

            // Geri dönüldüğünde orijinal ortamı geri getir
            foreach (GameObject obj in hiddenBackgrounds)
            {
                if (obj != null) obj.SetActive(true);
            }
            hiddenBackgrounds.Clear();

            RenderSettings.fog = false; // Sisi kapat
            
            // Eğer başta bir Skybox vardıysa onu geri yükle
            if (originalSkybox != null) RenderSettings.skybox = originalSkybox;

            Camera[] allCams = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCams)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.backgroundColor = Color.black; // Orijinal uzay siyahına geri döndür!
            }
        }
    }

    private void SetPolarizingVisible(bool visible)
    {
        foreach (var p in PolarizingPlanets)
            if (p != null) p.SetActive(visible);
    }
}
