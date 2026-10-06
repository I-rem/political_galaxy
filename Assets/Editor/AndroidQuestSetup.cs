using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

public class AndroidQuestSetup
{
    public static void Apply()
    {
        Debug.Log("Starting Android & Quest Build Setup...");
        
        // Optimize Player Settings for Meta Quest 3
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        
        Debug.Log("Player Settings optimized for Quest 3 (ASTC, API 29+, Linear Color).");

        // Ensure XRGeneralSettings for Android exists
        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

        if (buildTargetSettings != null)
        {
            XRPackageMetadataStore.AssignLoader(buildTargetSettings.Manager, "UnityEngine.XR.OpenXR.OpenXRLoader", BuildTargetGroup.Android);
            
            var openXrSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXrSettings != null)
            {
                var feature = openXrSettings.GetFeature<OculusTouchControllerProfile>();
                if (feature != null)
                {
                    feature.enabled = true;
                    Debug.Log("Oculus Touch Controller Profile enabled for Android!");
                }
            }
            EditorUtility.SetDirty(buildTargetSettings.Manager);
        }
        else
        {
            Debug.LogWarning("XRGeneralSettings for Android not found.");
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log("Android Quest Setup Complete!");
    }
}
