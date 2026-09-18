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


def bind_shorts_half_to_rig(obj, rig, side, row_count=5, samples=32):
    """Blend the trunk leg from pelvis to thigh instead of rigid pelvis parenting.

    This keeps the upper cloth anchored under the waistband while allowing the
    lower opening to follow the thigh during stance, slips and rear-foot drive.
    """
    pelvis_group = obj.vertex_groups.new(name="pelvis")
    thigh_name = "thigh_l" if side > 0 else "thigh_r"
    thigh_group = obj.vertex_groups.new(name=thigh_name)
    # Top remains mostly with the pelvis; the hem follows the thigh strongly.
    thigh_weights = (0.12, 0.28, 0.48, 0.70, 0.88)
    for row in range(row_count):
        ids = list(range(row * samples, (row + 1) * samples))
        tw = thigh_weights[row]
        pelvis_group.add(ids, 1.0 - tw, "REPLACE")
        thigh_group.add(ids, tw, "REPLACE")
    # Top-cap center is the final generated vertex.
    top_center = row_count * samples
    if top_center < len(obj.data.vertices):
        pelvis_group.add([top_center], 1.0, "REPLACE")
    arm = obj.modifiers.new("ShortsArmature", "ARMATURE")
    arm.object = rig
    # Armature deformation must happen before smoothing/thickness modifiers.
    while obj.modifiers.find(arm.name) > 0:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_move_up(modifier=arm.name)


def translate_pose_bone_world(rig, bone_name, delta):
    """Translate a pose bone by an armature/world-aligned delta safely.

    PoseBone.location is expressed in the bone's local basis. Convert the
    requested world-aligned delta into that basis instead of assigning
    PoseBone.matrix, which would fight later Euler rotations and parent-space
    direct posing.
    """
    pb = rig.pose.bones[bone_name]
    rest_rot = rig.data.bones[bone_name].matrix_local.to_3x3()
    local_delta = rest_rot.inverted() @ Vector(delta)
    pb.location += local_delta
    bpy.context.view_layer.update()


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
    # Keep the small gold mark physically attached to the glove.  The previous
    # world-space sphere did not rotate with the fist and visibly floated away
    # in side/punch views.
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=32,
        radius=0.021,
        depth=0.004,
        location=center + Vector((0, -0.096, 0.024)),
        rotation=(math.radians(90), 0, 0),
    )
    badge = bpy.context.object
    badge.data.materials.append(gold)
    badge.parent = glove
    badge.matrix_parent_inverse = glove.matrix_world.inverted()
    return {"side": side, "main": glove, "cuff": cuff, "badge": badge}


