import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.armatures, bpy.data.materials):
        pass


def material(name, color, metallic=0.0, roughness=0.5):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1.0)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
    return m


def image_material(name, image_path, color=(1.0, 1.0, 1.0), roughness=0.45):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1.0)
    m.use_nodes = True
    nodes = m.node_tree.nodes
    links = m.node_tree.links
    nodes.clear()
    out = nodes.new("ShaderNodeOutputMaterial")
    bsdf = nodes.new("ShaderNodeBsdfPrincipled")
    bsdf.inputs["Roughness"].default_value = roughness
    tex = nodes.new("ShaderNodeTexImage")
    tex.image = bpy.data.images.load(str(image_path), check_existing=True)
    tex.interpolation = "Linear"
    links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    return m


def apply_transform(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.select_set(False)


def smooth(obj):
    if obj.type == "MESH":
        for p in obj.data.polygons:
            p.use_smooth = True


def bind_rigid(obj, armature, bone_name):
    apply_transform(obj)
    group = obj.vertex_groups.new(name=bone_name)
    group.add(list(range(len(obj.data.vertices))), 1.0, "REPLACE")
    mod = obj.modifiers.new(name="Armature", type="ARMATURE")
    mod.object = armature
    obj.parent = armature


def bind_auto(obj, armature):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.parent_set(type="ARMATURE_AUTO", keep_transform=True)
    bpy.ops.object.select_all(action="DESELECT")


def skin_body(name, armature, left, right, mat):
    # One continuous body surface generated from the same joint topology as the
    # exported rig. This removes the detached-capsule/mannequin read while still
    # leaving gloves, shorts and boots as independent equipment meshes.
    verts = [
        (0.0, 0.0, 0.90),   # 0 pelvis
        (0.0, 0.0, 1.08),   # 1 lower spine
        (0.0, 0.0, 1.28),   # 2 chest
        (0.0, 0.0, 1.42),   # 3 upper chest
        (0.0, 0.0, 1.50),   # 4 neck base
        left["shoulder"], left["elbow"], left["wrist"],
        right["shoulder"], right["elbow"], right["wrist"],
        left["hip"], left["knee"], left["ankle"],
        right["hip"], right["knee"], right["ankle"],
    ]
    edges = [
        (0, 1), (1, 2), (2, 3), (3, 4),
        (3, 5), (5, 6), (6, 7),
        (3, 8), (8, 9), (9, 10),
        (0, 11), (11, 12), (12, 13),
        (0, 14), (14, 15), (15, 16),
    ]
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, edges, [])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    skin = obj.modifiers.new(name="OrganicBody", type="SKIN")
    bpy.context.view_layer.objects.active = obj
    bpy.context.view_layer.update()
    data = mesh.skin_vertices[0].data
    radii = {
        0: (.20, .14), 1: (.19, .13), 2: (.27, .15), 3: (.30, .16), 4: (.085, .075),
        5: (.105, .095), 6: (.075, .070), 7: (.058, .055),
        8: (.105, .095), 9: (.075, .070), 10: (.058, .055),
        11: (.115, .105), 12: (.082, .078), 13: (.058, .055),
        14: (.115, .105), 15: (.082, .078), 16: (.058, .055),
    }
    for idx, radius in radii.items():
        data[idx].radius = radius
    data[0].use_root = True
    bpy.ops.object.modifier_apply(modifier=skin.name)
    sub = obj.modifiers.new(name="BodySubdivision", type="SUBSURF")
    sub.subdivision_type = "CATMULL_CLARK"
    sub.levels = 2
    sub.render_levels = 2
    bpy.ops.object.modifier_apply(modifier=sub.name)
    smooth(obj)
    bind_auto(obj, armature)
    return obj


