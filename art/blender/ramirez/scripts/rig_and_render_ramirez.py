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


TARGET_WEIGHTS = [
    ("macrodetails/caucasian-male-young.target.gz", 0.86),
    ("macrodetails/african-male-young.target.gz", 0.14),
    ("macrodetails/universal-male-young-maxmuscle-minweight.target.gz", 0.58),
    ("macrodetails/universal-male-young-maxmuscle-averageweight.target.gz", 0.42),
    ("macrodetails/proportions/male-young-maxmuscle-averageweight-idealproportions.target.gz", 0.80),
    ("torso/torso-vshape-incr.target.gz", 0.38),
    ("torso/measure-shoulder-dist-incr.target.gz", 0.14),
    ("torso/measure-bust-circ-incr.target.gz", 0.18),
    ("torso/measure-napetowaist-dist-incr.target.gz", 0.10),
    ("torso/torso-muscle-pectoral-incr.target.gz", 1.00),
    ("torso/torso-muscle-dorsi-incr.target.gz", 0.96),
    ("stomach/stomach-tone-incr.target.gz", 1.00),
    ("neck/measure-neck-height-incr.target.gz", 0.10),
    ("neck/measure-neck-circ-incr.target.gz", 0.18),
    ("neck/neck-scale-horiz-incr.target.gz", 0.10),
    ("neck/neck-back-scale-depth-incr.target.gz", 0.08),
    ("arms/l-upperarm-shoulder-muscle-incr.target.gz", 0.90),
    ("arms/r-upperarm-shoulder-muscle-incr.target.gz", 0.90),
    ("arms/l-upperarm-muscle-incr.target.gz", 0.92),
    ("arms/r-upperarm-muscle-incr.target.gz", 0.92),
    ("arms/measure-upperarm-circ-incr.target.gz", 0.32),
    ("arms/l-upperarm-scale-horiz-incr.target.gz", 0.04),
    ("arms/r-upperarm-scale-horiz-incr.target.gz", 0.04),
    ("arms/l-upperarm-scale-depth-incr.target.gz", 0.04),
    ("arms/r-upperarm-scale-depth-incr.target.gz", 0.04),
    ("arms/l-lowerarm-muscle-incr.target.gz", 0.74),
    ("arms/r-lowerarm-muscle-incr.target.gz", 0.74),
    ("arms/l-lowerarm-scale-horiz-incr.target.gz", 0.03),
    ("arms/r-lowerarm-scale-horiz-incr.target.gz", 0.03),
    ("arms/l-lowerarm-scale-depth-incr.target.gz", 0.03),
    ("arms/r-lowerarm-scale-depth-incr.target.gz", 0.03),
    ("legs/l-upperleg-muscle-incr.target.gz", 0.72),
    ("legs/r-upperleg-muscle-incr.target.gz", 0.72),
    ("legs/measure-thigh-circ-incr.target.gz", 0.22),
    ("legs/l-lowerleg-muscle-incr.target.gz", 0.68),
    ("legs/r-lowerleg-muscle-incr.target.gz", 0.68),
    ("legs/measure-calf-circ-incr.target.gz", 0.20),
    ("buttocks/buttocks-volume-incr.target.gz", 0.16),
    ("head/head-age-incr.target.gz", 0.30),
    ("head/head-rectangular.target.gz", 0.44),
    ("head/head-fat-decr.target.gz", 0.18),
    ("chin/chin-width-incr.target.gz", 0.38),
    ("chin/chin-prominent-incr.target.gz", 0.28),
    ("chin/chin-bones-incr.target.gz", 0.34),
    ("chin/chin-height-incr.target.gz", 0.10),
    ("cheek/l-cheek-bones-incr.target.gz", 0.30),
    ("cheek/r-cheek-bones-incr.target.gz", 0.30),
    ("cheek/l-cheek-volume-decr.target.gz", 0.10),
    ("cheek/r-cheek-volume-decr.target.gz", 0.10),
    ("forehead/forehead-temple-decr.target.gz", 0.10),
    ("eyebrows/eyebrows-angle-down.target.gz", 0.22),
    ("eyebrows/eyebrows-trans-down.target.gz", 0.08),
    ("eyes/l-eye-scale-decr.target.gz", 0.06),
    ("eyes/r-eye-scale-decr.target.gz", 0.06),
    ("nose/nose-scale-depth-incr.target.gz", 0.18),
    ("nose/nose-width1-incr.target.gz", 0.08),
    ("nose/nose-width2-incr.target.gz", 0.06),
    ("nose/nose-hump-incr.target.gz", 0.06),
    ("mouth/mouth-scale-horiz-incr.target.gz", 0.08),
    ("mouth/mouth-angles-down.target.gz", 0.06),
]


def read_vertices_and_groups(base_obj):
    verts = []
    refs = {}
    group = None
    for raw in base_obj.read_text(encoding="utf-8", errors="replace").splitlines():
        line = raw.strip()
        if line.startswith("v "):
            verts.append(list(map(float, line.split()[1:4])))
        elif line.startswith("g "):
            group = line[2:].strip()
            refs.setdefault(group, set())
        elif line.startswith("f "):
            for token in line.split()[1:]:
                refs.setdefault(group, set()).add(int(token.split("/")[0]) - 1)
    return verts, refs


def apply_target(verts, path, weight):
    with gzip.open(path, "rt", encoding="utf-8", errors="replace") as handle:
        for raw in handle:
            p = raw.split()
            if len(p) < 4:
                continue
            idx = int(p[0])
            verts[idx][0] += float(p[1]) * weight
            verts[idx][1] += float(p[2]) * weight
            verts[idx][2] += float(p[3]) * weight


