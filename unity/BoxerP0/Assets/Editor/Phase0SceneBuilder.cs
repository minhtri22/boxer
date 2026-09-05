using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoxerP0.Editor
{
    public static class Phase0SceneBuilder
    {
        private const int WebTargetFps = 60;

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new("Boxer P0 Bootstrap");
            root.AddComponent<BoxerBootstrap>();
            root.AddComponent<BoxerVisualShell>();
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
                throw new Exception($"Windows build failed: {report.summary.result} / {report.summary.totalErrors} errors");
            }

            File.WriteAllText(
                Path.Combine(buildDir, "build-metadata.txt"),
                $"evidence=EDITOR\nunity={Application.unityVersion}\nplatform=Windows x64\nresult={report.summary.result}\nsize_bytes={report.summary.totalSize}\n");
            Debug.Log($"P0_WINDOWS_BUILD={exe}");
        }

        public static void BuildWebPlayer()
        {
            Build();
            string buildMarker = Environment.GetEnvironmentVariable("BOXER_BUILD_MARKER");
            if (string.IsNullOrWhiteSpace(buildMarker)) buildMarker = "local-web";
            PlayerSettings.bundleVersion = buildMarker.Trim();
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            AssetDatabase.SaveAssets();

            string repoRoot = Directory.GetParent(
                Directory.GetParent(
                    Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
            string buildDir = Path.Combine(repoRoot, "builds", "web", "boxer-p0-web");
            Directory.CreateDirectory(buildDir);

            BuildPlayerOptions options = new()
            {
                scenes = new[] { "Assets/Scenes/Phase0Boxer.unity" },
                locationPathName = buildDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed: {report.summary.result} / {report.summary.totalErrors} errors");
            }

            string buildFilesDir = Path.Combine(buildDir, "Build");
            string dataPath = FindSingleBuildFile(buildFilesDir, "*.data");
            string wasmPath = FindSingleBuildFile(buildFilesDir, "*.wasm");
            string dataSha256 = Sha256(dataPath);
            string wasmSha256 = Sha256(wasmPath);

            string evidenceDir = Path.Combine(repoRoot, "evidence", "phase0", "web-iphone", "WEB_BUILD");
            Directory.CreateDirectory(evidenceDir);
            File.WriteAllText(
                Path.Combine(evidenceDir, "build-metadata.txt"),
                $"evidence=WEB_BUILD\n" +
                $"unity={Application.unityVersion}\n" +
                $"target=WebGL\n" +
                $"result={report.summary.result}\n" +
                $"output={buildDir}\n" +
                $"size_bytes={report.summary.totalSize}\n" +
                $"template=PROJECT:BoxerP0Mobile\n" +
                $"compression=disabled_for_static_pages\n" +
                $"development_build=false\n" +
                $"target_fps={WebTargetFps}\n" +
                $"data_sha256={dataSha256}\n" +
                $"wasm_sha256={wasmSha256}\n" +
                $"diagnostic_overlay=true\n" +
                $"web_telemetry_mode=in_memory_counters_no_csv\n" +
                $"visual_shell=procedural_p0_5\n" +
                $"build_commit={buildMarker}\n");
            Debug.Log($"P0_WEB_BUILD={buildDir} marker={buildMarker} data={dataSha256} wasm={wasmSha256}");
        }

        private static string FindSingleBuildFile(string directory, string pattern)
        {
            string[] files = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
            if (files.Length != 1)
            {
                throw new Exception($"Expected exactly one {pattern} in {directory}, found {files.Length}");
            }
            return files[0];
        }

        private static string Sha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
