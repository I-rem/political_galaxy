using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class FinalFixSkybox
{
    [MenuItem("VR/Final Fix Skybox and Tint")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. Set Skybox Material
        Material animatedBG = AssetDatabase.LoadAssetAtPath<Material>("Assets/AnimatedBG.mat");
        if (animatedBG != null)
        {
            RenderSettings.skybox = animatedBG;
            Debug.Log("Skybox set to AnimatedBG.mat");
        }

        // 2. Set VisorGlass Tint to Blue
        Material visorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VisorGlass.mat");
        if (visorMat != null)
        {
            visorMat.SetColor("_GlassTint", new Color(0.7f, 0.85f, 1.0f, 1.0f));
            EditorUtility.SetDirty(visorMat);
        }

        // 3. Make sure Camera clear flags are Skybox
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
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

        Debug.Log("Starting VR 3D Build Final Fix...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
