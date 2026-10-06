using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class FinalPolish
{
    [MenuItem("VR/Final Polish And Build")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // Fix Camera Visuals (Make it black space, not default unity sky)
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.farClipPlane = 6000f; // Ensure planets far away are visible
            
            // Re-add the Gravity Wind Force Field
            var ff = cam.GetComponent<ParticleSystemForceField>();
            if (ff == null) ff = cam.gameObject.AddComponent<ParticleSystemForceField>();
            ff.shape = ParticleSystemForceFieldShape.Sphere;
            ff.endRange = 300f;
            ff.gravity = -5000f;
            ff.drag = 1.5f;
        }

        // Just to ensure XR Interaction Toolkit is fully clean
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Build
        System.IO.Directory.CreateDirectory("Builds");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/MidtermPolarOriginal_3D.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR 3D Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
