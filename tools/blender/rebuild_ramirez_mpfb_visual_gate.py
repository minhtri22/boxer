import argparse
import gzip
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
    parser.add_argument("--mpfb", required=True)
    return parser.parse_args(argv)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for blocks in (
        bpy.data.meshes,
        bpy.data.curves,
        bpy.data.materials,
        bpy.data.cameras,
        bpy.data.lights,
    ):
        for block in list(blocks):
            if block.users == 0:
                blocks.remove(block)


def read_body_obj(path):
    verts = []
    faces = []
    group = None
    for raw in path.read_text(encoding="utf-8", errors="replace").splitlines():
        line = raw.strip()
        if line.startswith("v "):
            _, x, y, z, *_ = line.split()
            verts.append([float(x), float(y), float(z)])
        elif line.startswith("g "):
            group = line[2:].strip()
        elif group == "body" and line.startswith("f "):
            face = []
            for token in line.split()[1:]:
                face.append(int(token.split("/")[0]) - 1)
            faces.append(face)
    if not faces:
        raise RuntimeError(f"No body faces found in {path}")
    used_max = max(max(face) for face in faces)
    return verts[: used_max + 1], faces


def apply_target(verts, path, weight):
    if not path.exists():
        raise FileNotFoundError(path)
    opener = gzip.open if path.suffix == ".gz" else open
    with opener(path, "rt", encoding="utf-8", errors="replace") as handle:
        for raw in handle:
            line = raw.strip()
            if not line or line.startswith("#"):
                continue
            parts = line.split()
            if len(parts) < 4:
                continue
            idx = int(parts[0])
            if idx >= len(verts):
                continue
            verts[idx][0] += float(parts[1]) * weight
            verts[idx][1] += float(parts[2]) * weight
            verts[idx][2] += float(parts[3]) * weight


