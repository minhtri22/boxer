import argparse
from pathlib import Path
import sys

import bpy


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    return parser.parse_args(argv)


def main() -> None:
    args = parse_args()
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)

    rig = bpy.data.objects.get("RamirezRig")
    if rig is None or rig.type != "ARMATURE":
        raise RuntimeError("RamirezRig is missing")

    rig.data.pose_position = "REST"
    bpy.ops.object.select_all(action="DESELECT")
    export_objects = [rig]
    for obj in bpy.context.scene.objects:
        if obj.type == "MESH" and not obj.hide_render and obj.name != "StudioFloor":
            export_objects.append(obj)

    for obj in export_objects:
        obj.hide_viewport = False
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig

    bpy.ops.export_scene.fbx(
        filepath=str(output),
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        add_leaf_bones=False,
        bake_anim=False,
        axis_forward="-Z",
        axis_up="Y",
        apply_unit_scale=True,
        use_space_transform=True,
        mesh_smooth_type="FACE",
    )
    print(f"RAMIREZ_GAME_READY_FBX={output}")
    print(f"EXPORTED_OBJECTS={len(export_objects)}")


if __name__ == "__main__":
    main()
