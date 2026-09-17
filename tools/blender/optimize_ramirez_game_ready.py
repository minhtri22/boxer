import argparse
from pathlib import Path
import sys

import bpy
from mathutils import Vector


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--body-ratio", type=float, default=0.65)
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(argv)


def apply_body_decimation(body, ratio: float) -> None:
    if not 0.25 <= ratio <= 1.0:
        raise ValueError(f"body ratio out of supported range: {ratio}")

    bpy.context.view_layer.objects.active = body
    body.select_set(True)

    decimate = body.modifiers.new(name="GameReadyBodyDecimate", type="DECIMATE")
    decimate.decimate_type = "COLLAPSE"
    decimate.ratio = ratio
    decimate.use_collapse_triangulate = True

    while body.modifiers.find(decimate.name) > 0:
        bpy.ops.object.modifier_move_up(modifier=decimate.name)

    bpy.ops.object.modifier_apply(modifier=decimate.name)
    body.select_set(False)


def reduce_surface_subdivision(obj_name: str, modifier_name: str) -> None:
    obj = bpy.data.objects.get(obj_name)
    if obj is None:
        raise RuntimeError(f"missing object: {obj_name}")
    modifier = obj.modifiers.get(modifier_name)
    if modifier is None or modifier.type != "SUBSURF":
        raise RuntimeError(f"missing subdivision modifier {modifier_name} on {obj_name}")
    modifier.levels = 0
    modifier.render_levels = 0


def parent_to_bone(obj, rig, bone_name: str) -> None:
    world = obj.matrix_world.copy()
    obj.parent = rig
    obj.parent_type = "BONE"
    obj.parent_bone = bone_name
    obj.matrix_world = world


def place_and_parent_glove(rig, side: int, bone_suffix: str, names: dict) -> None:
    forearm = rig.pose.bones[f"lowerarm_{bone_suffix}"]
    elbow = rig.matrix_world @ forearm.head
    wrist = rig.matrix_world @ forearm.tail
    direction = (wrist - elbow).normalized()
    center = wrist + direction * 0.090

    main = bpy.data.objects[names["main"]]
    thumb = bpy.data.objects[names["thumb"]]
    cuff = bpy.data.objects[names["cuff"]]
    badge = bpy.data.objects[names["badge"]]

    main.location = center
    thumb.location = center + Vector((side * 0.060, -0.012, -0.018))
    cuff.location = wrist + direction * 0.014
    cuff.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    badge.location = center + Vector((0.0, -0.118, 0.020))
    bpy.context.view_layer.update()

    for obj in (main, thumb, cuff, badge):
        parent_to_bone(obj, rig, f"lowerarm_{bone_suffix}")


def main() -> None:
    args = parse_args()
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)

    body = bpy.data.objects.get("Ramirez_CC0_Humanoid")
    armature = bpy.data.objects.get("RamirezRig")
    if body is None:
        raise RuntimeError("Ramirez_CC0_Humanoid is missing")
    if armature is None or armature.type != "ARMATURE":
        raise RuntimeError("RamirezRig is missing")

    armature.data.pose_position = "REST"
    for pose_bone in armature.pose.bones:
        pose_bone.location = (0.0, 0.0, 0.0)
        pose_bone.rotation_mode = "QUATERNION"
        pose_bone.rotation_quaternion = (1.0, 0.0, 0.0, 0.0)
        pose_bone.scale = (1.0, 1.0, 1.0)

    bpy.ops.object.select_all(action="DESELECT")
    apply_body_decimation(body, args.body_ratio)

    reduce_surface_subdivision("RamirezHair", "HairSurfaceSubdivision")
    reduce_surface_subdivision("RamirezShorts.L", "ShortsClothSubdivision")
    reduce_surface_subdivision("RamirezShorts.R", "ShortsClothSubdivision")

    place_and_parent_glove(
        armature,
        1,
        "l",
        {"main": "RamirezGlove.L", "thumb": "Sphere", "cuff": "Cylinder", "badge": "Sphere.001"},
    )
    place_and_parent_glove(
        armature,
        -1,
        "r",
        {"main": "RamirezGlove.R", "thumb": "Sphere.002", "cuff": "Cylinder.001", "badge": "Sphere.003"},
    )

    armature.data.pose_position = "POSE"
    bpy.ops.wm.save_as_mainfile(filepath=str(output))
    print(f"GAME_READY_BLEND={output}")
    print(f"BODY_DECIMATE_RATIO={args.body_ratio:.4f}")


if __name__ == "__main__":
    main()
