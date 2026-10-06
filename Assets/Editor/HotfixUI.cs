using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.EventSystems;

public class HotfixUI
{
    [MenuItem("VR/Hotfix UI And Controllers")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. Fix Controllers Tracking (The old code created actions but didn't enable them!)
        var left = GameObject.Find("Left Controller");
        if (left != null)
        {
            var actionCtrl = left.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
            if (actionCtrl != null)
            {
                // Force XRI Default Input Actions instead of manual broken bindings
                string assetPath = "Packages/com.unity.xr.interaction.toolkit/Runtime/Input/XRI Default Input Actions.inputactions";
                var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(assetPath);
                if (inputAsset != null)
                {
                    actionCtrl.positionAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI LeftHand Interaction/Position"));
                    actionCtrl.rotationAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI LeftHand Interaction/Rotation"));
                    actionCtrl.selectAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI LeftHand Interaction/Select"));
                    actionCtrl.activateAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI LeftHand Interaction/Activate"));
                    actionCtrl.uiPressAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI LeftHand Interaction/UI Press"));
                }
            }
        }
        
        var right = GameObject.Find("Right Controller");
        if (right != null)
        {
            var actionCtrl = right.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
            if (actionCtrl != null)
            {
                string assetPath = "Packages/com.unity.xr.interaction.toolkit/Runtime/Input/XRI Default Input Actions.inputactions";
                var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(assetPath);
                if (inputAsset != null)
                {
                    actionCtrl.positionAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI RightHand Interaction/Position"));
                    actionCtrl.rotationAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI RightHand Interaction/Rotation"));
                    actionCtrl.selectAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI RightHand Interaction/Select"));
                    actionCtrl.activateAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI RightHand Interaction/Activate"));
                    actionCtrl.uiPressAction = new UnityEngine.InputSystem.InputActionProperty(inputAsset.FindAction("XRI RightHand Interaction/UI Press"));
                }
            }
        }

        // 2. Add InputActionManager to the rig
        var rig = GameObject.Find("XR Origin (VR)");
        if (rig != null)
        {
            var inputManager = rig.GetComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>();
            if (inputManager == null) inputManager = rig.AddComponent<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>();
            
            string assetPath = "Packages/com.unity.xr.interaction.toolkit/Runtime/Input/XRI Default Input Actions.inputactions";
            var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(assetPath);
            if (inputAsset != null)
            {
                inputManager.actionAssets = new System.Collections.Generic.List<UnityEngine.InputSystem.InputActionAsset> { inputAsset };
            }
        }

        // 3. Add EventSystem with XRUIInputModule to the scene (So UI clicking actually works)
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            var xrUI = esObj.AddComponent<XRUIInputModule>();
            
            // Map UI clicks
            string assetPath = "Packages/com.unity.xr.interaction.toolkit/Runtime/Input/XRI Default Input Actions.inputactions";
            var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(assetPath);
            if (inputAsset != null)
            {
                xrUI.leftClickAction = new UnityEngine.InputSystem.InputActionReference();
                xrUI.leftClickAction = UnityEngine.InputSystem.InputActionReference.Create(inputAsset.FindAction("XRI UI/Click"));
                xrUI.pointAction = UnityEngine.InputSystem.InputActionReference.Create(inputAsset.FindAction("XRI UI/Point"));
                xrUI.scrollWheelAction = UnityEngine.InputSystem.InputActionReference.Create(inputAsset.FindAction("XRI UI/ScrollWheel"));
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Build
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
