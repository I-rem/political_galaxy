using UnityEditor;
using UnityEngine;

public class SetupSimulation : EditorWindow
{
    [MenuItem("Polarization/Auto Setup Scene")]
    public static void SetupScene()
    {
        // 0. Aggressive Cleanup (Esikden kalan her şeyi temizle)
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj == null) continue;
            string n = obj.name.ToLower();
            if (n.Contains("canvas") || n.Contains("panel") || n.Contains("wall") || 
                n.Contains("eventsystem") || n.Contains("checkpoint") || n.Contains("portal"))
            {
                DestroyImmediate(obj);
            }
        }

        // 1. Setup Environment
        GameObject envObj = GameObject.Find("SimulationEnvironment");
        if (envObj != null) DestroyImmediate(envObj);
        envObj = new GameObject("SimulationEnvironment");
        
        DataLoader loader = envObj.AddComponent<DataLoader>();
        PlanetManager pManager = envObj.AddComponent<PlanetManager>();
        pManager.dataLoader = loader;

        // 2. Setup Player
        GameObject playerObj = GameObject.Find("SpacePlayer");
        if (playerObj != null) DestroyImmediate(playerObj);
        playerObj = new GameObject("SpacePlayer");
        playerObj.transform.position = new Vector3(0, 0, -400); // Start a bit far
        playerObj.tag = "Player";
        
        var cc = playerObj.AddComponent<CharacterController>();
        cc.radius = 1f;
        cc.height = 3f;
        
        playerObj.AddComponent<SpaceFPSController>();

        // 3. Setup Camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
        }
        mainCam.transform.SetParent(playerObj.transform);
        mainCam.transform.localPosition = new Vector3(0, 0.8f, 0);
        mainCam.transform.localRotation = Quaternion.identity;
        
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = new Color(0.01f, 0.01f, 0.02f); // Very dark blue/black space
        mainCam.farClipPlane = 5000f; // See distant planets

        // Add a simple directional light if missing
        Light[] lights = FindObjectsOfType<Light>();
        bool hasSun = false;
        foreach(var l in lights)
        {
            if(l.type == LightType.Directional) hasSun = true;
        }
        if (!hasSun)
        {
            GameObject lightObj = new GameObject("SunLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.95f, 0.9f);
        }

        Debug.Log("Scene configured successfully! You can press Play now.");
    }
}