def joint_centers(base_obj, target_root):
    verts, refs = read_vertices_and_groups(base_obj)
    for rel, weight in TARGET_WEIGHTS:
        apply_target(verts, target_root / rel, weight)
    body_ids = refs["body"]
    ys = [verts[i][1] for i in body_ids]
    ymin, ymax = min(ys), max(ys)
    scale = 1.80 / (ymax - ymin)

    def convert(v):
        return Vector((v[0] * scale, -v[2] * scale, (v[1] - ymin) * scale))

    converted = [convert(v) for v in verts]
    result = {}
    for name, ids in refs.items():
        if not name.startswith("joint-") or not ids:
            continue
        pts = [converted[i] for i in ids]
        result[name] = sum(pts, Vector()) / len(pts)
    return result, converted


def remove_old_accessories():
    for name in ["RamirezTrunksNeutral", "RamirezWaistband", "RamirezHairCap"]:
        obj = bpy.data.objects.get(name)
        if obj:
            bpy.data.objects.remove(obj, do_unlink=True)


def ensure_skin_material():
    mat = bpy.data.materials.get("RamirezSkin")
    if not mat or not mat.use_nodes:
        return
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (0.090, 0.028, 0.014, 1.0)
    bsdf.inputs["Roughness"].default_value = 0.62
    noise = nodes.get("SkinMicroNoise") or nodes.new("ShaderNodeTexNoise")
    noise.name = "SkinMicroNoise"
    noise.inputs["Scale"].default_value = 45.0
    noise.inputs["Detail"].default_value = 3.0
    noise.inputs["Roughness"].default_value = 0.7
    bump = nodes.get("SkinMicroBump") or nodes.new("ShaderNodeBump")
    bump.name = "SkinMicroBump"
    bump.inputs["Strength"].default_value = 0.055
    bump.inputs["Distance"].default_value = 0.003
    links.new(noise.outputs["Fac"], bump.inputs["Height"])
    links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])


def add_face_stubble(body):
    # Keep the beard close to skin value so the low-poly face does not read as
    # a painted-on black mask under bright review lighting.
    stubble = make_material("RamirezStubble", (0.070, 0.026, 0.016, 1), roughness=0.78)
    if stubble.name not in body.data.materials:
        body.data.materials.append(stubble)
    stubble_index = list(body.data.materials).index(stubble)
    for poly in body.data.polygons:
        c = poly.center
        chin = 1.555 <= c.z <= 1.615 and c.y <= -0.115 and abs(c.x) <= 0.085
        jaw = 1.590 <= c.z <= 1.645 and c.y <= -0.105 and 0.042 <= abs(c.x) <= 0.095
        moustache = 1.625 <= c.z <= 1.643 and c.y <= -0.150 and abs(c.x) <= 0.050
        brow = 1.676 <= c.z <= 1.696 and c.y <= -0.145 and 0.022 <= abs(c.x) <= 0.082
        if chin or jaw or moustache or brow:
            poly.material_index = stubble_index


