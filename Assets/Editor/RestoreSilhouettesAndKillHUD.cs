using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class RestoreSilhouettesAndKillHUD
{
    [MenuItem("VR/Restore True Silhouettes")]
    public static void DoFix()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        
        // 1. Kill HUD completely
        var oldVisor = GameObject.Find("VisorHUD_Canvas");
        if (oldVisor != null) Object.DestroyImmediate(oldVisor);
        
        // 2. Restore true space skybox
        Material spaceSkybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/DinV/Dynamic Space Background/Sprites/UzayArkaplan.mat");
        if (spaceSkybox != null)
        {
            RenderSettings.skybox = spaceSkybox;
            Debug.Log("Restored Skybox to UzayArkaplan!");
        }

        // 3. Spawn Distant Silhouettes
        // Clean old ones if they exist
        var oldSil1 = GameObject.Find("DistantSilhouette_1");
        if (oldSil1 != null) Object.DestroyImmediate(oldSil1);
        var oldSil2 = GameObject.Find("DistantSilhouette_2");
        if (oldSil2 != null) Object.DestroyImmediate(oldSil2);

        Material mat1 = AssetDatabase.LoadAssetAtPath<Material>("Assets/düz.mat");
        Material mat2 = AssetDatabase.LoadAssetAtPath<Material>("Assets/elips.mat");

        if (mat1 != null)
        {
            GameObject quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad1.name = "DistantSilhouette_1";
            quad1.transform.position = new Vector3(0, 0, 4500f);
            quad1.transform.localScale = new Vector3(5000f, 5000f, 1f);
            // Face the origin
            quad1.transform.rotation = Quaternion.LookRotation((Vector3.zero - quad1.transform.position).normalized);
            var mr = quad1.GetComponent<MeshRenderer>();
            mr.material = mat1;
            // Disable colliders so it doesn't block rays
            Object.DestroyImmediate(quad1.GetComponent<Collider>());
        }

        if (mat2 != null)
        {
            GameObject quad2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad2.name = "DistantSilhouette_2";
            quad2.transform.position = new Vector3(0, 0, -4500f);
            quad2.transform.localScale = new Vector3(5000f, 5000f, 1f);
            // Face the origin
            quad2.transform.rotation = Quaternion.LookRotation((Vector3.zero - quad2.transform.position).normalized);
            var mr = quad2.GetComponent<MeshRenderer>();
            mr.material = mat2;
            Object.DestroyImmediate(quad2.GetComponent<Collider>());
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

        Debug.Log("Starting VR 3D Build Final-Final...");
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed!");
    }
}
