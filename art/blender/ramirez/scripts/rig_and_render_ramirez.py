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
    ("arms/l-lowerarm-muscle-incr.target.gz", 0.94),
    ("arms/r-lowerarm-muscle-incr.target.gz", 0.94),
    ("arms/l-lowerarm-scale-horiz-incr.target.gz", 0.10),
    ("arms/r-lowerarm-scale-horiz-incr.target.gz", 0.10),
    ("arms/l-lowerarm-scale-depth-incr.target.gz", 0.10),
    ("arms/r-lowerarm-scale-depth-incr.target.gz", 0.10),
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
    # Aim for a living, sweaty boxer rather than a uniform plastic mannequin.
    # The reference has broad diffuse skin response, restrained specular and
    # subtle micro breakup. Keep this procedural so Review5 remains portable.
    bsdf.inputs["Base Color"].default_value = (0.082, 0.024, 0.012, 1.0)
    bsdf.inputs["Roughness"].default_value = 0.71
    if "IOR" in bsdf.inputs:
        bsdf.inputs["IOR"].default_value = 1.40
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.28
    if "Subsurface Weight" in bsdf.inputs:
        bsdf.inputs["Subsurface Weight"].default_value = 0.07
    elif "Subsurface" in bsdf.inputs:
        bsdf.inputs["Subsurface"].default_value = 0.07
    noise = nodes.get("SkinMicroNoise") or nodes.new("ShaderNodeTexNoise")
    noise.name = "SkinMicroNoise"
    noise.inputs["Scale"].default_value = 62.0
    noise.inputs["Detail"].default_value = 4.0
    noise.inputs["Roughness"].default_value = 0.7
    bump = nodes.get("SkinMicroBump") or nodes.new("ShaderNodeBump")
    bump.name = "SkinMicroBump"
    bump.inputs["Strength"].default_value = 0.085
    bump.inputs["Distance"].default_value = 0.0018
    links.new(noise.outputs["Fac"], bump.inputs["Height"])
    links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])

    # Break up roughness independently from the normal so highlights do not
    # form one continuous glossy sheet across chest, arms and face.
    rough_noise = nodes.get("SkinRoughnessNoise") or nodes.new("ShaderNodeTexNoise")
    rough_noise.name = "SkinRoughnessNoise"
    rough_noise.inputs["Scale"].default_value = 9.0
    rough_noise.inputs["Detail"].default_value = 2.0
    rough_noise.inputs["Roughness"].default_value = 0.62
    ramp = nodes.get("SkinRoughnessRamp") or nodes.new("ShaderNodeValToRGB")
    ramp.name = "SkinRoughnessRamp"
    ramp.color_ramp.elements[0].position = 0.28
    ramp.color_ramp.elements[0].color = (0.48, 0.48, 0.48, 1.0)
    ramp.color_ramp.elements[1].position = 0.76
    ramp.color_ramp.elements[1].color = (0.78, 0.78, 0.78, 1.0)
    links.new(rough_noise.outputs["Fac"], ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"], bsdf.inputs["Roughness"])


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


def make_glove_leather_material():
    """Dark burgundy boxing leather with broad, broken highlights.

    The approved reference reads as padded leather: the highlight is soft and
    uneven, with fine surface breakup.  A low-roughness uniform Principled
    shader made the previous gloves read as hard plastic.
    """
    mat = make_material("GloveRed", (0.052, 0.0032, 0.0065, 1.0), metallic=0.0, roughness=0.56)
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    if "IOR" in bsdf.inputs:
        bsdf.inputs["IOR"].default_value = 1.39
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.30

    micro = nodes.get("GloveLeatherMicro") or nodes.new("ShaderNodeTexNoise")
    micro.name = "GloveLeatherMicro"
    micro.inputs["Scale"].default_value = 115.0
    micro.inputs["Detail"].default_value = 3.2
    micro.inputs["Roughness"].default_value = 0.72
    bump = nodes.get("GloveLeatherBump") or nodes.new("ShaderNodeBump")
    bump.name = "GloveLeatherBump"
    bump.inputs["Strength"].default_value = 0.12
    bump.inputs["Distance"].default_value = 0.0009
    links.new(micro.outputs["Fac"], bump.inputs["Height"])
    links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])

    rough_noise = nodes.get("GloveRoughnessNoise") or nodes.new("ShaderNodeTexNoise")
    rough_noise.name = "GloveRoughnessNoise"
    rough_noise.inputs["Scale"].default_value = 7.0
    rough_noise.inputs["Detail"].default_value = 2.0
    rough_noise.inputs["Roughness"].default_value = 0.68
    rough_ramp = nodes.get("GloveRoughnessRamp") or nodes.new("ShaderNodeValToRGB")
    rough_ramp.name = "GloveRoughnessRamp"
    rough_ramp.color_ramp.elements[0].position = 0.28
    rough_ramp.color_ramp.elements[0].color = (0.44, 0.44, 0.44, 1.0)
    rough_ramp.color_ramp.elements[1].position = 0.76
    rough_ramp.color_ramp.elements[1].color = (0.68, 0.68, 0.68, 1.0)
    links.new(rough_noise.outputs["Fac"], rough_ramp.inputs["Fac"])
    links.new(rough_ramp.outputs["Color"], bsdf.inputs["Roughness"])
    return mat


def add_glove_crease(parent, name, points, material, thickness=0.0022):
    curve = bpy.data.curves.new(name + ".Curve", type="CURVE")
    curve.dimensions = "3D"
    curve.resolution_u = 3
    curve.bevel_depth = thickness
    curve.bevel_resolution = 2
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for bp, point in zip(spline.bezier_points, points):
        bp.co = point
        bp.handle_left_type = "AUTO"
        bp.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    obj.parent = parent
    obj.location = (0.0, 0.0, 0.0)
    return obj


