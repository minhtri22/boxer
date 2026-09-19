import json
import math
import runpy
import sys
from pathlib import Path

import bpy


def world_point(rig, bone_name, tail=False):
    bone = rig.pose.bones[bone_name]
    point = bone.tail if tail else bone.head
    return rig.matrix_world @ point


def pose_snapshot(rig):
    return {
        "pelvis": tuple(world_point(rig, "pelvis")),
        "head": tuple(world_point(rig, "head")),
        "wrist_l": tuple(world_point(rig, "lowerarm_l", tail=True)),
        "wrist_r": tuple(world_point(rig, "lowerarm_r", tail=True)),
        "hip_l": tuple(world_point(rig, "thigh_l")),
        "hip_r": tuple(world_point(rig, "thigh_r")),
        "knee_l": tuple(world_point(rig, "calf_l")),
        "knee_r": tuple(world_point(rig, "calf_r")),
        "ankle_l": tuple(world_point(rig, "foot_l")),
        "ankle_r": tuple(world_point(rig, "foot_r")),
        "toe_l": tuple(world_point(rig, "foot_l", tail=True)),
        "toe_r": tuple(world_point(rig, "foot_r", tail=True)),
    }


def distance(a, b):
    return math.sqrt(sum((x - y) ** 2 for x, y in zip(a, b)))


def joint_angle(a, b, c):
    ba = tuple(x - y for x, y in zip(a, b))
    bc = tuple(x - y for x, y in zip(c, b))
    dot = sum(x * y for x, y in zip(ba, bc))
    mag_ba = math.sqrt(sum(x * x for x in ba))
    mag_bc = math.sqrt(sum(x * x for x in bc))
    cosine = max(-1.0, min(1.0, dot / max(1e-9, mag_ba * mag_bc)))
    return math.degrees(math.acos(cosine))


def check_trunks(report):
    left = bpy.data.objects["RamirezShorts.L"]
    right = bpy.data.objects["RamirezShorts.R"]
    waistband = bpy.data.objects["RamirezWaistband"]
    legs = [left, right]

    for leg in legs:
        unweighted = sum(1 for vertex in leg.data.vertices if not vertex.groups)
        solidify = next(mod for mod in leg.modifiers if mod.type == "SOLIDIFY")
        report[f"{leg.name}.fully_weighted"] = unweighted == 0
        report[f"{leg.name}.thin_fabric"] = solidify.thickness <= 0.002

    # The generated lower ring is the final 32 vertices before the cap center.
    samples = 32
    lower_l = [left.data.vertices[i].co.x for i in range(len(left.data.vertices) - 1 - samples, len(left.data.vertices) - 1)]
    lower_r = [right.data.vertices[i].co.x for i in range(len(right.data.vertices) - 1 - samples, len(right.data.vertices) - 1)]
    inner_gap = min(lower_l) - max(lower_r)
    report["trunks.separate_leg_openings"] = inner_gap > 0.0
    report["trunks.inner_gap_m"] = round(inner_gap, 4)
    upper_l = [left.data.vertices[i].co.x for i in range(samples)]
    upper_r = [right.data.vertices[i].co.x for i in range(samples)]
    upper_width = (max(upper_l) - min(upper_l) + max(upper_r) - min(upper_r)) / 2.0
    lower_width = (max(lower_l) - min(lower_l) + max(lower_r) - min(lower_r)) / 2.0
    report["trunks.upper_opening_width_m"] = round(upper_width, 4)
    report["trunks.lower_opening_width_m"] = round(lower_width, 4)
    report["trunks.cloth_flare"] = lower_width - upper_width >= 0.045
    lower_z_l = [left.data.vertices[i].co.z for i in range(len(left.data.vertices) - 1 - samples, len(left.data.vertices) - 1)]
    lower_z_r = [right.data.vertices[i].co.z for i in range(len(right.data.vertices) - 1 - samples, len(right.data.vertices) - 1)]
    hem_range = max(max(lower_z_l) - min(lower_z_l), max(lower_z_r) - min(lower_z_r))
    report["trunks.hem_drape_range_m"] = round(hem_range, 4)
    # Large vertical hem variation was the exact visual defect reported as
    # "torn shorts".  The reference calls for a continuous hem with only a
    # shallow cloth drape, not a pronounced side split/notch.
    report["trunks.hem_drape"] = 0.004 <= hem_range <= 0.020
    report["trunks.no_torn_notch"] = hem_range <= 0.020

    for side in ("L", "R"):
        liner = bpy.data.objects.get(f"RamirezShorts.InnerLeg.{side}")
        report[f"trunks.inner_leg_{side.lower()}.exists"] = liner is not None
        if liner is not None:
            report[f"trunks.inner_leg_{side.lower()}.coverage_depth"] = liner.dimensions.z >= 0.24
    report["waistband.height_ok"] = 0.035 <= waistband.dimensions.z <= 0.070
    report["waistband.height_m"] = round(waistband.dimensions.z, 4)