def make_material(name, rgba, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = rgba
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = rgba
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def add_bone(edit_bones, name, head, tail, parent=None):
    bone = edit_bones.new(name)
    bone.head = head
    bone.tail = tail
    bone.use_connect = False
    if parent:
        bone.parent = edit_bones[parent]
    return bone


def build_armature(joints):
    arm_data = bpy.data.armatures.new("RamirezArmature")
    rig = bpy.data.objects.new("RamirezRig", arm_data)
    bpy.context.collection.objects.link(rig)
    rig.show_in_front = True
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    eb = arm_data.edit_bones

    pelvis = joints["joint-pelvis"]
    s4 = joints["joint-spine-4"]
    s3 = joints["joint-spine-3"]
    s2 = joints["joint-spine-2"]
    s1 = joints["joint-spine-1"]
    head = joints["joint-head"]
    neck_base = Vector((0, (s1.y + head.y) * 0.5, 1.49))

    add_bone(eb, "hips", pelvis + Vector((0, 0, -0.08)), s4)
    add_bone(eb, "spine.lower", s4, s3, "hips")
    add_bone(eb, "spine.mid", s3, s2, "spine.lower")
    add_bone(eb, "spine.chest", s2, s1, "spine.mid")
    add_bone(eb, "neck", s1, neck_base, "spine.chest")
    add_bone(eb, "head", neck_base, Vector((head.x, head.y, 1.79)), "neck")

    for side, mh_side in (("L", "l"), ("R", "r")):
        clav = joints[f"joint-{mh_side}-clavicle"]
        shoulder = joints[f"joint-{mh_side}-shoulder"]
        elbow = joints[f"joint-{mh_side}-elbow"]
        hand = joints[f"joint-{mh_side}-hand"]
        hip = joints[f"joint-{mh_side}-upper-leg"]
        knee = joints[f"joint-{mh_side}-knee"]
        ankle = joints[f"joint-{mh_side}-ankle"]
        foot = joints[f"joint-{mh_side}-foot-1"]
        add_bone(eb, f"clavicle.{side}", clav, shoulder, "spine.chest")
        add_bone(eb, f"upper_arm.{side}", shoulder, elbow, f"clavicle.{side}")
        add_bone(eb, f"forearm.{side}", elbow, hand, f"upper_arm.{side}")
        hand_tail = hand + (hand - elbow).normalized() * 0.11
        add_bone(eb, f"hand.{side}", hand, hand_tail, f"forearm.{side}")
        add_bone(eb, f"thigh.{side}", hip, knee, "hips")
        add_bone(eb, f"shin.{side}", knee, ankle, f"thigh.{side}")
        add_bone(eb, f"foot.{side}", ankle, foot, f"shin.{side}")

    bpy.ops.object.mode_set(mode="OBJECT")
    rig.hide_render = True
    return rig


def build_mpfb_game_engine_rig(joints, converted_verts, rig_json_path):
    rig_def = json.loads(rig_json_path.read_text(encoding="utf-8"))
    arm_data = bpy.data.armatures.new("RamirezGameEngineArmature")
    rig = bpy.data.objects.new("RamirezRig", arm_data)
    bpy.context.collection.objects.link(rig)
    rig.show_in_front = True
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")

    def resolve_point(spec):
        cube_name = spec.get("cube_name")
        if cube_name:
            if cube_name not in joints:
                raise KeyError(f"Missing joint group for rig point: {cube_name}")
            return Vector(joints[cube_name])
        ids = spec.get("vertex_indices") or []
        valid = [converted_verts[i] for i in ids if 0 <= i < len(converted_verts)]
        if valid:
            return sum(valid, Vector()) / len(valid)
        raise RuntimeError(f"Cannot resolve rig point: {spec}")

    for name, spec in rig_def.items():
        bone = arm_data.edit_bones.new(name)
        bone.head = resolve_point(spec["head"])
        bone.tail = resolve_point(spec["tail"])
        bone.roll = float(spec.get("roll", 0.0))
        bone.use_connect = bool(spec.get("use_connect", False))
        bone.use_inherit_rotation = bool(spec.get("use_inherit_rotation", True))
        bone.use_local_location = bool(spec.get("use_local_location", True))
        inherit_scale = spec.get("inherit_scale")
        if inherit_scale:
            bone.inherit_scale = inherit_scale

    for name, spec in rig_def.items():
        parent = spec.get("parent")
        if parent:
            arm_data.edit_bones[name].parent = arm_data.edit_bones[parent]

    bpy.ops.object.mode_set(mode="OBJECT")
    rig.hide_render = True
    return rig


def apply_mpfb_weights(body, rig, weights_json_path):
    weight_def = json.loads(weights_json_path.read_text(encoding="utf-8"))
    if str(weight_def.get("license", "")).upper() != "CC0":
        raise RuntimeError(f"Expected CC0 weights, got {weight_def.get('license')}")

    body.vertex_groups.clear()
    vertex_count = len(body.data.vertices)
    for bone_name, assignments in weight_def["weights"].items():
        if bone_name not in rig.data.bones:
            continue
        group = body.vertex_groups.new(name=bone_name)
        for idx, weight in assignments:
            idx = int(idx)
            weight = float(weight)
            if idx < vertex_count and weight > 0.0:
                group.add([idx], weight, "REPLACE")

    for modifier in list(body.modifiers):
        if modifier.type == "ARMATURE":
            body.modifiers.remove(modifier)
    arm_mod = body.modifiers.new("RamirezGameEngineSkin", "ARMATURE")
    arm_mod.object = rig
    arm_mod.use_deform_preserve_volume = True
    body.parent = rig
    body.matrix_parent_inverse = rig.matrix_world.inverted()


def auto_weight_body(body, rig):
    bpy.ops.object.select_all(action="DESELECT")
    body.select_set(True)
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.parent_set(type="ARMATURE_AUTO")


def add_ik_target(name, location):
    obj = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(obj)
    obj.empty_display_type = "SPHERE"
    obj.empty_display_size = 0.035
    obj.location = location
    obj.hide_render = True
    return obj


def setup_arm_ik(rig, joints):
    targets = {}
    for side, sign in (("L", 1), ("R", -1)):
        hand = joints[f"joint-{'l' if side == 'L' else 'r'}-hand"]
        target = add_ik_target(f"IK_Hand_{side}", hand)
        pole = add_ik_target(f"IK_ElbowPole_{side}", Vector((sign * 0.48, 0.08, 1.18)))
        pb = rig.pose.bones[f"forearm.{side}"]
        ik = pb.constraints.new("IK")
        ik.name = "RamirezArmIK"
        ik.target = target
        ik.pole_target = pole
        ik.chain_count = 2
        ik.use_tail = True
        ik.pole_angle = math.radians(-90)
        targets[side] = (target, pole)
    return targets


def parent_to_bone(obj, rig, bone_name):
    world = obj.matrix_world.copy()
    obj.parent = rig
    obj.parent_type = "BONE"
    obj.parent_bone = bone_name
    obj.matrix_world = world


def add_boxing_glove(name, center, side):
    red = make_material("GloveRed", (0.070, 0.003, 0.005, 1), metallic=0.0, roughness=0.34)
    white = make_material("WrapWhite", (0.52, 0.47, 0.40, 1), roughness=0.62)
    gold = make_material("GloveGold", (0.42, 0.22, 0.035, 1), metallic=0.58, roughness=0.30)
    # Build one fused pear-shaped shell so the glove reads as boxing equipment
    # rather than overlapping primitive spheres. Metaballs are converted to a
    # regular mesh immediately; the saved asset contains no procedural field.
    meta = bpy.data.metaballs.new(name + ".Meta")
    meta.resolution = 0.010
    meta.render_resolution = 0.006
    meta.threshold = 0.62
    glove = bpy.data.objects.new(name, meta)
    bpy.context.collection.objects.link(glove)
    glove.location = center
    for co, radius, sx, sy, sz in [
        ((0.0, 0.0, 0.026), 0.101, 0.98, 0.88, 1.00),
        ((0.0, 0.004, -0.050), 0.082, 0.96, 0.92, 1.05),
        ((side * 0.053, -0.004, -0.033), 0.047, 0.86, 0.78, 1.12),
    ]:
        elem = meta.elements.new()
        elem.type = "ELLIPSOID"
        elem.co = co
        elem.radius = radius
        elem.size_x = sx
        elem.size_y = sy
        elem.size_z = sz
        elem.stiffness = 2.0
    bpy.context.view_layer.objects.active = glove
    glove.select_set(True)
    bpy.ops.object.convert(target="MESH")
    glove = bpy.context.object
    glove.name = name
    glove.data.materials.append(red)
    for p in glove.data.polygons:
        p.use_smooth = True
    bevel = glove.modifiers.new("GloveSoftSurface", "BEVEL")
    bevel.width = 0.0025
    bevel.segments = 2
    bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.050, depth=0.072, location=center)
    cuff = bpy.context.object
    cuff.data.materials.append(white)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=20, ring_count=12, location=center + Vector((0, -0.100, 0.015)))
    badge = bpy.context.object
    badge.scale = (0.022, 0.008, 0.022)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    badge.data.materials.append(gold)
    return {"side": side, "main": glove, "cuff": cuff, "badge": badge}