def make_material(name, rgba, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = rgba
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
    return mat


def build_body(base_obj, target_root):
    verts, faces = read_body_obj(base_obj)

    # MPFB/MakeHuman macro basis: young male, lean/high-muscle interpolation.
    # These are CC0 data assets, applied as vertex displacements to the CC0 base.
    target_weights = [
        ("macrodetails/caucasian-male-young.target.gz", 0.82),
        ("macrodetails/african-male-young.target.gz", 0.18),
        ("macrodetails/universal-male-young-maxmuscle-minweight.target.gz", 0.42),
        ("macrodetails/universal-male-young-maxmuscle-averageweight.target.gz", 0.58),
        ("macrodetails/proportions/male-young-maxmuscle-averageweight-idealproportions.target.gz", 0.72),
        ("torso/torso-vshape-incr.target.gz", 0.26),
        ("torso/measure-shoulder-dist-incr.target.gz", 0.24),
        ("torso/measure-bust-circ-incr.target.gz", 0.22),
        ("torso/measure-waist-circ-incr.target.gz", 0.16),
        ("torso/torso-muscle-pectoral-incr.target.gz", 0.60),
        ("torso/torso-muscle-dorsi-incr.target.gz", 0.55),
        ("stomach/stomach-tone-incr.target.gz", 0.80),
        ("neck/measure-neck-height-incr.target.gz", 0.16),
        ("neck/neck-scale-horiz-incr.target.gz", 0.14),
        ("arms/l-upperarm-shoulder-muscle-incr.target.gz", 0.74),
        ("arms/r-upperarm-shoulder-muscle-incr.target.gz", 0.74),
        ("arms/l-upperarm-muscle-incr.target.gz", 0.76),
        ("arms/r-upperarm-muscle-incr.target.gz", 0.76),
        ("arms/measure-upperarm-circ-incr.target.gz", 0.34),
        ("arms/l-lowerarm-muscle-incr.target.gz", 0.52),
        ("arms/r-lowerarm-muscle-incr.target.gz", 0.52),
        ("legs/l-upperleg-muscle-incr.target.gz", 0.46),
        ("legs/r-upperleg-muscle-incr.target.gz", 0.46),
        ("legs/measure-thigh-circ-incr.target.gz", 0.18),
        ("legs/l-lowerleg-muscle-incr.target.gz", 0.40),
        ("legs/r-lowerleg-muscle-incr.target.gz", 0.40),
        ("legs/measure-calf-circ-incr.target.gz", 0.14),
        ("buttocks/buttocks-volume-incr.target.gz", 0.12),
        ("chin/chin-width-incr.target.gz", 0.16),
        ("chin/chin-prominent-incr.target.gz", 0.10),
        ("chin/chin-bones-incr.target.gz", 0.12),
        ("cheek/l-cheek-bones-incr.target.gz", 0.10),
        ("cheek/r-cheek-bones-incr.target.gz", 0.10),
        ("eyebrows/eyebrows-angle-down.target.gz", 0.06),
        ("nose/nose-scale-depth-incr.target.gz", 0.06),
    ]
    for rel, weight in target_weights:
        apply_target(verts, target_root / rel, weight)

    # MakeHuman coordinates are X=left/right, Y=up, Z=front/back. Convert to
    # Blender X/Y/Z and normalize exact body height to the Ramirez reference.
    mh_min_y = min(v[1] for v in verts)
    mh_max_y = max(v[1] for v in verts)
    scale = 1.80 / (mh_max_y - mh_min_y)
    converted = [(v[0] * scale, -v[2] * scale, (v[1] - mh_min_y) * scale) for v in verts]

    mesh = bpy.data.meshes.new("Ramirez_CC0_Humanoid_Mesh")
    mesh.from_pydata(converted, [], faces)
    mesh.update(calc_edges=True)
    body = bpy.data.objects.new("Ramirez_CC0_Humanoid", mesh)
    bpy.context.collection.objects.link(body)
    for poly in mesh.polygons:
        poly.use_smooth = True

    skin = make_material("RamirezSkin", (0.27, 0.115, 0.070, 1.0), roughness=0.49)
    body.data.materials.append(skin)
    return body, target_weights


def add_ring_mesh(name, rings, sides, material):
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
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    bevel = obj.modifiers.new("ClothEdgeSoftness", "BEVEL")
    bevel.width = 0.008
    bevel.segments = 2
    return obj


def add_neutral_trunks():
    burgundy = make_material("RamirezTrunks", (0.20, 0.018, 0.025, 1.0), metallic=0.05, roughness=0.34)
    gold = make_material("RamirezGold", (0.66, 0.39, 0.08, 1.0), metallic=0.65, roughness=0.28)
    trunks = add_ring_mesh(
        "RamirezTrunksNeutral",
        [(0.735, 0.355, 0.185), (0.845, 0.315, 0.170), (0.975, 0.245, 0.148)],
        48,
        burgundy,
    )
    belt = add_ring_mesh(
        "RamirezWaistband",
        [(0.955, 0.238, 0.143), (0.995, 0.234, 0.141)],
        48,
        gold,
    )
    return trunks, belt


def add_hair_cap():
    hair = make_material("RamirezHair", (0.012, 0.009, 0.008, 1.0), roughness=0.62)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=48, ring_count=24, location=(0, -0.018, 1.748))
    cap = bpy.context.object
    cap.name = "RamirezHairCap"
    cap.scale = (0.110, 0.100, 0.082)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Keep only the upper hemisphere for a close-cropped textured silhouette.
    mesh = cap.data
    to_remove = [v.index for v in mesh.vertices if v.co.z < -0.010]
    if to_remove:
        bpy.context.view_layer.objects.active = cap
        cap.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="DESELECT")
        bpy.ops.object.mode_set(mode="OBJECT")
        for idx in to_remove:
            mesh.vertices[idx].select = True
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.delete(type="VERT")
        bpy.ops.object.mode_set(mode="OBJECT")
    cap.data.materials.append(hair)
    return cap


