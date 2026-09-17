import json
from pathlib import Path

import bpy


def production_object(obj):
    return obj.name.startswith("Ramirez") and obj.type in {"MESH", "CURVE", "FONT"}


def main():
    repo = Path(bpy.data.filepath).resolve().parents[4]
    depsgraph = bpy.context.evaluated_depsgraph_get()
    vertex_count = 0
    triangle_count = 0
    geometry_objects = []
    material_names = set()

    for obj in bpy.data.objects:
        if not production_object(obj):
            continue
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        if mesh is None:
            continue
        vertex_count += len(mesh.vertices)
        mesh.calc_loop_triangles()
        triangle_count += len(mesh.loop_triangles)
        geometry_objects.append(obj.name)
        for slot in obj.material_slots:
            if slot.material:
                material_names.add(slot.material.name)
        evaluated.to_mesh_clear()

    image_textures = {}
    for material_name in sorted(material_names):
        material = bpy.data.materials.get(material_name)
        if not material or not material.use_nodes:
            continue
        for node in material.node_tree.nodes:
            if node.type == "TEX_IMAGE" and node.image:
                image_textures[node.image.name] = list(node.image.size)

    rig = bpy.data.objects.get("RamirezRig")
    if rig is None:
        rig = next((o for o in bpy.data.objects if o.type == "ARMATURE" and o.name.startswith("Ramirez")), None)

    body = bpy.data.objects.get("Ramirez_Master_Body")
    body_vertices = len(body.data.vertices) if body and body.type == "MESH" else None
    body_triangles = None
    if body and body.type == "MESH":
        body.data.calc_loop_triangles()
        body_triangles = len(body.data.loop_triangles)

    report = {
        "blend": bpy.data.filepath,
        "production_geometry_objects": sorted(geometry_objects),
        "evaluated_vertex_count": vertex_count,
        "evaluated_triangle_count": triangle_count,
        "body_vertex_count": body_vertices,
        "body_triangle_count": body_triangles,
        "material_count": len(material_names),
        "materials": sorted(material_names),
        "bone_count": len(rig.data.bones) if rig else 0,
        "texture_count": len(image_textures),
        "texture_resolutions": image_textures,
        "procedural_texture_datablocks": sorted(tex.name for tex in bpy.data.textures),
        "exclusions": ["REF_RAMIREZ_APPROVED reference images", "StudioFloor", "lights", "camera"],
    }
    out = repo / "art" / "blender" / "ramirez" / "asset-audit.json"
    out.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
