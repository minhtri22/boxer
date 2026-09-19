import json
import math

import bpy
from mathutils import Vector


def percentile(values, q):
    values = sorted(values)
    if not values:
        raise RuntimeError("empty percentile input")
    idx = min(len(values) - 1, max(0, int(round((len(values) - 1) * q))))
    return values[idx]


def limb_diameter(body, rig, bone_name, q=0.70, t_min=0.30, t_max=0.70, min_weight=0.75):
    vg = body.vertex_groups.get(bone_name)
    bone = rig.data.bones.get(bone_name)
    if vg is None or bone is None:
        raise RuntimeError(f"missing skinning data for {bone_name}")
    head = Vector(bone.head_local)
    axis = Vector(bone.tail_local) - head
    axis_len2 = axis.length_squared
    radii = []
    for vertex in body.data.vertices:
        try:
            weight = vg.weight(vertex.index)
        except RuntimeError:
            continue
        if weight < min_weight:
            continue
        point = Vector(vertex.co)
        t = max(0.0, min(1.0, (point - head).dot(axis) / axis_len2))
        if t < t_min or t > t_max:
            continue
        nearest = head + axis * t
        radii.append((point - nearest).length)
    # Measure the central limb belly. Excluding both joint ends prevents
    # shoulder/elbow overlap vertices from inflating the apparent diameter.
    return 2.0 * percentile(radii, q)


def weighted_width(body, group_names, min_weight=0.35):
    group_ids = {
        body.vertex_groups[name].index
        for name in group_names
        if body.vertex_groups.get(name) is not None
    }
    xs = []
    for vertex in body.data.vertices:
        if any(g.group in group_ids and g.weight >= min_weight for g in vertex.groups):
            xs.append(float(vertex.co.x))
    if not xs:
        raise RuntimeError(f"no vertices for groups {group_names}")
    return max(xs) - min(xs)


report = {"checks": {}, "metrics": {}, "failures": []}
checks = report["checks"]
metrics = report["metrics"]

body = bpy.data.objects.get("Ramirez_Master_Body")
rig = bpy.data.objects.get("RamirezRig")
if body is None or rig is None:
    raise RuntimeError("Ramirez body/rig missing")

glove_l = bpy.data.objects.get("RamirezGlove.L")
glove_r = bpy.data.objects.get("RamirezGlove.R")
for name, obj in (("glove_l", glove_l), ("glove_r", glove_r)):
    checks[f"{name}.exists"] = obj is not None
    if obj is not None:
        metrics[f"{name}.width_m"] = round(float(obj.dimensions.x), 4)

for side in ("L", "R"):
    neck = bpy.data.objects.get(f"RamirezGlove.{side}.Neck")
    cuff = bpy.data.objects.get(f"RamirezGlove.{side}.Cuff")
    thumb = bpy.data.objects.get(f"RamirezGlove.{side}.Thumb")
    bridge = bpy.data.objects.get(f"RamirezGlove.{side}.ThumbBridge")
    thumb_crease = bpy.data.objects.get(f"RamirezGlove.{side}.ThumbCrease")
    palm_crease = bpy.data.objects.get(f"RamirezGlove.{side}.PalmCrease")
    checks[f"glove_{side.lower()}.neck_exists"] = neck is not None
    checks[f"glove_{side.lower()}.cuff_exists"] = cuff is not None
    checks[f"glove_{side.lower()}.thumb_exists"] = thumb is not None
    checks[f"glove_{side.lower()}.thumb_bridge_exists"] = bridge is not None
    checks[f"glove_{side.lower()}.thumb_crease_exists"] = thumb_crease is not None
    checks[f"glove_{side.lower()}.palm_crease_exists"] = palm_crease is not None
    main = glove_l if side == "L" else glove_r
    if main is not None and neck is not None and cuff is not None:
        main_w = float(main.dimensions.x)
        main_h = float(main.dimensions.z)
        neck_w = float(neck.dimensions.x)
        cuff_w = float(cuff.dimensions.x)
        metrics[f"glove_{side.lower()}.body_height_to_width_ratio"] = round(main_h / main_w, 3)
        metrics[f"glove_{side.lower()}.neck_width_m"] = round(neck_w, 4)
        metrics[f"glove_{side.lower()}.cuff_width_m"] = round(cuff_w, 4)
        metrics[f"glove_{side.lower()}.neck_to_body_ratio"] = round(neck_w / main_w, 3)
        metrics[f"glove_{side.lower()}.cuff_to_body_ratio"] = round(cuff_w / main_w, 3)
        checks[f"glove_{side.lower()}.boxing_body_taper"] = 1.25 <= main_h / main_w <= 1.60
        checks[f"glove_{side.lower()}.neck_cinched"] = 0.48 <= neck_w / main_w <= 0.68
        checks[f"glove_{side.lower()}.cuff_cinched"] = 0.44 <= cuff_w / main_w <= 0.62
        checks[f"glove_{side.lower()}.neck_wider_than_cuff"] = neck_w > cuff_w
        if thumb is not None:
            thumb_ratio = float(thumb.dimensions.x) / main_w
            metrics[f"glove_{side.lower()}.thumb_to_body_ratio"] = round(thumb_ratio, 3)
            checks[f"glove_{side.lower()}.thumb_proportion"] = 0.45 <= thumb_ratio <= 0.72

