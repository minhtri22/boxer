import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    return parser.parse_args(argv)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (
        bpy.data.meshes,
        bpy.data.curves,
        bpy.data.metaballs,
        bpy.data.armatures,
        bpy.data.materials,
        bpy.data.cameras,
        bpy.data.lights,
    ):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def material(name, rgba, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = rgba
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
    return mat


def add_meta_element(meta, co, size, stiffness=2.0, rotation=None):
    e = meta.elements.new()
    e.type = "ELLIPSOID"
    e.co = co
    e.radius = 1.0
    e.size_x = size[0]
    e.size_y = size[1]
    e.size_z = size[2]
    e.stiffness = stiffness
    if rotation is not None:
        e.rotation = rotation
    return e


def convert_meta(name, meta_obj, mat, resolution=0.035):
    meta_obj.data.resolution = resolution
    meta_obj.data.render_resolution = min(resolution, 0.025)
    bpy.context.view_layer.objects.active = meta_obj
    meta_obj.select_set(True)
    bpy.ops.object.convert(target="MESH")
    obj = bpy.context.object
    obj.name = name
    if mat:
        obj.data.materials.append(mat)
    for p in obj.data.polygons:
        p.use_smooth = True
    sub = obj.modifiers.new("SurfaceSubdivision", "SUBSURF")
    sub.subdivision_type = "CATMULL_CLARK"
    sub.levels = 1
    sub.render_levels = 1
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=sub.name)
    return obj


def new_meta(name):
    data = bpy.data.metaballs.new(name + "Meta")
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    return obj, data


def blockout_ellipsoid(name, location, scale):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=16, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return obj


def blockout_segment(name, start, end, r_start, r_end):
    a = Vector(start)
    b = Vector(end)
    direction = b - a
    length = direction.length
    bpy.ops.mesh.primitive_cone_add(
        vertices=28,
        radius1=r_start,
        radius2=r_end,
        depth=length,
        location=(a + b) * 0.5,
    )
    obj = bpy.context.object
    obj.name = name
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(direction.normalized())
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return obj


def blockout_loft(name, rings, sides=40):
    verts = []
    faces = []
    for z, rx, ry in rings:
        for i in range(sides):
            a = 2.0 * math.pi * i / sides
            verts.append((math.cos(a) * rx, math.sin(a) * ry, z))
    for r in range(len(rings) - 1):
        for i in range(sides):
            j = (i + 1) % sides
            a = r * sides + i
            b = r * sides + j
            c = (r + 1) * sides + j
            d = (r + 1) * sides + i
            faces.append((a, b, c, d))
    bottom = len(verts)
    verts.append((0.0, 0.0, rings[0][0]))
    top = len(verts)
    verts.append((0.0, 0.0, rings[-1][0]))
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((bottom, j, i))
        a = (len(rings) - 1) * sides + i
        b = (len(rings) - 1) * sides + j
        faces.append((top, a, b))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    return obj


def fuse_body_sources(sources, mat):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in sources:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = sources[0]
    bpy.ops.object.join()
    body = bpy.context.object
    body.name = "RamirezBody_BlockoutJoined"
    body.data.remesh_voxel_size = 0.0105
    body.data.remesh_voxel_adaptivity = 0.0
    body.data.use_remesh_fix_poles = True
    body.data.use_remesh_preserve_volume = True
    bpy.ops.object.voxel_remesh()
    body.name = "RamirezBody_Coherent"
    smooth_mod = body.modifiers.new("AnatomySurfaceSmooth", "SMOOTH")
    smooth_mod.factor = 0.16
    smooth_mod.iterations = 2
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.modifier_apply(modifier=smooth_mod.name)
    for p in body.data.polygons:
        p.use_smooth = True
    body.data.materials.append(mat)
    return body


