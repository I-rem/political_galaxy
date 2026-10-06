using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class CleanupHUD
{
    [MenuItem("VR/Cleanup HUD Final")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            var hud = cam.GetComponent("HelmetHUD");
            if (hud != null) Object.DestroyImmediate(hud);
        }

        var canvasHud = GameObject.Find("HelmetHUD_Canvas");
        if (canvasHud != null) Object.DestroyImmediate(canvasHud);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Build
        System.IO.Directory.CreateDirectory("Builds");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/MidtermPolarOriginal_3D.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR 3D Build (NO HUD, CRISP TEXT)...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
