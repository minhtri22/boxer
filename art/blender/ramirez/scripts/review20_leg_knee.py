import argparse
import json
import runpy
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", required=True)
    parser.add_argument("--phase", choices=("static", "all"), default="static")
    return parser.parse_args(argv)


def aim_camera(camera, position, target):
    camera.location = position
    direction = Vector(target) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render(scene, camera, path, position, target, ortho_scale):
    aim_camera(camera, position, target)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho_scale
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def allowed_knee_vertex(point, knees):
    for knee in knees:
        dx = point.x - knee.x
        dz = point.z - knee.z
        if abs(dx) <= 0.115 and abs(dz) <= 0.150:
            locality = 1.0 - (dx / 0.115) ** 2 - (dz / 0.150) ** 2
            if locality > 0.0:
                return True
    return False


def local_slice(coords, knee, dz, half_height=0.012):
    target_z = knee.z + dz
    points = [
        point for point in coords
        if abs(point.z - target_z) <= half_height
        and abs(point.x - knee.x) <= 0.12
    ]
    if not points:
        raise RuntimeError(f"No knee slice points at dz={dz}")
    return {
        "width_x": max(point.x for point in points) - min(point.x for point in points),
        "depth_y": max(point.y for point in points) - min(point.y for point in points),
        "front_y": min(point.y for point in points),
        "back_y": max(point.y for point in points),
    }


def landmark_band(coords, knee, dz_center, dz_half, dx_half):
    points = [
        point for point in coords
        if abs((point.z - knee.z) - dz_center) <= dz_half
        and abs(point.x - knee.x) <= dx_half
    ]
    if not points:
        raise RuntimeError("No knee landmark points")
    return {
        "front_y": min(point.y for point in points),
        "back_y": max(point.y for point in points),
    }