def update_glove_pose(rig, glove, bone_name):
    forearm = rig.pose.bones[bone_name]
    elbow = rig.matrix_world @ forearm.head
    wrist = rig.matrix_world @ forearm.tail
    direction = (wrist - elbow).normalized()
    center = wrist + direction * 0.078
    glove["main"].location = center
    glove["cuff"].location = wrist + direction * 0.014
    glove["cuff"].rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    glove["badge"].location = center + Vector((0, -0.100, 0.018))


def add_short_leg(name, x, material):
    sides = 36
    rings = [(0.735, 0.150, 0.155), (0.855, 0.143, 0.145), (0.965, 0.133, 0.132)]
    verts, faces = [], []
    for z, rx, ry in rings:
        for i in range(sides):
            a = 2 * math.pi * i / sides
            verts.append((x + math.cos(a) * rx, math.sin(a) * ry, z))
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
    bevel = obj.modifiers.new("ShortsSoftEdge", "BEVEL")
    bevel.width = 0.006
    bevel.segments = 2
    return obj


def add_front_overlap(material):
    name = "RamirezShorts.FrontOverlap"
    # Narrow central gusset closes the crotch while preserving separate leg openings.
    front_y = -0.158
    back_y = 0.138
    z0, z1 = 0.790, 0.963
    xb, xt = 0.040, 0.026
    verts = [
        (-xb, front_y, z0), (xb, front_y, z0), (xt, front_y, z1), (-xt, front_y, z1),
        (-xb, back_y, z0), (xb, back_y, z0), (xt, back_y, z1), (-xt, back_y, z1),
    ]
    faces = [
        (0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1),
        (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0),
    ]
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    flap = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(flap)
    flap.data.materials.append(material)
    bevel = flap.modifiers.new("FrontOverlapSoftEdge", "BEVEL")
    bevel.width = 0.008
    bevel.segments = 3
    return flap


def add_boxing_shorts_half(name, side, material, accent_material):
    rows = [
        # z, inner-x, outer-x, depth
        (0.982, 0.012, 0.222, 0.166),
        (0.895, 0.020, 0.236, 0.178),
        (0.800, 0.034, 0.252, 0.190),
        (0.705, 0.060, 0.272, 0.202),
    ]
    samples = 10
    verts = []
    for z, inner, outer, depth in rows:
        for front_sign in (-1, 1):
            for i in range(samples):
                t = i / (samples - 1)
                x_abs = inner + (outer - inner) * t
                # Rounded cloth shell around the hip/thigh instead of flat panels.
                curve = 1.0 - 0.18 * t * t
                y = front_sign * depth * curve
                z_local = z
                if z == rows[-1][0]:
                    z_local = 0.730 - 0.030 * t
                verts.append((side * x_abs, y, z_local))

    faces = []
    stride = samples * 2
    for r in range(len(rows) - 1):
        for surface in range(2):
            base = r * stride + surface * samples
            next_base = (r + 1) * stride + surface * samples
            for i in range(samples - 1):
                if surface == 0:
                    faces.append((base + i, base + i + 1, next_base + i + 1, next_base + i))
                else:
                    faces.append((base + i + 1, base + i, next_base + i, next_base + i + 1))
    # Inner and outer seams connect front/back surfaces.
    for r in range(len(rows) - 1):
        a0 = r * stride
        a1 = (r + 1) * stride
        b0 = a0 + samples
        b1 = a1 + samples
        faces.append((a0, a1, b1, b0))
        faces.append((a0 + samples - 1, b0 + samples - 1, b1 + samples - 1, a1 + samples - 1))
    # Waist and hem closure.
    top = 0
    bottom = (len(rows) - 1) * stride
    for i in range(samples - 1):
        faces.append((top + i, top + samples + i, top + samples + i + 1, top + i + 1))
        faces.append((bottom + i + 1, bottom + samples + i + 1, bottom + samples + i, bottom + i))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    obj.data.materials.append(accent_material)
    bevel = obj.modifiers.new("ShortsClothSoftness", "BEVEL")
    bevel.width = 0.008
    bevel.segments = 3
    sub = obj.modifiers.new("ShortsClothSubdivision", "SUBSURF")
    sub.levels = 2
    sub.render_levels = 2
    return obj


