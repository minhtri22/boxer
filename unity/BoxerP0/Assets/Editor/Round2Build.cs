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
                PlayerSettings.bundleVersion=(ev?"ev-":"w1-")+sha;
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
                metadata.AppendLine("art_source_sha=3e4e13c4b4451cb542e068652508ec12e248225f");
                metadata.AppendLine("blend_sha256=419466859d7f119f98d7e69dc2673f0b378dbf5a892f0e2a6d7254cbc974af12");
                metadata.AppendLine("productVersion="+PlayerSettings.bundleVersion);
                metadata.AppendLine("unity="+Application.unityVersion);
                metadata.AppendLine("result="+report.summary.result);
                metadata.AppendLine("build_seconds="+report.summary.totalTime.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                metadata.AppendLine("total_bytes="+report.summary.totalSize);
                metadata.AppendLine("contact=shared_anatomical_pose_relative_sphere_sweep_240Hz");
                metadata.AppendLine("presentation=accepted_ramirez_native_rig_readonly_mapping_no_bottom_controls");
                metadata.AppendLine("vitals=wave1_authoritative_hp_stamina_capacity_quality_recovery_ko");
                metadata.AppendLine("product_loop=home_preview_ring_intro_bell_fight_end_bell_result_rematch");
                metadata.AppendLine("ring_intro=owner_clip_5.166667s_no_ai_no_input_no_vitals_no_clock_epoch_checked_media_completion");
                metadata.AppendLine("ring_audio=owner_clip_time_segment_bell_5.15_end_crowd_0.25_4.85_not_isolated_stems");
                metadata.AppendLine("training=separate_optional_head_footwork_four_punch_families_no_ai_no_score_persist_completion");
                metadata.AppendLine("coach=approved_2d_reference_art_five_native_cards_three_existing_unscored_drills_two_information_modules_no_stat_upgrade");
                metadata.AppendLine("hp_ko_balance=HP-KO-001_OWNER_HOLD_COMBAT_RULES_UNCHANGED");
                metadata.AppendLine("training_guides=native_boot_head_glove_icons_directional_light_trails_repeat_tap_training_only");
                metadata.AppendLine("training_glass=live_scene_separable_blur_reduced_256x512_max_card_only_no_fight_postprocess");
                metadata.AppendLine("player_arms=continuous_two_segment_tucked_pov_elbow_visual_only_shared_shoulder_glove_anchors");
                metadata.AppendLine("punch_feel=per_intent_ballistic_shared_pose_fast_pull_guard_settle_60ms_swipe_no_combo_buffer");
                metadata.AppendLine("combat_timeline=phase_boundary_substeps_max_240Hz_contact_vitals_same_timeline_carry_over_no_global_hitstop");
                metadata.AppendLine("impact=resolved_receipt_head_body_block_layered_sfx_bounded_native_bone_reaction_material_pressure_35ms_no_camera_shake");
                metadata.AppendLine("combat_fairness=multiply_high_ai_choice_shared_all_volume_contact_neutral_straight_engagement_range");
                metadata.AppendLine("footwork=pov_facing_relative_axes_shared_forward_ring_extent_minimum_torso_separation");
                foreach(string side in new[]{"Left","Right"})
                {
                    using var gloveHash=SHA256.Create();
                    using var gloveStream=File.OpenRead(Path.Combine(Application.dataPath,"Resources/Boxer3D/PlayerPOVGlove_"+side+"_W1.fbx"));
                    metadata.AppendLine("player_glove_"+side.ToLowerInvariant()+"_sha256="+BitConverter.ToString(gloveHash.ComputeHash(gloveStream)).Replace("-","").ToLowerInvariant());
                }
                metadata.AppendLine("desktop_validation=explicit_query_flag_synthetic_no_sensor_claim");
                using var hash=SHA256.Create();
                using(var stream=File.OpenRead(Path.Combine(Application.dataPath,"Resources/Boxer3D/Ramirez_UAT3.fbx")))
                    metadata.AppendLine("fbx_sha256="+BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant());
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
