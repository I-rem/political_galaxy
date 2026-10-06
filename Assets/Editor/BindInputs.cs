using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class BindInputs
{
    [MenuItem("VR/Fix Inputs & Build")]
    public static void FixAndBuild()
    {
        var scene = EditorSceneManager.GetActiveScene();
        
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
        
        // Build the APK
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" }; // force sample scene

        System.IO.Directory.CreateDirectory("Builds");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/MidtermPolar.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR Bind Inputs Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
