using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class RestoreVisorHUD
{
    [MenuItem("VR/Restore Visor HUD")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            // Temizlik
            var oldHud = GameObject.Find("HelmetHUD_Canvas");
            if (oldHud != null) Object.DestroyImmediate(oldHud);
            var oldVisor = GameObject.Find("VisorHUD_Canvas");
            if (oldVisor != null) Object.DestroyImmediate(oldVisor);

            // Yenisini olustur
            GameObject hudObj = new GameObject("VisorHUD_Canvas");
            hudObj.transform.SetParent(cam.transform, false);
            hudObj.transform.localPosition = new Vector3(0, 0, 0.4f); 
            hudObj.transform.localRotation = Quaternion.identity;
            hudObj.transform.localScale = Vector3.one * 0.001f; 

            Canvas canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            
            var raycaster = hudObj.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false; 

            RectTransform canvasRT = hudObj.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(1000, 1000);

            // Cami olustur
            GameObject glass = new GameObject("GlassImage");
            glass.transform.SetParent(hudObj.transform, false);
            Image img = glass.AddComponent<Image>();
            
            // Materyali yukle
            Material glassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VisorGlass.mat");
            if (glassMat != null)
            {
                img.material = glassMat;
                img.color = new Color(1, 1, 1, 0.3f); // Hafif saydam
            }
            else
            {
                Debug.LogError("VisorGlass.mat bulunamadi!");
            }
            
            RectTransform rt = glass.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Build
        System.IO.Directory.CreateDirectory("Builds");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/MidtermPolarOriginal_3D.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR 3D Build with VISOR HUD...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