def build_body(skin):
    # Temporary anatomical blockout only. Everything below is fused through
    # voxel remesh and the source pieces cease to exist in the final body mesh.
    sources = []
    sources.append(
        blockout_loft(
            "TorsoCore",
            [
                (0.885, 0.175, 0.125),
                (0.990, 0.155, 0.108),
                (1.100, 0.160, 0.112),
                (1.220, 0.195, 0.128),
                (1.335, 0.230, 0.142),
                (1.425, 0.245, 0.145),
            ],
        )
    )
    # Neck/jaw/head chain with visible neck clearance.
    sources += [
        blockout_segment("Neck", (0, 0.005, 1.405), (0, 0.000, 1.590), 0.082, 0.070),
        blockout_ellipsoid("Jaw", (0, -0.010, 1.610), (0.094, 0.084, 0.085)),
        blockout_ellipsoid("Chin", (0, -0.030, 1.555), (0.064, 0.060, 0.050)),
        blockout_ellipsoid("Cranium", (0, -0.004, 1.700), (0.108, 0.097, 0.128)),
        blockout_ellipsoid("Cheek.L", (-0.045, -0.075, 1.665), (0.055, 0.035, 0.050)),
        blockout_ellipsoid("Cheek.R", (0.045, -0.075, 1.665), (0.055, 0.035, 0.050)),
        blockout_ellipsoid("Nose", (0, -0.101, 1.685), (0.023, 0.034, 0.030)),
        blockout_ellipsoid("Ear.L", (-0.108, 0.000, 1.690), (0.018, 0.013, 0.032)),
        blockout_ellipsoid("Ear.R", (0.108, 0.000, 1.690), (0.018, 0.013, 0.032)),
    ]
    # Traps, lats and pectoral masses overlap the torso instead of sitting on it.
    sources += [
        blockout_ellipsoid("Trap.L", (-0.100, 0.015, 1.455), (0.135, 0.086, 0.078)),
        blockout_ellipsoid("Trap.R", (0.100, 0.015, 1.455), (0.135, 0.086, 0.078)),
        blockout_ellipsoid("Pec.L", (-0.100, -0.100, 1.355), (0.128, 0.058, 0.068)),
        blockout_ellipsoid("Pec.R", (0.100, -0.100, 1.355), (0.128, 0.058, 0.068)),
        blockout_ellipsoid("Lat.L", (-0.165, 0.050, 1.285), (0.105, 0.078, 0.165)),
        blockout_ellipsoid("Lat.R", (0.165, 0.050, 1.285), (0.105, 0.078, 0.165)),
    ]

    for side in (-1, 1):
        shoulder = Vector((side * 0.270, 0.000, 1.405))
        elbow = Vector((side * 0.335, 0.000, 1.115))
        wrist = Vector((side * 0.360, -0.010, 0.830))
        sources += [
            blockout_ellipsoid(f"Deltoid.{side}", shoulder, (0.108, 0.096, 0.116)),
            blockout_segment(f"UpperArm.{side}", shoulder, elbow, 0.086, 0.062),
            blockout_ellipsoid(f"Biceps.{side}", (side * 0.310, -0.037, 1.260), (0.078, 0.060, 0.125)),
            blockout_ellipsoid(f"Triceps.{side}", (side * 0.308, 0.040, 1.245), (0.075, 0.058, 0.122)),
            blockout_ellipsoid(f"Elbow.{side}", elbow, (0.062, 0.059, 0.070)),
            blockout_segment(f"Forearm.{side}", elbow, wrist, 0.074, 0.049),
            blockout_ellipsoid(f"ForearmBelly.{side}", (side * 0.348, -0.008, 1.015), (0.074, 0.064, 0.120)),
        ]

        hip = Vector((side * 0.120, 0.020, 0.900))
        knee = Vector((side * 0.125, 0.005, 0.505))
        ankle = Vector((side * 0.130, 0.005, 0.145))
        sources += [
            blockout_ellipsoid(f"Glute.{side}", (side * 0.120, 0.065, 0.900), (0.125, 0.112, 0.130)),
            blockout_segment(f"Thigh.{side}", hip, knee, 0.116, 0.066),
            blockout_ellipsoid(f"Quad.{side}", (side * 0.125, -0.040, 0.700), (0.110, 0.090, 0.195)),
            blockout_ellipsoid(f"Hamstring.{side}", (side * 0.125, 0.055, 0.690), (0.102, 0.080, 0.185)),
            blockout_ellipsoid(f"Knee.{side}", knee, (0.066, 0.064, 0.075)),
            blockout_segment(f"Shin.{side}", knee, ankle, 0.070, 0.051),
            blockout_ellipsoid(f"Calf.{side}", (side * 0.130, 0.050, 0.330), (0.080, 0.070, 0.140)),
            blockout_ellipsoid(f"FootCore.{side}", (side * 0.130, -0.070, 0.075), (0.072, 0.145, 0.060)),
        ]

    return fuse_body_sources(sources, skin)


