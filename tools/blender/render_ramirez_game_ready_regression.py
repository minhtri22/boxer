import argparse
from pathlib import Path
import sys

import bpy

sys.path.insert(0, str(Path(__file__).resolve().parent))
from rig_ramirez_mpfb_visual_gate import pose_targets, render_pose, update_glove_pose


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(argv)


def existing_gloves():
    return {
        "L": {
            "side": 1,
            "main": bpy.data.objects["RamirezGlove.L"],
            "thumb": bpy.data.objects["Sphere"],
            "cuff": bpy.data.objects["Cylinder"],
            "badge": bpy.data.objects["Sphere.001"],
        },
        "R": {
            "side": -1,
            "main": bpy.data.objects["RamirezGlove.R"],
            "thumb": bpy.data.objects["Sphere.002"],
            "cuff": bpy.data.objects["Cylinder.001"],
            "badge": bpy.data.objects["Sphere.003"],
        },
    }


def neutral_pose(rig, gloves):
    for pose_bone in rig.pose.bones:
        pose_bone.rotation_mode = "XYZ"
        pose_bone.rotation_euler = (0.0, 0.0, 0.0)
        pose_bone.location = (0.0, 0.0, 0.0)
        pose_bone.scale = (1.0, 1.0, 1.0)
    bpy.context.view_layer.update()
    if gloves["L"]["main"].parent_type != "BONE":
        update_glove_pose(rig, gloves["L"], "lowerarm_l")
    if gloves["R"]["main"].parent_type != "BONE":
        update_glove_pose(rig, gloves["R"], "lowerarm_r")
    bpy.context.view_layer.update()


def main():
    args = parse_args()
    output = Path(args.output)
    output.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    camera = bpy.data.objects["VisualGateCamera"]
    rig = bpy.data.objects["RamirezRig"]
    gloves = existing_gloves()

    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"

    neutral_pose(rig, gloves)
    render_pose(scene, camera, output, "01_front.png", (0.0, -4.2, 0.95))

    pose_targets(rig, gloves, "guard")
    render_pose(scene, camera, output, "06_guard.png", (0.0, -4.2, 0.98))

    pose_targets(rig, gloves, "jab")
    render_pose(scene, camera, output, "07_jab_extension.png", (2.55, -3.55, 1.02))


if __name__ == "__main__":
    main()