def add_lofted_glove_shell(name, material):
    """Build Review18 as a curved padded-fist cage instead of an axial loft.

    Local +Z follows the forearm/hand axis after update_glove_pose(). Cross
    sections now rotate along a curved centerline that bulges through the
    knuckle dome, rolls down over folded fingers, then returns toward the palm.
    The closure sits behind and below the striking face, so subdivision cannot
    pull the front into the Review17 paddle/boat-nose silhouette.
    """
    sides = 48
    # z, center_y, half_width_x, half_depth_in_curve_normal
    sections = [
        (-0.110,  0.000, 0.056, 0.052),  # wrist throat
        (-0.060, -0.002, 0.082, 0.064),  # palm transition
        (-0.005, -0.004, 0.104, 0.078),  # backhand swells quickly
        ( 0.045, -0.008, 0.116, 0.088),  # knuckle dome begins
        ( 0.085, -0.016, 0.121, 0.094),  # maximum knuckle dome
        ( 0.115, -0.036, 0.120, 0.094),  # broad striking shoulder
        ( 0.126, -0.070, 0.116, 0.091),  # face starts rolling downward
        ( 0.118, -0.108, 0.110, 0.084),  # folded finger mass
        ( 0.092, -0.137, 0.101, 0.074),  # undercut under fingers
        ( 0.052, -0.151, 0.090, 0.063),  # palm return
        ( 0.012, -0.146, 0.074, 0.052),  # closure shoulder behind face
        (-0.018, -0.128, 0.056, 0.041),  # underside terminal ring
    ]
    terminal = (0.0, -0.103, -0.040)
    verts, faces = [], []

    normals = []
    for index, (z, center_y, _rx, _ry) in enumerate(sections):
        if index == 0:
            z2, y2 = sections[1][0], sections[1][1]
            dz, dy = z2 - z, y2 - center_y
        elif index == len(sections) - 1:
            z1, y1 = sections[index - 1][0], sections[index - 1][1]
            dz, dy = z - z1, center_y - y1
        else:
            z1, y1 = sections[index - 1][0], sections[index - 1][1]
            z2, y2 = sections[index + 1][0], sections[index + 1][1]
            dz, dy = z2 - z1, y2 - y1
        length = max((dz * dz + dy * dy) ** 0.5, 1e-9)
        normals.append((dz / length, -dy / length))

    for (z, center_y, rx, ry), (normal_y, normal_z) in zip(sections, normals):
        for i in range(sides):
            angle = 2.0 * math.pi * i / sides
            radial_x = math.cos(angle) * rx
            radial_normal = math.sin(angle) * ry
            verts.append((
                radial_x,
                center_y + normal_y * radial_normal,
                z + normal_z * radial_normal,
            ))

    for ring in range(len(sections) - 1):
        for i in range(sides):
            j = (i + 1) % sides
            a = ring * sides + i
            b = ring * sides + j
            c = (ring + 1) * sides + j
            d = (ring + 1) * sides + i
            faces.append((a, b, c, d))

    wrist_center = len(verts)
    verts.append((0.0, sections[0][1], sections[0][0]))
    terminal_index = len(verts)
    verts.append(terminal)
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((wrist_center, j, i))
        a = (len(sections) - 1) * sides + i
        b = (len(sections) - 1) * sides + j
        faces.append((terminal_index, a, b))

    mesh = bpy.data.meshes.new(name + ".Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    glove = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(glove)
    glove.data.materials.append(material)
    for poly in glove.data.polygons:
        poly.use_smooth = True

    subdivision = glove.modifiers.new("GlovePaddedSubdivision", "SUBSURF")
    subdivision.subdivision_type = "CATMULL_CLARK"
    subdivision.levels = 1
    subdivision.render_levels = 2
    max_forward_z = max(section[0] for section in sections)
    max_knuckle_width = max(section[2] for section in sections[:7])
    striking_half_width = min(
        section[2] for section in sections if section[0] >= max_forward_z - 0.015
    )
    knuckle_section = sections[4]
    folded_section = sections[7]
    glove["shape_profile"] = "curved_padded_fist_cage_v18"
    glove["topology_family"] = "curved_sweep_two_mass"
    glove["distal_terminal"] = "under_palm_terminal_behind_striking_face"
    glove["max_forward_z_m"] = max_forward_z
    glove["striking_face_half_width_m"] = striking_half_width
    glove["knuckle_half_width_m"] = max_knuckle_width
    glove["wrist_half_width_m"] = sections[0][2]
    glove["forward_projection_m"] = max_forward_z - sections[2][0]
    glove["knuckle_to_fold_drop_m"] = abs(folded_section[1] - knuckle_section[1])
    glove["front_roll_backtrack_m"] = max_forward_z - folded_section[0]
    glove["undercut_return_m"] = max_forward_z - sections[9][0]
    glove["terminal_recess_from_face_m"] = max_forward_z - terminal[2]
    glove["terminal_under_palm_y_m"] = terminal[1]
    glove["terminal_ring_half_width_m"] = sections[-1][2]
    return glove


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


def bind_shorts_half_to_rig(obj, rig, side, row_count=None, samples=32):
    """Blend the trunk leg from pelvis to thigh instead of rigid pelvis parenting.

    This keeps the upper cloth anchored under the waistband while allowing the
    lower opening to follow the thigh during stance, slips and rear-foot drive.
    """
    pelvis_group = obj.vertex_groups.new(name="pelvis")
    thigh_name = "thigh_l" if side > 0 else "thigh_r"
    thigh_group = obj.vertex_groups.new(name=thigh_name)
    # Infer the generated ring count so later silhouette edits cannot silently
    # leave the final cloth row unweighted (the Review4 six-ring mesh exposed
    # exactly that failure).  Top remains pelvis-led; the hem follows the thigh.
    if row_count is None:
        row_count = max(1, (len(obj.data.vertices) - 1) // samples)
    for row in range(row_count):
        ids = list(range(row * samples, (row + 1) * samples))
        t = row / max(1, row_count - 1)
        tw = 0.10 + 0.78 * (t ** 1.10)
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


def thicken_rendered_forearms(body, rig, factor=1.56, upperarm_factor=1.00):
    """Shape the loaded body toward the approved athletic boxer silhouette.

    TARGET_WEIGHTS above are used to reconstruct joint locations for the rig;
    they do not mutate the already-authored body mesh loaded from the static
    Blender source.  The prior Review6 therefore changed numbers without
    visibly changing arm thickness.  This deformation uses the final skinning
    groups and expands vertices radially around each lower-arm bone axis, with
    the bone weight as a smooth falloff so elbow/wrist transitions stay clean.
    """
    for bone_name in ("lowerarm_l", "lowerarm_r"):
        vg = body.vertex_groups.get(bone_name)
        bone = rig.data.bones.get(bone_name)
        if vg is None or bone is None:
            raise RuntimeError(f"Missing forearm skinning data: {bone_name}")

        head = Vector(bone.head_local)
        tail = Vector(bone.tail_local)
        axis = tail - head
        axis_len2 = axis.length_squared
        if axis_len2 <= 1e-12:
            raise RuntimeError(f"Degenerate forearm bone: {bone_name}")

        for v in body.data.vertices:
            try:
                w = vg.weight(v.index)
            except RuntimeError:
                continue
            if w <= 0.02:
                continue
            p = Vector(v.co)
            t = max(0.0, min(1.0, (p - head).dot(axis) / axis_len2))
            on_axis = head + axis * t
            radial = p - on_axis
            wrist_falloff = 0.62 + 0.38 * math.sin(math.pi * t)
            gain = 1.0 + (factor - 1.0) * w * wrist_falloff
            v.co = on_axis + radial * gain

    for bone_name in ("upperarm_l", "upperarm_r"):
        vg = body.vertex_groups.get(bone_name)
        bone = rig.data.bones.get(bone_name)
        if vg is None or bone is None:
            continue
        head = Vector(bone.head_local)
        tail = Vector(bone.tail_local)
        axis = tail - head
        axis_len2 = axis.length_squared
        for v in body.data.vertices:
            try:
                w = vg.weight(v.index)
            except RuntimeError:
                continue
            if w <= 0.05:
                continue
            p = Vector(v.co)
            t = max(0.0, min(1.0, (p - head).dot(axis) / axis_len2))
            on_axis = head + axis * t
            radial = p - on_axis
            mid_belly = 0.72 + 0.28 * math.sin(math.pi * t)
            gain = 1.0 + (upperarm_factor - 1.0) * w * mid_belly
            v.co = on_axis + radial * gain

    # Expand only the rib-cage region.  Arms are already shaped by the bone
    # groups above; keeping this x-limited preserves the compact waist.
    for v in body.data.vertices:
        p = Vector(v.co)
        if 1.08 <= p.z <= 1.48 and abs(p.x) <= 0.34:
            z_t = (p.z - 1.08) / 0.40
            bell = math.sin(math.pi * max(0.0, min(1.0, z_t)))
            p.x *= 1.0 + 0.12 * bell
            p.y *= 1.0 + 0.09 * bell
            v.co = p
    body.data.update()


def shape_boxer_legs_and_knees(body, rig):
    """Add anatomical knee landmarks without changing overall leg proportion.

    Review20 deliberately keeps the already-approved thigh/calf mass.  It only
    sculpts a compact band around each knee joint so the leg reads as
    thigh -> femoral condyles/patella -> patellar tendon/tibial tuberosity ->
    calf, instead of a smooth tube through the joint.
    """
    profile = "anatomical_boxer_knee_v20"
    if body.get("knee_shape_profile") == profile:
        return

    changed = 0
    max_displacement = 0.0
    for side_name in ("l", "r"):
        thigh = rig.data.bones.get(f"thigh_{side_name}")
        if thigh is None:
            raise RuntimeError(f"Missing thigh bone for knee shaping: {side_name}")
        knee = Vector(thigh.tail_local)

        for vertex in body.data.vertices:
            original = Vector(vertex.co)
            dx = original.x - knee.x
            dz = original.z - knee.z
            if abs(dx) > 0.115 or abs(dz) > 0.150:
                continue

            # Elliptical locality gate; this prevents the correction from
            # leaking into the thigh belly or calf belly.
            locality = max(0.0, 1.0 - (dx / 0.115) ** 2 - (dz / 0.150) ** 2)
            if locality <= 0.0:
                continue
            locality = locality * locality

            p = original.copy()
            y_rel = p.y - knee.y
            front_gate = max(0.0, min(1.0, (-y_rel - 0.005) / 0.060))
            back_gate = max(0.0, min(1.0, (y_rel - 0.005) / 0.055))

            def gauss(value, center, sigma):
                return math.exp(-0.5 * ((value - center) / sigma) ** 2)

            # 1) Patella: a compact anterior oval just above the joint line.
            patella = (
                gauss(dz, 0.014, 0.034)
                * gauss(dx, 0.0, 0.050)
                * front_gate
                * locality
            )
            p.y -= 0.0135 * patella

            # 2) Medial/lateral femoral condyles: preserve a bony double-knot
            # around the joint rather than pinching the leg into an hourglass.
            condyle_z = gauss(dz, 0.010, 0.032) * locality
            condyle_side = gauss(abs(dx), 0.048, 0.022)
            if abs(dx) > 1e-6:
                p.x += math.copysign(0.0050 * condyle_z * condyle_side, dx)

            # 3) Patellar tendon recess followed by the tibial tuberosity.  The
            # small in/out sequence is what makes the front of the knee read as
            # folded anatomy instead of one continuous round bulge.
            tendon = (
                gauss(dz, -0.026, 0.022)
                * gauss(dx, 0.0, 0.046)
                * front_gate
                * locality
            )
            tibial_bump = (
                gauss(dz, -0.064, 0.026)
                * gauss(dx, 0.0, 0.043)
                * front_gate
                * locality
            )
            p.y += 0.0045 * tendon
            p.y -= 0.0060 * tibial_bump

            # 4) Popliteal hollow: pull the rear center of the joint inward.
            popliteal = (
                gauss(dz, 0.000, 0.038)
                * gauss(dx, 0.0, 0.060)
                * back_gate
                * locality
            )
            p.y -= 0.0070 * popliteal

            displacement = (p - original).length
            if displacement > 1e-7:
                vertex.co = p
                changed += 1
                max_displacement = max(max_displacement, displacement)

    body["knee_shape_profile"] = profile
    body["knee_changed_vertices"] = changed
    body["knee_max_displacement_m"] = max_displacement
    body.data.update()


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


GLOVE_MASS_SCALE = 0.76


def add_boxing_glove(name, center, side):
    red = make_glove_leather_material()
    seam = make_material("GloveSeam", (0.018, 0.0015, 0.0025, 1), metallic=0.0, roughness=0.70)
    white = make_material("WrapWhite", (0.72, 0.68, 0.61, 1), roughness=0.78)
    gold = make_material("GloveGold", (0.42, 0.22, 0.035, 1), metallic=0.58, roughness=0.30)
    # Continuous lofted shell: broad knuckles, tapered wrist, and a distal
    # centerline that folds downward like a closed boxing fist.
    glove = add_lofted_glove_shell(name, red)
    glove.location = center
    # Review19 proportion correction: preserve the approved Review18 cage
    # topology/silhouette, but reduce the entire padded fist mass relative to
    # Ramirez's head and forearm.  Neck/cuff remain unscaled so wrist retention
    # stays believable and no body/arm/pose dimensions are touched.
    glove.scale = (GLOVE_MASS_SCALE,) * 3
    glove["mass_scale"] = GLOVE_MASS_SCALE

    # Padded thumb and bridge: separate forms create the unmistakable boxing
    # glove thumb pocket visible in the approved reference.
    bpy.ops.mesh.primitive_uv_sphere_add(segments=48, ring_count=24, radius=1.0, location=(0, 0, 0))
    thumb = bpy.context.object
    thumb.name = name + ".Thumb"
    thumb.scale = (0.056, 0.047, 0.082)
    thumb.rotation_euler = (math.radians(18.0), math.radians(side * 12.0), math.radians(side * 5.0))
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    thumb.data.materials.append(red)
    for p in thumb.data.polygons:
        p.use_smooth = True
    thumb.parent = glove
    thumb.location = (side * 0.072, -0.038, -0.012)

    bpy.ops.mesh.primitive_uv_sphere_add(segments=40, ring_count=20, radius=1.0, location=(0, 0, 0))
    bridge = bpy.context.object
    bridge.name = name + ".ThumbBridge"
    bridge.scale = (0.042, 0.042, 0.060)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bridge.data.materials.append(red)
    for p in bridge.data.polygons:
        p.use_smooth = True
    bridge.parent = glove
    bridge.location = (side * 0.048, -0.020, 0.012)

    # Leather seam/crease cues keep the glove from reading as a toy or a hard
    # plastic blob.  They are deliberately subtle and follow the thumb pocket.
    creases = [
        add_glove_crease(
            glove,
            name + ".ThumbCrease",
            [
                (side * 0.025, -0.121, 0.006),
                (side * 0.047, -0.125, -0.020),
                (side * 0.064, -0.119, -0.047),
                (side * 0.066, -0.110, -0.068),
            ],
            seam,
            thickness=0.0024,
        ),
        add_glove_crease(
            glove,
            name + ".PalmCrease",
            [
                (-0.052, -0.119, -0.075),
                (0.000, -0.126, -0.086),
                (0.052, -0.119, -0.075),
            ],
            seam,
            thickness=0.0018,
        ),
    ]
    # Retention geometry is intentionally split into three readable parts:
    # glove body -> cinched red neck -> tight white wrist cuff.  The old build
    # jumped directly from the large glove shell to a same-width cylinder,
    # which visually read as a glove that could slide off the wrist.
    bpy.ops.mesh.primitive_cone_add(
        vertices=48,
        radius1=0.052,
        radius2=0.061,
        depth=0.066,
        location=center,
    )
    neck = bpy.context.object
    neck.name = name + ".Neck"
    neck.data.materials.append(red)
    neck_bevel = neck.modifiers.new("GloveNeckSoftness", "BEVEL")
    neck_bevel.width = 0.004
    neck_bevel.segments = 3

    bpy.ops.mesh.primitive_cone_add(
        vertices=48,
        radius1=0.059,
        radius2=0.057,
        depth=0.105,
        location=center,
    )
    cuff = bpy.context.object
    cuff.name = name + ".Cuff"
    cuff.data.materials.append(white)
    cuff_bevel = cuff.modifiers.new("GloveCuffSoftness", "BEVEL")
    cuff_bevel.width = 0.005
    cuff_bevel.segments = 3
    # Keep the small gold mark physically attached to the glove.  The previous
    # world-space sphere did not rotate with the fist and visibly floated away
    # in side/punch views.
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=32,
        radius=0.021,
        depth=0.004,
        location=center + Vector((0, -0.096 * GLOVE_MASS_SCALE, 0.024 * GLOVE_MASS_SCALE)),
        rotation=(math.radians(90), 0, 0),
    )
    badge = bpy.context.object
    badge.name = name + ".Badge"
    badge.data.materials.append(gold)
    badge.scale = (GLOVE_MASS_SCALE,) * 3
    badge.parent = glove
    badge.matrix_parent_inverse = glove.matrix_world.inverted()
    return {
        "side": side,
        "main": glove,
        "thumb": thumb,
        "thumb_bridge": bridge,
        "creases": creases,
        "neck": neck,
        "cuff": cuff,
        "badge": badge,
    }


def update_glove_pose(rig, glove, bone_name):
    forearm = rig.pose.bones[bone_name]
    elbow = rig.matrix_world @ forearm.head
    wrist = rig.matrix_world @ forearm.tail
    direction = (wrist - elbow).normalized()
    center = wrist + direction * 0.086
    glove_rot = direction.to_track_quat("Z", "Y")
    glove["main"].location = center
    glove["main"].rotation_euler = glove_rot.to_euler()
    glove["neck"].location = wrist + direction * 0.054
    glove["neck"].rotation_euler = glove_rot.to_euler()
    glove["cuff"].location = wrist + direction * 0.016
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
    z0, z1 = 0.705, 0.972
    # A real pair of boxing trunks has a continuous seat/front body under the
    # waistband. Keep this panel wide enough to overlap both leg shells so no
    # body skin can become visible between them during stance or deformation.
    xb, xt = 0.128, 0.132
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


def add_trunks_inner_body(material):
    """Continuous upper trunks body joining both leg shells around the pelvis."""
    name = "RamirezShorts.InnerBody"
    samples = 64
    rings = [
        # Visible continuous seat/front.  This sits outside the skin and
        # overlaps the two loose leg shells below, eliminating the torn flap.
        (0.976, 0.205, 0.158),
        (0.930, 0.209, 0.162),
        (0.884, 0.214, 0.166),
        (0.838, 0.218, 0.169),
        (0.800, 0.218, 0.168),
    ]
    verts, faces = [], []
    for z, rx, ry in rings:
        for i in range(samples):
            a = 2.0 * math.pi * i / samples
            verts.append((math.cos(a) * rx, math.sin(a) * ry, z))
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
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    bevel = obj.modifiers.new("InnerBodySoftness", "BEVEL")
    bevel.width = 0.0015
    bevel.segments = 2
    solid = obj.modifiers.new("InnerBodyThickness", "SOLIDIFY")
    solid.thickness = 0.0015
    solid.offset = 0.0
    return obj


def add_trunks_inner_leg(name, side, material):
    """Opaque fitted liner for one upper thigh under the loose outer shell.

    The central inner body closes the seat/crotch, while these two separated
    liners follow the thighs and prevent skin from showing through the inner
    edge of either loose leg shell during split stance.  They remain narrower
    than the visible outer trunks, so they only act as coverage insurance.
    """
    samples = 48
    center_x = side * 0.104
    rings = [
        (0.930, 0.122, 0.136),
        (0.850, 0.124, 0.138),
        (0.770, 0.123, 0.136),
        # Extend slightly below the visible outer hem.  This layer is hidden
        # in normal views, but guarantees that a stance/deformation gap can
        # reveal fabric only, never body skin.
        (0.680, 0.118, 0.130),
    ]
    verts, faces = [], []
    for z, rx, ry in rings:
        for i in range(samples):
            a = 2.0 * math.pi * i / samples
            verts.append((center_x + math.cos(a) * rx, math.sin(a) * ry, z))
    for r in range(len(rings) - 1):
        a0 = r * samples
        a1 = (r + 1) * samples
        for i in range(samples):
            j = (i + 1) % samples
            face = (a0 + i, a0 + j, a1 + j, a1 + i)
            if side < 0:
                face = tuple(reversed(face))
            faces.append(face)
    top_center = len(verts)
    verts.append((center_x, 0.0, rings[0][0]))
    for i in range(samples):
        j = (i + 1) % samples
        face = (top_center, j, i)
        if side < 0:
            face = tuple(reversed(face))
        faces.append(face)
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for poly in mesh.polygons:
        poly.use_smooth = True
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    solid = obj.modifiers.new("InnerLegThickness", "SOLIDIFY")
    solid.thickness = 0.0015
    solid.offset = 0.0
    return obj


def add_boxing_shorts_half(name, side, material, accent_material):
    # Review5: boxer trunks must read as hanging cloth, not a rigid tube.
    # The upper rings stay close to the hips while the lower rings flare and
    # drape away from the thigh.  Left/right openings remain physically split.
    rows = [
        # z, center_x, radius_x, radius_y
        # Keep the waistband/hip fit close to the body and use only a modest
        # flare toward the hem. Previous values made each leg read like a
        # separate bell/skirt around the thigh.
        (0.900, 0.092, 0.122, 0.145),
        (0.864, 0.094, 0.124, 0.147),
        (0.824, 0.097, 0.127, 0.150),
        (0.784, 0.101, 0.130, 0.153),
        (0.744, 0.106, 0.133, 0.156),
        (0.710, 0.111, 0.136, 0.158),
        (0.684, 0.114, 0.138, 0.160),
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
                0.028 * math.sin(3.0 * a + side * 0.7)
                + 0.014 * math.sin(5.0 * a - side * 0.35)
            )
            x = side * (center_x + math.cos(a) * rx * wrinkle)
            y = math.sin(a) * ry * (1.0 + 0.018 * math.cos(2.0 * a))
            z_local = z
            back_drape = max(0.0, math.sin(a))
            front_drape = max(0.0, -math.sin(a))
            if row_i == len(rows) - 1:
                # No side split/notch.  The user-visible reference reads as a
                # continuous boxing hem; large vertical variation was being
                # interpreted as torn fabric.  Keep only a shallow cloth drape.
                z_local -= 0.008 * back_drape
                z_local -= 0.004 * front_drape
            elif row_i == len(rows) - 2:
                z_local -= 0.003 * back_drape
                z_local -= 0.0015 * front_drape
            verts.append((x, y, z_local))

    faces = []
    face_materials = []
    for r in range(len(rows) - 1):
        base = r * samples
        next_base = (r + 1) * samples
        for i in range(samples):
            j = (i + 1) % samples
            face = (base + i, base + j, next_base + j, next_base + i)
            # The right shell is geometrically mirrored by `side=-1`; reverse
            # winding as well so its normals remain outward. Keeping the left
            # winding on mirrored coordinates caused large front-facing areas
            # of the right leg to disappear and look like torn cloth.
            if side < 0:
                face = tuple(reversed(face))
            faces.append(face)
            # Reference trunks carry a readable gold side stripe and a gold
            # hem band. Keep both on the cloth surface instead of separate
            # block-like geometry.
            side_stripe = i in (0, 1, samples - 1, samples - 2)
            hem_band = r == len(rows) - 2
            face_materials.append(1 if side_stripe or hem_band else 0)

    # Close the upper ring under the waistband.  The lower ring intentionally
    # remains open to preserve a true boxing-trunk leg opening.
    top_center = len(verts)
    verts.append((side * rows[0][1], 0.0, rows[0][0]))
    for i in range(samples):
        j = (i + 1) % samples
        face = (top_center, j, i)
        if side < 0:
            face = tuple(reversed(face))
        faces.append(face)
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for poly in mesh.polygons:
        poly.use_smooth = True
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    obj.data.materials.append(accent_material)
    for poly, material_index in zip(obj.data.polygons[: len(face_materials)], face_materials):
        poly.material_index = material_index
    bevel = obj.modifiers.new("ShortsClothSoftness", "BEVEL")
    bevel.width = 0.0012
    bevel.segments = 2
    sub = obj.modifiers.new("ShortsClothSubdivision", "SUBSURF")
    sub.levels = 2
    sub.render_levels = 2
    solid = obj.modifiers.new("ShortsClothThickness", "SOLIDIFY")
    solid.thickness = 0.00125
    solid.offset = 0.0
    return obj


def add_shorts_side_panel(name, side, material):
    # Curved side tape following the outer cloth silhouette rather than a flat
    # rectangular plate.
    curve_data = bpy.data.curves.new(name + "Curve", type="CURVE")
    curve_data.dimensions = "3D"
    curve_data.bevel_depth = 0.006
    curve_data.bevel_resolution = 3
    spline = curve_data.splines.new("BEZIER")
    spline.bezier_points.add(3)
    points = [
        (side * 0.216, -0.010, 0.954),
        (side * 0.232, -0.008, 0.872),
        (side * 0.250, -0.004, 0.790),
        (side * 0.275, 0.002, 0.726),
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
        (0.948, 0.201, 0.140),
        (0.958, 0.204, 0.142),
        (0.984, 0.204, 0.143),
        (0.999, 0.200, 0.140),
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
    bevel.width = 0.0018
    bevel.segments = 2
    solid = belt.modifiers.new("WaistbandThickness", "SOLIDIFY")
    solid.thickness = 0.0018
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
    burgundy = make_material("RamirezTrunks", (0.070, 0.0035, 0.0075, 1), metallic=0.0, roughness=0.38)
    gold = make_material("RamirezGold", (0.40, 0.20, 0.030, 1), metallic=0.58, roughness=0.30)
    if burgundy.use_nodes:
        nodes = burgundy.node_tree.nodes
        links = burgundy.node_tree.links
        bsdf = nodes.get("Principled BSDF")
        if "Specular IOR Level" in bsdf.inputs:
            bsdf.inputs["Specular IOR Level"].default_value = 0.22
        noise = nodes.get("TrunksClothNoise") or nodes.new("ShaderNodeTexNoise")
        noise.name = "TrunksClothNoise"
        noise.inputs["Scale"].default_value = 85.0
        noise.inputs["Detail"].default_value = 2.5
        noise.inputs["Roughness"].default_value = 0.66
        bump = nodes.get("TrunksClothBump") or nodes.new("ShaderNodeBump")
        bump.name = "TrunksClothBump"
        bump.inputs["Strength"].default_value = 0.045
        bump.inputs["Distance"].default_value = 0.0008
        links.new(noise.outputs["Fac"], bump.inputs["Height"])
        links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])
    left = add_boxing_shorts_half("RamirezShorts.L", 1, burgundy, gold)
    right = add_boxing_shorts_half("RamirezShorts.R", -1, burgundy, gold)
    bind_shorts_half_to_rig(left, rig, 1)
    bind_shorts_half_to_rig(right, rig, -1)
    # One continuous upper garment replaces the old flat front overlap panel.
    # It remains attached to the pelvis while the two leg shells deform below.
    inner_body = add_trunks_inner_body(burgundy)
    parent_to_bone(inner_body, rig, "pelvis")
    liner_l = add_trunks_inner_leg("RamirezShorts.InnerLeg.L", 1, burgundy)
    liner_r = add_trunks_inner_leg("RamirezShorts.InnerLeg.R", -1, burgundy)
    bind_shorts_half_to_rig(liner_l, rig, 1, row_count=4, samples=48)
    bind_shorts_half_to_rig(liner_r, rig, -1, row_count=4, samples=48)
    belt = add_boxing_waistband(gold)
    plaque_black = make_material("WaistbandPlaque", (0.010, 0.008, 0.007, 1), roughness=0.48)
    bpy.ops.mesh.primitive_cube_add(location=(0, -0.160, 0.965))
    plaque = bpy.context.object
    plaque.name = "RamirezWaistbandPlaque"
    plaque.scale = (0.070, 0.004, 0.018)
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
    label.data.size = 0.040
    label.data.extrude = 0.0008
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

    return [left, right, inner_body, liner_l, liner_r, belt, plaque, label, cap]


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