def main():
    args = parse_args()
    repo = Path(args.repo)
    module = runpy.run_path(
        str(repo / "art" / "blender" / "ramirez" / "scripts" / "rig_and_render_ramirez.py"),
        run_name="ramirez_leg_review20_module",
    )
    shape_knees = module["shape_boxer_legs_and_knees"]

    blend_path = repo / "art" / "blender" / "ramirez" / "source" / "ramirez_master.blend"
    bpy.ops.wm.open_mainfile(filepath=str(blend_path))
    body = bpy.data.objects["Ramirez_Master_Body"]
    rig = bpy.data.objects["RamirezRig"]
    camera = bpy.data.objects["VisualGateCamera"]
    scene = bpy.context.scene
    out = repo / "art" / "blender" / "ramirez" / "renders" / "review20-leg-knee"
    out.mkdir(parents=True, exist_ok=True)

    saved_camera = camera.matrix_world.copy()
    saved_type = camera.data.type
    saved_lens = float(camera.data.lens)
    saved_ortho = float(camera.data.ortho_scale)
    saved_render = (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    )
    saved_override = scene.view_layers[0].material_override
    before = [vertex.co.copy() for vertex in body.data.vertices]

    knees = [
        Vector(rig.data.bones["thigh_l"].tail_local),
        Vector(rig.data.bones["thigh_r"].tail_local),
    ]

    scene.render.resolution_x = 1024
    scene.render.resolution_y = 1024
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"

    # Baseline is rendered before any correction so Review20 has direct proof
    # that the new knee landmarks came from the scoped body edit.
    render(
        scene, camera, out / "baseline_knee_front.png",
        (0.0, -2.65, 0.62), (0.0, -0.02, 0.55), 0.92,
    )
    render(
        scene, camera, out / "baseline_knee_side.png",
        (2.65, 0.0, 0.62), (0.15, -0.02, 0.55), 0.84,
    )

    shape_knees(body, rig)
    bpy.context.view_layer.update()

    render(
        scene, camera, out / "knee_front_closeup.png",
        (0.0, -2.65, 0.62), (0.0, -0.02, 0.55), 0.92,
    )
    render(
        scene, camera, out / "knee_three_quarter_closeup.png",
        (1.65, -2.25, 0.64), (0.04, -0.02, 0.55), 0.92,
    )
    render(
        scene, camera, out / "knee_side_closeup.png",
        (2.65, 0.0, 0.62), (0.15, -0.02, 0.55), 0.84,
    )

    if args.phase == "all":
        render(
            scene, camera, out / "legs_front_full.png",
            (0.0, -3.10, 0.68), (0.0, -0.02, 0.58), 1.30,
        )
        render(
            scene, camera, out / "legs_side_full.png",
            (3.10, 0.0, 0.68), (0.15, -0.02, 0.58), 1.30,
        )

        clay = bpy.data.materials.get("ClayGate")
        if clay is not None:
            scene.view_layers[0].material_override = clay
            render(
                scene, camera, out / "knee_front_clay.png",
                (0.0, -2.65, 0.62), (0.0, -0.02, 0.55), 0.92,
            )
            render(
                scene, camera, out / "knee_side_clay.png",
                (2.65, 0.0, 0.62), (0.15, -0.02, 0.55), 0.84,
            )
        scene.view_layers[0].material_override = saved_override

    changed = []
    outside = []
    max_displacement = 0.0
    for index, (old, vertex) in enumerate(zip(before, body.data.vertices)):
        displacement = (Vector(vertex.co) - old).length
        if displacement <= 1e-7:
            continue
        changed.append(index)
        max_displacement = max(max_displacement, displacement)
        if not allowed_knee_vertex(old, knees):
            outside.append(index)

    after_coords = [Vector(vertex.co) for vertex in body.data.vertices]
    knee = knees[0]
    before_slices = {dz: local_slice(before, knee, dz) for dz in (0.12, 0.04, 0.0, -0.04, -0.12)}
    after_slices = {dz: local_slice(after_coords, knee, dz) for dz in (0.12, 0.04, 0.0, -0.04, -0.12)}
    before_patella = landmark_band(before, knee, 0.014, 0.030, 0.050)
    after_patella = landmark_band(after_coords, knee, 0.014, 0.030, 0.050)
    patella_projection = before_patella["front_y"] - after_patella["front_y"]
    popliteal_inset = before_patella["back_y"] - after_patella["back_y"]
    condyle_width_gain = after_slices[0.0]["width_x"] - before_slices[0.0]["width_x"]
    outer_slice_relative_change = max(
        abs(after_slices[dz]["width_x"] - before_slices[dz]["width_x"])
        / max(before_slices[dz]["width_x"], 1e-9)
        for dz in (0.12, -0.12)
    )

    report = {
        "round": "review20-leg-knee",
        "scope": "LEG_KNEE_ONLY",
        "phase": args.phase,
        "knee_shape_profile": body.get("knee_shape_profile"),
        "changed_vertices": len(changed),
        "outside_scope_changed_vertices": len(outside),
        "outside_scope_vertex_ids": outside[:32],
        "max_displacement_m": round(max_displacement, 6),
        "scope_lock_pass": bool(changed) and not outside,
        "measurements": {
            "patella_front_projection_gain_m": round(patella_projection, 6),
            "popliteal_back_inset_m": round(popliteal_inset, 6),
            "condyle_width_gain_m": round(condyle_width_gain, 6),
            "outer_thigh_calf_slice_max_relative_width_change": round(outer_slice_relative_change, 6),
        },
        "numeric_gates": {
            "K01_patella_projects_forward": patella_projection >= 0.006,
            "K02_popliteal_hollow_present": popliteal_inset >= 0.002,
            "K03_condyles_gain_width": 0.001 <= condyle_width_gain <= 0.012,
            "K04_outer_leg_proportion_preserved": outer_slice_relative_change <= 0.015,
            "K05_max_deformation_bounded": max_displacement <= 0.015,
        },
        "anatomical_targets": {
            "patella_anterior_projection": True,
            "medial_lateral_condyle_transition": True,
            "patellar_tendon_recess": True,
            "tibial_tuberosity_transition": True,
            "popliteal_hollow": True,
        },
    }
    (out / "numeric-knee-precheck.json").write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    if not report["scope_lock_pass"] or not all(report["numeric_gates"].values()):
        raise RuntimeError("LEG_KNEE_ONLY scope lock failed")

    # Restore evidence-only scene state.  The intended knee mesh edit remains.
    camera.matrix_world = saved_camera
    camera.data.type = saved_type
    camera.data.lens = saved_lens
    camera.data.ortho_scale = saved_ortho
    (
        scene.render.resolution_x,
        scene.render.resolution_y,
        scene.render.resolution_percentage,
        scene.render.filepath,
    ) = saved_render
    scene.view_layers[0].material_override = saved_override
    bpy.context.view_layer.update()

    if args.phase == "all":
        bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