def update_glove_pose(rig, glove, bone_name):
    forearm = rig.pose.bones[bone_name]
    elbow = rig.matrix_world @ forearm.head
    wrist = rig.matrix_world @ forearm.tail
    direction = (wrist - elbow).normalized()
    center = wrist + direction * 0.078
    glove_rot = direction.to_track_quat("Z", "Y")
    glove["main"].location = center
    glove["main"].rotation_euler = glove_rot.to_euler()
    glove["cuff"].location = wrist + direction * 0.014
    glove["cuff"].rotation_euler = glove_rot.to_euler()


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
    # Loose boxing-trunk leg shell.  The old implementation was built from two
    # flat front/back sheets and read as a rigid skirt.  Concentric elliptical
    # rings give each leg a real opening, outward flare and soft cloth drape.
    rows = [
        # z, center_x, radius_x, radius_y
        (0.972, 0.088, 0.124, 0.148),
        (0.914, 0.094, 0.126, 0.154),
        (0.852, 0.101, 0.127, 0.160),
        (0.790, 0.110, 0.126, 0.166),
        (0.728, 0.119, 0.123, 0.171),
    ]
    samples = 32
    verts = []
    for row_i, (z, center_x, rx, ry) in enumerate(rows):
        t_row = row_i / (len(rows) - 1)
        for i in range(samples):
            a = 2.0 * math.pi * i / samples
            # Low-frequency cloth waviness keeps highlights from reading as a
            # hard geometric panel while preserving a clean game-ready shell.
            wrinkle = 1.0 + t_row * (
                0.060 * math.sin(3.0 * a + side * 0.7)
                + 0.028 * math.sin(5.0 * a - side * 0.35)
            )
            x = side * (center_x + math.cos(a) * rx * wrinkle)
            y = math.sin(a) * ry * (1.0 + 0.025 * math.cos(2.0 * a))
            z_local = z
            if row_i == len(rows) - 1:
                # Slightly longer on the outside/back like the approved trunks,
                # with an actual open hem around each thigh.
                outside = 0.5 + 0.5 * side * math.cos(a)
                back = max(0.0, math.sin(a))
                z_local -= 0.020 * outside + 0.010 * back
            verts.append((x, y, z_local))

    faces = []
    for r in range(len(rows) - 1):
        base = r * samples
        next_base = (r + 1) * samples
        for i in range(samples):
            j = (i + 1) % samples
            faces.append((base + i, base + j, next_base + j, next_base + i))

    # Close the upper ring under the waistband.  The lower ring intentionally
    # remains open to preserve a true boxing-trunk leg opening.
    top_center = len(verts)
    verts.append((side * rows[0][1], 0.0, rows[0][0]))
    for i in range(samples):
        j = (i + 1) % samples
        faces.append((top_center, j, i))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for poly in mesh.polygons:
        poly.use_smooth = True
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    obj.data.materials.append(accent_material)
    # Integrate the approved gold side/hem treatment into the deforming cloth
    # itself.  This replaces the previous rigid gold rods that floated beside
    # the shorts during stance and punch poses.
    for poly in obj.data.polygons:
        c = poly.center
        # Narrow textile trim only.  A broad threshold turned the entire outer
        # third of each leg into a rigid-looking gold slab in Review 2.
        outer_side = side * c.x > 0.232
        hem_band = c.z < 0.735
        if outer_side or hem_band:
            poly.material_index = 1
    bevel = obj.modifiers.new("ShortsClothSoftness", "BEVEL")
    bevel.width = 0.004
    bevel.segments = 3
    sub = obj.modifiers.new("ShortsClothSubdivision", "SUBSURF")
    sub.levels = 1
    sub.render_levels = 1
    solid = obj.modifiers.new("ShortsClothThickness", "SOLIDIFY")
    solid.thickness = 0.0035
    solid.offset = 0.0
    return obj


def add_shorts_side_panel(name, side, material):
    # Curved side tape following the outer cloth silhouette rather than a flat
    # rectangular plate.
    curve_data = bpy.data.curves.new(name + "Curve", type="CURVE")
    curve_data.dimensions = "3D"
    curve_data.bevel_depth = 0.008
    curve_data.bevel_resolution = 3
    spline = curve_data.splines.new("BEZIER")
    spline.bezier_points.add(3)
    points = [
        (side * 0.232, -0.010, 0.958),
        (side * 0.244, -0.008, 0.875),
        (side * 0.260, -0.005, 0.790),
        (side * 0.273, 0.000, 0.704),
    ]
    for bp, co in zip(spline.bezier_points, points):
        bp.co = co
        bp.handle_left_type = "AUTO"
        bp.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, curve_data)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def add_boxing_waistband(material):
    name = "RamirezWaistband"
    samples = 64
    rings = [
        (0.935, 0.218, 0.151),
        (0.952, 0.224, 0.155),
        (0.975, 0.225, 0.156),
        (0.998, 0.222, 0.153),
        (1.012, 0.216, 0.150),
    ]
    verts = []
    faces = []
    for r, (z, rx, ry) in enumerate(rings):
        for i in range(samples):
            a = 2.0 * math.pi * i / samples
            ripple = 1.0 + 0.008 * math.sin(6.0 * a + r * 0.4)
            verts.append((math.cos(a) * rx * ripple, math.sin(a) * ry * ripple, z))
    for r in range(len(rings) - 1):
        a0 = r * samples
        a1 = (r + 1) * samples
        for i in range(samples):
            j = (i + 1) % samples
            faces.append((a0 + i, a0 + j, a1 + j, a1 + i))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for poly in mesh.polygons:
        poly.use_smooth = True
    belt = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(belt)
    belt.data.materials.append(material)
    bevel = belt.modifiers.new("WaistbandSoftEdge", "BEVEL")
    bevel.width = 0.0035
    bevel.segments = 3
    solid = belt.modifiers.new("WaistbandThickness", "SOLIDIFY")
    solid.thickness = 0.004
    solid.offset = 0.0
    return belt


