using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class FeedbackFixes3
{
    [MenuItem("VR/Feedback Fixes 3")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. ADD TRACKED DEVICE GRAPHIC RAYCASTER TO ALL UI CANVASES
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            }
        }
        
        // 2. FIX HELMET HUD PLANE DISTANCE & UNLIT
        var hud = Object.FindAnyObjectByType<HelmetHUD>();
        if (hud != null)
        {
            var canvas = hud.GetComponent<Canvas>();
            if (canvas != null) {
                canvas.planeDistance = 0.02f; // Extreme close so it doesn't cast shadow or clip wrist
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
