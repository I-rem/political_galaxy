using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class VRBatchSetup
{
    public static void RunBatch()
    {
        Debug.Log("Starting Automated VR Batch Setup...");
        
        // 1. Remove FPS
        var fps = GameObject.FindObjectOfType<SpaceFPSController>();
        if (fps != null)
        {
            Debug.Log("Removing SpaceFPSController...");
            GameObject.DestroyImmediate(fps.gameObject);
        }

        // 2. We can try to instantiate the XR Origin
        EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (VR)");
        var xrOrigin = GameObject.Find("XR Origin (VR)");
        
        if (xrOrigin != null)
        {
            xrOrigin.tag = "Player";
            xrOrigin.transform.position = new Vector3(0, 1.5f, 0); 
            Debug.Log("Successfully created and configured XR Origin.");
        }
        else
        {
            Debug.LogError("Failed to create XR Origin in batch mode.");
        }

        // Save scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Batch setup finished and scene saved.");
    }
}
