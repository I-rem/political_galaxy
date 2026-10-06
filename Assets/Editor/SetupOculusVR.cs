using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using Unity.XR.Oculus;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SetupOculusVR
{
    [MenuItem("VR/Setup And Build Oculus Final")]
    public static void DoSetupAndBuild()
    {
        // 1. Enable Oculus for Android
        var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        if (generalSettings == null)
        {
            var settings = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/XRManagerSettings.asset");
            
            generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            generalSettings.Manager = settings;
            AssetDatabase.CreateAsset(generalSettings, "Assets/XRGeneralSettings.asset");
        }
        
        if (generalSettings != null && generalSettings.Manager != null)
        {
            var loaders = generalSettings.Manager.activeLoaders;
            bool hasOculus = false;
            foreach (var l in loaders) if (l.name.Contains("Oculus")) hasOculus = true;
            
            if (!hasOculus)
            {
                var oculusLoader = ScriptableObject.CreateInstance<OculusLoader>();
                AssetDatabase.CreateAsset(oculusLoader, "Assets/OculusLoader.asset");
                generalSettings.Manager.TryAddLoader(oculusLoader);
            }
        }

        // 2. Setup the Scene
        var scene = EditorSceneManager.GetActiveScene();
        
        // Remove SpacePlayer
        var spacePlayer = GameObject.Find("SpacePlayer");
        if (spacePlayer != null) Object.DestroyImmediate(spacePlayer);

        // Remove old Main Camera
        var oldCam = GameObject.Find("Main Camera");
        if (oldCam != null) Object.DestroyImmediate(oldCam);

        // Instantiate XR Rig (using Starter Assets)
        GameObject rig = null;
        string prefabPath = "Packages/com.unity.xr.interaction.toolkit/Runtime/XR Interaction Setup.prefab";
        if (!System.IO.File.Exists(prefabPath))
        {
            prefabPath = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        }
        
        GameObject rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (rigPrefab != null)
        {
            rig = PrefabUtility.InstantiatePrefab(rigPrefab) as GameObject;
        }

        // Rotate 180 degrees so player faces planets
        if (rig != null)
        {
            rig.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // 4. Run AutoFixVR Logic
        if (rig != null)
        {
            var xrOrigin = rig.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                var spaceFlight = xrOrigin.GetComponent<VRFlightController>();
                if (spaceFlight == null) xrOrigin.gameObject.AddComponent<VRFlightController>();
            }
        }

        EditorSceneManager.SaveScene(scene);
        
        // 5. Build
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
