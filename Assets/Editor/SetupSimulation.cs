using UnityEditor;
using UnityEngine;

public class SetupSimulation : EditorWindow
{
    [MenuItem("Polarization/Safe Setup Scene")]
    public static void SetupScene()
    {
        // 1. Setup Environment (Tamamen Güvenli - SİLME İŞLEMİ YOK)
        GameObject envObj = GameObject.Find("SimulationEnvironment");
        if (envObj == null) envObj = new GameObject("SimulationEnvironment");
        
        DataLoader loader = envObj.GetComponent<DataLoader>();
        if (loader == null) loader = envObj.AddComponent<DataLoader>();

        PlanetManager pManager = envObj.GetComponent<PlanetManager>();
        if (pManager == null) pManager = envObj.AddComponent<PlanetManager>();
        pManager.dataLoader = loader;
        
        AudioManager audioMan = envObj.GetComponent<AudioManager>();
        if (audioMan == null) audioMan = envObj.AddComponent<AudioManager>();
        
        // Ses dosyalarını otomatik ata (Eğer boşlarsa)
        if (audioMan.ambianceClip == null) audioMan.ambianceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/y2mate.com - Wind Blowing Sound Effect.mp3");
        if (audioMan.uiClickClip == null) audioMan.uiClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MadRooster Sound Library/UI/Click - Button - Swipe/Click_Aqua.wav");
        if (audioMan.orbitEntryClip == null) audioMan.orbitEntryClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/MadRooster Sound Library/UI/Sci-fi/Click_Open_Scifi.wav");

        if (envObj.GetComponent<IntroManager>() == null) envObj.AddComponent<IntroManager>();
        if (envObj.GetComponent<TweetUIManager>() == null) envObj.AddComponent<TweetUIManager>();

        // 2. Setup Player (Güvenli - SİLME YOK)
        GameObject playerObj = GameObject.Find("SpacePlayer");
        if (playerObj == null) {
            playerObj = new GameObject("SpacePlayer");
            playerObj.transform.position = new Vector3(0, 0, -400); // Start a bit far
            playerObj.tag = "Player";
        }
        
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc == null) {
            cc = playerObj.AddComponent<CharacterController>();
            cc.radius = 1f;
            cc.height = 3f;
        }
        
        if (playerObj.GetComponent<SpaceFPSController>() == null) playerObj.AddComponent<SpaceFPSController>();

        // 3. Setup Camera (Güvenli)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
        }
        if (mainCam.GetComponent<GravityWindEffect>() == null)
        {
            mainCam.gameObject.AddComponent<GravityWindEffect>();
        }
        mainCam.transform.SetParent(playerObj.transform);
        if (mainCam.transform.localPosition == Vector3.zero) mainCam.transform.localPosition = new Vector3(0, 0.8f, 0);
        
        mainCam.clearFlags = CameraClearFlags.Skybox;
        mainCam.backgroundColor = new Color(0.01f, 0.01f, 0.02f); // Sadece skybox yoksa bu renk görünür
        mainCam.farClipPlane = 5000f; 

        // Light (Güvenli)
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

        // 4. Setup VR Helmet UI
        string uiPath = "Assets/UI/ui.png";
        TextureImporter importer = AssetImporter.GetAtPath(uiPath) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            
            if (changed)
            {
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
            }
        }

        Sprite helmetSprite = null;
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(uiPath);
        foreach (var asset in allAssets)
        {
            if (asset is Sprite)
            {
                helmetSprite = asset as Sprite;
                break;
            }
        }

        if (helmetSprite != null)
        {
            GameObject existingHelmet = GameObject.Find("HelmetCanvas");
            if (existingHelmet == null)
            {
                GameObject helmetCanvasObj = new GameObject("HelmetCanvas");
                // Hiçbir yere bağlamıyoruz (Root level) ki rahatça bulunsun
                
                Canvas helmetCanvas = helmetCanvasObj.AddComponent<Canvas>();
                helmetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                helmetCanvas.worldCamera = mainCam;
                helmetCanvas.planeDistance = 1.5f; 
                helmetCanvas.sortingOrder = 50;
                
                UnityEngine.UI.CanvasScaler cScaler = helmetCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                cScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cScaler.referenceResolution = new Vector2(1920, 1080);
                
                GameObject imageObj = new GameObject("HelmetImage");
                imageObj.transform.SetParent(helmetCanvasObj.transform, false);
                UnityEngine.UI.Image img = imageObj.AddComponent<UnityEngine.UI.Image>();
                img.sprite = helmetSprite;
                img.raycastTarget = false;
                
                RectTransform rect = imageObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.sizeDelta = Vector2.zero; 
                
                Debug.Log("HelmetCanvas başarıyla oluşturuldu ve sahneye eklendi!");
            }
        }
        else
        {
            Debug.LogError("Assets/UI/ui.png dosyası bulunamadı veya Sprite'a çevrilemedi! Lütfen dosyanın tam olarak Assets/UI klasöründe ui.png adıyla olduğuna emin olun.");
        }

        Debug.Log("Sahne tamamen güvenli bir şekilde kuruldu! Hiçbir custom objeniz (UI, canvas vb.) silinmedi.");
    }
}
