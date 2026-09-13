"""Read-only source/geometry inspection; NOT a Unity test or runtime validation.

Run from any directory: python tools/uat2_preflight.py
Outputs JSON on stdout. No gameplay values are modified.
"""
import hashlib
import json
import math
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = ROOT / "unity/BoxerP0/Assets/Scripts"


def read(name):
    return (SCRIPTS / name).read_text(encoding="utf-8-sig")


def scalar(source, name):
    match = re.search(r"\b" + re.escape(name) + r"\s*=\s*(-?[\d.]+)f\s*;", source)
    if not match:
        raise ValueError("Cannot extract source constant: " + name)
    return float(match[1])


arm = read("ArmVisualEmbodiment.cs")
player = read("PlayerBoxer.cs")
targets = re.findall(
    r"PunchIntent\.(\w+) => new Vector3\((-?[\d.]+)f, (-?[\d.]+)f, (-?[\d.]+)f\)",
    player[player.index("private static Vector3 PunchTargetLocal"):],
)
if len(targets) != 8:
    raise ValueError("Expected exactly eight authoritative punch endpoints")

upper = scalar(arm, "_upperArmLength")
forearm = scalar(arm, "_forearmLength")
width = scalar(arm, "_shoulderWidth")
height = scalar(arm, "ShoulderHeight")
forward = scalar(arm, "ShoulderForward")
radius = scalar(arm, "_visualGloveRadius")
rows = []
for intent, x, y, z in targets:
    left = intent == "Jab" or intent.startswith("Lead")
    shoulder = [-width if left else width, height, forward]
    target = list(map(float, (x, y, z)))
    vector = [b - a for a, b in zip(shoulder, target)]
    distance = math.sqrt(sum(v * v for v in vector))
    solved = min(distance, upper + forearm - 0.001)
    wrist = [a + v * solved / distance for a, v in zip(shoulder, vector)]
    rows.append({
        "intent": intent,
        "shoulder_local": shoulder,
        "target_local": target,
        "shoulder_to_endpoint_m": round(distance, 6),
        "visual_wrist_at_full_extension_local": [round(v, 6) for v in wrist],
        "endpoint_to_visual_wrist_gap_m": round(distance - solved, 6),
    })

relevant = [
    "OpponentBoxer.cs", "OpponentLegEmbodiment.cs", "PlayerBoxer.cs",
    "ArmVisualEmbodiment.cs", "P1BodyRotationMath.cs", "BoxerVisualShell.cs",
    "P1VCombatPresentation.cs", "BoxerBootstrap.cs",
]
build = ROOT / "builds/web/boxer-p0-web"
build_files = [build / "index.html"] + sorted((build / "Build").glob("*"))
marker = re.search(r'const productVersion = "([^"]+)"', (build / "index.html").read_text())
output = {
    "evidence": "STATIC_SOURCE_INSPECTION_NOT_UNITY_TEST",
    "inspected_head": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
    "scope": "Neutral player full-extension endpoints; no A1/A3 modifiers; current fixed visual shoulder; reproduces ArmChainMath reach clamp only",
    "upper_arm_m": upper,
    "forearm_m": forearm,
    "solver_max_wrist_distance_m": upper + forearm - 0.001,
    "visual_glove_radius_m": radius,
    "punches": rows,
    "source_sha256": {name: hashlib.sha256((SCRIPTS / name).read_bytes()).hexdigest() for name in relevant},
    "existing_artifact_only": {
        "product_version": marker[1] if marker else None,
        "sha256": {p.relative_to(build).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest() for p in build_files if p.is_file()},
        "note": "These are pre-existing files, not a Round 2 build. Their original source provenance is not certified by this inspection.",
    },
}
print(json.dumps(output, indent=2))
