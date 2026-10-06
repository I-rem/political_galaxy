using UnityEditor;
using UnityEngine;
using System.Linq;

public class TestSampleApi
{
    public static void Check()
    {
        var samples = UnityEditor.PackageManager.UI.Sample.FindByPackage("com.unity.xr.interaction.toolkit", "");
        foreach(var s in samples)
        {
            Debug.Log("Found Sample: " + s.displayName);
        }
    }
}
