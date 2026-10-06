using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class FeedbackFixes4
{
    [MenuItem("VR/Feedback Fixes 4")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. REMOVE HELMET HUD (Black Square)
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            var hud = cam.GetComponent<HelmetHUD>();
            if (hud != null) Object.DestroyImmediate(hud);
            
            var canvasHud = GameObject.Find("HelmetHUD_Canvas");
            if (canvasHud != null) Object.DestroyImmediate(canvasHud);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
