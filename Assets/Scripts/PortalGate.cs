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
    private bool isInitialized = false;

    void Update()
    {
        // Player'ı bir kez bul
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) { playerTransform = p.transform; isInitialized = true; }
            else return;
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

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.white;
            }
        }
        else if (!nowOnBridgeSide && playerIsOnBridgeSide)
        {
            playerIsOnBridgeSide = false;
            SetPolarizingVisible(true);
            if (BridgePlanet != null) BridgePlanet.SetActive(false);

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
            }
        }
    }

    private void SetPolarizingVisible(bool visible)
    {
        foreach (var p in PolarizingPlanets)
            if (p != null) p.SetActive(visible);
    }
}