def build_segmented_body(armature, left, right, skin):
    """Build a stable boxer body from overlapping anatomical meshes.

    The Round2 runtime drives individual rig bones directly. A single auto-weighted
    skin surface twists badly under those large procedural rotations, so the UAT
    asset uses smooth, overlapping rigid-bound sections. The silhouette stays
    continuous while each section follows one authoritative bone without weight
    collapse or shoulder ghosting.
    """
    loft(
        "Abdomen",
        [
            (0.88, .205, .135, 0, 0),
            (0.99, .198, .128, 0, 0),
            (1.10, .205, .132, 0, 0),
            (1.19, .235, .145, 0, 0),
        ],
        skin,
        armature,
        "spine",
        28,
    )
    loft(
        "Torso",
        [
            (1.12, .215, .140, 0, 0),
            (1.24, .265, .152, 0, 0),
            (1.36, .305, .165, 0, 0),
            (1.44, .275, .150, 0, 0),
        ],
        skin,
        armature,
        "chest",
        32,
    )
    capsule("NeckBody", (0, 0, 1.425), (0, 0, 1.555), .073, .068, skin, armature, "neck", .015, 20)
    capsule("Trap.L", (-.17, .01, 1.395), (-.055, .0, 1.49), .082, .058, skin, armature, "chest", .012, 18)
    capsule("Trap.R", (.17, .01, 1.395), (.055, .0, 1.49), .082, .058, skin, armature, "chest", .012, 18)
    for suffix, p in (("L", left), ("R", right)):
        # Overlap deltoid/hip/knee masses with the tapered limb sections so the
        # fighter reads as one body even though deformation is intentionally rigid.
        ellipsoid(f"Deltoid.{suffix}", p["shoulder"], (.115, .105, .125), skin, armature, f"upper_arm.{suffix}", 20, 14)
        capsule(f"UpperArmBody.{suffix}", p["shoulder"], p["elbow"], .100, .078, skin, armature, f"upper_arm.{suffix}", .018, 20)
        capsule(f"ForearmBody.{suffix}", p["elbow"], p["wrist"], .079, .061, skin, armature, f"forearm.{suffix}", .016, 20)
        leg_dir = (Vector(p["knee"]) - Vector(p["hip"])).normalized()
        shin_dir = (Vector(p["ankle"]) - Vector(p["knee"])).normalized()
        capsule(f"ThighBody.{suffix}", Vector(p["hip"]) - leg_dir * .025, Vector(p["knee"]) + leg_dir * .045, .126, .088, skin, armature, f"thigh.{suffix}", .020, 22)
        capsule(f"ShinBody.{suffix}", Vector(p["knee"]) - shin_dir * .045, Vector(p["ankle"]) + shin_dir * .020, .086, .060, skin, armature, f"shin.{suffix}", .016, 20)


def orient_between(obj, a, b):
    d = Vector(b) - Vector(a)
    obj.location = (Vector(a) + Vector(b)) * 0.5
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0.0, 0.0, 1.0)).rotation_difference(d.normalized())
    return d.length


def capsule(name, a, b, r1, r2, mat, armature, bone_name, bevel=0.018, vertices=20):
    length = (Vector(b) - Vector(a)).length
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r1, radius2=r2, depth=length)
    obj = bpy.context.object
    obj.name = name
    orient_between(obj, a, b)
    if bevel > 0:
        bevel_mod = obj.modifiers.new(name="SoftEdges", type="BEVEL")
        bevel_mod.width = bevel
        bevel_mod.segments = 3
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=bevel_mod.name)
    obj.data.materials.append(mat)
    smooth(obj)
    bind_rigid(obj, armature, bone_name)
    return obj