def build_glove(name, side, red, gold):
    wrist = Vector((side * 0.390, -0.020, 0.820))
    obj, meta = new_meta(name + "Authoring")
    add_meta_element(meta, wrist + Vector((0, -0.005, 0.030)), (0.105, 0.095, 0.115), 2.0)
    add_meta_element(meta, wrist + Vector((0, -0.035, 0.095)), (0.118, 0.105, 0.080), 2.0)
    add_meta_element(meta, wrist + Vector((-side * 0.070, -0.045, 0.045)), (0.060, 0.052, 0.070), 1.85)
    glove = convert_meta(name, obj, red, resolution=0.022)
    badge = make_ellipsoid(name + "Badge", wrist + Vector((0, -0.105, 0.090)), (0.038, 0.010, 0.028), gold)
    return [glove, badge]


def make_ellipsoid(name, location, scale, mat, segments=32, rings=20):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if mat:
        obj.data.materials.append(mat)
    for p in obj.data.polygons:
        p.use_smooth = True
    return obj


def loft(name, rings, mat, sides=48):
    verts = []
    faces = []
    for z, rx, ry, ox, oy in rings:
        for i in range(sides):
            a = 2.0 * math.pi * i / sides
            verts.append((ox + math.cos(a) * rx, oy + math.sin(a) * ry, z))
    for r in range(len(rings) - 1):
        for i in range(sides):
            j = (i + 1) % sides
            a = r * sides + i
            b = r * sides + j
            c = (r + 1) * sides + j
            d = (r + 1) * sides + i
            faces.append((a, b, c, d))
    bottom = len(verts)
    verts.append((rings[0][3], rings[0][4], rings[0][0]))
    top = len(verts)
    verts.append((rings[-1][3], rings[-1][4], rings[-1][0]))
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((bottom, j, i))
        a = (len(rings) - 1) * sides + i
        b = (len(rings) - 1) * sides + j
        faces.append((top, a, b))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    for p in obj.data.polygons:
        p.use_smooth = True
    bevel = obj.modifiers.new("GarmentSoftEdge", "BEVEL")
    bevel.width = 0.010
    bevel.segments = 3
    return obj