def add_shorts_side_panel(name, side, material):
    y_front, y_back = -0.151, -0.132
    z_top, z_bottom = 0.970, 0.715
    top_in, top_out = side * 0.199, side * 0.217
    bottom_in, bottom_out = side * 0.218, side * 0.239
    verts = [
        (top_in, y_front, z_top), (top_out, y_front, z_top),
        (bottom_out, y_front, z_bottom), (bottom_in, y_front, z_bottom),
        (top_in, y_back, z_top), (top_out, y_back, z_top),
        (bottom_out, y_back, z_bottom), (bottom_in, y_back, z_bottom),
    ]
    faces = [
        (0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1),
        (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0),
    ]
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    bevel = obj.modifiers.new("ShortsSideTrimSoftness", "BEVEL")
    bevel.width = 0.004
    bevel.segments = 2
    return obj


def add_boxing_boot(name, ankle, foot, side, rig, bone_name):
    white = make_material("BootWhite", (0.58, 0.55, 0.49, 1), roughness=0.55)
    gold = make_material("BootGold", (0.42, 0.22, 0.035, 1), metallic=0.55, roughness=0.32)
    red = make_material("BootRed", (0.075, 0.003, 0.005, 1), roughness=0.42)
    dark = make_material("BootSole", (0.025, 0.021, 0.020, 1), roughness=0.68)

    shoe_center = Vector((foot.x, foot.y - 0.020, max(0.055, foot.z + 0.052)))
    bpy.ops.mesh.primitive_cube_add(location=shoe_center)
    shoe = bpy.context.object
    shoe.name = name + ".Upper"
    shoe.scale = (0.057, 0.126, 0.043)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    shoe.data.materials.append(white)
    shoe_bevel = shoe.modifiers.new("BootUpperRound", "BEVEL")
    shoe_bevel.width = 0.024
    shoe_bevel.segments = 5

    cuff_center = Vector((ankle.x, ankle.y, 0.155))
    bpy.ops.mesh.primitive_cone_add(vertices=32, radius1=0.050, radius2=0.057, depth=0.225, location=cuff_center)
    cuff = bpy.context.object
    cuff.name = name + ".Cuff"
    cuff.data.materials.append(white)
    bevel = cuff.modifiers.new("BootCuffSoftness", "BEVEL")
    bevel.width = 0.010
    bevel.segments = 3

    bpy.ops.mesh.primitive_cube_add(location=(ankle.x, ankle.y - 0.052, 0.155))
    tongue = bpy.context.object
    tongue.name = name + ".Tongue"
    tongue.scale = (0.040, 0.009, 0.102)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    tongue.data.materials.append(red)
    tongue_bevel = tongue.modifiers.new("BootTongueRound", "BEVEL")
    tongue_bevel.width = 0.008
    tongue_bevel.segments = 3

    bpy.ops.mesh.primitive_uv_sphere_add(segments=28, ring_count=14, location=shoe_center + Vector((0, -0.015, -0.054)))
    sole = bpy.context.object
    sole.name = name + ".Sole"
    sole.scale = (0.067, 0.145, 0.016)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    sole.data.materials.append(dark)

    bpy.ops.mesh.primitive_cube_add(location=(ankle.x + side * 0.050, ankle.y - 0.004, 0.155))
    stripe = bpy.context.object
    stripe.name = name + ".GoldStripe"
    stripe.scale = (0.005, 0.040, 0.096)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    stripe.data.materials.append(gold)
    laces = []
    for i in range(7):
        z = 0.075 + i * 0.025
        bpy.ops.mesh.primitive_cube_add(location=(ankle.x, ankle.y - 0.057, z))
        lace = bpy.context.object
        lace.name = f"{name}.Lace.{i:02d}"
        lace.scale = (0.033, 0.0035, 0.0027)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        lace.data.materials.append(red)
        laces.append(lace)
    for obj in (shoe, cuff, tongue, sole, stripe, *laces):
        parent_to_bone(obj, rig, bone_name)
    return [shoe, cuff, tongue, sole, stripe, *laces]


