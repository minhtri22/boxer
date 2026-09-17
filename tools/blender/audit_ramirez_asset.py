import argparse
import json
from pathlib import Path
import sys

import bpy


def tri_count(mesh) -> int:
    mesh.calc_loop_triangles()
    return len(mesh.loop_triangles)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True)
    parser.add_argument("--exclude", action="append", default=["StudioFloor"])
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    args = parser.parse_args(argv)

    excluded = set(args.exclude)
    scene = bpy.context.scene
    depsgraph = bpy.context.evaluated_depsgraph_get()

    rows = []
    materials = set()
    raw_total = 0
    evaluated_total = 0

    for obj in scene.objects:
        if obj.type != "MESH" or obj.hide_render or obj.name in excluded:
            continue

        raw = tri_count(obj.data)
        evaluated_obj = obj.evaluated_get(depsgraph)
        evaluated_mesh = evaluated_obj.to_mesh()
        try:
            evaluated = tri_count(evaluated_mesh)
            vertices = len(evaluated_mesh.vertices)
        finally:
            evaluated_obj.to_mesh_clear()

        object_materials = [
            slot.material.name for slot in obj.material_slots if slot.material is not None
        ]
        materials.update(object_materials)
        rows.append(
            {
                "name": obj.name,
                "raw_triangles": raw,
                "evaluated_triangles": evaluated,
                "evaluated_vertices": vertices,
                "materials": object_materials,
                "modifiers": [f"{modifier.name}:{modifier.type}" for modifier in obj.modifiers],
            }
        )
        raw_total += raw
        evaluated_total += evaluated

    armatures = [obj for obj in scene.objects if obj.type == "ARMATURE"]
    images = []
    for image in bpy.data.images:
        if image.source == "VIEWER":
            continue
        images.append(
            {
                "name": image.name,
                "source": image.source,
                "size": [int(image.size[0]), int(image.size[1])],
                "filepath": image.filepath,
            }
        )
    report = {
        "blend_file": bpy.data.filepath,
        "blender_version": bpy.app.version_string,
        "excluded_objects": sorted(excluded),
        "scene_object_count": len(scene.objects),
        "renderable_asset_mesh_count": len(rows),
        "armature_object_count": len(armatures),
        "bone_count_total": sum(len(obj.data.bones) for obj in armatures),
        "unique_material_count": len(materials),
        "unique_materials": sorted(materials),
        "image_texture_count": len(images),
        "image_textures": images,
        "raw_triangles_total": raw_total,
        "evaluated_triangles_total": evaluated_total,
        "mesh_objects": sorted(rows, key=lambda row: (-row["evaluated_triangles"], row["name"])),
    }

    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({key: value for key, value in report.items() if key != "mesh_objects"}, indent=2))


if __name__ == "__main__":
    main()
