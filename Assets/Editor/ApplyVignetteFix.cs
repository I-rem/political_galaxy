using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class ApplyVignetteFix
{
    [MenuItem("VR/Apply Vignette Fix")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var origin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (origin != null)
        {
            var vig = origin.gameObject.GetComponent<CustomVignetteController>();
            if (vig == null)
            {
                origin.gameObject.AddComponent<CustomVignetteController>();
            }
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

        Debug.Log("Starting VR 3D Build (Vignette Add)...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
