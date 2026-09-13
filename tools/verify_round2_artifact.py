"""Verify a committed-source Round 2 build without modifying the artifact."""
import argparse
import hashlib
import json
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("artifact", type=Path)
parser.add_argument("source_sha")
args = parser.parse_args()
root = args.artifact.resolve(strict=True)
provenance = dict(line.split("=", 1) for line in (root / "provenance.txt").read_text().splitlines() if "=" in line)
assert provenance["source_sha"] == args.source_sha
version = "r2-" + args.source_sha
assert provenance["productVersion"] == version
assert 'const productVersion = "' + version + '";' in (root / "index.html").read_text(encoding="utf-8")
files = {}
for file in sorted(root.rglob("*")):
    if not file.is_file():
        continue
    relative = file.relative_to(root).as_posix()
    digest = hashlib.sha256(file.read_bytes()).hexdigest()
    if relative != "provenance.txt":
        assert provenance.get(relative) == digest, f"Artifact changed after build: {relative}"
    files[relative] = {"sha256": digest, "bytes": file.stat().st_size}
manifest_hash = hashlib.sha256(json.dumps(files, sort_keys=True, separators=(",", ":")).encode()).hexdigest()
print(json.dumps({"result": "PASS", "source_sha": args.source_sha, "productVersion": version,
                  "unity": provenance["unity"], "manifest_sha256": manifest_hash, "files": files}, indent=2))
