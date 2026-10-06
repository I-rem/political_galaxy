using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class UserFeedbackFixes
{
    [MenuItem("VR/Apply User Feedback Fixes")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            // 1. Fix Camera Far Clip Plane so we can see distant objects!
            cam.farClipPlane = 6000f;

            // 2. Fix HUD Scale and Position
            var hudObj = GameObject.Find("OriginalHUD_Canvas");
            if (hudObj != null)
            {
                hudObj.transform.localPosition = new Vector3(0, 0, 0.35f); 
                // Quest 3 FOV is huge, increase scale significantly so it hits the edges
                hudObj.transform.localScale = Vector3.one * 0.00085f; 
                
                // Opacity slightly reduced so it doesn't block entirely
                var img = hudObj.GetComponentInChildren<Image>();
                if (img != null) img.color = new Color(1, 1, 1, 0.65f);
            }
        }

        // 3. Move Silhouettes closer just in case (800 units instead of 4500)
        var quad1 = GameObject.Find("DistantSilhouette_1");
        if (quad1 != null)
        {
            quad1.transform.position = new Vector3(0, 0, 800f);
            quad1.transform.localScale = new Vector3(1000f, 1000f, 1f);
        }

        var quad2 = GameObject.Find("DistantSilhouette_2");
        if (quad2 != null)
        {
            quad2.transform.position = new Vector3(0, 0, -800f);
            quad2.transform.localScale = new Vector3(1000f, 1000f, 1f);
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

        Debug.Log("Starting VR 3D Build (User Feedback Fixes)...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
