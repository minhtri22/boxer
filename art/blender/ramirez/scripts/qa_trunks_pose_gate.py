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
        "knee_l": tuple(world_point(rig, "calf_l")),
        "knee_r": tuple(world_point(rig, "calf_r")),
        "ankle_l": tuple(world_point(rig, "foot_l")),
        "ankle_r": tuple(world_point(rig, "foot_r")),
    }


def distance(a, b):
    return math.sqrt(sum((x - y) ** 2 for x, y in zip(a, b)))


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
    report["waistband.height_ok"] = 0.035 <= waistband.dimensions.z <= 0.070
    report["waistband.height_m"] = round(waistband.dimensions.z, 4)


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
    report["metric.guard.wrist_l_z"] = round(guard["wrist_l"][2], 4)
    report["metric.guard.wrist_r_z"] = round(guard["wrist_r"][2], 4)
    report["pose.guard.forward_crouch"] = guard["pelvis"][1] - guard["head"][1] >= 0.08
    report["pose.guard.knees_bent"] = guard["knee_l"][2] < 0.47 and guard["knee_r"][2] < 0.47
    report["pose.guard.split_stance"] = guard["ankle_l"][1] < -0.15 and guard["ankle_r"][1] > 0.12
    report["pose.guard.hands_home"] = guard["wrist_l"][2] > 1.38 and guard["wrist_r"][2] > 1.40

    jab = snapshots["jab"]
    report["pose.jab.lead_extension"] = jab["wrist_l"][1] < -0.58
    report["pose.jab.rear_guard"] = jab["wrist_r"][1] > -0.40 and jab["wrist_r"][2] > 1.40
    report["pose.jab.grounded"] = jab["ankle_l"][2] < 0.10 and jab["ankle_r"][2] < 0.10

    cross = snapshots["cross"]
    report["metric.guard.ankle_r_z"] = round(guard["ankle_r"][2], 4)
    report["metric.cross.ankle_r_z"] = round(cross["ankle_r"][2], 4)
    report["metric.cross.rear_heel_delta_z"] = round(cross["ankle_r"][2] - guard["ankle_r"][2], 4)
    report["pose.cross.rear_extension"] = cross["wrist_r"][1] < -0.58
    report["pose.cross.lead_guard"] = cross["wrist_l"][1] > -0.40 and cross["wrist_l"][2] > 1.40
    report["pose.cross.rear_heel_release"] = cross["ankle_r"][2] - guard["ankle_r"][2] > 0.025

    slip_l = snapshots["slip_left"]
    slip_r = snapshots["slip_right"]
    report["pose.slip_left.head_off_center"] = abs(slip_l["head"][0] - guard["head"][0]) > 0.035
    report["pose.slip_right.head_off_center"] = abs(slip_r["head"][0] - guard["head"][0]) > 0.035
    report["pose.slip_left.grounded"] = slip_l["ankle_l"][2] < 0.10 and slip_l["ankle_r"][2] < 0.10
    report["pose.slip_right.grounded"] = slip_r["ankle_l"][2] < 0.10 and slip_r["ankle_r"][2] < 0.10

    counter = snapshots["slip_counter"]
    report["pose.slip_counter.head_off_center"] = abs(counter["head"][0] - guard["head"][0]) > 0.025
    report["pose.slip_counter.rear_extension"] = counter["wrist_r"][1] < -0.56
    report["pose.slip_counter.loaded_base"] = counter["knee_l"][2] < 0.47 and counter["knee_r"][2] < 0.47

    recover = snapshots["recover_guard"]
    report["pose.recover.guard_return_l"] = distance(recover["wrist_l"], guard["wrist_l"]) < 0.08
    report["pose.recover.guard_return_r"] = distance(recover["wrist_r"], guard["wrist_r"]) < 0.08


def main():
    repo = Path(bpy.data.filepath).resolve().parents[4]
    script_path = repo / "art" / "blender" / "ramirez" / "scripts" / "rig_and_render_ramirez.py"
    report = {}
    check_trunks(report)
    check_poses(report, script_path)
    failures = [key for key, value in report.items() if isinstance(value, bool) and not value]
    result = {"checks": report, "failures": failures, "pass": not failures}
    print("TRUNKS_POSE_QA=" + json.dumps(result, indent=2, sort_keys=True))
    if failures:
        sys.exit(1)


if __name__ == "__main__":
    main()
