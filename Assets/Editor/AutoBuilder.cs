using UnityEditor;
using UnityEngine;
using System.Linq;

public class AutoBuilder
{
    public static void BuildAPK()
    {
        Debug.Log("Starting Automated APK Build...");
        
        // Force embedded paths just in case they are unset
        EditorPrefs.SetInt("JdkUseEmbedded", 1);
        EditorPrefs.SetInt("SdkUseEmbedded", 1);
        EditorPrefs.SetInt("NdkUseEmbedded", 1);

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes enabled in Build Settings!");
            return;
        }

        System.IO.Directory.CreateDirectory("Builds");
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/MidtermPolar.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            Debug.LogError("Build failed with " + summary.totalErrors + " errors.");
        }
    }
}