def pose_arm_direct(rig, side, elbow_hint, hand_hint, shoulder_offset=(0.0, 0.0, 0.0)):
    suffix = side.lower()
    clavicle_name = f"clavicle_{suffix}"
    upper_name = f"upperarm_{suffix}"
    lower_name = f"lowerarm_{suffix}"
    clavicle = rig.pose.bones[clavicle_name]
    clavicle_head = Vector(clavicle.head)
    shoulder_target = Vector(clavicle.tail) + Vector(shoulder_offset)
    shoulder = set_pose_bone_direction(rig, clavicle_name, clavicle_head, shoulder_target)
    upper = rig.pose.bones[upper_name]
    elbow = set_pose_bone_direction(rig, upper_name, shoulder, elbow_hint)
    wrist = set_pose_bone_direction(rig, lower_name, elbow, hand_hint)
    return elbow, wrist


def pose_leg_direct(rig, side, knee_hint, ankle_hint, foot_yaw_degrees=0.0, foot_pitch_degrees=0.0):
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
    pitch = math.radians(foot_pitch_degrees)
    cp, sp = math.cos(pitch), math.sin(pitch)
    foot_vec = Vector((
        foot_vec.x,
        foot_vec.y * cp - foot_vec.z * sp,
        foot_vec.y * sp + foot_vec.z * cp,
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
            "L": ((0.355, -0.175, 1.185), (0.240, -0.305, 1.255)),
            "R": ((-0.395, -0.165, 1.190), (-0.305, -0.290, 1.250)),
        },
        "guard": {
            "L": ((0.360, -0.185, 1.185), (0.245, -0.315, 1.255)),
            "R": ((-0.400, -0.175, 1.190), (-0.310, -0.295, 1.250)),
        },
        "jab": {
            "L": ((0.245, -0.420, 1.365), (0.125, -0.700, 1.415)),
            "R": ((-0.338, -0.175, 1.280), (-0.160, -0.295, 1.460)),
        },
        "cross": {
            "L": ((0.338, -0.175, 1.280), (0.165, -0.298, 1.455)),
            "R": ((-0.245, -0.420, 1.360), (-0.125, -0.700, 1.410)),
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
            "L": ((0.345, -0.190, 1.265), (0.175, -0.310, 1.450)),
            "R": ((-0.335, -0.175, 1.275), (-0.160, -0.292, 1.455)),
        },
        "slip_right": {
            "L": ((0.335, -0.175, 1.275), (0.160, -0.292, 1.455)),
            "R": ((-0.345, -0.190, 1.265), (-0.175, -0.310, 1.450)),
        },
        "slip_counter": {
            "L": ((0.338, -0.175, 1.278), (0.165, -0.298, 1.455)),
            "R": ((-0.248, -0.405, 1.355), (-0.125, -0.685, 1.405)),
        },
        "recover_guard": {
            "L": ((0.360, -0.185, 1.185), (0.245, -0.313, 1.255)),
            "R": ((-0.400, -0.175, 1.190), (-0.310, -0.293, 1.250)),
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
        translate_pose_bone_world(rig, "pelvis", (0.0, -0.004, -0.075))
        # Positive X pitches the upper body toward the opponent/camera.
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(9)
        rig.pose.bones["neck_01"].rotation_euler.x = math.radians(5)

    if name in {"static_stance", "guard", "recover_guard"}:
        # MPFB spine local-Y is almost aligned with world-Z, so local-Y is the
        # correct twist/yaw axis for orthodox blading.
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-7)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-10)
    elif name == "jab":
        # Lead shoulder reaches without over-rotating the hips.
        translate_pose_bone_world(rig, "pelvis", (0.008, -0.012, -0.004))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-8)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-15)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(10)
    elif name == "cross":
        # Rear hip and shoulder drive through together; the rear heel is lifted
        # and foot pivot is supplied by the leg target below.
        translate_pose_bone_world(rig, "pelvis", (0.016, -0.024, -0.008))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(14)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(23)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(10)
    elif name == "slip_left":
        translate_pose_bone_world(rig, "pelvis", (0.046, -0.010, -0.016))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-7)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-11)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-9)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(5)
    elif name == "slip_right":
        translate_pose_bone_world(rig, "pelvis", (-0.046, -0.010, -0.016))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(-7)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(-10)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(9)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(-5)
    elif name == "slip_counter":
        # Counter keeps the head off the center line while the rear hip/shoulder
        # rotate into the straight.  This is intentionally distinct from a
        # static slip followed by an unrelated arm extension.
        translate_pose_bone_world(rig, "pelvis", (0.020, -0.024, -0.018))
        rig.pose.bones["pelvis"].rotation_euler.y = math.radians(14)
        rig.pose.bones["spine_03"].rotation_euler.y = math.radians(22)
        rig.pose.bones["spine_03"].rotation_euler.z = math.radians(-5)
        rig.pose.bones["spine_03"].rotation_euler.x = math.radians(11)
        rig.pose.bones["neck_01"].rotation_euler.z = math.radians(3)
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

    shoulder_offsets = {
        "static_stance": {"L": (0.012, -0.025, 0.004), "R": (-0.012, -0.025, 0.004)},
        "guard": {"L": (0.015, -0.035, 0.006), "R": (-0.015, -0.035, 0.006)},
        "jab": {"L": (0.020, -0.075, 0.012), "R": (-0.012, -0.030, 0.004)},
        "cross": {"L": (0.012, -0.030, 0.004), "R": (-0.020, -0.075, 0.012)},
        "slip_left": {"L": (0.015, -0.035, 0.004), "R": (-0.012, -0.030, 0.004)},
        "slip_right": {"L": (0.012, -0.030, 0.004), "R": (-0.015, -0.035, 0.004)},
        "slip_counter": {"L": (0.012, -0.030, 0.004), "R": (-0.020, -0.070, 0.010)},
        "recover_guard": {"L": (0.015, -0.035, 0.006), "R": (-0.015, -0.035, 0.006)},
    }
    for side in ("L", "R"):
        elbow_hint, hand_hint = poses[name][side]
        shoulder_offset = shoulder_offsets.get(name, {}).get(side, (0.0, 0.0, 0.0))
        pose_arm_direct(rig, side, elbow_hint, hand_hint, shoulder_offset=shoulder_offset)

    # Orthodox base: lead (L) foot forward, rear (R) foot back, both knees bent.
    # Each action keeps that split stance and changes loading/pivot rather than
    # returning to a square mannequin base.
    leg_targets = {
        "default": {
            "L": ((0.158, -0.110, 0.445), (0.198, -0.190, 0.074), 8.0),
            "R": ((-0.152, 0.080, 0.455), (-0.198, 0.160, 0.078), -24.0),
        },
        "static_stance": {
            "L": ((0.160, -0.260, 0.390), (0.204, -0.224, 0.072), 8.0),
            "R": ((-0.202, 0.216, 0.420), (-0.206, 0.188, 0.076), -20.0),
        },
        "guard": {
            "L": ((0.162, -0.270, 0.380), (0.206, -0.228, 0.072), 8.0),
            "R": ((-0.204, 0.220, 0.420), (-0.208, 0.190, 0.076), -20.0),
        },
        "jab": {
            "L": ((0.166, -0.276, 0.378), (0.214, -0.240, 0.072), 5.0),
            "R": ((-0.158, 0.220, 0.400), (-0.206, 0.188, 0.076), -24.0),
        },
        "cross": {
            "L": ((0.166, -0.260, 0.392), (0.210, -0.230, 0.072), 8.0),
            "R": ((-0.145, 0.155, 0.350), (-0.190, 0.165, 0.235), -42.0, 22.0),
        },
        "slip_left": {
            "L": ((0.145, -0.315, 0.350), (0.198, -0.228, 0.072), 8.0),
            "R": ((-0.180, 0.205, 0.425), (-0.214, 0.190, 0.076), -24.0),
        },
        "slip_right": {
            "L": ((0.180, -0.245, 0.425), (0.214, -0.226, 0.072), 8.0),
            "R": ((-0.145, 0.275, 0.350), (-0.200, 0.188, 0.078), -26.0),
        },
        "slip_counter": {
            "L": ((0.148, -0.282, 0.370), (0.202, -0.228, 0.072), 8.0),
            "R": ((-0.145, 0.190, 0.335), (-0.190, 0.165, 0.225), -42.0, 20.0),
        },
        "recover_guard": {
            "L": ((0.162, -0.268, 0.382), (0.206, -0.226, 0.072), 8.0),
            "R": ((-0.204, 0.218, 0.422), (-0.208, 0.188, 0.076), -20.0),
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
        target = selected_legs[side]
        knee_hint, ankle_hint, foot_yaw = target[:3]
        foot_pitch = target[3] if len(target) > 3 else 0.0
        pose_leg_direct(
            rig,
            side,
            knee_hint,
            ankle_hint,
            foot_yaw_degrees=foot_yaw,
            foot_pitch_degrees=foot_pitch,
        )

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
    review_root = repo / "art" / "blender" / "ramirez" / "renders" / "review14-glove-reference-correction"
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

    # Reference-style portrait lighting: softer key, much weaker fill and a
    # restrained rim. This keeps muscle form while avoiding wax/plastic sheen.
    if bpy.data.objects.get("Key"):
        bpy.data.objects["Key"].data.energy = 620
        if hasattr(bpy.data.objects["Key"].data, "size"):
            bpy.data.objects["Key"].data.size = 4.0
    if bpy.data.objects.get("Fill"):
        bpy.data.objects["Fill"].data.energy = 70
    if bpy.data.objects.get("Rim"):
        bpy.data.objects["Rim"].data.energy = 360

    data_root = mpfb / "src" / "mpfb" / "data"
    joints, converted_verts = joint_centers(data_root / "3dobjs" / "base.obj", data_root / "targets")
    rig = build_mpfb_game_engine_rig(
        joints,
        converted_verts,
        data_root / "rigs" / "standard" / "rig.game_engine.json",
    )
    apply_mpfb_weights(body, rig, data_root / "rigs" / "standard" / "weights.game_engine.json")
    thicken_rendered_forearms(body, rig, factor=1.56, upperarm_factor=1.00)
    shape_boxer_legs_and_knees(body, rig)
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
    metrics_path = repo / "art" / "blender" / "ramirez" / "rig-metrics.json"
    metrics_path.write_text(json.dumps(report, indent=2), encoding="utf-8", newline="\n")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