def add_trunks_and_hair(rig):
    burgundy = make_material("RamirezTrunks", (0.050, 0.0025, 0.005, 1), metallic=0.03, roughness=0.34)
    gold = make_material("RamirezGold", (0.40, 0.20, 0.030, 1), metallic=0.58, roughness=0.30)
    left = add_boxing_shorts_half("RamirezShorts.L", 1, burgundy, gold)
    right = add_boxing_shorts_half("RamirezShorts.R", -1, burgundy, gold)
    bpy.ops.mesh.primitive_cylinder_add(vertices=64, radius=0.218, depth=0.070, location=(0, 0, 0.965))
    belt = bpy.context.object
    belt.name = "RamirezWaistband"
    belt.scale.y = 0.70
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    belt.data.materials.append(gold)
    plaque_black = make_material("WaistbandPlaque", (0.010, 0.008, 0.007, 1), roughness=0.48)
    bpy.ops.mesh.primitive_cube_add(location=(0, -0.160, 0.965))
    plaque = bpy.context.object
    plaque.name = "RamirezWaistbandPlaque"
    plaque.scale = (0.080, 0.010, 0.028)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    plaque.data.materials.append(plaque_black)
    bevel = plaque.modifiers.new("PlaqueSoftness", "BEVEL")
    bevel.width = 0.004
    bevel.segments = 2

    bpy.ops.object.text_add(location=(0, -0.172, 0.965), rotation=(math.radians(90), 0, 0))
    label = bpy.context.object
    label.name = "RamirezWaistbandLabel"
    label.data.body = "RAMIREZ"
    label.data.align_x = "CENTER"
    label.data.align_y = "CENTER"
    label.data.size = 0.050
    label.data.extrude = 0.0015
    label.data.materials.append(gold)
    gusset = add_front_overlap(burgundy)
    trim_left = add_shorts_side_panel("RamirezShorts.GoldSide.L", 1, gold)
    trim_right = add_shorts_side_panel("RamirezShorts.GoldSide.R", -1, gold)
    for obj in (left, right, belt, plaque, label, gusset, trim_left, trim_right):
        parent_to_bone(obj, rig, "pelvis")

    hair = make_material("RamirezHair", (0.002, 0.0015, 0.001, 1), roughness=0.92)
    if hair.use_nodes:
        nodes = hair.node_tree.nodes
        links = hair.node_tree.links
        bsdf = nodes.get("Principled BSDF")
        noise = nodes.get("HairCurlNoise") or nodes.new("ShaderNodeTexNoise")
        noise.name = "HairCurlNoise"
        noise.inputs["Scale"].default_value = 48.0
        noise.inputs["Detail"].default_value = 5.0
        noise.inputs["Roughness"].default_value = 0.82
        bump = nodes.get("HairCurlBump") or nodes.new("ShaderNodeBump")
        bump.name = "HairCurlBump"
        bump.inputs["Strength"].default_value = 0.32
        bump.inputs["Distance"].default_value = 0.010
        links.new(noise.outputs["Fac"], bump.inputs["Height"])
        links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    sides = 48
    rings = 10
    verts = []
    faces = []
    center = Vector((0, -0.048, 1.700))
    rx, ry, rz = 0.116, 0.112, 0.122
    for r in range(rings + 1):
        for i in range(sides):
            a = 2 * math.pi * i / sides
            front = max(0.0, -math.sin(a))
            side_factor = abs(math.cos(a))
            theta_max = 1.18 - 0.32 * front + 0.035 * side_factor
            theta = theta_max * r / rings
            rr = math.sin(theta)
            z = center.z + math.cos(theta) * rz
            verts.append((center.x + math.cos(a) * rx * rr, center.y + math.sin(a) * ry * rr, z))
    for r in range(rings):
        for i in range(sides):
            j = (i + 1) % sides
            a = r * sides + i
            b = r * sides + j
            c = (r + 1) * sides + j
            d = (r + 1) * sides + i
            faces.append((a, b, c, d))
    mesh = bpy.data.meshes.new("RamirezHairMesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    cap = bpy.data.objects.new("RamirezHair", mesh)
    bpy.context.collection.objects.link(cap)
    cap.data.materials.append(hair)
    tex = bpy.data.textures.new("HairSurfaceNoise", type="CLOUDS")
    tex.noise_scale = 0.016
    sub = cap.modifiers.new("HairSurfaceSubdivision", "SUBSURF")
    sub.levels = 2
    sub.render_levels = 2
    disp = cap.modifiers.new("HairTexture", "DISPLACE")
    disp.texture = tex
    disp.strength = 0.010
    disp.mid_level = 0.52
    parent_to_bone(cap, rig, "head")

    # Add a sparse deterministic curl layer so the silhouette reads as the
    # approved wet/curly hair instead of a smooth helmet.
    curls = []
    for ring_i, theta in enumerate((0.18, 0.31, 0.44, 0.57, 0.70, 0.83, 0.96)):
        samples = 10 + ring_i * 2
        for i in range(samples):
            a = 2 * math.pi * i / samples + ring_i * 0.23
            sx = math.sin(theta)
            loc = Vector((
                center.x + math.cos(a) * rx * sx * 0.92,
                center.y + math.sin(a) * ry * sx * 0.92,
                center.z + math.cos(theta) * rz + 0.006,
            ))
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.0085 + 0.0015 * (ring_i % 2), location=loc)
            curl = bpy.context.object
            curl.name = f"RamirezHairCurl.{ring_i:02d}.{i:02d}"
            curl.scale.z = 0.78
            bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
            curl.data.materials.append(hair)
            parent_to_bone(curl, rig, "head")
            curls.append(curl)

    return [left, right, belt, plaque, label, gusset, trim_left, trim_right, cap, *curls]


def set_pose_bone_direction(rig, bone_name, head, toward):
    pb = rig.pose.bones[bone_name]
    direction = Vector(toward) - Vector(head)
    if direction.length < 1e-6:
        return Vector(pb.tail)
    rest_rot = rig.data.bones[bone_name].matrix_local.to_3x3()
    rest_dir = (rest_rot @ Vector((0, 1, 0))).normalized()
    delta = rest_dir.rotation_difference(direction.normalized())
    matrix = (delta.to_matrix() @ rest_rot).to_4x4()
    matrix.translation = Vector(head)
    pb.matrix = matrix
    bpy.context.view_layer.update()
    return Vector(pb.tail)


def pose_arm_direct(rig, side, elbow_hint, hand_hint):
    suffix = side.lower()
    upper_name = f"upperarm_{suffix}"
    lower_name = f"lowerarm_{suffix}"
    upper = rig.pose.bones[upper_name]
    shoulder = Vector(upper.head)
    elbow = set_pose_bone_direction(rig, upper_name, shoulder, elbow_hint)
    wrist = set_pose_bone_direction(rig, lower_name, elbow, hand_hint)
    return elbow, wrist


