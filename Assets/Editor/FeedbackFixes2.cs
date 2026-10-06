using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class FeedbackFixes2
{
    [MenuItem("VR/Feedback Fixes 2")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. ADD TRACKED DEVICE GRAPHIC RAYCASTER TO ALL UI CANVASES SO LASERS WORK
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            }
        }
        
        // 2. FIX HELMET HUD PLANE DISTANCE SO IT DOESNT CAST SHADOW OVER WRIST
        var hud = Object.FindAnyObjectByType<HelmetHUD>();
        if (hud != null)
        {
            var canvas = hud.GetComponent<Canvas>();
            if (canvas != null) canvas.planeDistance = 0.05f; // Very close to eyes to not clip wrists
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