glove_material = bpy.data.materials.get("GloveRed")
checks["glove_material.exists"] = glove_material is not None
if glove_material is not None and glove_material.use_nodes:
    nodes = glove_material.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    checks["glove_material.leather_micro_surface"] = (
        nodes.get("GloveLeatherMicro") is not None
        and nodes.get("GloveLeatherBump") is not None
    )
    checks["glove_material.broken_roughness"] = (
        nodes.get("GloveRoughnessNoise") is not None
        and nodes.get("GloveRoughnessRamp") is not None
        and bsdf is not None
        and bsdf.inputs["Roughness"].is_linked
    )
else:
    checks["glove_material.leather_micro_surface"] = False
    checks["glove_material.broken_roughness"] = False

for side in ("L", "R"):
    for stem in ("RamirezShorts.InnerLeg",):
        obj = bpy.data.objects.get(f"{stem}.{side}")
        checks[f"inner_leg_{side.lower()}.exists"] = obj is not None
        if obj is not None:
            checks[f"inner_leg_{side.lower()}.coverage_size"] = (
                obj.dimensions.x >= 0.23 and obj.dimensions.z >= 0.24
            )

inner_body = bpy.data.objects.get("RamirezShorts.InnerBody")
checks["inner_body.exists"] = inner_body is not None
if inner_body is not None:
    checks["inner_body.coverage_size"] = (
        inner_body.dimensions.x >= 0.40 and inner_body.dimensions.z >= 0.16
    )

for side, glove in (("l", glove_l), ("r", glove_r)):
    diameter = limb_diameter(body, rig, f"lowerarm_{side}")
    upper_diameter = limb_diameter(body, rig, f"upperarm_{side}")
    metrics[f"forearm_{side}.diameter_p90_m"] = round(diameter, 4)
    metrics[f"upperarm_{side}.diameter_p90_m"] = round(upper_diameter, 4)
    arm_ratio = upper_diameter / diameter
    metrics[f"upperarm_to_forearm_{side}.ratio"] = round(arm_ratio, 3)
    checks[f"upperarm_to_forearm_{side}.reference_band"] = 1.05 <= arm_ratio <= 1.25
    if glove is not None:
        ratio = float(glove.dimensions.x) / diameter
        metrics[f"glove_to_forearm_{side}.ratio"] = round(ratio, 3)
        # Locked reference criterion: glove remains visibly boxing-sized, but
        # must transition into a robust forearm rather than a thin stick.
        checks[f"glove_to_forearm_{side}.reference_band"] = 1.55 <= ratio <= 2.05
        cuff = bpy.data.objects.get(f"RamirezGlove.{side.upper()}.Cuff")
        if cuff is not None:
            cuff_ratio = float(cuff.dimensions.x) / diameter
            metrics[f"cuff_to_forearm_{side}.ratio"] = round(cuff_ratio, 3)
            # Cuff must visibly compress around the wrist/forearm junction.
            checks[f"cuff_to_forearm_{side}.retention_band"] = 0.88 <= cuff_ratio <= 1.08

chest_width = weighted_width(body, ("spine_02", "spine_03"))
waist_width = weighted_width(body, ("spine_01",))
metrics["chest_width_m"] = round(chest_width, 4)
metrics["waist_width_m"] = round(waist_width, 4)
metrics["chest_to_waist_ratio"] = round(chest_width / waist_width, 3)
checks["chest_to_waist.reference_band"] = 1.25 <= chest_width / waist_width <= 1.55

for key, value in checks.items():
    if value is not True:
        report["failures"].append(key)

report["pass"] = not report["failures"]
print("REFERENCE_LOCK_QA=" + json.dumps(report, indent=2, sort_keys=True))
raise SystemExit(0 if report["pass"] else 1)
