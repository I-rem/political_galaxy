using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class FinalVRFix
{
    [MenuItem("VR/Apply Final Fixes & Build")]
    public static void ApplyAndBuild()
    {
        AssetDatabase.Refresh(); // Ensure the newly copied Starter Assets are registered

        var scene = EditorSceneManager.GetActiveScene();
        
        // 1. Rotate XR Origin 180 degrees
        var origin = GameObject.Find("XR Origin (VR)");
        if (origin != null)
        {
            origin.transform.rotation = Quaternion.Euler(0, 180, 0);
            Debug.Log("Rotated XR Origin 180 degrees to face planets.");
        }
        else
        {
            Debug.LogError("XR Origin not found!");
        }

        // 2. Fix InputActionManager (Add Starter Assets)
        var actionManager = Object.FindAnyObjectByType<InputActionManager>();
        if (actionManager != null)
        {
            var assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/XRI Default Input Actions.inputactions";
            var inputActionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            
            if (inputActionAsset != null)
            {
                if (actionManager.actionAssets == null) 
                {
                    actionManager.actionAssets = new System.Collections.Generic.List<InputActionAsset>();
                }

                if (!actionManager.actionAssets.Contains(inputActionAsset))
                {
                    actionManager.actionAssets.Add(inputActionAsset);
                    Debug.Log("Successfully assigned XRI Default Input Actions to InputActionManager!");
                    EditorUtility.SetDirty(actionManager);
                }
            }
            else
            {
                Debug.LogError("Failed to load InputActionAsset from " + assetPath);
            }
        }
        else
        {
            Debug.LogError("InputActionManager not found in scene!");
        }

        EditorSceneManager.SaveScene(scene);
        
        // 3. Build the APK
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

        Debug.Log("Starting Automated VR Fix Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