def pose_leg_direct(rig, side, knee_hint, ankle_hint):
    suffix = side.lower()
    thigh_name = f"thigh_{suffix}"
    calf_name = f"calf_{suffix}"
    thigh = rig.pose.bones[thigh_name]
    hip = Vector(thigh.head)
    knee = set_pose_bone_direction(rig, thigh_name, hip, knee_hint)
    ankle = set_pose_bone_direction(rig, calf_name, knee, ankle_hint)
    foot_name = f"foot_{suffix}"
    rest_foot = rig.data.bones[foot_name]
    rest_vec = rest_foot.tail_local - rest_foot.head_local
    set_pose_bone_direction(rig, foot_name, ankle, ankle + rest_vec)
    return knee, ankle


def pose_targets(rig, gloves, name):
    # Reset pose state before applying deterministic arm segments.
    for pb in rig.pose.bones:
        pb.rotation_mode = "XYZ"
        pb.rotation_euler = (0, 0, 0)
        pb.scale = (1, 1, 1)

    poses = {
        "guard": {
            "L": ((0.30, -0.20, 1.29), (0.15, -0.35, 1.44)),
            "R": ((-0.27, -0.18, 1.30), (-0.11, -0.30, 1.49)),
        },
        "jab": {
            "L": ((0.16, -0.28, 1.44), (0.10, -0.55, 1.45)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
        "cross": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.16, -0.28, 1.43), (-0.10, -0.56, 1.44)),
        },
        "hook": {
            "L": ((0.42, -0.18, 1.38), (0.21, -0.37, 1.39)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
        "uppercut": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.34, -0.18, 1.24), (-0.18, -0.34, 1.42)),
        },
        "slip_left": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
        "slip_right": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
        "step_forward": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
        "step_backward": {
            "L": ((0.34, -0.20, 1.29), (0.18, -0.36, 1.45)),
            "R": ((-0.34, -0.19, 1.28), (-0.17, -0.35, 1.44)),
        },
    }

    if name == "guard":
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-2)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-3)
        rig.pose.bones["neck_01"].rotation_euler.x = math.radians(3)
    elif name == "slip_left":
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-8)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(8)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(-5)
    elif name == "slip_right":
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(8)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-8)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(5)
    elif name in ("jab", "hook"):
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-4)
    elif name == "cross":
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(5)
    elif name == "uppercut":
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-3)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(3)
    elif name == "step_forward":
        rig.pose.bones["pelvis"].rotation_euler.z = math.radians(-2)
        rig.pose.bones["thigh_l"].rotation_euler.x = math.radians(-8)
        rig.pose.bones["calf_l"].rotation_euler.x = math.radians(10)
        rig.pose.bones["thigh_r"].rotation_euler.x = math.radians(4)
    elif name == "step_backward":
        rig.pose.bones["pelvis"].rotation_euler.z = math.radians(2)
        rig.pose.bones["thigh_l"].rotation_euler.x = math.radians(5)
        rig.pose.bones["thigh_r"].rotation_euler.x = math.radians(-7)
        rig.pose.bones["calf_r"].rotation_euler.x = math.radians(9)
    bpy.context.view_layer.update()

    for side in ("L", "R"):
        elbow_hint, hand_hint = poses[name][side]
        pose_arm_direct(rig, side, elbow_hint, hand_hint)

    # A compact orthodox boxing base is used for every combat pose; the step
    # frames exaggerate only enough to expose hip/knee deformation for review.
    leg_targets = {
        "default": {
            "L": ((0.150, -0.050, 0.520), (0.202, -0.055, 0.074)),
            "R": ((-0.150, -0.018, 0.520), (-0.202, 0.012, 0.074)),
        },
        "guard": {
            "L": ((0.155, -0.100, 0.495), (0.205, -0.135, 0.074)),
            "R": ((-0.150, 0.060, 0.500), (-0.205, 0.115, 0.074)),
        },
        "step_forward": {
            "L": ((0.155, -0.085, 0.510), (0.202, -0.125, 0.074)),
            "R": ((-0.150, -0.010, 0.525), (-0.202, 0.035, 0.074)),
        },
        "step_backward": {
            "L": ((0.150, -0.030, 0.525), (0.202, -0.020, 0.074)),
            "R": ((-0.155, 0.040, 0.510), (-0.202, 0.105, 0.074)),
        },
    }
    selected_legs = leg_targets.get(name, leg_targets["default"])
    for side in ("L", "R"):
        knee_hint, ankle_hint = selected_legs[side]
        pose_leg_direct(rig, side, knee_hint, ankle_hint)

    # The source hand is open; compress it inside the glove after the forearm
    # matrices are final so no fingers remain visible through the glove shell.
    rig.pose.bones["hand_l"].scale = (0.42, 0.42, 0.42)
    rig.pose.bones["hand_r"].scale = (0.42, 0.42, 0.42)
    bpy.context.view_layer.update()
    if gloves["L"]["main"].parent_type != "BONE":
        update_glove_pose(rig, gloves["L"], "lowerarm_l")
    if gloves["R"]["main"].parent_type != "BONE":
        update_glove_pose(rig, gloves["R"], "lowerarm_r")
    bpy.context.view_layer.update()


def render_pose(scene, camera, out, filename, camera_position=(0, -4.2, 0.98)):
    camera.location = camera_position
    direction = Vector((0, 0, 1.00)) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    scene.render.filepath = str(out / filename)
    bpy.ops.render.render(write_still=True)