def add_boxing_boot(name, ankle, foot, side, rig, bone_name):
    white = make_material("BootWhite", (0.58, 0.55, 0.49, 1), roughness=0.55)
    gold = make_material("BootGold", (0.42, 0.22, 0.035, 1), metallic=0.55, roughness=0.32)
    red = make_material("BootRed", (0.075, 0.003, 0.005, 1), roughness=0.42)
    dark = make_material("BootSole", (0.025, 0.021, 0.020, 1), roughness=0.68)

    shoe_center = Vector((foot.x, foot.y - 0.035, max(0.060, foot.z + 0.050)))
    bpy.ops.mesh.primitive_uv_sphere_add(segments=36, ring_count=20, location=shoe_center)
    shoe = bpy.context.object
    shoe.name = name + ".Upper"
    shoe.scale = (0.058, 0.132, 0.047)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    shoe.data.materials.append(red)
    for poly in shoe.data.polygons:
        poly.use_smooth = True

    cuff_center = Vector((ankle.x, ankle.y, 0.155))
    bpy.ops.mesh.primitive_cone_add(vertices=32, radius1=0.050, radius2=0.057, depth=0.225, location=cuff_center)
    cuff = bpy.context.object
    cuff.name = name + ".Cuff"
    cuff.data.materials.append(red)
    bevel = cuff.modifiers.new("BootCuffSoftness", "BEVEL")
    bevel.width = 0.010
    bevel.segments = 3

    bpy.ops.mesh.primitive_uv_sphere_add(segments=28, ring_count=14, location=(ankle.x, ankle.y - 0.049, 0.155))
    tongue = bpy.context.object
    tongue.name = name + ".Tongue"
    tongue.scale = (0.040, 0.010, 0.102)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    tongue.data.materials.append(white)

    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, location=shoe_center + Vector((0, -0.006, -0.052)))
    sole = bpy.context.object
    sole.name = name + ".Sole"
    sole.scale = (0.065, 0.142, 0.014)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    sole.data.materials.append(dark)

    stripe_curve = bpy.data.curves.new(name + ".GoldStripeCurve", type="CURVE")
    stripe_curve.dimensions = "3D"
    stripe_curve.bevel_depth = 0.0045
    stripe_curve.bevel_resolution = 3
    stripe_spline = stripe_curve.splines.new("BEZIER")
    stripe_spline.bezier_points.add(2)
    for bp, co in zip(stripe_spline.bezier_points, [
        (ankle.x + side * 0.051, ankle.y - 0.004, 0.075),
        (ankle.x + side * 0.053, ankle.y - 0.003, 0.155),
        (ankle.x + side * 0.050, ankle.y - 0.001, 0.238),
    ]):
        bp.co = co
        bp.handle_left_type = "AUTO"
        bp.handle_right_type = "AUTO"
    stripe = bpy.data.objects.new(name + ".GoldStripe", stripe_curve)
    bpy.context.collection.objects.link(stripe)
    stripe.data.materials.append(gold)
    laces = []
    for i in range(7):
        z = 0.075 + i * 0.025
        bpy.ops.mesh.primitive_cube_add(location=(ankle.x, ankle.y - 0.058, z))
        lace = bpy.context.object
        lace.name = f"{name}.Lace.{i:02d}"
        lace.scale = (0.030, 0.0026, 0.0018)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        lace.data.materials.append(red)
        laces.append(lace)
    for obj in (shoe, cuff, tongue, sole, stripe, *laces):
        parent_to_bone(obj, rig, bone_name)
    return [shoe, cuff, tongue, sole, stripe, *laces]