def build_equipment(red, gold, white, dark):
    objects = []
    # High-waist boxing trunks with a compact waist and broader leg hem.
    shorts = loft(
        "RamirezTrunks",
        [
            (0.805, 0.220, 0.145, 0, 0.012),
            (0.900, 0.215, 0.145, 0, 0.008),
            (0.985, 0.180, 0.125, 0, 0.000),
        ],
        red,
    )
    waistband = loft(
        "RamirezWaistband",
        [(0.970, 0.183, 0.128, 0, 0), (1.035, 0.185, 0.130, 0, 0)],
        gold,
    )
    objects += [shorts, waistband]
    objects += build_glove("RamirezGlove.L", -1, red, gold)
    objects += build_glove("RamirezGlove.R", 1, red, gold)

    # Wraps are tapered custom lofts rather than body cylinders.
    for side in (-1, 1):
        x = side * 0.382
        wrap = loft(
            f"WristWrap.{side}",
            [
                (0.785, 0.064, 0.058, x, -0.012),
                (0.835, 0.068, 0.061, x, -0.014),
                (0.875, 0.060, 0.056, x, -0.016),
            ],
            white,
            sides=32,
        )
        objects.append(wrap)
        boot = loft(
            f"BoxingBoot.{side}",
            [
                (0.030, 0.082, 0.135, side * 0.130, -0.060),
                (0.110, 0.073, 0.090, side * 0.130, -0.015),
                (0.225, 0.066, 0.064, side * 0.130, 0.005),
            ],
            white,
            sides=36,
        )
        objects.append(boot)
        toe = make_ellipsoid(
            f"BoxingBootToe.{side}",
            (side * 0.130, -0.100, 0.070),
            (0.082, 0.145, 0.060),
            white,
            28,
            16,
        )
        objects.append(toe)

    # Hair is separate grooming geometry, not a body construction element.
    hair = make_ellipsoid("RamirezHair", (0, 0.006, 1.758), (0.108, 0.098, 0.070), dark, 36, 20)
    objects.append(hair)
    return objects


def make_floor(mat):
    bpy.ops.mesh.primitive_plane_add(size=6, location=(0, 0, 0.0))
    floor = bpy.context.object
    floor.name = "StudioFloor"
    floor.data.materials.append(mat)
    return floor


def look_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def create_studio():
    world = bpy.context.scene.world or bpy.data.worlds.new("World")
    bpy.context.scene.world = world
    world.color = (0.012, 0.012, 0.014)

    for loc, energy, size in [
        ((-2.4, -3.2, 3.4), 900, 2.4),
        ((2.3, -1.7, 2.7), 650, 1.8),
        ((0.0, 2.6, 3.0), 900, 2.0),
    ]:
        bpy.ops.object.light_add(type="AREA", location=loc)
        light = bpy.context.object
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        look_at(light, (0, 0, 1.0))


def set_camera(location, target=(0, 0, 0.95), ortho=2.12):
    cam = bpy.data.objects.get("GateCamera")
    if cam is None:
        bpy.ops.object.camera_add(location=location)
        cam = bpy.context.object
        cam.name = "GateCamera"
    cam.location = location
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = ortho
    look_at(cam, target)
    bpy.context.scene.camera = cam
    return cam


def render(path, camera_location, target=(0, 0, 0.95), ortho=2.12, size=(900, 1200)):
    set_camera(camera_location, target, ortho)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = size[0]
    scene.render.resolution_y = size[1]
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(path)
    scene.render.film_transparent = False
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)


def mesh_stats(obj):
    tris = sum(max(1, len(p.vertices) - 2) for p in obj.data.polygons)
    return {"vertices": len(obj.data.vertices), "triangles": tris}


def cross_section_width(obj, z0, z1, x_limit=None):
    xs = []
    for v in obj.data.vertices:
        world = obj.matrix_world @ v.co
        if not (z0 <= world.z <= z1):
            continue
        if x_limit is not None and abs(world.x) > x_limit:
            continue
        xs.append(world)
    if not xs:
        return 0.0
    return max(v.x for v in xs) - min(v.x for v in xs)


def measurements(body):
    shoulder = cross_section_width(body, 1.34, 1.48)
    waist = cross_section_width(body, 1.00, 1.12, 0.28)
    pelvis = cross_section_width(body, 0.86, 0.97, 0.30)
    neck = cross_section_width(body, 1.50, 1.58, 0.16)
    return {
        "height_m": round(max((body.matrix_world @ v.co).z for v in body.data.vertices), 4),
        "shoulder_width_m": round(shoulder, 4),
        "waist_width_m": round(waist, 4),
        "pelvis_width_m": round(pelvis, 4),
        "neck_width_m": round(neck, 4),
        "shoulder_to_waist_ratio": round(shoulder / waist, 3) if waist else None,
        "designed_upper_arm_radius_m": 0.086,
        "designed_elbow_radius_m": 0.062,
        "designed_forearm_belly_radius_m": 0.074,
        "designed_wrist_radius_m": 0.049,
        "designed_thigh_radius_m": 0.116,
        "designed_knee_radius_m": 0.066,
        "designed_calf_radius_m": 0.080,
        "designed_ankle_radius_m": 0.051,
        "head_center_z_m": 1.665,
        "neck_center_z_m": 1.535,
        "upper_chest_center_z_m": 1.355,
    }


