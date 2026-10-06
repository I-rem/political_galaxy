using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class AddOriginalHUD
{
    [MenuItem("VR/Add Original HUD")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            // Varsa eskisini temizle
            var oldHud = GameObject.Find("OriginalHUD_Canvas");
            if (oldHud != null) Object.DestroyImmediate(oldHud);

            // Canvas Olustur
            GameObject hudObj = new GameObject("OriginalHUD_Canvas");
            hudObj.transform.SetParent(cam.transform, false);
            hudObj.transform.localPosition = new Vector3(0, 0, 0.40f); 
            hudObj.transform.localRotation = Quaternion.identity;
            hudObj.transform.localScale = Vector3.one * 0.00045f; // Boyutu kamerada tam oturacak sekilde 

            Canvas canvas = hudObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            
            // Lazer isinlarini engellemesin
            var raycaster = hudObj.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false; 

            RectTransform canvasRT = hudObj.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(1600, 1000); // Orijinal 1600x1000 boyutu

            // Resim objesi
            GameObject imgObj = new GameObject("HUD_Image");
            imgObj.transform.SetParent(hudObj.transform, false);
            Image img = imgObj.AddComponent<Image>();
            
            // Orijinal PNG'yi bul
            Sprite hudSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ui.png");
            if (hudSprite != null)
            {
                img.sprite = hudSprite;
                img.color = new Color(1, 1, 1, 0.9f); // Neredeyse tamamen opak, sadece %10 seffaf
            }
            else
            {
                Debug.LogError("Assets/UI/ui.png sprite olarak bulunamadi!");
            }
            
            RectTransform rt = imgObj.GetComponent<RectTransform>();
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

        Debug.Log("Starting VR 3D Build (WITH ORIGINAL HUD)...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
