import argparse
import hashlib
import json
import runpy
import struct
import sys
from pathlib import Path

import bpy
from mathutils import Vector


GLOVE_PREFIX = "RamirezGlove."
GLOVE_MATERIALS = {"GloveRed", "GloveSeam", "GloveGold", "WrapWhite"}


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    return parser.parse_args(argv)


def matrix_values(matrix):
    return tuple(round(float(v), 9) for row in matrix for v in row)


def vector_values(vector):
    return tuple(round(float(v), 9) for v in vector)


def mesh_digest(obj):
    if obj.type != "MESH":
        return None
    h = hashlib.sha256()
    for vertex in obj.data.vertices:
        h.update(struct.pack("<3d", float(vertex.co.x), float(vertex.co.y), float(vertex.co.z)))
    for poly in obj.data.polygons:
        h.update(struct.pack("<I", len(poly.vertices)))
        for index in poly.vertices:
            h.update(struct.pack("<I", int(index)))
    return h.hexdigest()


def is_glove_object(obj):
    mats = {
        mat.name for mat in getattr(obj.data, "materials", []) if mat is not None
    }
    parent_is_glove = obj.parent is not None and obj.parent.name.startswith(GLOVE_PREFIX)
    return obj.name.startswith(GLOVE_PREFIX) or parent_is_glove or bool(mats & GLOVE_MATERIALS)


def non_glove_snapshot():
    objects = {}
    for obj in sorted(bpy.data.objects, key=lambda item: item.name):
        if is_glove_object(obj):
            continue
        entry = {
            "type": obj.type,
            # Authoring state is local. matrix_world is evaluated through the
            # rig/depsgraph and can differ by float noise after pose restore.
            "matrix_basis": matrix_values(obj.matrix_basis),
            "mesh_digest": mesh_digest(obj),
        }
        if obj.type == "LIGHT":
            entry["energy"] = round(float(obj.data.energy), 9)
            entry["size"] = round(float(getattr(obj.data, "size", 0.0)), 9)
        if obj.type == "CAMERA":
            entry["lens"] = round(float(obj.data.lens), 9)
            entry["camera_type"] = obj.data.type
            entry["ortho_scale"] = round(float(obj.data.ortho_scale), 9)
        objects[obj.name] = entry
    rig = bpy.data.objects.get("RamirezRig")
    pose = {}
    if rig is not None:
        for bone in sorted(rig.pose.bones, key=lambda item: item.name):
            pose[bone.name] = {
                "location": vector_values(bone.location),
                "scale": vector_values(bone.scale),
                "rotation_mode": bone.rotation_mode,
                "rotation_euler": vector_values(bone.rotation_euler),
                "rotation_quaternion": vector_values(bone.rotation_quaternion),
                "rotation_axis_angle": vector_values(bone.rotation_axis_angle),
            }
    materials = {}
    for mat in sorted(bpy.data.materials, key=lambda item: item.name):
        if mat.name in GLOVE_MATERIALS:
            continue
        materials[mat.name] = tuple(round(float(v), 8) for v in mat.diffuse_color)
    return {"objects": objects, "pose": pose, "materials": materials}


def snapshot_diff(before, after):
    diffs = []
    for section in ("objects", "pose", "materials"):
        left = before.get(section, {})
        right = after.get(section, {})
        for key in sorted(set(left) | set(right)):
            if key not in left:
                diffs.append({"section": section, "key": key, "change": "added"})
            elif key not in right:
                diffs.append({"section": section, "key": key, "change": "removed"})
            elif left[key] != right[key]:
                diffs.append({
                    "section": section,
                    "key": key,
                    "change": "changed",
                    "before": left[key],
                    "after": right[key],
                })
    return diffs


def remove_existing_gloves():
    for obj in list(bpy.data.objects):
        if is_glove_object(obj):
            bpy.data.objects.remove(obj, do_unlink=True)


def capture_pose_channels(rig):
    return {
        bone.name: {
            "location": bone.location.copy(),
            "scale": bone.scale.copy(),
            "rotation_mode": bone.rotation_mode,
            "rotation_euler": bone.rotation_euler.copy(),
            "rotation_quaternion": bone.rotation_quaternion.copy(),
            "rotation_axis_angle": tuple(float(v) for v in bone.rotation_axis_angle),
        }
        for bone in rig.pose.bones
    }


def restore_pose_channels(rig, saved):
    for name, state in saved.items():
        bone = rig.pose.bones[name]
        bone.location = state["location"]
        bone.scale = state["scale"]
        bone.rotation_mode = state["rotation_mode"]
        if bone.rotation_mode == "QUATERNION":
            bone.rotation_quaternion = state["rotation_quaternion"]
        elif bone.rotation_mode == "AXIS_ANGLE":
            bone.rotation_axis_angle = state["rotation_axis_angle"]
        else:
            bone.rotation_euler = state["rotation_euler"]


