using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class FinalFixScene
{
    [MenuItem("VR/Final Fix Scene")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // Remove SpacePlayer
        var spacePlayer = GameObject.Find("SpacePlayer");
        if (spacePlayer != null) Object.DestroyImmediate(spacePlayer);

        // Remove old Main Camera
        var oldCam = GameObject.Find("Main Camera");
        if (oldCam != null) Object.DestroyImmediate(oldCam);

        GameObject rig = new GameObject("XR Origin (VR)");
        var xrOrigin = rig.AddComponent<Unity.XR.CoreUtils.XROrigin>();
        var camOffset = new GameObject("Camera Offset");
        camOffset.transform.SetParent(rig.transform, false);
        var mainCam = new GameObject("Main Camera");
        mainCam.transform.SetParent(camOffset.transform, false);
        var camComp = mainCam.AddComponent<Camera>();
        mainCam.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        xrOrigin.CameraFloorOffsetObject = camOffset;
        xrOrigin.Camera = camComp;
        
        var leftCtrl = new GameObject("Left Controller");
        leftCtrl.transform.SetParent(camOffset.transform, false);
        leftCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
        leftCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        leftCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();

        var rightCtrl = new GameObject("Right Controller");
        rightCtrl.transform.SetParent(camOffset.transform, false);
        rightCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
        rightCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
        rightCtrl.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();

        rig.transform.rotation = Quaternion.Euler(0, 180, 0);
        rig.AddComponent<VRFlightController>();
        
        // Setup Inputs
        AutoFixVR.FixVR();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        
        Debug.Log("Scene fixed and saved!");

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