def add_trunks_and_hair(rig):
    burgundy = make_material("RamirezTrunks", (0.050, 0.0025, 0.005, 1), metallic=0.0, roughness=0.52)
    gold = make_material("RamirezGold", (0.40, 0.20, 0.030, 1), metallic=0.58, roughness=0.30)
    if burgundy.use_nodes:
        nodes = burgundy.node_tree.nodes
        links = burgundy.node_tree.links
        bsdf = nodes.get("Principled BSDF")
        noise = nodes.get("TrunksClothNoise") or nodes.new("ShaderNodeTexNoise")
        noise.name = "TrunksClothNoise"
        noise.inputs["Scale"].default_value = 24.0
        noise.inputs["Detail"].default_value = 2.5
        noise.inputs["Roughness"].default_value = 0.66
        bump = nodes.get("TrunksClothBump") or nodes.new("ShaderNodeBump")
        bump.name = "TrunksClothBump"
        bump.inputs["Strength"].default_value = 0.09
        bump.inputs["Distance"].default_value = 0.004
        links.new(noise.outputs["Fac"], bump.inputs["Height"])
        links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    left = add_boxing_shorts_half("RamirezShorts.L", 1, burgundy, gold)
    right = add_boxing_shorts_half("RamirezShorts.R", -1, burgundy, gold)
    bind_shorts_half_to_rig(left, rig, 1)
    bind_shorts_half_to_rig(right, rig, -1)
    belt = add_boxing_waistband(gold)
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
    for obj in (belt, plaque, label):
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

    # Review 2 used individual ico-spheres for curls; from normal review
    # distance they read as obvious beads.  Keep curl character in the
    # displaced continuous cap so the hair remains one coherent surface.
    tex.noise_scale = 0.011
    disp.strength = 0.014

    return [left, right, belt, plaque, label, cap]


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


def pose_leg_direct(rig, side, knee_hint, ankle_hint, foot_yaw_degrees=0.0):
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
    yaw = math.radians(foot_yaw_degrees)
    c, s = math.cos(yaw), math.sin(yaw)
    foot_vec = Vector((
        rest_vec.x * c - rest_vec.y * s,
        rest_vec.x * s + rest_vec.y * c,
        rest_vec.z,
    ))
    set_pose_bone_direction(rig, foot_name, ankle, ankle + foot_vec)
    return knee, ankle


