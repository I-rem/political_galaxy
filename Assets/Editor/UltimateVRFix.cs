using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Linq;

public class UltimateVRFix
{
    [MenuItem("VR/Ultimate VR Fix & Build")]
    public static void FixAndBuild()
    {
        var scene = EditorSceneManager.GetActiveScene();
        
        // 1. Create / Fix EventSystem
        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var esObj = new GameObject("EventSystem");
            eventSystem = esObj.AddComponent<EventSystem>();
        }
        
        // Ensure it has the correct XR UI Input Module
        var xrInputModule = eventSystem.gameObject.GetComponent<XRUIInputModule>();
        if (xrInputModule == null)
        {
            var oldInput = eventSystem.gameObject.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (oldInput != null) Object.DestroyImmediate(oldInput);
            
            xrInputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }
        
        // 2. Create / Fix InputActionManager
        var actionManager = Object.FindAnyObjectByType<InputActionManager>();
        if (actionManager == null)
        {
            var amObj = new GameObject("InputActionManager");
            actionManager = amObj.AddComponent<InputActionManager>();
        }
        
        var assetPath = "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/XRI Default Input Actions.inputactions";
        var inputActionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
        
        if (inputActionAsset != null)
        {
            if (actionManager.actionAssets == null) 
                actionManager.actionAssets = new System.Collections.Generic.List<InputActionAsset>();
                
            if (!actionManager.actionAssets.Contains(inputActionAsset))
                actionManager.actionAssets.Add(inputActionAsset);
        }
        
        // 3. Make sure XR Interaction Manager exists
        var interactionManager = Object.FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
        if (interactionManager == null)
        {
            var imObj = new GameObject("XR Interaction Manager");
            imObj.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
        }

        // 4. Add physical visible cubes to the controllers so the user physically sees their hands
        var leftController = GameObject.Find("Left Controller");
        if (leftController != null && leftController.transform.Find("VisibleHand") == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "VisibleHand";
            cube.transform.SetParent(leftController.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); // 5cm cube
            Object.DestroyImmediate(cube.GetComponent<Collider>()); // prevent physics issues
        }

        var rightController = GameObject.Find("Right Controller");
        if (rightController != null && rightController.transform.Find("VisibleHand") == null)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "VisibleHand";
            cube.transform.SetParent(rightController.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); // 5cm cube
            Object.DestroyImmediate(cube.GetComponent<Collider>()); // prevent physics issues
        }

        EditorSceneManager.SaveScene(scene);
        
        // Build the APK
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };

        System.IO.Directory.CreateDirectory("Builds");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/MidtermPolar.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting Ultimate VR Fix Build...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
