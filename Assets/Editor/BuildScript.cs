using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    public static void BuildAPK()
    {
        string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" }; // Adjust if needed
        string buildPath = "Builds/MidtermPolarOriginal.apk";

        Directory.CreateDirectory("Builds");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting VR Build for Original Project...");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Completed with Result: " + report.summary.result);
    }
}
