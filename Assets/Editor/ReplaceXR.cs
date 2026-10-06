using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

public class ReplaceXR
{
    [MenuItem("VR/Final Polish & Replace Origin")]
    public static void ApplyAndBuild()
    {
        var scene = EditorSceneManager.GetActiveScene();
        
        // Find existing barebones XR Origin
        var oldOrigin = GameObject.Find("XR Origin (VR)");
        Vector3 oldPos = new Vector3(0, 1.5f, 0); // Default
        if (oldOrigin != null)
        {
            oldPos = oldOrigin.transform.position;
            GameObject.DestroyImmediate(oldOrigin);
        }

        // Also clean up any extra InputActionManager if it was placed separately
        var oldManagers = Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var m in oldManagers)
        {
            if (m.gameObject.name == "XR Interaction Setup" || m.gameObject.name == "InputActionManager")
            {
                GameObject.DestroyImmediate(m.gameObject);
            }
        }

        // Load the fully-featured Starter Assets XR Origin prefab
        var prefabPath = "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            var newOrigin = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            newOrigin.name = "XR Origin (VR)"; // Keep name simple
            newOrigin.tag = "Player"; // PortalGate.cs needs this!
            newOrigin.transform.position = oldPos;
            newOrigin.transform.rotation = Quaternion.Euler(0, 180, 0); // Face planets (-Z)
            
            Debug.Log("Successfully replaced barebones origin with fully-featured XR Origin!");
        }
        else
        {
            Debug.LogError("Could not find XR Origin prefab at: " + prefabPath);
            return;
        }

        EditorSceneManager.SaveScene(scene);
        
        // Build the APK
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

        Debug.Log("Starting Automated VR Origin Replacement Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