def pose_targets(rig, gloves, name):
    # Reset pose state before applying deterministic arm segments.
    for pb in rig.pose.bones:
        pb.rotation_mode = "XYZ"
        pb.location = (0, 0, 0)
        pb.rotation_euler = (0, 0, 0)
        pb.scale = (1, 1, 1)

    poses = {
        "static_stance": {
            "L": ((0.285, -0.205, 1.300), (0.135, -0.335, 1.445)),
            "R": ((-0.275, -0.185, 1.305), (-0.115, -0.305, 1.485)),
        },
        "guard": {
            "L": ((0.285, -0.215, 1.295), (0.135, -0.355, 1.440)),
            "R": ((-0.270, -0.190, 1.305), (-0.105, -0.315, 1.485)),
        },
        "jab": {
            "L": ((0.190, -0.400, 1.405), (0.100, -0.705, 1.415)),
            "R": ((-0.275, -0.185, 1.305), (-0.110, -0.310, 1.480)),
        },
        "cross": {
            "L": ((0.280, -0.185, 1.325), (0.145, -0.255, 1.495)),
            "R": ((-0.175, -0.410, 1.400), (-0.070, -0.720, 1.410)),
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
            "L": ((0.275, -0.205, 1.285), (0.135, -0.335, 1.435)),
            "R": ((-0.255, -0.190, 1.300), (-0.100, -0.310, 1.470)),
        },
        "slip_right": {
            "L": ((0.255, -0.195, 1.300), (0.100, -0.315, 1.470)),
            "R": ((-0.275, -0.205, 1.285), (-0.135, -0.335, 1.435)),
        },
        "slip_counter": {
            "L": ((0.275, -0.185, 1.325), (0.140, -0.255, 1.490)),
            "R": ((-0.170, -0.395, 1.390), (-0.055, -0.690, 1.405)),
        },
        "recover_guard": {
            "L": ((0.280, -0.205, 1.300), (0.135, -0.345, 1.445)),
            "R": ((-0.265, -0.185, 1.305), (-0.105, -0.305, 1.480)),
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

    # Every review pose starts from a real boxing base: lowered center of mass,
    # slight forward crouch, tucked chin and active shoulders.  Punch-specific
    # rotation is layered on top instead of leaving the torso upright.
    if name in {"static_stance", "guard", "jab", "cross", "slip_left", "slip_right", "slip_counter", "recover_guard"}:
        translate_pose_bone_world(rig, "pelvis", (0.0, 0.0, -0.064))
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-7)
        rig.pose.bones["neck_01"].rotation_euler.x = math.radians(9)

    if name in {"static_stance", "guard", "recover_guard"}:
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-5)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-7)
    elif name == "jab":
        # Lead shoulder reaches without over-rotating the hips.
        translate_pose_bone_world(rig, "pelvis", (0.010, -0.010, -0.006))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-7)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-15)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-8)
    elif name == "cross":
        # Rear hip and shoulder drive through together; the rear heel is lifted
        # and foot pivot is supplied by the leg target below.
        translate_pose_bone_world(rig, "pelvis", (0.016, -0.018, -0.008))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(17)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(28)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-9)
    elif name == "slip_left":
        translate_pose_bone_world(rig, "pelvis", (-0.024, 0.0, -0.012))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-3)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-4)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-13)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(-6)
    elif name == "slip_right":
        translate_pose_bone_world(rig, "pelvis", (0.024, 0.0, -0.012))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(2)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(3)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(13)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(6)
    elif name == "slip_counter":
        # Counter keeps the head off the center line while the rear hip/shoulder
        # rotate into the straight.  This is intentionally distinct from a
        # static slip followed by an unrelated arm extension.
        translate_pose_bone_world(rig, "pelvis", (-0.018, -0.015, -0.014))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(14)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(25)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-9)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(-10)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(-4)
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

    # Orthodox base: lead (L) foot forward, rear (R) foot back, both knees bent.
    # Each action keeps that split stance and changes loading/pivot rather than
    # returning to a square mannequin base.
    leg_targets = {
        "default": {
            "L": ((0.158, -0.110, 0.445), (0.198, -0.190, 0.074), 8.0),
            "R": ((-0.152, 0.080, 0.455), (-0.198, 0.160, 0.078), -24.0),
        },
        "static_stance": {
            "L": ((0.158, -0.112, 0.442), (0.198, -0.190, 0.074), 8.0),
            "R": ((-0.152, 0.082, 0.452), (-0.198, 0.160, 0.078), -24.0),
        },
        "guard": {
            "L": ((0.160, -0.116, 0.438), (0.200, -0.195, 0.074), 8.0),
            "R": ((-0.154, 0.084, 0.450), (-0.200, 0.165, 0.078), -24.0),
        },
        "jab": {
            "L": ((0.162, -0.130, 0.432), (0.208, -0.210, 0.074), 6.0),
            "R": ((-0.152, 0.080, 0.455), (-0.198, 0.160, 0.078), -24.0),
        },
        "cross": {
            "L": ((0.160, -0.116, 0.440), (0.200, -0.192, 0.074), 8.0),
            "R": ((-0.126, 0.048, 0.422), (-0.172, 0.132, 0.142), -46.0),
        },
        "slip_left": {
            "L": ((0.138, -0.120, 0.425), (0.192, -0.190, 0.074), 8.0),
            "R": ((-0.170, 0.084, 0.448), (-0.208, 0.160, 0.078), -24.0),
        },
        "slip_right": {
            "L": ((0.175, -0.114, 0.440), (0.208, -0.190, 0.074), 8.0),
            "R": ((-0.134, 0.082, 0.432), (-0.192, 0.160, 0.082), -26.0),
        },
        "slip_counter": {
            "L": ((0.142, -0.118, 0.430), (0.194, -0.190, 0.074), 8.0),
            "R": ((-0.130, 0.048, 0.418), (-0.174, 0.130, 0.138), -44.0),
        },
        "recover_guard": {
            "L": ((0.160, -0.114, 0.438), (0.200, -0.192, 0.074), 8.0),
            "R": ((-0.154, 0.082, 0.450), (-0.200, 0.162, 0.078), -24.0),
        },
        "step_forward": {
            "L": ((0.155, -0.085, 0.510), (0.202, -0.125, 0.074), 8.0),
            "R": ((-0.150, -0.010, 0.525), (-0.202, 0.035, 0.074), -22.0),
        },
        "step_backward": {
            "L": ((0.150, -0.030, 0.525), (0.202, -0.020, 0.074), 8.0),
            "R": ((-0.155, 0.040, 0.510), (-0.202, 0.105, 0.074), -22.0),
        },
    }
    selected_legs = leg_targets.get(name, leg_targets["default"])
    for side in ("L", "R"):
        knee_hint, ankle_hint, foot_yaw = selected_legs[side]
        pose_leg_direct(rig, side, knee_hint, ankle_hint, foot_yaw_degrees=foot_yaw)

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