def aim_camera(camera, position, target):
    camera.location = position
    direction = Vector(target) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render(scene, camera, path, position, target, ortho_scale):
    aim_camera(camera, position, target)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho_scale
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def main():
    args = parse_args()
    repo = Path(args.repo)
    module = runpy.run_path(
        str(repo / "art" / "blender" / "ramirez" / "scripts" / "rig_and_render_ramirez.py"),
        run_name="ramirez_glove_review16_module",
    )
    add_boxing_glove = module["add_boxing_glove"]
    update_glove_pose = module["update_glove_pose"]
    pose_targets = module["pose_targets"]

    blend_path = repo / "art" / "blender" / "ramirez" / "source" / "ramirez_master.blend"
    bpy.ops.wm.open_mainfile(filepath=str(blend_path))
    rig = bpy.data.objects["RamirezRig"]
    camera = bpy.data.objects["VisualGateCamera"]
    scene = bpy.context.scene

    baseline = non_glove_snapshot()
    saved_camera_matrix = camera.matrix_world.copy()
    saved_lens = float(camera.data.lens)
    saved_camera_type = camera.data.type
    saved_ortho_scale = float(camera.data.ortho_scale)
    saved_render = (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    )
    saved_pose = capture_pose_channels(rig)

    remove_existing_gloves()
    wrist_l = rig.matrix_world @ rig.pose.bones["lowerarm_l"].tail
    wrist_r = rig.matrix_world @ rig.pose.bones["lowerarm_r"].tail
    gloves = {
        "L": add_boxing_glove("RamirezGlove.L", wrist_l, 1),
        "R": add_boxing_glove("RamirezGlove.R", wrist_r, -1),
    }
    update_glove_pose(rig, gloves["L"], "lowerarm_l")
    update_glove_pose(rig, gloves["R"], "lowerarm_r")

    out = repo / "art" / "blender" / "ramirez" / "renders" / "review16-glove-fix"
    out.mkdir(parents=True, exist_ok=True)
    scene.render.resolution_x = 1024
    scene.render.resolution_y = 1024
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    camera.data.type = "ORTHO"

    # Static evidence uses the exact saved pose; only the evidence camera moves.
    static_views = [
        ("glove_front_closeup.png", (0.0, -1.55, 1.47), (0.0, -0.13, 1.43), 0.62),
        ("glove_three_quarter_closeup.png", (0.92, -1.30, 1.50), (0.0, -0.11, 1.43), 0.62),
        # Side view is centered on the fists (negative world-Y), not the torso,
        # so the complete curled striking mass and white cuff stay in frame.
        ("glove_side_closeup.png", (1.58, -0.43, 1.49), (0.0, -0.43, 1.42), 0.64),
    ]
    for filename, position, target, scale in static_views:
        render(scene, camera, out / filename, position, target, scale)

    combat_views = [
        ("guard", "neutral_guard_closeup.png", (0.38, -1.72, 1.50), (0.0, -0.16, 1.43), 0.74),
        ("jab", "jab_closeup.png", (0.34, -2.05, 1.50), (0.0, -0.36, 1.42), 0.78),
        ("cross", "cross_closeup.png", (-0.34, -2.05, 1.50), (0.0, -0.36, 1.42), 0.78),
    ]
    for pose_name, filename, position, target, scale in combat_views:
        pose_targets(rig, gloves, pose_name)
        render(scene, camera, out / filename, position, target, scale)

    # Restore every non-glove transient used for evidence before saving.
    restore_pose_channels(rig, saved_pose)
    bpy.context.view_layer.update()
    update_glove_pose(rig, gloves["L"], "lowerarm_l")
    update_glove_pose(rig, gloves["R"], "lowerarm_r")
    camera.matrix_world = saved_camera_matrix
    camera.data.lens = saved_lens
    camera.data.type = saved_camera_type
    camera.data.ortho_scale = saved_ortho_scale
    (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    ) = saved_render
    bpy.context.view_layer.update()

    after = non_glove_snapshot()
    scope_diffs = snapshot_diff(baseline, after)
    scope_lock_pass = not scope_diffs
    left = gloves["L"]["main"]
    cuff = gloves["L"]["cuff"]
    neck = gloves["L"]["neck"]
    report = {
        "scope": "GLOVE_ONLY",
        "scope_lock_non_glove_unchanged": scope_lock_pass,
        "scope_diffs": scope_diffs,
        "shape_profile": left.get("shape_profile"),
        "distal_terminal": left.get("distal_terminal"),
        "fist_curl_drop_m": round(float(left.get("fist_curl_drop_m", 0.0)), 4),
        "knuckle_to_wrist_width_ratio": round(
            float(left.get("knuckle_half_width_m", 0.0))
            / max(float(left.get("wrist_half_width_m", 1.0)), 1e-9),
            3,
        ),
        "terminal_ring_half_width_m": round(float(left.get("terminal_ring_half_width_m", 0.0)), 4),
        "cuff_length_m": round(float(cuff.dimensions.z), 4),
        "cuff_width_m": round(float(cuff.dimensions.x), 4),
        "neck_width_m": round(float(neck.dimensions.x), 4),
        "automated_geometry_checks": {
            "G01_no_flat_cut_terminal": left.get("distal_terminal") == "single_rounded_pole_no_planar_cap",
            "G02_structured_clenched_fist": (
                left.get("shape_profile") == "rounded_curled_fist_v16"
                and float(left.get("knuckle_half_width_m", 0.0))
                / max(float(left.get("wrist_half_width_m", 1.0)), 1e-9) >= 1.7
            ),
            "G03_downward_curl": float(left.get("fist_curl_drop_m", 0.0)) >= 0.12,
            "G04_white_cuff_visible": (
                cuff.active_material is not None
                and cuff.active_material.name == "WrapWhite"
                and float(cuff.dimensions.z) >= 0.095
            ),
            "G05_cuff_tighter_than_red_neck": float(cuff.dimensions.x) < float(neck.dimensions.x),
        },
    }
    (out / "automated-glove-precheck.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8", newline="\n"
    )
    if not scope_lock_pass:
        raise RuntimeError("SCOPE LOCK FAILED: non-glove scene state changed")
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
