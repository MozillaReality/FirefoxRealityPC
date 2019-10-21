// C# example.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public class FxRBuild
{
    private static string BuildOutputExePath;
    private static bool BuildSuccessfull;

    [MenuItem("FxR/Windows Build")]
    public static void BuildGame()
    {
        BuildSuccessfull = false;
        // Get filename.
        string nightlyBuildPath =
            EditorUtility.OpenFolderPanel("Choose top-level folder of Nightly Build", "", "firefox");
        if (string.IsNullOrEmpty(nightlyBuildPath)) return;
        Debug.Log("Path to nightly build selected: " + nightlyBuildPath);
        if (!File.Exists(Path.Combine(nightlyBuildPath, "firefox.exe")))
        {
            Debug.LogError("No Firefox executable found in provided nightly build path!");
            return;
        }

        string saveFolder = EditorUtility.SaveFolderPanel("Choose folder to save built executable", "", "");
        if (string.IsNullOrEmpty(saveFolder)) return;
//        string[] levels = new string[] {"Assets/Scene1.unity", "Assets/Scene2.unity"};
        Debug.Log("Path to output build: " + saveFolder);

        List<string> scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            scenes.Add(scene.path);
            Debug.Log("path: " + scene.path);
        }

        // Build player
        BuildOutputExePath = Path.Combine(saveFolder, "FirefoxReality.exe");
        Debug.Log("Path to output exe: " + BuildOutputExePath);
        var buildReport = BuildPipeline.BuildPlayer(scenes.ToArray(), BuildOutputExePath, BuildTarget.StandaloneWindows64,
            BuildOptions.None);

        if (buildReport.summary.result == BuildResult.Succeeded)
        {
            // Copy nightly build to streaming assets directory
            string streamingAssetsDestination = Path.Combine(saveFolder, "FirefoxReality_Data", "StreamingAssets", "firefox");
            DirectoryCopy(nightlyBuildPath, streamingAssetsDestination, true);
            BuildSuccessfull = true;
            Debug.Log("Build successful.");
        }
        else
        {
            Debug.Log("Build failed.");
        }
    }

    [MenuItem("FxR/Windows Build And Run")]
    public static void BuildAndRunGame()
    {
        BuildGame();

        if (BuildSuccessfull)
        {
            // Run the game 
            Process proc = new Process();
            proc.StartInfo.FileName = BuildOutputExePath;
            proc.Start();
        }
    }

    // From: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }
}