def check_visual_reference_gate(report, repo):
    gate_path = repo / "art" / "blender" / "ramirez" / "review5-visual-gate.json"
    report["visual_reference.gate_file_exists"] = gate_path.exists()
    if not gate_path.exists():
        report["visual_reference.pass"] = False
        return
    gate = json.loads(gate_path.read_text(encoding="utf-8"))
    checks = gate.get("checks", {})
    required_visual_checks = (
        "reference_turnaround_match",
        "anatomy_plausible",
        "trunks_visual_match",
        "glove_scale_match",
        "trunk_no_skin_exposure",
        "forearm_glove_transition_match",
        "body_proportion_reference_match",
    )
    report["visual_reference.required_checks_present"] = all(
        key in checks for key in required_visual_checks
    )
    for key in required_visual_checks:
        report[f"visual_reference.{key}"] = checks.get(key) is True
    report["visual_reference.pass"] = (
        bool(gate.get("pass"))
        and report["visual_reference.required_checks_present"]
        and all(checks.get(key) is True for key in required_visual_checks)
    )


def check_poses(report, script_path):
    module = runpy.run_path(str(script_path), run_name="ramirez_pose_module")
    pose_targets = module["pose_targets"]
    # runpy returns a mapping, but the function keeps its own globals mapping.
    # Patch that exact namespace so pose QA can exercise bones without needing
    # the render-only glove cuff/badge bundle.
    pose_targets.__globals__["update_glove_pose"] = lambda *args, **kwargs: None
    rig = bpy.data.objects["RamirezRig"]
    gloves = {
        "L": {"main": bpy.data.objects["RamirezGlove.L"]},
        "R": {"main": bpy.data.objects["RamirezGlove.R"]},
    }

    snapshots = {}
    for pose in ("guard", "jab", "cross", "slip_left", "slip_right", "slip_counter", "recover_guard"):
        pose_targets(rig, gloves, pose)
        bpy.context.view_layer.update()
        snapshots[pose] = pose_snapshot(rig)

    guard = snapshots["guard"]
    guard_knee_l_angle = joint_angle(guard["hip_l"], guard["knee_l"], guard["ankle_l"])
    guard_knee_r_angle = joint_angle(guard["hip_r"], guard["knee_r"], guard["ankle_r"])
    report["metric.guard.head_z"] = round(guard["head"][2], 4)
    report["metric.guard.wrist_l_z"] = round(guard["wrist_l"][2], 4)
    report["metric.guard.wrist_r_z"] = round(guard["wrist_r"][2], 4)
    report["metric.guard.wrist_l_x"] = round(guard["wrist_l"][0], 4)
    report["metric.guard.wrist_r_x"] = round(guard["wrist_r"][0], 4)
    report["metric.guard.head_x"] = round(guard["head"][0], 4)
    report["metric.guard.wrist_l_head_distance"] = round(distance(guard["wrist_l"], guard["head"]), 4)
    report["metric.guard.wrist_r_head_distance"] = round(distance(guard["wrist_r"], guard["head"]), 4)
    report["metric.guard.knee_l_z"] = round(guard["knee_l"][2], 4)
    report["metric.guard.knee_r_z"] = round(guard["knee_r"][2], 4)
    report["metric.guard.knee_r_x"] = round(guard["knee_r"][0], 4)
    report["metric.guard.ankle_r_x"] = round(guard["ankle_r"][0], 4)
    report["metric.guard.knee_l_angle_deg"] = round(guard_knee_l_angle, 2)
    report["metric.guard.knee_r_angle_deg"] = round(guard_knee_r_angle, 2)
    report["pose.guard.forward_crouch"] = guard["pelvis"][1] - guard["head"][1] >= 0.10
    report["pose.guard.knees_bent"] = guard_knee_l_angle < 165.0 and guard_knee_r_angle < 165.0
    report["pose.guard.split_stance"] = guard["ankle_l"][1] < -0.15 and guard["ankle_r"][1] > 0.12
    report["pose.guard.hands_below_chin"] = (
        guard["head"][2] - 0.38 < guard["wrist_l"][2] < guard["head"][2] - 0.22
        and guard["head"][2] - 0.38 < guard["wrist_r"][2] < guard["head"][2] - 0.22
    )
    report["pose.guard.face_open"] = (
        abs(guard["wrist_l"][0] - guard["head"][0]) > 0.14
        and abs(guard["wrist_r"][0] - guard["head"][0]) > 0.14
        and distance(guard["wrist_l"], guard["head"]) < 0.48
        and distance(guard["wrist_r"], guard["head"]) < 0.48
    )
    report["pose.guard.right_knee_tracks_foot"] = abs(guard["knee_r"][0] - guard["ankle_r"][0]) < 0.035

    jab = snapshots["jab"]
    report["pose.jab.lead_extension"] = jab["wrist_l"][1] < -0.58
    report["pose.jab.rear_guard"] = jab["wrist_r"][1] > -0.40 and jab["wrist_r"][2] > 1.40
    report["pose.jab.grounded"] = jab["ankle_l"][2] < 0.10 and jab["ankle_r"][2] < 0.10
    report["pose.jab.body_involvement"] = jab["pelvis"][1] < guard["pelvis"][1] - 0.012

    cross = snapshots["cross"]
    report["metric.guard.ankle_r_z"] = round(guard["ankle_r"][2], 4)
    report["metric.cross.ankle_r_z"] = round(cross["ankle_r"][2], 4)
    cross_rear_heel_above_toe = cross["ankle_r"][2] - cross["toe_r"][2]
    report["metric.cross.rear_heel_above_toe_z"] = round(cross_rear_heel_above_toe, 4)
    report["metric.cross.rear_toe_z"] = round(cross["toe_r"][2], 4)
    report["pose.cross.rear_extension"] = cross["wrist_r"][1] < -0.58
    report["pose.cross.lead_guard"] = cross["wrist_l"][1] > -0.40 and cross["wrist_l"][2] > 1.40
    report["pose.cross.rear_heel_release"] = cross_rear_heel_above_toe > 0.035 and cross["toe_r"][2] < 0.08
    report["pose.cross.forward_drive"] = cross["pelvis"][1] < guard["pelvis"][1] - 0.020

    slip_l = snapshots["slip_left"]
    slip_r = snapshots["slip_right"]
    slip_l_angle_l = joint_angle(slip_l["hip_l"], slip_l["knee_l"], slip_l["ankle_l"])
    slip_l_angle_r = joint_angle(slip_l["hip_r"], slip_l["knee_r"], slip_l["ankle_r"])
    slip_r_angle_l = joint_angle(slip_r["hip_l"], slip_r["knee_l"], slip_r["ankle_l"])
    slip_r_angle_r = joint_angle(slip_r["hip_r"], slip_r["knee_r"], slip_r["ankle_r"])
    report["metric.slip_left.knee_l_angle_deg"] = round(slip_l_angle_l, 2)
    report["metric.slip_left.knee_r_angle_deg"] = round(slip_l_angle_r, 2)
    report["metric.slip_right.knee_l_angle_deg"] = round(slip_r_angle_l, 2)
    report["metric.slip_right.knee_r_angle_deg"] = round(slip_r_angle_r, 2)
    report["metric.slip_left.knee_angle_delta_deg"] = round(abs(slip_l_angle_l - slip_l_angle_r), 2)
    report["metric.slip_right.knee_angle_delta_deg"] = round(abs(slip_r_angle_l - slip_r_angle_r), 2)
    report["pose.slip_left.head_off_center"] = abs(slip_l["head"][0] - guard["head"][0]) > 0.055
    report["pose.slip_right.head_off_center"] = abs(slip_r["head"][0] - guard["head"][0]) > 0.055
    report["pose.slip_left.pelvis_shift"] = abs(slip_l["pelvis"][0] - guard["pelvis"][0]) > 0.035
    report["pose.slip_right.pelvis_shift"] = abs(slip_r["pelvis"][0] - guard["pelvis"][0]) > 0.035
    report["pose.slip_left.asymmetric_load"] = abs(slip_l_angle_l - slip_l_angle_r) > 4.0
    report["pose.slip_right.asymmetric_load"] = abs(slip_r_angle_l - slip_r_angle_r) > 4.0
    report["pose.slip_left.grounded"] = slip_l["ankle_l"][2] < 0.10 and slip_l["ankle_r"][2] < 0.10
    report["pose.slip_right.grounded"] = slip_r["ankle_l"][2] < 0.10 and slip_r["ankle_r"][2] < 0.10

    counter = snapshots["slip_counter"]
    counter_knee_l_angle = joint_angle(counter["hip_l"], counter["knee_l"], counter["ankle_l"])
    counter_knee_r_angle = joint_angle(counter["hip_r"], counter["knee_r"], counter["ankle_r"])
    report["metric.slip_counter.ankle_r_z"] = round(counter["ankle_r"][2], 4)
    counter_rear_heel_above_toe = counter["ankle_r"][2] - counter["toe_r"][2]
    report["metric.slip_counter.rear_heel_above_toe_z"] = round(counter_rear_heel_above_toe, 4)
    report["metric.slip_counter.rear_toe_z"] = round(counter["toe_r"][2], 4)
    report["metric.slip_counter.knee_l_angle_deg"] = round(counter_knee_l_angle, 2)
    report["metric.slip_counter.knee_r_angle_deg"] = round(counter_knee_r_angle, 2)
    report["pose.slip_counter.head_off_center"] = abs(counter["head"][0] - guard["head"][0]) > 0.025
    report["pose.slip_counter.rear_extension"] = counter["wrist_r"][1] < -0.56
    report["pose.slip_counter.loaded_base"] = counter_knee_l_angle < 165.0 and counter_knee_r_angle < 165.0
    report["pose.slip_counter.rear_heel_release"] = counter_rear_heel_above_toe > 0.035 and counter["toe_r"][2] < 0.08

    recover = snapshots["recover_guard"]
    report["pose.recover.guard_return_l"] = distance(recover["wrist_l"], guard["wrist_l"]) < 0.08
    report["pose.recover.guard_return_r"] = distance(recover["wrist_r"], guard["wrist_r"]) < 0.08


def main():
    repo = Path(bpy.data.filepath).resolve().parents[4]
    script_path = repo / "art" / "blender" / "ramirez" / "scripts" / "rig_and_render_ramirez.py"
    report = {}
    check_trunks(report)
    check_poses(report, script_path)
    check_visual_reference_gate(report, repo)
    failures = [key for key, value in report.items() if isinstance(value, bool) and not value]
    result = {"checks": report, "failures": failures, "pass": not failures}
    print("TRUNKS_POSE_QA=" + json.dumps(result, indent=2, sort_keys=True))
    if failures:
        sys.exit(1)


if __name__ == "__main__":
    main()