def render_neutral_views(scene, camera, out):
    views = [
        ("18_front_final.png", (0.0, -4.2, 0.95)),
        ("19_three_quarter_final.png", (2.55, -3.55, 1.00)),
        ("20_side_final.png", (4.25, 0.0, 0.98)),
        ("21_back_final.png", (0.0, 4.2, 0.95)),
        ("21b_three_quarter_back_final.png", (-2.55, 3.55, 1.00)),
    ]
    for filename, position in views:
        render_pose(scene, camera, out, filename, position)


def main():
    args = parse_args()
    repo = Path(args.repo)
    mpfb = Path(args.mpfb)
    material_out = repo / "art" / "blender" / "ramirez" / "renders" / "material"
    clay_out = repo / "art" / "blender" / "ramirez" / "renders" / "clay"
    material_out.mkdir(parents=True, exist_ok=True)
    clay_out.mkdir(parents=True, exist_ok=True)
    blend = repo / "art" / "blender" / "ramirez" / "source" / "ramirez_master_static.blend"
    rigged_blend = repo / "art" / "blender" / "ramirez" / "source" / "ramirez_master.blend"
    bpy.ops.wm.open_mainfile(filepath=str(blend))
    body = bpy.data.objects["Ramirez_Master_Body"]
    remove_old_accessories()
    ensure_skin_material()
    add_face_stubble(body)

    if bpy.data.objects.get("Key"):
        bpy.data.objects["Key"].data.energy = 900
    if bpy.data.objects.get("Fill"):
        bpy.data.objects["Fill"].data.energy = 180
    if bpy.data.objects.get("Rim"):
        bpy.data.objects["Rim"].data.energy = 650

    data_root = mpfb / "src" / "mpfb" / "data"
    joints, converted_verts = joint_centers(data_root / "3dobjs" / "base.obj", data_root / "targets")
    rig = build_mpfb_game_engine_rig(
        joints,
        converted_verts,
        data_root / "rigs" / "standard" / "rig.game_engine.json",
    )
    apply_mpfb_weights(body, rig, data_root / "rigs" / "standard" / "weights.game_engine.json")
    smooth = body.modifiers.new("RamirezCorrectiveSmooth", "CORRECTIVE_SMOOTH")
    smooth.factor = 0.42
    smooth.iterations = 4
    smooth.smooth_type = "LENGTH_WEIGHTED"
    smooth.use_pin_boundary = True
    add_trunks_and_hair(rig)
    add_boxing_boot("RamirezBoot.L", joints["joint-l-ankle"], joints["joint-l-foot-1"], 1, rig, "foot_l")
    add_boxing_boot("RamirezBoot.R", joints["joint-r-ankle"], joints["joint-r-foot-1"], -1, rig, "foot_r")
    gloves = {
        "L": add_boxing_glove("RamirezGlove.L", joints["joint-l-hand"], 1),
        "R": add_boxing_glove("RamirezGlove.R", joints["joint-r-hand"], -1),
    }

    camera = bpy.data.objects["VisualGateCamera"]
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"

    update_glove_pose(rig, gloves["L"], "lowerarm_l")
    update_glove_pose(rig, gloves["R"], "lowerarm_r")
    rig.pose.bones["hand_l"].scale = (0.42, 0.42, 0.42)
    rig.pose.bones["hand_r"].scale = (0.42, 0.42, 0.42)
    bpy.context.view_layer.update()
    render_neutral_views(scene, camera, material_out)

    # Deformation gate uses a neutral clay override so material/lighting cannot
    # hide shoulder, elbow, neck, hip or knee failures.
    clay = bpy.data.materials.get("ClayGate") or make_material("ClayGate", (0.42, 0.42, 0.40, 1.0), roughness=0.72)
    scene.view_layers[0].material_override = clay
    for pose_name, filename in [
        ("guard", "09_boxing_guard.png"),
        ("jab", "10_jab_extension.png"),
        ("cross", "11_cross_extension.png"),
        ("hook", "12_hook_pose.png"),
        ("uppercut", "13_uppercut_pose.png"),
        ("slip_left", "14_slip_left.png"),
        ("slip_right", "15_slip_right.png"),
        ("step_forward", "16_step_forward.png"),
        ("step_backward", "17_step_backward.png"),
    ]:
        pose_targets(rig, gloves, pose_name)
        cam_pos = (0, -4.2, 0.98) if pose_name == "guard" else (2.55, -3.55, 1.02)
        render_pose(scene, camera, clay_out, filename, camera_position=cam_pos)
    scene.view_layers[0].material_override = None

    for pose_name, filename in [
        ("guard", "22_guard_final.png"),
        ("jab", "23_jab_final.png"),
        ("hook", "24_hook_final.png"),
        ("uppercut", "25_uppercut_final.png"),
        ("slip_left", "26_slip_final.png"),
    ]:
        pose_targets(rig, gloves, pose_name)
        cam_pos = (0, -4.2, 0.98) if pose_name == "guard" else (2.55, -3.55, 1.02)
        render_pose(scene, camera, material_out, filename, camera_position=cam_pos)

    bpy.ops.wm.save_as_mainfile(filepath=str(rigged_blend))
    report = {
        "rigged_blend": str(rigged_blend),
        "bone_count": len(rig.data.bones),
        "body_vertices": len(body.data.vertices),
        "body_triangles": sum(len(p.vertices) - 2 for p in body.data.polygons),
        "rendered_poses": [
            "guard", "jab", "cross", "hook", "uppercut", "slip_left",
            "slip_right", "step_forward", "step_backward"
        ],
    }
    (repo / "art" / "blender" / "ramirez" / "rig-metrics.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8"
    )
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
