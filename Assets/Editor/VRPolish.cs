using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class VRPolish
{
    [MenuItem("VR/Apply VR Polish & Build")]
    public static void ApplyAndBuild()
    {
        // 1. Fix Anti-Aliasing (Wonky stars and blurry text)
        var urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (var guid in urpAssets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset != null)
            {
                asset.msaaSampleCount = 4; // 4x MSAA
                asset.renderScale = 1.0f;
                EditorUtility.SetDirty(asset);
                Debug.Log($"Applied 4x MSAA to {asset.name}");
            }
        }
        
        // 2. Build the APK
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        System.IO.Directory.CreateDirectory("Builds");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/MidtermPolar.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR Polish Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