def setup_world():
    world = bpy.context.scene.world or bpy.data.worlds.new("World")
    bpy.context.scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value = (0.013, 0.013, 0.016, 1.0)
    bg.inputs["Strength"].default_value = 0.33

    floor_mat = make_material("Floor", (0.045, 0.045, 0.052, 1.0), roughness=0.66)
    bpy.ops.mesh.primitive_plane_add(size=10, location=(0, 0, -0.006))
    floor = bpy.context.object
    floor.name = "StudioFloor"
    floor.data.materials.append(floor_mat)

    for name, loc, energy, size in (
        ("Key", (-2.2, -3.0, 3.2), 1150, 2.4),
        ("Fill", (2.7, -1.5, 2.3), 700, 2.0),
        ("Rim", (0.4, 2.8, 2.8), 950, 1.8),
    ):
        data = bpy.data.lights.new(name, "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(obj)
        obj.location = loc
        direction = Vector((0, 0, 0.95)) - obj.location
        obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_camera():
    data = bpy.data.cameras.new("VisualGateCamera")
    cam = bpy.data.objects.new("VisualGateCamera", data)
    bpy.context.collection.objects.link(cam)
    bpy.context.scene.camera = cam
    data.type = "ORTHO"
    data.ortho_scale = 2.05
    return cam


def point_camera(cam, position, target=(0, 0, 0.92)):
    cam.location = position
    direction = Vector(target) - cam.location
    cam.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_views(output_dir, camera):
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.render.image_settings.color_mode = "RGBA"

    views = [
        ("01_front.png", (0.0, -4.2, 0.95)),
        ("02_three_quarter_front.png", (2.55, -3.55, 1.00)),
        ("03_side.png", (4.25, 0.0, 0.98)),
        ("04_back.png", (0.0, 4.2, 0.95)),
        ("05_three_quarter_back.png", (-2.55, 3.55, 1.00)),
    ]
    for filename, position in views:
        point_camera(camera, position)
        scene.render.filepath = str(output_dir / filename)
        bpy.ops.render.render(write_still=True)


def body_metrics(body):
    coords = [body.matrix_world @ v.co for v in body.data.vertices]
    height = max(v.z for v in coords) - min(v.z for v in coords)

    def width_band(z0, z1, x_limit=None):
        xs = [v.x for v in coords if z0 <= v.z <= z1 and (x_limit is None or abs(v.x) <= x_limit)]
        return max(xs) - min(xs) if xs else None

    shoulder = width_band(1.34, 1.50, 0.44)
    chest = width_band(1.20, 1.36, 0.42)
    waist = width_band(0.90, 1.03, 0.34)
    pelvis = width_band(0.78, 0.92, 0.34)
    return {
        "height_m": round(height, 4),
        "shoulder_width_m": round(shoulder, 4) if shoulder else None,
        "chest_width_m": round(chest, 4) if chest else None,
        "waist_width_m": round(waist, 4) if waist else None,
        "pelvis_width_m": round(pelvis, 4) if pelvis else None,
        "shoulder_to_waist_ratio": round(shoulder / waist, 4) if shoulder and waist else None,
        "vertices": len(body.data.vertices),
        "triangles": sum(len(p.vertices) - 2 for p in body.data.polygons),
    }


def main():
    args = parse_args()
    repo = Path(args.repo)
    mpfb = Path(args.mpfb)
    base_obj = mpfb / "src" / "mpfb" / "data" / "3dobjs" / "base.obj"
    target_root = mpfb / "src" / "mpfb" / "data" / "targets"
    out = repo / "evidence" / "p1-ramirez-rebuild" / "blender-visual-gate-mpfb"
    out.mkdir(parents=True, exist_ok=True)
    blend_path = repo / "art" / "blender" / "source" / "ramirez_mpfb_visual_gate.blend"
    blend_path.parent.mkdir(parents=True, exist_ok=True)

    clear_scene()
    body, target_weights = build_body(base_obj, target_root)
    add_neutral_trunks()
    add_hair_cap()
    setup_world()
    camera = setup_camera()

    metrics = body_metrics(body)
    metrics["method"] = "MPFB CC0 base.obj + CC0 target displacement + Blender authoring"
    metrics["source_base"] = str(base_obj)
    metrics["target_weights"] = [{"target": rel, "weight": weight} for rel, weight in target_weights]
    (out / "neutral-metrics.json").write_text(json.dumps(metrics, indent=2), encoding="utf-8")

    provenance = {
        "upstream": "makehumancommunity/mpfb2",
        "upstream_version": "v2.0.17",
        "upstream_commit": "80919fa4682335c41847f761a4d79dcad4124732",
        "base_asset": str(base_obj),
        "asset_license": "CC0 1.0",
        "note": "Only CC0 mesh/target data is consumed. MPFB GPL addon implementation is not copied into Boxer.",
    }
    (out / "source-provenance.json").write_text(json.dumps(provenance, indent=2), encoding="utf-8")

    render_views(out, camera)
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(json.dumps({"blend": str(blend_path), "output": str(out), "metrics": metrics}, indent=2))


if __name__ == "__main__":
    main()
