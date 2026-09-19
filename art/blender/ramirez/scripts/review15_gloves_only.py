import argparse
import json
import runpy
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    return parser.parse_args(argv)


def remove_existing_gloves():
    for obj in list(bpy.data.objects):
        if obj.name.startswith("RamirezGlove."):
            bpy.data.objects.remove(obj, do_unlink=True)


def aim_camera(camera, position, target):
    camera.location = position
    direction = Vector(target) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render(scene, camera, out_path, position, target):
    aim_camera(camera, position, target)
    scene.render.filepath = str(out_path)
    bpy.ops.render.render(write_still=True)


def main():
    args = parse_args()
    repo = Path(args.repo)
    script_path = repo / "art" / "blender" / "ramirez" / "scripts" / "rig_and_render_ramirez.py"
    module = runpy.run_path(str(script_path), run_name="ramirez_glove_review15_module")
    add_boxing_glove = module["add_boxing_glove"]
    update_glove_pose = module["update_glove_pose"]
    pose_targets = module["pose_targets"]

    blend_path = repo / "art" / "blender" / "ramirez" / "source" / "ramirez_master.blend"
    bpy.ops.wm.open_mainfile(filepath=str(blend_path))
    rig = bpy.data.objects["RamirezRig"]
    camera = bpy.data.objects["VisualGateCamera"]
    scene = bpy.context.scene

    # Preserve every non-glove state exactly.  Poses and camera are changed only
    # transiently to render evidence, then restored before saving the blend.
    saved_pose = {pb.name: pb.matrix_basis.copy() for pb in rig.pose.bones}
    saved_camera_matrix = camera.matrix_world.copy()
    saved_lens = camera.data.lens
    saved_render = (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    )

    remove_existing_gloves()
    wrist_l = rig.matrix_world @ rig.pose.bones["lowerarm_l"].tail
    wrist_r = rig.matrix_world @ rig.pose.bones["lowerarm_r"].tail
    gloves = {
        "L": add_boxing_glove("RamirezGlove.L", wrist_l, 1),
        "R": add_boxing_glove("RamirezGlove.R", wrist_r, -1),
    }
    update_glove_pose(rig, gloves["L"], "lowerarm_l")
    update_glove_pose(rig, gloves["R"], "lowerarm_r")

    review_root = repo / "art" / "blender" / "ramirez" / "renders" / "review15-gloves-only"
    static_out = review_root / "static-closeup"
    combat_out = review_root / "combat-closeup"
    static_out.mkdir(parents=True, exist_ok=True)
    combat_out.mkdir(parents=True, exist_ok=True)

    scene.render.resolution_x = 900
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    camera.data.lens = 78.0

    # Static close-ups: existing pose only, with evidence camera moved closer.
    static_views = [
        ("01_front_closeup.png", (0.0, -1.95, 1.47), (0.0, -0.13, 1.42)),
        ("02_three_quarter_front_closeup.png", (1.20, -1.58, 1.50), (0.0, -0.11, 1.42)),
        ("03_side_closeup.png", (1.92, -0.08, 1.48), (0.0, -0.08, 1.40)),
    ]
    for filename, position, target in static_views:
        render(scene, camera, static_out / filename, position, target)

    # Required guard/jab/cross evidence reuses the already-approved pose logic.
    combat_views = [
        ("guard", "01_neutral_guard_closeup.png", (0.48, -2.02, 1.49), (0.0, -0.16, 1.42)),
        ("jab", "02_jab_closeup.png", (0.42, -2.45, 1.49), (0.0, -0.34, 1.40)),
        ("cross", "03_cross_closeup.png", (-0.42, -2.45, 1.49), (0.0, -0.34, 1.40)),
    ]
    for pose_name, filename, position, target in combat_views:
        pose_targets(rig, gloves, pose_name)
        render(scene, camera, combat_out / filename, position, target)

    # Restore pose/camera/render state byte-for-byte at the object-property level.
    for bone_name, matrix_basis in saved_pose.items():
        rig.pose.bones[bone_name].matrix_basis = matrix_basis
    bpy.context.view_layer.update()
    update_glove_pose(rig, gloves["L"], "lowerarm_l")
    update_glove_pose(rig, gloves["R"], "lowerarm_r")
    camera.matrix_world = saved_camera_matrix
    camera.data.lens = saved_lens
    (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    ) = saved_render
    bpy.context.view_layer.update()

    left = gloves["L"]["main"]
    cuff = gloves["L"]["cuff"]
    report = {
        "scope": "GLOVES_ONLY",
        "review_root": str(review_root),
        "shape_profile": left.get("shape_profile"),
        "fist_curl_drop_m": round(float(left.get("fist_curl_drop_m", 0.0)), 4),
        "knuckle_to_wrist_width_ratio": round(
            float(left.get("knuckle_half_width_m", 0.0))
            / max(float(left.get("wrist_half_width_m", 1.0)), 1e-9),
            3,
        ),
        "glove_dimensions_m": [round(float(v), 4) for v in left.dimensions],
        "cuff_dimensions_m": [round(float(v), 4) for v in cuff.dimensions],
        "automated_prechecks": {
            "G01_non_round_structured_profile": (
                left.get("shape_profile") == "hooked_boxing_loft_v15"
                and float(left.get("knuckle_half_width_m", 0.0))
                / max(float(left.get("wrist_half_width_m", 1.0)), 1e-9) >= 1.65
            ),
            "G02_downward_fist_curl_geometry": float(left.get("fist_curl_drop_m", 0.0)) >= 0.065,
            "G04_white_cuff_present_and_substantial": (
                gloves["L"]["cuff"].active_material is not None
                and gloves["L"]["cuff"].active_material.name == "WrapWhite"
                and float(cuff.dimensions.z) >= 0.095
            ),
        },
    }
    (review_root / "automated-glove-precheck.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8", newline="\n"
    )
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
