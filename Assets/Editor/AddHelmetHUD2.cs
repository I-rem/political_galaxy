using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class AddHelmetHUD2
{
    [MenuItem("VR/Add Helmet HUD 2")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        
        if (cam != null)
        {
            // Varsa eskisini temizle
            var oldHud = cam.GetComponent<HelmetHUD>();
            if (oldHud != null) Object.DestroyImmediate(oldHud);

            // Yenisini ekle
            cam.gameObject.AddComponent<HelmetHUD>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        
        Debug.Log("HUD Added to Camera!");
    }
}