def render_static_stance_views(scene, camera, out, rig, gloves):
    pose_targets(rig, gloves, "static_stance")
    views = [
        ("01_front.png", (0.0, -4.2, 0.95)),
        ("02_three_quarter_front.png", (2.55, -3.55, 1.00)),
        ("03_side.png", (4.25, 0.0, 0.98)),
        ("04_back.png", (0.0, 4.2, 0.95)),
        ("05_three_quarter_back.png", (-2.55, 3.55, 1.00)),
    ]
    for filename, position in views:
        render_pose(scene, camera, out, filename, position)


def main():
    args = parse_args()
    repo = Path(args.repo)
    mpfb = Path(args.mpfb)
    review_root = repo / "art" / "blender" / "ramirez" / "renders" / "review3"
    static_out = review_root / "static"
    combat_out = review_root / "combat"
    combat_clay_out = review_root / "combat-clay"
    static_out.mkdir(parents=True, exist_ok=True)
    combat_out.mkdir(parents=True, exist_ok=True)
    combat_clay_out.mkdir(parents=True, exist_ok=True)
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
    render_static_stance_views(scene, camera, static_out, rig, gloves)

    # Deformation gate uses a neutral clay override so material/lighting cannot
    # hide shoulder, elbow, neck, hip or knee failures.
    clay = bpy.data.materials.get("ClayGate") or make_material("ClayGate", (0.42, 0.42, 0.40, 1.0), roughness=0.72)
    scene.view_layers[0].material_override = clay
    review_poses = [
        ("guard", "01_neutral_guard.png", (1.10, -4.10, 1.00)),
        ("jab", "02_jab.png", (1.45, -4.05, 1.02)),
        ("cross", "03_cross.png", (-1.55, -4.00, 1.02)),
        ("slip_left", "04_slip_left.png", (1.05, -4.10, 1.00)),
        ("slip_right", "05_slip_right.png", (-1.05, -4.10, 1.00)),
        ("slip_counter", "06_slip_counter.png", (-1.45, -4.05, 1.02)),
        ("recover_guard", "07_recover_guard.png", (1.10, -4.10, 1.00)),
    ]
    for pose_name, filename, cam_pos in review_poses:
        pose_targets(rig, gloves, pose_name)
        render_pose(scene, camera, combat_clay_out, filename, camera_position=cam_pos)
    scene.view_layers[0].material_override = None

    for pose_name, filename, cam_pos in review_poses:
        pose_targets(rig, gloves, pose_name)
        render_pose(scene, camera, combat_out, filename, camera_position=cam_pos)

    # Leave the Blender source in the same approved-reference stance used by
    # the five static review views so opening the file never presents an A-pose.
    pose_targets(rig, gloves, "static_stance")
    bpy.ops.wm.save_as_mainfile(filepath=str(rigged_blend))
    report = {
        "rigged_blend": str(rigged_blend),
        "bone_count": len(rig.data.bones),
        "body_vertices": len(body.data.vertices),
        "body_triangles": sum(len(p.vertices) - 2 for p in body.data.polygons),
        "static_review_pose": "static_stance",
        "rendered_poses": [
            "guard", "jab", "cross", "slip_left", "slip_right",
            "slip_counter", "recover_guard"
        ],
        "review_root": str(review_root),
    }
    (repo / "art" / "blender" / "ramirez" / "rig-metrics.json").write_text(
        json.dumps(report, indent=2), encoding="utf-8"
    )
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
