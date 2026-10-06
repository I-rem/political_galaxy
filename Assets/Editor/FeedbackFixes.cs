using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class FeedbackFixes
{
    [MenuItem("VR/Feedback Fixes")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. FIX LASERS (Make them red/blue instead of pink)
        Material redMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        redMat.color = Color.red;
        AssetDatabase.CreateAsset(redMat, "Assets/RedLaser.mat");

        Material blueMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        blueMat.color = Color.cyan;
        AssetDatabase.CreateAsset(blueMat, "Assets/BlueLaser.mat");

        var leftLine = GameObject.Find("Left Controller")?.GetComponent<LineRenderer>();
        if (leftLine != null) leftLine.material = blueMat;

        var rightLine = GameObject.Find("Right Controller")?.GetComponent<LineRenderer>();
        if (rightLine != null) rightLine.material = redMat;

        // 2. FIX STARTING ROTATION
        var rig = GameObject.Find("XR Origin (VR)");
        if (rig != null)
        {
            // Reset to 0,0,0 so they face Z (where planets are)
            rig.transform.rotation = Quaternion.identity;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
