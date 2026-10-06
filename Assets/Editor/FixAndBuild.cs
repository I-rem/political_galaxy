using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEditor.XR.OpenXR.Features;

public class FixAndBuild
{
    public static void Execute()
    {
        Debug.Log("Starting Automated XR Fix and Build...");

        // 1. Force Initialize XR on Startup
        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        if (buildTargetSettings != null)
        {
            buildTargetSettings.InitManagerOnStart = true;
            Debug.Log("Set InitManagerOnStart = true");
            
            XRPackageMetadataStore.AssignLoader(buildTargetSettings.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);

            var openXrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXrSettings != null)
            {
                // Find and enable OculusTouchControllerProfile
                var touchFeature = openXrSettings.GetFeature<OculusTouchControllerProfile>();
                if (touchFeature != null) {
                    touchFeature.enabled = true;
                    Debug.Log("Enabled OculusTouchControllerProfile");
                }

                // Enable Meta Quest Support feature
                // MetaQuestFeature is in UnityEngine.XR.OpenXR.Features.MetaQuestSupport namespace
                var questFeatureType = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.Name == "MetaQuestFeature");
                
                if (questFeatureType != null)
                {
                    var feature = openXrSettings.GetFeature(questFeatureType);
                    if (feature != null)
                    {
                        feature.enabled = true;
                        Debug.Log("Enabled MetaQuestFeature (Meta Quest Support)!");
                    }
                    else
                    {
                        Debug.LogError("MetaQuestFeature instance not found in OpenXRSettings!");
                    }
                }
                else
                {
                    Debug.LogError("MetaQuestFeature type not found in assemblies!");
                }
            }
            EditorUtility.SetDirty(buildTargetSettings.Manager);
        }
        else
        {
            Debug.LogError("XRGeneralSettings for Android not found.");
        }
        
        AssetDatabase.SaveAssets();

        // 1.5 Fix Anti-Aliasing (Wonky stars and blurry text)
        var urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (var guid in urpAssets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(path);
            if (asset != null)
            {
                asset.msaaSampleCount = 4; // 4x MSAA
                asset.renderScale = 1.0f;
                EditorUtility.SetDirty(asset);
                Debug.Log($"Applied 4x MSAA to {asset.name}");
            }
        }
        AssetDatabase.SaveAssets();

        // Force embedded paths just in case
        EditorPrefs.SetInt("JdkUseEmbedded", 1);
        EditorPrefs.SetInt("SdkUseEmbedded", 1);
        EditorPrefs.SetInt("NdkUseEmbedded", 1);

        // 2. Build the APK
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes enabled in Build Settings!");
            return;
        }

        System.IO.Directory.CreateDirectory("Builds");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/MidtermPolar.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.LogError("Build failed with " + summary.totalErrors + " errors.");
        }
    }
}
