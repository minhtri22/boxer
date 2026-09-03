using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoxerP0.Editor
{
    public static class Phase0SceneBuilder
    {
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new("Boxer P0 Bootstrap");
            root.AddComponent<BoxerBootstrap>();
            string scenePath = "Assets/Scenes/Phase0Boxer.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

            PlayerSettings.productName = "Boxer P0";
            PlayerSettings.companyName = "Boxer Research";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Debug.Log($"P0_SCENE_BUILT={scenePath}");
        }

        public static void BuildWindowsPlayer()
        {
            Build();
            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string buildDir = Path.Combine(repoRoot, "evidence", "phase0", "EDITOR", "windows-build");
            Directory.CreateDirectory(buildDir);
            string exe = Path.Combine(buildDir, "BoxerP0.exe");
            BuildPlayerOptions options = new()
            {
                scenes = new[] { "Assets/Scenes/Phase0Boxer.unity" },
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception($"Windows build failed: {report.summary.result} / {report.summary.totalErrors} errors");
            }

            File.WriteAllText(
                Path.Combine(buildDir, "build-metadata.txt"),
                $"evidence=EDITOR\nunity={Application.unityVersion}\nplatform=Windows x64\nresult={report.summary.result}\nsize_bytes={report.summary.totalSize}\n");
            Debug.Log($"P0_WINDOWS_BUILD={exe}");
        }
    }
}