def ellipsoid(name, location, scale, mat, armature=None, bone_name=None, segments=24, rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(mat)
    smooth(obj)
    if armature and bone_name:
        bind_rigid(obj, armature, bone_name)
    else:
        apply_transform(obj)
    return obj


def box(name, location, scale, mat, armature=None, bone_name=None, bevel=0.02):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    if bevel:
        mod = obj.modifiers.new(name="SoftEdges", type="BEVEL")
        mod.width = bevel
        mod.segments = 3
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.data.materials.append(mat)
    smooth(obj)
    if armature and bone_name:
        bind_rigid(obj, armature, bone_name)
    else:
        apply_transform(obj)
    return obj


def loft(name, rings, mat, armature=None, bone_name=None, sides=32):
    verts = []
    uvs = []
    faces = []
    for j, (z, rx, ry, ox, oy) in enumerate(rings):
        for i in range(sides):
            a = 2.0 * math.pi * i / sides
            verts.append((ox + math.cos(a) * rx, oy + math.sin(a) * ry, z))
            uvs.append((i / sides, j / max(1, len(rings) - 1)))
    for j in range(len(rings) - 1):
        for i in range(sides):
            n = (i + 1) % sides
            a = j * sides + i
            b = j * sides + n
            c = (j + 1) * sides + n
            d = (j + 1) * sides + i
            faces.append((a, b, c, d))
    bottom = len(verts)
    verts.append((rings[0][3], rings[0][4], rings[0][0]))
    top = len(verts)
    verts.append((rings[-1][3], rings[-1][4], rings[-1][0]))
    for i in range(sides):
        n = (i + 1) % sides
        faces.append((bottom, n, i))
        a = (len(rings) - 1) * sides + i
        b = (len(rings) - 1) * sides + n
        faces.append((top, a, b))
    mesh = bpy.data.meshes.new(name + "Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    smooth(obj)
    if armature and bone_name:
        bind_rigid(obj, armature, bone_name)
    return obj


def add_bone(edit_bones, name, head, tail, parent=None):
    bone = edit_bones.new(name)
    bone.head = head
    bone.tail = tail
    bone.use_deform = True
    if parent:
        bone.parent = edit_bones[parent]
        bone.use_connect = False
    return bone


def build_armature():
    arm_data = bpy.data.armatures.new("RamirezRig")
    arm = bpy.data.objects.new("RamirezRig", arm_data)
    bpy.context.collection.objects.link(arm)
    bpy.context.view_layer.objects.active = arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    e = arm_data.edit_bones
    add_bone(e, "root", (0, 0, 0), (0, 0, 0.15))
    add_bone(e, "pelvis", (0, 0, 0.86), (0, 0, 1.02), "root")
    add_bone(e, "spine", (0, 0, 1.02), (0, 0, 1.25), "pelvis")
    add_bone(e, "chest", (0, 0, 1.25), (0, 0, 1.43), "spine")
    add_bone(e, "neck", (0, 0, 1.44), (0, 0, 1.55), "chest")
    add_bone(e, "head", (0, 0, 1.55), (0, 0, 1.72), "neck")

    left = {
        "shoulder": (-0.245, 0.00, 1.415),
        "elbow": (-0.355, -0.045, 1.205),
        "wrist": (-0.205, -0.205, 1.345),
        "hip": (-0.17, 0.00, 0.89),
        "knee": (-0.19, -0.02, 0.48),
        "ankle": (-0.18, -0.09, 0.09),
    }
    right = {
        "shoulder": (0.245, 0.00, 1.415),
        "elbow": (0.345, -0.035, 1.215),
        "wrist": (0.215, -0.205, 1.345),
        "hip": (0.17, 0.00, 0.89),
        "knee": (0.20, 0.035, 0.50),
        "ankle": (0.20, 0.075, 0.09),
    }
    for suffix, p in (("L", left), ("R", right)):
        side = -1 if suffix == "L" else 1
        add_bone(e, f"clavicle.{suffix}", (side * 0.08, 0, 1.405), p["shoulder"], "chest")
        add_bone(e, f"upper_arm.{suffix}", p["shoulder"], p["elbow"], f"clavicle.{suffix}")
        add_bone(e, f"forearm.{suffix}", p["elbow"], p["wrist"], f"upper_arm.{suffix}")
        hand_tail = Vector(p["wrist"]) + (Vector(p["wrist"]) - Vector(p["elbow"])).normalized() * 0.11
        add_bone(e, f"hand.{suffix}", p["wrist"], hand_tail, f"forearm.{suffix}")
        add_bone(e, f"thigh.{suffix}", p["hip"], p["knee"], "pelvis")
        add_bone(e, f"shin.{suffix}", p["knee"], p["ankle"], f"thigh.{suffix}")
        foot_tail = Vector(p["ankle"]) + Vector((0, -0.19, -0.01))
        add_bone(e, f"foot.{suffix}", p["ankle"], foot_tail, f"shin.{suffix}")
        add_bone(e, f"toe.{suffix}", foot_tail, foot_tail + Vector((0, -0.09, 0)), f"foot.{suffix}")
    bpy.ops.object.mode_set(mode="OBJECT")
    arm.select_set(False)
    return arm, left, right


def add_text_mesh(text, name, location, size, mat, armature, bone_name):
    bpy.ops.object.text_add(location=location, rotation=(math.radians(90), 0, 0))
    obj = bpy.context.object
    obj.name = name
    obj.data.body = text
    obj.data.align_x = "CENTER"
    obj.data.align_y = "CENTER"
    obj.data.size = size
    obj.data.extrude = 0.004
    obj.data.bevel_depth = 0.001
    obj.data.materials.append(mat)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    bind_rigid(obj, armature, bone_name)
    return obj


def build_ramirez(repo):
    skin = material("Skin", (0.42, 0.20, 0.11), 0.0, 0.45)
    hair = material("Hair", (0.025, 0.018, 0.014), 0.0, 0.62)
    red = material("RamirezRed", (0.36, 0.015, 0.025), 0.05, 0.30)
    red2 = material("RamirezRedLeather", (0.48, 0.025, 0.03), 0.0, 0.22)
    gold = material("BoxerGold", (0.72, 0.43, 0.10), 0.45, 0.26)
    white = material("WrapWhite", (0.78, 0.74, 0.66), 0.0, 0.72)
    head_tex = repo / "unity" / "BoxerP0" / "Assets" / "Resources" / "EV" / "RamirezHead.png"
    head_mat = image_material("RamirezHeadTexture", head_tex, roughness=0.42)
    arm, left, right = build_armature()

    build_segmented_body(arm, left, right, skin)
    head_obj = ellipsoid("Head", (0, -0.010, 1.645), (.116, .105, .145), head_mat, arm, "head", 36, 24)
    head_obj.rotation_euler[2] = math.radians(-90)
    for x in (-.118, .118):
        ellipsoid(f"Ear{x}", (x, 0, 1.64), (.018, .012, .032), skin, arm, "head", 14, 8)
    # Hair/beard detail is already carried by the approved Ramirez head texture.

    loft("Shorts", [(0.79, .225, .155, 0, 0), (0.88, .235, .16, 0, 0), (0.99, .225, .15, 0, 0)], red, arm, "pelvis", 32)
    loft("Waistband", [(0.965, .235, .162, 0, 0), (1.025, .238, .164, 0, 0)], gold, arm, "pelvis", 32)
    add_text_mesh("RAMIREZ", "WaistName", (0, -0.168, 0.995), 0.064, hair, arm, "pelvis")
    for suffix, p in (("L", left), ("R", right)):
        ellipsoid(f"GlovePalm.{suffix}", p["wrist"], (.114, .101, .122), red2, arm, f"hand.{suffix}", 24, 16)
        knuckle = Vector(p["wrist"]) + Vector((0, -.038, .040))
        ellipsoid(f"GloveKnuckle.{suffix}", knuckle, (.120, .108, .078), red2, arm, f"hand.{suffix}", 24, 14)
        # White wrist wrap/cuff just behind the glove.
        cuff_center = Vector(p["wrist"]) + (Vector(p["elbow"]) - Vector(p["wrist"])).normalized() * .105
        cuff_end = Vector(p["wrist"]) + (Vector(p["elbow"]) - Vector(p["wrist"])).normalized() * .19
        capsule(f"Cuff.{suffix}", cuff_center, cuff_end, .082, .072, white, arm, f"hand.{suffix}", .008, 18)
        # Gold knuckle badge.
        front = Vector(p["wrist"]) + Vector((0, -.112, .015))
        ellipsoid(f"GloveBadge.{suffix}", front, (.046, .010, .035), gold, arm, f"hand.{suffix}", 14, 8)

        side = -1 if suffix == "L" else 1
        short_end = Vector(p["hip"]).lerp(Vector(p["knee"]), .32)
        capsule(f"ShortLeg.{suffix}", p["hip"], short_end, .148, .132, red, arm, f"thigh.{suffix}", .012, 24)
        trim_a = Vector(p["hip"]).lerp(Vector(p["knee"]), .29)
        trim_b = Vector(p["hip"]).lerp(Vector(p["knee"]), .34)
        capsule(f"ShortHem.{suffix}", trim_a, trim_b, .137, .134, gold, arm, f"thigh.{suffix}", .006, 20)
        box(f"ShortStripe.{suffix}", (side * .205, -.025, .865), (.018, .015, .10), gold, arm, f"thigh.{suffix}", .006)

        ankle = Vector(p["ankle"])
        ellipsoid(f"Boot.{suffix}", ankle + Vector((0, -.055, .02)), (.095, .16, .07), white, arm, f"foot.{suffix}", 20, 12)
        capsule(f"BootUpper.{suffix}", ankle + Vector((0, 0, .02)), ankle + Vector((0, 0, .20)), .080, .069, white, arm, f"shin.{suffix}", .01, 18)
        box(f"BootSole.{suffix}", ankle + Vector((0, -.07, -.045)), (.095, .16, .018), red, arm, f"foot.{suffix}", .008)
        box(f"BootTongue.{suffix}", ankle + Vector((0, -.077, .095)), (.034, .012, .075), red, arm, f"shin.{suffix}", .006)

    return arm


def build_pov_glove():
    black = material("POVBlackLeather", (0.018, 0.022, 0.028), 0.08, 0.24)
    gold = material("POVGold", (0.78, 0.48, 0.12), 0.45, 0.24)
    skin = material("POVSkin", (0.43, 0.22, 0.13), 0.0, 0.48)

    data = bpy.data.armatures.new("POVGloveRig")
    arm = bpy.data.objects.new("POVGloveRig", data)
    bpy.context.collection.objects.link(arm)
    bpy.context.view_layer.objects.active = arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    add_bone(data.edit_bones, "root", (0, 0, -.35), (0, 0, -.25))
    add_bone(data.edit_bones, "forearm", (0, 0, -.31), (0, 0, -.09), "root")
    add_bone(data.edit_bones, "hand", (0, 0, -.09), (0, 0, .05), "forearm")
    bpy.ops.object.mode_set(mode="OBJECT")
    arm.select_set(False)

    capsule("POVForearm", (0, 0, -.285), (0, 0, -.10), .054, .066, skin, arm, "forearm", .012, 20)
    capsule("POVCuff", (0, 0, -.125), (0, 0, -.035), .078, .084, black, arm, "hand", .010, 20)
    ellipsoid("POVGlovePalm", (0, -.010, .026), (.098, .084, .108), black, arm, "hand", 26, 18)
    ellipsoid("POVGloveKnuckles", (0, -.036, .064), (.102, .090, .070), black, arm, "hand", 26, 16)
    box("POVGoldBand", (0, -.083, -.055), (.070, .009, .015), gold, arm, "hand", .005)
    ellipsoid("POVBadge", (0, -.094, .072), (.038, .007, .028), gold, arm, "hand", 14, 8)
    return arm


def ensure_collection(name):
    col = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    if col.name not in bpy.context.scene.collection.children:
        try:
            bpy.context.scene.collection.children.link(col)
        except RuntimeError:
            pass
    return col


def move_family_to_collection(armature, collection):
    objs = [armature] + [o for o in bpy.context.scene.objects if o.parent == armature]
    for obj in objs:
        for old in list(obj.users_collection):
            old.objects.unlink(obj)
        collection.objects.link(obj)
    return objs


def export_fbx(path, objects):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.hide_render = False
        obj.hide_viewport = False
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path),
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
    bpy.ops.object.select_all(action="DESELECT")


def look_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_preview(path, ramirez_objects, pov_objects):
    for obj in pov_objects:
        obj.hide_render = True
    for obj in ramirez_objects:
        obj.hide_render = False
    world = bpy.context.scene.world
    world.color = (0.008, 0.008, 0.012)
    bpy.ops.mesh.primitive_plane_add(size=8, location=(0, 0, 0))
    floor = bpy.context.object
    floor.data.materials.append(material("PreviewFloor", (0.035, 0.025, 0.02), 0.0, 0.75))
    bpy.ops.object.light_add(type="AREA", location=(-2.2, -2.4, 3.2))
    key = bpy.context.object
    key.data.energy = 1050
    key.data.shape = "DISK"
    key.data.size = 2.4
    look_at(key, (0, 0, 1.15))
    bpy.ops.object.light_add(type="AREA", location=(2.0, -0.7, 2.4))
    rim = bpy.context.object
    rim.data.energy = 780
    rim.data.size = 1.7
    look_at(rim, (0, 0, 1.25))
    bpy.ops.object.light_add(type="AREA", location=(0, 2.2, 2.6))
    back = bpy.context.object
    back.data.energy = 900
    back.data.size = 1.3
    look_at(back, (0, 0, 1.3))
    bpy.ops.object.camera_add(location=(0, -4.0, 1.28))
    cam = bpy.context.object
    cam.data.lens = 66
    look_at(cam, (0, 0, 1.02))
    bpy.context.scene.camera = cam
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 960
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(path)
    scene.render.film_transparent = False
    bpy.ops.render.render(write_still=True)


def mesh_stats(objects):
    tris = 0
    verts = 0
    for obj in objects:
        if obj.type != "MESH":
            continue
        verts += len(obj.data.vertices)
        for poly in obj.data.polygons:
            tris += max(1, len(poly.vertices) - 2)
    return {"vertices": verts, "triangles": tris, "mesh_objects": sum(1 for o in objects if o.type == "MESH")}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    script_args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(script_args)
    repo = Path(args.repo).resolve()
    source_dir = repo / "art" / "blender" / "source"
    export_dir = repo / "art" / "blender" / "exports"
    evidence_dir = repo / "evidence" / "p1-blender3d"
    unity_dir = repo / "unity" / "BoxerP0" / "Assets" / "Resources" / "Boxer3D"
    for d in (source_dir, export_dir, evidence_dir, unity_dir):
        d.mkdir(parents=True, exist_ok=True)

    clear_scene()
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0

    ramirez_arm = build_ramirez(repo)
    ramirez_collection = ensure_collection("Ramirez")
    ramirez_objects = move_family_to_collection(ramirez_arm, ramirez_collection)
    pov_arm = build_pov_glove()
    pov_collection = ensure_collection("PlayerPOV")
    pov_objects = move_family_to_collection(pov_arm, pov_collection)

    blend_path = source_dir / "boxer_uat3_blender_assets.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))

    opponent_fbx = export_dir / "Ramirez_UAT3.fbx"
    glove_fbx = export_dir / "PlayerPOVGlove_UAT3.fbx"
    export_fbx(opponent_fbx, ramirez_objects)
    export_fbx(glove_fbx, pov_objects)
    export_fbx(unity_dir / opponent_fbx.name, ramirez_objects)
    export_fbx(unity_dir / glove_fbx.name, pov_objects)

    preview = evidence_dir / "blender-ramirez-preview.png"
    render_preview(preview, ramirez_objects, pov_objects)

    manifest = {
        "blender_version": bpy.app.version_string,
        "source": str(blend_path.relative_to(repo)),
        "exports": [str(opponent_fbx.relative_to(repo)), str(glove_fbx.relative_to(repo))],
        "ramirez": mesh_stats(ramirez_objects),
        "player_pov": mesh_stats(pov_objects),
        "rig_bones": [b.name for b in ramirez_arm.data.bones],
        "player_rig_bones": [b.name for b in pov_arm.data.bones],
        "reference_target": "docs/handoff/reference-ui/06-opponent-ramirez-turnaround.jpg and 03-pov-combat-hud.jpg",
    }
    (evidence_dir / "blender-asset-manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
