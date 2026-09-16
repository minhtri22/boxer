using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Round2Build
    {
        public static void Web()
        {
            string sha=Environment.GetEnvironmentVariable("BOXER_BUILD_MARKER");
            if(string.IsNullOrEmpty(sha)||sha.Length!=40) throw new Exception("Full committed source SHA required");
            string scenePath=Path.Combine(Application.dataPath,"Scenes/Phase0Boxer.unity");
            string projectSettingsPath=Path.GetFullPath(Path.Combine(Application.dataPath,"../ProjectSettings/ProjectSettings.asset"));
            byte[] sceneBefore=File.ReadAllBytes(scenePath);
            byte[] projectSettingsBefore=File.ReadAllBytes(projectSettingsPath);
            string versionBefore=PlayerSettings.bundleVersion;
            var compressionBefore=PlayerSettings.WebGL.compressionFormat;
            string templateBefore=PlayerSettings.WebGL.template;
            try
            {
                Phase0SceneBuilder.Build();
                bool ev=Environment.GetEnvironmentVariable("BOXER_VISUAL_STAGE")=="P1-EV";
                PlayerSettings.bundleVersion=(ev?"ev-":"r2-")+sha;
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.template="PROJECT:BoxerP0Mobile";
                AssetDatabase.SaveAssets();
                string repo=Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
                string output=Path.Combine(repo,"builds/web/boxer-round2");
                if(Directory.Exists(output)) throw new Exception("Clean build requires an absent output directory");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{"Assets/Scenes/Phase0Boxer.unity"},
                    locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None });
                if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Round2 WebGL failed: "+report.summary.result);
                var metadata=new StringBuilder();
                metadata.AppendLine("source_sha="+sha);
                metadata.AppendLine("productVersion="+PlayerSettings.bundleVersion);
                metadata.AppendLine("unity="+Application.unityVersion);
                metadata.AppendLine("result="+report.summary.result);
                metadata.AppendLine("build_seconds="+report.summary.totalTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                metadata.AppendLine("total_bytes="+report.summary.totalSize);
                metadata.AppendLine("contact=shared_anatomical_pose_relative_sphere_sweep_240Hz");
                metadata.AppendLine("presentation="+(ev?"P1_EV_reference_textured_articulated_3d_rig_no_bottom_controls":"world_space_articulated_body_P1V_HUD_only"));
                metadata.AppendLine("desktop_validation=explicit_query_flag_synthetic_no_sensor_claim");
                using var hash=SHA256.Create();
                foreach(string file in Directory.GetFiles(output,"*",SearchOption.AllDirectories))
                {
                    using var stream=File.OpenRead(file);
                    metadata.AppendLine(Path.GetRelativePath(output,file).Replace('\\','/')+"="+BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant());
                }
                File.WriteAllText(Path.Combine(output,"provenance.txt"),metadata.ToString());
                UnityEngine.Debug.Log("ROUND2_WEB_BUILD_SUCCESS\n"+metadata);
            }
            finally
            {
                PlayerSettings.bundleVersion=versionBefore;
                PlayerSettings.WebGL.compressionFormat=compressionBefore;
                PlayerSettings.WebGL.template=templateBefore;
                AssetDatabase.SaveAssets();
                File.WriteAllBytes(scenePath,sceneBefore);
                File.WriteAllBytes(projectSettingsPath,projectSettingsBefore);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }
    }
}
