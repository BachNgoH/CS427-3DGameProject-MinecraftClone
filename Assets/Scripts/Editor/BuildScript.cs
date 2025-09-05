using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        // Define build settings
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        // Get scenes from Build Settings
        buildPlayerOptions.scenes = GetScenePaths();
        
        // Set target platform and architecture
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.locationPathName = "Builds/Windows/Minecraft4Unity.exe";
        
        // Build options
        buildPlayerOptions.options = BuildOptions.None; // For release
        // buildPlayerOptions.options = BuildOptions.Development; // For development
        
        // Start build
        Debug.Log("Starting Windows build...");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        // Check build result
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! Size: {report.summary.totalSize} bytes");
            Debug.Log($"Build location: {buildPlayerOptions.locationPathName}");
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors");
        }
    }
    
    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
    
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        // Windows
        BuildWindows();
        
        // Mac (if you have Mac build support)
        // BuildMac();
        
        // Linux (if you have Linux build support)  
        // BuildLinux();
        
        Debug.Log("All builds completed!");
    }
}