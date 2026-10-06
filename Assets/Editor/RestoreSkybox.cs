using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class RestoreSkybox
{
    [MenuItem("VR/Restore Skybox")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            Debug.Log("Restored Camera clearFlags to Skybox.");
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

        Debug.Log("Starting VR 3D Build (with Skybox)...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
