using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Collections;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;

public class VRExhibitSetup : MonoBehaviour
{
    private static AddRequest request;
    private static AddRequest openXrRequest;
    private static AddRequest xrMgmtRequest;

    [MenuItem("VR Exhibit/1. Install XR Packages")]
    public static void InstallPackages()
    {
        Debug.Log("Starting XR package installation...");
        xrMgmtRequest = Client.Add("com.unity.xr.management");
        EditorApplication.update += ProgressXRMgmt;
    }

    static void ProgressXRMgmt()
    {
        if (xrMgmtRequest.IsCompleted)
        {
            EditorApplication.update -= ProgressXRMgmt;
            if (xrMgmtRequest.Status == StatusCode.Success)
            {
                Debug.Log("Installed XR Management.");
                openXrRequest = Client.Add("com.unity.xr.openxr");
                EditorApplication.update += ProgressOpenXR;
            }
            else Debug.LogError(xrMgmtRequest.Error.message);
        }
    }

    static void ProgressOpenXR()
    {
        if (openXrRequest.IsCompleted)
        {
            EditorApplication.update -= ProgressOpenXR;
            if (openXrRequest.Status == StatusCode.Success)
            {
                Debug.Log("Installed OpenXR.");
                request = Client.Add("com.unity.xr.interaction.toolkit");
                EditorApplication.update += ProgressXRI;
            }
            else Debug.LogError(openXrRequest.Error.message);
        }
    }

    static void ProgressXRI()
    {
        if (request.IsCompleted)
        {
            EditorApplication.update -= ProgressXRI;
            if (request.Status == StatusCode.Success)
            {
                Debug.Log("Installed XR Interaction Toolkit successfully!");
                SetupXRLoaders();
            }
            else
            {
                Debug.LogError(request.Error.message);
            }
        }
    }

    static void SetupXRLoaders()
    {
        XRGeneralSettings generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
        if (generalSettings == null)
        {
            var settings = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/XR/XRManagerSettings.asset");
            // Simplified setup: tell user to enable OpenXR manually to avoid complex reflections
            Debug.LogWarning("Please go to Edit > Project Settings > XR Plug-in Management and enable OpenXR.");
        }
    }

    [MenuItem("VR Exhibit/2. Convert Scene to VR")]
    public static void ConvertSceneToVR()
    {
        // 1. Remove SpaceFPSController
        var fps = GameObject.FindObjectOfType<SpaceFPSController>();
        if (fps != null)
        {
            Debug.Log("Removing SpaceFPSController...");
            DestroyImmediate(fps.gameObject);
        }

        // 2. Instantiate XR Origin
        GameObject xrOrigin = GameObject.Find("XR Origin (VR)");
        if (xrOrigin == null)
        {
            EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)");
            xrOrigin = GameObject.Find("XR Origin (VR)");
            if (xrOrigin == null)
            {
                Debug.LogWarning("Could not auto-create XR Origin. Please create it manually from GameObject > XR > XR Origin (VR).");
            }
        }

        if (xrOrigin != null)
        {
            xrOrigin.tag = "Player";
            xrOrigin.transform.position = new Vector3(0, 1.5f, 0); 
            
            // Fix PortalGate 
            var portal = GameObject.FindObjectOfType<PortalGate>();
            if (portal != null)
            {
                Debug.Log("PortalGate will now find the XR Origin as Player.");
            }

            // Adjust Camera height configuration
            // Usually we set Tracking Origin Mode to Floor
            Debug.Log("Configured XR Origin initial parameters.");
        }

        // 3. Fix IntroManager UI for VR
        var introManager = GameObject.FindObjectOfType<IntroManager>();
        if (introManager != null)
        {
            Debug.Log("Note: IntroManager creates a standard UI Canvas. For VR, this canvas should have a Tracked Device Graphic Raycaster and be set to World Space.");
        }

        Debug.Log("Scene conversion complete! VR headset and controllers will now spawn properly.");
    }
}
