using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using Unity.XR.Oculus;

public class LinkOculusLoader
{
    [MenuItem("VR/Force Oculus Setup")]
    public static void ForceSetup()
    {
        var buildTarget = BuildTargetGroup.Android;
        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTarget);
        
        if (generalSettings == null)
        {
            Debug.Log("Creating XRGeneralSettingsPerBuildTarget...");
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject("com.unity.xr.management.loader_settings", out buildTargetSettings);
            
            if (buildTargetSettings == null)
            {
                buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(buildTargetSettings, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject("com.unity.xr.management.loader_settings", buildTargetSettings, true);
            }
            
            generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            generalSettings.Manager = managerSettings;
            
            AssetDatabase.CreateAsset(generalSettings, "Assets/XR/XRGeneralSettings_Android.asset");
            AssetDatabase.CreateAsset(managerSettings, "Assets/XR/XRManagerSettings_Android.asset");
            
            buildTargetSettings.SetSettingsForBuildTarget(buildTarget, generalSettings);
        }

        if (generalSettings.Manager == null)
        {
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            generalSettings.Manager = managerSettings;
            AssetDatabase.CreateAsset(managerSettings, "Assets/XR/XRManagerSettings_Android.asset");
        }

        var loaders = generalSettings.Manager.activeLoaders;
        bool hasOculus = false;
        foreach (var l in loaders) if (l.name.Contains("Oculus")) hasOculus = true;
        
        if (!hasOculus)
        {
            var oculusLoader = ScriptableObject.CreateInstance<OculusLoader>();
            AssetDatabase.CreateAsset(oculusLoader, "Assets/XR/OculusLoader.asset");
            var success = generalSettings.Manager.TryAddLoader(oculusLoader);
            Debug.Log("Added Oculus Loader: " + success);
            
            EditorUtility.SetDirty(generalSettings);
            EditorUtility.SetDirty(generalSettings.Manager);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.Log("Oculus Loader already present!");
        }
        
        // Final Build Trigger
        Debug.Log("Starting VR 3D Build...");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/MidtermPolarOriginal_3D.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