def main():
    args = parse_args()
    repo = Path(args.repo).resolve()
    out = repo / "evidence" / "p1-ramirez-rebuild" / "blender-visual-gate"
    source_dir = repo / "art" / "blender" / "source"
    out.mkdir(parents=True, exist_ok=True)
    source_dir.mkdir(parents=True, exist_ok=True)

    clear_scene()
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0

    skin = material("RamirezSkin", (0.39, 0.19, 0.105, 1.0), 0.0, 0.43)
    red = material("RamirezBurgundy", (0.35, 0.018, 0.025, 1.0), 0.08, 0.26)
    gold = material("RamirezGold", (0.72, 0.40, 0.085, 1.0), 0.55, 0.25)
    white = material("RamirezWrapWhite", (0.74, 0.70, 0.64, 1.0), 0.0, 0.66)
    dark = material("RamirezHairDark", (0.018, 0.012, 0.009, 1.0), 0.0, 0.58)
    floor_mat = material("StudioFloorMat", (0.030, 0.026, 0.024, 1.0), 0.0, 0.78)

    body = build_body(skin)
    gear = build_equipment(red, gold, white, dark)
    make_floor(floor_mat)
    create_studio()

    color_objects = [body] + gear
    render(out / "01_front.png", (0.0, -4.0, 0.98))
    render(out / "02_three_quarter_front.png", (-3.0, -3.0, 1.02))
    render(out / "03_side.png", (-4.0, 0.0, 1.00))
    render(out / "04_back.png", (0.0, 4.0, 0.98))
    render(out / "05_three_quarter_back.png", (3.0, 3.0, 1.02))

    # Clay turnaround: temporarily swap every visible character material to clay
    # and render four orientations as separate images for deterministic compositing.
    clay = material("Clay", (0.46, 0.46, 0.46, 1.0), 0.0, 0.62)
    original_materials = {}
    for obj in color_objects:
        if obj.type != "MESH":
            continue
        original_materials[obj.name] = [m for m in obj.data.materials]
        obj.data.materials.clear()
        obj.data.materials.append(clay)
    clay_views = [
        ("clay_front.png", (0.0, -4.0, 0.98)),
        ("clay_side.png", (-4.0, 0.0, 1.0)),
        ("clay_back.png", (0.0, 4.0, 0.98)),
        ("clay_three_quarter.png", (3.0, -3.0, 1.02)),
    ]
    for filename, camera in clay_views:
        render(out / filename, camera)

    for obj in color_objects:
        if obj.type != "MESH":
            continue
        obj.data.materials.clear()
        for mat in original_materials.get(obj.name, []):
            obj.data.materials.append(mat)

    metrics = {
        "phase": "BLENDER_VISUAL_GATE_NEUTRAL_PRE_RIG",
        "method": "anatomical authoring volumes fused to one coherent metaball-derived humanoid mesh; equipment modeled separately",
        "source_provenance": "original in-repo Blender construction; no external base mesh or third-party character asset used",
        "body_mesh": mesh_stats(body),
        "measurements": measurements(body),
        "material_count_character": len({m.name for obj in color_objects for m in obj.data.materials}),
        "bone_count": 0,
        "note": "Neutral proportion gate intentionally precedes rigging and Unity integration.",
    }
    (out / "neutral-metrics.json").write_text(json.dumps(metrics, indent=2), encoding="utf-8")

    blend_path = source_dir / "ramirez_v2_visual_gate.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(json.dumps(metrics, indent=2))


if __name__ == "__main__":
    main()
